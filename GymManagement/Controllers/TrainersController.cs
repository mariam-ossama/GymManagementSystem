using GymManagement.BLL.Services.Classes;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.PL.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ITrainerService _trainerService;

        public TrainersController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var trainers = await _trainerService.GetAllTrainersAsync(ct);
            return View(trainers);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var trainer = await _trainerService.GetTrainerDetailsAsync(id, ct);
            if (trainer is null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(trainer);
            }
        }
        #region Create Trainer
        [HttpGet]
        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(CreateTrainerViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(nameof(Create), model);
            var result = await _trainerService.CreateTrainerAsync(model, ct);
            if (result)
                TempData["SuccessMessage"] = "Trainer Created Successfully";
            else
                TempData["ErrorMessage"] = "Failed To Create Trainer";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Edit Trainer
        //GET BaseUrl/Trainers/Edit/{id}
        // Gets edit form
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var trainer = await _trainerService.GetTrainerToUpdate(id, ct);
            if(trainer == null)
            {
                TempData["ErrorMessage"] = "Trainer Not Found";
                return RedirectToAction(nameof(Index));
            }
            return View(trainer);
        }
        //POST BaseUrl/Trainers/Edit/{id}
        // Gets edit form
        [HttpPost]
        public async Task<IActionResult> Edit(int id, TrainerToUpdateViewModel model, CancellationToken ct)
        {
            if(!ModelState.IsValid)
                return View(nameof(Edit), model);
            var result = await _trainerService.UpdateTrainerAsync(id, model, ct);
            if(result)
            {
                TempData["SuccessMessage"] = "Trainer Updated Successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed To Update Trainer";
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region DeleteMember
        // GET BaseUrl/Trainers/Delete/{id}
        // Delete => show Confirmation Form
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var member = await _trainerService.GetTrainerDetailsAsync(id);
            if (member == null)
            {
                TempData["ErrorMessage"] = "Trainer Not Found";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        // DELETE BaseUrl/Trainers/Delete/{id}
        // DeleteConfirmed => submit form
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id, CancellationToken ct)
        {
            var result = await _trainerService.RemoveTrainerAsync(id, ct);
            if (result)
            {
                TempData["SuccessMessage"] = "Trainer Deleted Successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed To Delete Trainer";
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
