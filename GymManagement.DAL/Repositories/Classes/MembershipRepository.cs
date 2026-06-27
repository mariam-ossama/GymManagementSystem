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
    public class MembershipRepository : GenericRepository<MemberShipViewModel> , IMembershipRepository
    {
        private readonly GymDbContext _dbContext;

        public MembershipRepository(GymDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<MemberShipViewModel>> GetAllMembershipsWithMembersAndPlansAsync(Expression<Func<MemberShipViewModel, bool>>? predicate = null, CancellationToken ct = default)
        {
            IQueryable<MemberShipViewModel> query = _dbContext.MemberShips
                              .AsNoTracking()
                              .Include(m => m.Member)
                              .Include(p => p.Plan);

            if (predicate is not null) 
                query = query.Where(predicate);

            return await query.ToListAsync(ct);
        }
    }
}
