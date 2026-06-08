using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;

namespace GymManagement.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IGenericRepository<Member> _memberRepository;
        public MemberService(IGenericRepository<Member> memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            // check email
            var emailExist = await _memberRepository.AnyAsync(m => m.Email == model.Email, ct);
            // check phone number
            var phoneExist = await _memberRepository.AnyAsync(m => m.Phone == model.Phone, ct);
            // if phone or email exists return false
            if (emailExist || phoneExist) return false;
            // else create member and return true
            var member = new Member()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Address = new Address()
                {
                    BuildingNumber = model.BuildingNumber,
                    City = model.City,
                    Street = model.Street
                },
                HealthRecord = new HealthRecord()
                {
                    BloodType = model.HealthRecordViewModel.BloodType,
                    Weight = model.HealthRecordViewModel.Weight,
                    Height = model.HealthRecordViewModel.Height,
                    Note = model.HealthRecordViewModel.Note
                }
            };
            var result = _memberRepository.AddAsync(member);
            return await result > 0;
        }

        public async Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct = default)
        {
            var members = await _memberRepository.GetAllAsync(ct:ct);
            if(!members.Any()) return [];

            //IList<MemberViewModel> membersViewModel = new List<MemberViewModel>();
            //foreach(var member in members)
            //{
            //    var memberViewModels = new MemberViewModel()
            //    {
            //        Name = member.Name,
            //        Email = member.Email,
            //        Gender = member.Gender.ToString(),
            //        Phone = member.Phone,
            //        Photo = member.Photo,
            //        Id = member.Id
            //    };
            //    membersViewModel.Add(memberViewModels);
            //}
            //return membersViewModel;

            var membersViewModel = members.Select(m => new MemberViewModel()
            {
                Email = m.Email,
                Name = m.Name,
                Phone = m.Phone,
                Photo = m.Photo,
                Gender = m.Gender.ToString(),
                Id = m.Id
            });
            return membersViewModel;
        }
    }
}
