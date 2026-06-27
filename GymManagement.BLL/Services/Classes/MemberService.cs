using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GymManagement.BLL.Services.Attachment;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;

namespace GymManagement.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;

        public MemberService(IUnitOfWork unitOfWork,
                             IMapper mapper,
                             IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
        }

        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            // check email
            var emailExist = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Email == model.Email, ct);
            // check phone number
            var phoneExist = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Phone == model.Phone, ct);
            // if phone or email exists return false
            if (emailExist || phoneExist) return false;

            // Upload Photo
            var storedPhtotName = await _attachmentService.UploadAsync(model.Photo.OpenReadStream(), model.Photo.FileName, "MembersPhotos");
            if(string.IsNullOrWhiteSpace(storedPhtotName)) return false;

            // else create member and return true
            var member = _mapper.Map<Member>(model);
            member.Photo = storedPhtotName;
            //var result = _memberRepository.AddAsync(member);
            _unitOfWork.GetRepository<Member>().Add(member); // Add Locally
            var result = await _unitOfWork.SaveChangesAsync(ct);
            if(result > 0) return true;
            else
            {
                // Delete Uploaded Photo
                _attachmentService.Delete(storedPhtotName, "MembersPhotos");
                return false;
            }
        }

        public async Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(ct:ct);
            if(!members.Any()) return [];

            var membersViewModel = _mapper.Map < IEnumerable<Member>, IEnumerable<MemberViewModel>>(members);
            return membersViewModel;
        }

        public async Task<MemberViewModel?> GetMemberDetailsByIdAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);
            if(member == null) return null;
            var model = _mapper.Map<MemberViewModel>(member);
            //var memberships = await _membershipRepository.GetAllAsync();
            //var activeMembership = memberships.FirstOrDefault(x => x.MemberId == memberId && x.EndDate > DateTime.Now);

            var activeMembership = await _unitOfWork.GetRepository<MemberShipViewModel>().FirstOrDefaultAsync(x => x.MemberId == memberId && x.EndDate > DateTime.Now);

            if(activeMembership is not null)
            {
                var activePlan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(activeMembership.PlanId, ct);
                model.PlanName = activePlan?.Name;
                model.MembershipStartDate = activeMembership.CreatedAt.ToString();
                model.MembershipEndDate = activeMembership.EndDate.ToString();
            }
            return model;
        }

        public async Task<HealthRecordViewModel?> GetMemberHealthRecordAsync(int memberId, CancellationToken ct = default)
        {
            var record = await _unitOfWork.GetRepository<HealthRecord>().FirstOrDefaultAsync(x => x.MemberId == memberId, ct: ct);
            if (record == null) return null;
            else
                return _mapper.Map<HealthRecord, HealthRecordViewModel>(record);
        }

        public async Task<MemberToUpdateViewModel?> GetMemberToUpdate(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);
            if (member == null) return null;
            else
                return _mapper.Map<MemberToUpdateViewModel>(member);
        }

        public async Task<bool> RemoveMemberAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);

            if (member == null) return false;

            var hasFutureBookings = await _unitOfWork.GetRepository<Booking>().AnyAsync(b => b.MemberId == memberId && b.Session.StartDate > DateTime.Now, ct); // Exception

            if (hasFutureBookings) return false;

            _unitOfWork.GetRepository<Member>().Delete(member);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }

        public async Task<bool> UpdateMemberDetailsAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(id, ct);
            if (member == null) return false;
            var emailExists = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Email == model.Email && m.Id != id);
            var phoneExists = await _unitOfWork.GetRepository<Member>().AnyAsync(m => m.Phone == model.Phone && m.Id != id);
            if (emailExists || phoneExists) return false;

            _mapper.Map(model, member);

            member.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Member>().Update(member);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;

        }
    }
}
