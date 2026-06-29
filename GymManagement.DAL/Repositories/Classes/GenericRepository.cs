using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GymManagement.DAL.Data.DbContexts;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.DAL.Repositories.Classes
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity, new()
    {
        private readonly GymDbContext _dbContext;
        private readonly DbSet<TEntity> _set;
        public GenericRepository(GymDbContext dbContext)
        {
            _dbContext = dbContext;
            _set = _dbContext.Set<TEntity>();
        }
        public void Add(TEntity entity)
        {
            _set.Add(entity);
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        {
            return _set.AsNoTracking().AnyAsync(predicate,ct);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? condition = null, CancellationToken ct = default)
        {
            if(condition == null)
                return await _set.AsNoTracking().CountAsync(ct);
            else
                return await _set.AsNoTracking().CountAsync(condition,ct);
        }

        public void Delete(TEntity entity)
        {
            _set.Remove(entity);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken ct = default)
        {
            IQueryable<TEntity> query = tracking ? _set : _set.AsNoTracking();
            return await query.FirstOrDefaultAsync(predicate, ct);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate = null, bool tracking = false, CancellationToken ct = default)
        {

            IQueryable<TEntity> query = tracking ? _set : _set.AsNoTracking();

            if (predicate is not null)
                query = query.Where(predicate);

            return await query.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default) => await _set.FindAsync(id, ct);

        public void Update(TEntity entity)
        {
            _set.Update(entity);
        }
    }
}
