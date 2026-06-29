using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.BookingViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagement.PL.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var bookings = await _bookingService.GetAllSessionsAsync(ct);
            return View(bookings);
        }
        [HttpGet]
        public async Task<IActionResult> Create(int id, CancellationToken ct)
        {
            var members = await _bookingService.GetMemberForDropDownAsync(id, ct);

            ViewBag.Members = new SelectList(members, "Id", "Name");
            ViewBag.SessionId = id;

            return View(new CreateBookingViewModel
            {
                SessionId = id
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _bookingService.CreateNewBookingAsync(model, ct);
            TempData[result.success ? "SuccessMessage" : "ErrorMessage"]
                = result.success ? "Booking Created Successfully" : result.error;
            return RedirectToAction(nameof(GetMembersForUpcommingSessions), new {id = model.SessionId});
        }
        [HttpGet]
        public async Task<IActionResult> GetMembersForUpcommingSessions(int id, CancellationToken ct)
        {
            var members = await _bookingService.GetMembersForUpcommingSessionAsync(id, ct);
            return View(members);
        }
        [HttpGet]
        public async Task<IActionResult> GetMembersForOngoingSessions(int id, CancellationToken ct)
        {
            var members = await _bookingService.GetMembersForOngoingSessionAsync(id,ct);
            return View(members);
        }
        [HttpPost]
        public async Task<IActionResult> Attended(int memberId, int sessionId, CancellationToken ct)
        {
            var result = await _bookingService.MarkAttendedAsync(memberId, sessionId, ct);
            TempData[result.success ? "SuccessMessage" : "ErrorMessage"]
                = result.success ? "Attendance Recorded Successfully" : result.error;
            return RedirectToAction(nameof(GetMembersForOngoingSessions), new {id = sessionId});
        }
        [HttpPost]
        public async Task<IActionResult> Cancel(int memberId, int sessionId, CancellationToken ct)
        {
            var result = await _bookingService.CancelBookingAsync(memberId, sessionId, ct);

            TempData[result.success ? "SuccessMessage" : "ErrorMessage"]
                = result.success
                    ? "Booking Cancelled Successfully"
                    : result.error;

            return RedirectToAction(
                nameof(GetMembersForUpcommingSessions),
                new { id = sessionId }
            );
        }
    }
}
