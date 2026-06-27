using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MembershipViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GymManagement.BLL.Services.Classes
{
    public class MembershipService : IMembershipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MembershipService(IUnitOfWork unitOfWork,
                                 IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result> CreateMembershipViewModelAsync(CreateMembershipViewModel model, CancellationToken ct = default)
        {
            var memberExists = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Id == model.MemberId, ct);
            if (!memberExists)
                return Result.NotFound("Member Not Found");

            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(model.PlanId, ct);
            if (plan == null)
                return Result.NotFound("Plan Not Found");
            if (!plan.IsActive)
                return Result.Fail("Plan is InActive");

            var hasActive = await _unitOfWork.MembershipRepository
                .AnyAsync(m => m.MemberId == model.MemberId && m.EndDate > DateTime.Now, ct);
            if (hasActive)
                return Result.Fail("Member has Already an Active Membership");

            var entity = new MemberShipViewModel()
            {
                MemberId = model.MemberId,
                PlanId = model.PlanId,
                CreatedAt = DateTime.Now,
                EndDate = (model.StartDate ?? DateTime.Now).AddDays(plan.DurationDays)
            };

            _unitOfWork.MembershipRepository.Add(entity);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Create a Membership");
        }

        public async Task<Result> DeleteMembershipViewModelAsync(int memberId, CancellationToken ct = default)
        {
            var active = await _unitOfWork.MembershipRepository
                .FirstOrDefaultAsync(m => m.MemberId == memberId && m.EndDate > DateTime.Now, true, ct);
            if (active == null)
                return Result.NotFound("No Active Membership for this Member");

            _unitOfWork.MembershipRepository.Delete(active);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.Ok() : Result.Fail("Failed To Delete a Membership");
        }

        public async Task<IEnumerable<MemberShipViewModel>> GetAllMembershipAsync(CancellationToken ct = default)
        {
            var memberships = await _unitOfWork.MembershipRepository
                .GetAllMembershipsWithMembersAndPlansAsync(m => m.EndDate > DateTime.Now, ct:ct);
            return _mapper.Map<IEnumerable<MemberShipViewModel>>(memberships);
        }

        public async Task<IEnumerable<MemberSelectListViewModel>> GetMembersForDropDownAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(ct:ct);
            return _mapper.Map<IEnumerable<MemberSelectListViewModel>>(members);
        }

        public async Task<IEnumerable<PlanSelectListViewModel>> GetPlansForDropDownAsync(CancellationToken ct = default)
        {
            var plans = await _unitOfWork.GetRepository<Plan>().GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<PlanSelectListViewModel>>(plans);
        }
    }
}
