using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.PlanViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;

namespace GymManagement.BLL.Services.Classes
{
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PlanViewModel>> GetAllPlansAsync(CancellationToken ct = default)
        {
            var plans = await _unitOfWork.GetRepository<Plan>().GetAllAsync();
            // check if there is any plan found
            if (!plans.Any()) return [];
            var plansViewModel = plans.Select(p => new PlanViewModel()
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DurationDays = p.DurationDays,
                Description = p.Description,
                IsActive = p.IsActive
            });
            return plansViewModel;
        }

        public async Task<PlanViewModel?> GetPlanDetailsByIdAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);

            if (plan == null) return null;

            var model = new PlanViewModel()
            {
                Name = plan.Name,
                Price = plan.Price,
                DurationDays = plan.DurationDays,
                Description = plan.Description,
                IsActive = plan.IsActive
            };
            return model;
        }

        public async Task<UpdatePlanViewModel?> GetPlanToUpdate(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            if(plan == null) return null;
            else
                return new UpdatePlanViewModel()
                {
                    PlanName = plan.Name,
                    Price = plan.Price,
                    DurationDays = plan.DurationDays,
                    Description = plan.Description
                };
        }

        public async Task<bool> TogglePlanActivationAsync(int planId, CancellationToken ct = default)
        {
            // Get plan itself
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            // check if exists
            if (plan == null) return false;
            // Cannot update or deactivate a plan with active memberships
            if (plan.IsActive)
            {
                var IsAnyemberships = await _unitOfWork.GetRepository<MemberShip>().AnyAsync(m => m.PlanId == planId && m.EndDate > DateTime.Now, ct);
                if(IsAnyemberships) return false;
            }
            // Update IsActive to true/false => make it reversible
            plan.IsActive = !plan.IsActive;
            plan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Plan>().Update(plan);

            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }

        public async Task<bool> UpdatePlanDetailsAsync(int id, UpdatePlanViewModel model, CancellationToken ct = default)
        {
            // Get the plan itself
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(id, ct);
            // check if exists
            if (plan == null) return false;
            // Plan name cannot be updated
            // Cannot update or deactivate a plan with active memberships
            var IsAnyemberships = await _unitOfWork.GetRepository<MemberShip>().AnyAsync(m => m.PlanId == id && m.EndDate > DateTime.Now,ct);
            if (IsAnyemberships) return false;

            plan.DurationDays = model.DurationDays;
            plan.Price = model.Price;
            plan.Description = model.Description;
            plan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Plan>().Update(plan);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;

        }
    }
}
