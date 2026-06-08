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

        // GET BaseUrl/Members/Details/{id}
        // HealthRecordDetails -> Show one member's details

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

        // POST BaseUrl/Members/Edit {Member}
        // Edit => Submit form
        #endregion

        #region DeleteMember
        // GET BaseUrl/Members/Delete/{id}
        // Delete => show Confirmation Form

        // DELETE BaseUrl/Members/Delete/{id}
        // DeleteConfirmed => submit form
        #endregion
    }
}
