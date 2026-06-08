using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Classes;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers
{
    public class PlansController : Controller
    {
        //private readonly GymDbContext dbContext;
        private readonly IGenericRepository<Plan> _planRepository;
        public PlansController(IGenericRepository<Plan> repository)
        {
            _planRepository = repository;
        }
        // Index action
        // GET Base_Url/Plans/Index -> Get all plans
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var plans = await _planRepository.GetAllAsync(ct:ct);
            return View(plans);
        }
        // Details Action
        // GET Base_Url/Plans/Details/{id} -> Get plan details by id
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var plan = await _planRepository.GetByIdAsync(id,ct);
            if (plan is null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(plan);
            }
        }
    }
}
