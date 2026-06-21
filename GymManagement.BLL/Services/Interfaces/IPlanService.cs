using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagement.BLL.Common;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.BLL.ViewModels.PlanViewModels;
using GymManagement.DAL.Data.Models;

namespace GymManagement.BLL.Services.Interfaces
{
    public interface IPlanService
    {
        Task<IEnumerable<PlanViewModel?>> GetAllPlansAsync(CancellationToken ct = default);
        Task<Result<PlanViewModel>> GetPlanDetailsByIdAsync(int planId, CancellationToken ct = default);
        Task<Result<UpdatePlanViewModel>> GetPlanToUpdate(int planId, CancellationToken ct = default);
        Task<Result> UpdatePlanDetailsAsync(int id, UpdatePlanViewModel model, CancellationToken ct = default);
        Task<Result> TogglePlanActivationAsync(int planId, CancellationToken ct = default);
    }
}
