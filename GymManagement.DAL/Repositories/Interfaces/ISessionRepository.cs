using GymManagement.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Repositories.Interfaces
{
    public interface ISessionRepository : IGenericRepository<Session>
    {
        public Task<IEnumerable<Session?>> GetAllSessionsWithTrainerAndCategory(Expression<Func<Session, bool>> predicate = null, CancellationToken ct = default);
        public Task<int> GetCountOfBookedSlotsAsync(int sessionId, CancellationToken ct = default);
        public Task<Session?> GetSessionByIdWithTrainerAndCategory(int sessionId, CancellationToken ct = default);
    }
}
