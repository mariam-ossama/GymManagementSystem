using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.PlanViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Classes;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers
{
    public class PlansController : Controller
    {
        private readonly IPlanService _planService;
        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }
        // Index action
        // GET Base_Url/Plans/Index -> Get all plans
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var plans = await _planService.GetAllPlansAsync(ct: ct);
            return View(plans);
        }
        // Details Action
        // GET Base_Url/Plans/Details/{id} -> Get plan details by id
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var plan = await _planService.GetPlanDetailsByIdAsync(id, ct);
            if (plan is null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(plan);
            }
        }

        // GET BaseUrl/Plans/Edit/{id}
        // Edit => show edit Form (pre-filled form)
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var plan = await _planService.GetPlanToUpdate(id, ct);
            if (plan == null)
            {
                TempData["ErrorMessage"] = "Plan not found";
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }

        // POST BaseUrl/Plans/Edit {Member}
        // Edit => Submit form
        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdatePlanViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _planService.UpdatePlanDetailsAsync(id, model, ct);
            if (result)
            {
                TempData["SuccessMessage"] = "Plan Updated Successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed To Update Plan";
            }
            return RedirectToAction(nameof(Index));
        }

        // Activate/Deactivate plan
        // POST BaseUrl/Plans/Activate/{id}
        // Edit => Deactivate plan
        [HttpPost]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var plan = await _planService.TogglePlanActivationAsync(id, ct);
            if(plan)
            {
                TempData["SuccessMessage"] = "Plan Status Changed Successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed To Change Plan Status";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
