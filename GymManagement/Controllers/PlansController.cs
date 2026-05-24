using GymManagement.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Controllers
{
    public class PlansController : Controller
    {
        private readonly GymDbContext dbContext;
        public PlansController()
        {
            dbContext = new GymDbContext();
        }
        // Index action
        // GET Base_Url/Plans/Index -> Get all plans
        public async Task<IActionResult> Index()
        {
            var plans = await dbContext.Plans.ToListAsync();
            return View(plans);
        }
        // Details Action
        // GET Base_Url/Plans/Details/{id} -> Get plan details by id
        public async Task<IActionResult> Details(int id)
        {
            var plan = await dbContext.Plans.FindAsync(id);
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
