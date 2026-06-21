using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagement.DAL.Data.Models;

namespace GymManagement.DAL.Repositories.Interfaces
{
    public interface ISessionRepository : IGenericRepository<Session>
    {
        public Task<IEnumerable<Session?>> GetAllSessionsWithTrainerAndCategory(CancellationToken ct);
        public Task<int> GetCountOfBookedSlotsAsync(int sessionId, CancellationToken ct = default);
        public Task<Session?> GetSessionByIdWithTrainerAndCategory(int sessionId, CancellationToken ct = default);
    }
}
