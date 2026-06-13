using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.PL.Controllers
{
    public class MembersController : Controller
    {
        private readonly IMemberService _memberService;
        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }
        // GET BaseUrl/Members/Index
        // Index - List all members
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var members = await _memberService.GetAllMembersAsync(ct);
            return View(members);
        }
        // GET BaseUrl/Members/Details/{id}
        // MemberDetails -> Show one member's details
        [HttpGet]
        public async Task<IActionResult> MemberDetails(int id, CancellationToken ct)
        {
            // Get member by its id
            var member = await _memberService.GetMemberDetailsByIdAsync(id,ct);
            // Check if member is null => Return Index with message
            if (member is null)
            {
                TempData["ErrorMessage"] = "Member Not Found";
                return RedirectToAction(nameof(Index));
            }
            // else => Return view data
            return View(member);
        }

        // GET BaseUrl/Members/Details/{id}
        // HealthRecordDetails -> Show one member's details
        [HttpGet]
        public async Task<IActionResult> HealthRecordDetails(int id, CancellationToken ct)
        {
            // Get HealthRecord by Member id
            var result = await _memberService.GetMemberHealthRecordAsync(id, ct);
            // Check if member is null => Return Index with message
            if(result is null)
            {
                TempData["ErrorMessage"] = "Health Record Not Found";
                return RedirectToAction(nameof(Index));
            }
            // else => Return view data
            return View(result);
        }

        #region CreateMember
        // GET BaseUrl/Members/Create
        // Create => show empty Form
        [HttpGet]
        public IActionResult Create() => View();
        // POST BaseUrl/Members/Create
        // CreateMember => Submit form
        [HttpPost]
        public async Task<IActionResult> CreateMember(CreateMemberViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(nameof(Create), model);

            var result = await _memberService.CreateMemberAsync(model, ct);

            if (result)
                TempData["SuccessMessage"] = "Member Created Successfully";
            else 
                TempData["ErrorMessage"] = "Failed To Create Member";

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region EditMember
        // GET BaseUrl/Members/Edit/{id}
        // Edit => show edit Form (pre-filled form)
        [HttpGet]
        public async Task<IActionResult> EditMember(int id, CancellationToken ct)
        {
            var member = await _memberService.GetMemberToUpdate(id, ct);
            if (member == null)
            {
                TempData["ErrorMessage"] = "Member Is Not Found";
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // POST BaseUrl/Members/Edit {Member}
        // Edit => Submit form
        [HttpPost]
        public async Task<IActionResult> EditMember(int id, MemberToUpdateViewModel model ,CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _memberService.UpdateMemberDetailsAsync(id, model, ct);
            if (result)
            {
                TempData["SuccessMessage"] = "Member Updated Successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to Update Member";
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region DeleteMember
        // GET BaseUrl/Members/Delete/{id}
        // Delete => show Confirmation Form
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var member =await _memberService.GetMemberDetailsByIdAsync(id, ct);
            if(member == null)
            {
                TempData["ErrorMessage"] = "Member Not Found";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        // DELETE BaseUrl/Members/Delete/{id}
        // DeleteConfirmed => submit form
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed([FromRoute]int id, CancellationToken ct)
        {
            var result = await _memberService.RemoveMemberAsync(id, ct);
            if(result)
            {
                TempData["SuccessMessage"] = "Member Deleted Successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed To Delete Member";
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
