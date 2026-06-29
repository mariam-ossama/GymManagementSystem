using GymManagement.DAL.Data.DbContexts;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Repositories.Classes
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        private readonly GymDbContext _dbContext;

        public SessionRepository(GymDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<Session?>> GetAllSessionsWithTrainerAndCategory(Expression<Func<Session, bool>> predicate = null, CancellationToken ct = default)
        {
            var query = _dbContext.Sessions
                .AsNoTracking()
                .Include(s => s.Trainer)
                .Include(s => s.Category)
                .AsNoTracking();
            if (predicate is not null)
                query = query.Where(predicate);
            return await query.ToListAsync(ct);
        }

        public async Task<int> GetCountOfBookedSlotsAsync(int sessionId, CancellationToken ct = default)
        {
            return await _dbContext.Bookings.AsNoTracking().CountAsync(b => b.SessionId == sessionId, ct);
        }

        public async Task<Session?> GetSessionByIdWithTrainerAndCategory(int sessionId, CancellationToken ct = default)
        {
            return await _dbContext.Sessions
                .AsNoTracking()
                .Include(t => t.Trainer)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(x => x.Id == sessionId);
        }
    }
}
