using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.PlanViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;

namespace GymManagement.BLL.Services.Classes
{
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlanService(IUnitOfWork unitOfWork,
                           IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PlanViewModel?>> GetAllPlansAsync(CancellationToken ct = default)
        {
            var plans = await _unitOfWork.GetRepository<Plan>().GetAllAsync();
            // check if there is any plan found
            if (!plans.Any()) return [];
            var plansViewModel = _mapper.Map<IEnumerable<PlanViewModel>>(plans);
            return plansViewModel;
        }

        public async Task<Result<PlanViewModel>> GetPlanDetailsByIdAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);

            if (plan == null) return Result<PlanViewModel>.NotFound("Plan Not Found");

            var model = _mapper.Map<Plan,PlanViewModel>(plan);
            return Result<PlanViewModel>.Ok(model);
        }

        public async Task<Result<UpdatePlanViewModel>> GetPlanToUpdate(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            if (plan == null) return Result<UpdatePlanViewModel>.NotFound("Plan Not Found");
            else
            {
                var mapped = _mapper.Map<UpdatePlanViewModel>(plan);
                return Result<UpdatePlanViewModel>.Ok(mapped);
            }
        }

        public async Task<Result> TogglePlanActivationAsync(int planId, CancellationToken ct = default)
        {
            // Get plan itself
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            // check if exists
            if (plan == null) return Result.NotFound("Plan Not Found");
            // Cannot update or deactivate a plan with active memberships
            if (plan.IsActive)
            {
                var IsAnyemberships = await _unitOfWork.GetRepository<MemberShip>().AnyAsync(m => m.PlanId == planId && m.EndDate > DateTime.Now, ct);
                if(IsAnyemberships) return Result.Validation("Cannot Deactivate a Plan with Membership");
            }
            // Update IsActive to true/false => make it reversible
            plan.IsActive = !plan.IsActive;
            plan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Plan>().Update(plan);

            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Update a Plan Status");
        }

        public async Task<Result> UpdatePlanDetailsAsync(int id, UpdatePlanViewModel model, CancellationToken ct = default)
        {
            // Get the plan itself
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(id, ct);
            // check if exists
            if (plan == null) return Result.NotFound("Plan Not Found");
            // Plan name cannot be updated
            // Cannot update or deactivate a plan with active memberships
            var IsAnyemberships = await _unitOfWork.GetRepository<MemberShip>().AnyAsync(m => m.PlanId == id && m.EndDate > DateTime.Now,ct);
            if (IsAnyemberships) return Result.Validation("Cannot Update or Deactivate a Plan With Active Memberships");

            _mapper.Map(model,plan);
            plan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Plan>().Update(plan);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Update a Plan");

        }
    }
}
