using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MembershipViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagement.PL.Controllers
{
    [Authorize]
    public class MembershipsController : Controller
    {
        private readonly IMembershipService _membershipService;

        public MembershipsController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        // Get All Memberships
        // GET BaseUrl/
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var memberships = await _membershipService.GetAllMembershipAsync(ct);
            return View(memberships);
        }
        // Get create membership form
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateDropDownAsync(ct);
            return View();
        }
        // Create A Membership
        [HttpPost]
        public async Task<IActionResult> Create(CreateMembershipViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDownAsync(ct);
                return View(model);
            }
            var result = await _membershipService.CreateMembershipViewModelAsync(model, ct);
            if(result.success)
            {
                TempData["SuccessMessage"] = "Membership Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.error;
            await PopulateDropDownAsync(ct);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            var result = await _membershipService.DeleteMembershipViewModelAsync(id, ct);
            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] 
                = result.success ? "Membership Cancelled Successfully" : result.error;
            return RedirectToAction(nameof(Index));
        }
        private async Task PopulateDropDownAsync(CancellationToken ct)
        {
            var plans = await _membershipService.GetPlansForDropDownAsync(ct);
            ViewBag.Plans = new SelectList(plans, "Id", "Name");

            var members = await _membershipService.GetMembersForDropDownAsync(ct);
            ViewBag.Members = new SelectList(members, "Id", "Name");
        }
    }
}
