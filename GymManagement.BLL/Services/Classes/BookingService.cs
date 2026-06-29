using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.BookingViewModels;
using GymManagement.BLL.ViewModels.MembershipViewModels;
using GymManagement.BLL.ViewModels.SessionViewModel;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Classes
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingService(IUnitOfWork unitOfWork,
                              IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result> CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(sessionId,ct);
            Console.WriteLine(session);
            if (session == null)
                return Result.NotFound("Session Not Found");
            if (session.StartDate <= DateTime.Now)
                return Result.Validation("Cannot Cancel a booking to a Session that Already Started");
            var booking = await _unitOfWork.BookingRepository
                .FirstOrDefaultAsync(b => b.SessionId == sessionId && b.MemberId == memberId, true, ct:ct);
            if (booking == null)
                return Result.NotFound("This Booking is Not Found");

            _unitOfWork.BookingRepository.Delete(booking);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.Ok() : Result.Fail("Failed To Cancel Booking");
        }

        public async Task<Result> CreateNewBookingAsync(CreateBookingViewModel model, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(model.SessionId, ct);
            if (session == null)
                return Result.NotFound("Session Not Found");
            if (session.StartDate <= DateTime.Now)
                return Result.Validation("Cannot Book a Session That has Already Started");
            var hasActiveMembership = await _unitOfWork.MembershipRepository
                .AnyAsync(m => m.MemberId == model.MemberId && m.EndDate > DateTime.Now, ct);
            if (!hasActiveMembership)
                return Result.Validation("Member Does Not Have an Active Membership");
            var alreadyBooked = await _unitOfWork.BookingRepository
                .AnyAsync(b => b.SessionId == model.SessionId && b.MemberId == model.MemberId, ct);
            if (alreadyBooked)
                return Result.Validation("Member is Already Booked For This Session");
            var booked = await _unitOfWork.SessionRepository.GetCountOfBookedSlotsAsync(model.SessionId, ct);
            if (booked >= session.Capacity)
                return Result.Validation("Session is Full");

            _unitOfWork.BookingRepository.Add(new Booking()
            {
                MemberId = model.MemberId,
                SessionId = model.SessionId,
                IsAttended = false,
                CreatedAt = DateTime.Now,
            });
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Book a Session");
        }

        public async Task<IEnumerable<SessionViewModel>> GetAllSessionsAsync(CancellationToken ct = default)
        {
            var bookings = await _unitOfWork.SessionRepository.GetAllSessionsWithTrainerAndCategory(x => x.EndDate >= DateTime.Now,ct);
            if (!bookings.Any())
                return null!;
            var mappedSession = _mapper.Map<IEnumerable<SessionViewModel>>(bookings);
            foreach(var session in mappedSession)
            {
                session.AvailableSlots = session.Capacity - await _unitOfWork.SessionRepository.GetCountOfBookedSlotsAsync(session.Id, ct);
            }
            return mappedSession;
        }

        public async Task<IEnumerable<MemberSelectListViewModel>> GetMemberForDropDownAsync(int sessionId, CancellationToken ct = default)
        {
            var bookings = await _unitOfWork.BookingRepository.GetAllAsync(x=>x.SessionId == sessionId, ct:ct);
            var bookedMembersIds = bookings.Select(x => x.MemberId);
            var availableMembers = await _unitOfWork.GetRepository<Member>().GetAllAsync(x => !bookedMembersIds.Contains(x.Id));
            return _mapper.Map<IEnumerable<MemberSelectListViewModel>>(availableMembers);
        }

        public async Task<IEnumerable<MemberForSessionViewModel>> GetMembersForOngoingSessionAsync(int sessionId, CancellationToken ct)
        {
            var bookings = await _unitOfWork.BookingRepository.GetBySessionIdAsync(sessionId, ct);
            return bookings.Select(b => new MemberForSessionViewModel()
            {
                MemberId = b.MemberId,
                SessionId = b.SessionId,
                MemberName = b.Member.Name,
                BookingDate = b.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
        }

        public async Task<IEnumerable<MemberForSessionViewModel>> GetMembersForUpcommingSessionAsync(int sessionId, CancellationToken ct)
        {
            var bookings = await _unitOfWork.BookingRepository.GetBySessionIdAsync(sessionId, ct);
            return bookings.Select(b => new MemberForSessionViewModel()
            {
                MemberId = b.MemberId,
                SessionId = b.SessionId,
                MemberName = b.Member.Name,
                BookingDate = b.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                IsAttended = b.IsAttended
            }).ToList();
        }

        public async Task<Result> MarkAttendedAsync(int memberId, int sessionId, CancellationToken ct = default)
        {
            var booking = await _unitOfWork.BookingRepository
                .FirstOrDefaultAsync(b => b.MemberId == memberId && b.SessionId == sessionId, true,ct);
            if (booking == null)
                return Result.NotFound("This Booking Is Not Found");
            booking.IsAttended = true;
            booking.UpdatedAt = DateTime.Now;
            _unitOfWork.BookingRepository.Update(booking);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Mark As Attended");
        }
    }
}
