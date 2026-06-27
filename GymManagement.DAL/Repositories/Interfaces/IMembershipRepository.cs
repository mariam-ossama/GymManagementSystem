using GymManagement.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Repositories.Interfaces
{
    public interface IMembershipRepository : IGenericRepository<MemberShipViewModel>
    {
        Task<List<MemberShipViewModel>> GetAllMembershipsWithMembersAndPlansAsync(Expression<Func<MemberShipViewModel, bool>>? predicate = null, CancellationToken ct = default);
    }
}
