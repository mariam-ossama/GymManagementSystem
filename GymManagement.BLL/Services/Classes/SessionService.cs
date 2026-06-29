using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.SessionViewModel;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Data.Models.Enums;
using GymManagement.DAL.Repositories.Interfaces;

namespace GymManagement.BLL.Services.Classes
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SessionService(IUnitOfWork unitOfWork,
                              IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result> CreateSessionAsync(CreateSessionViewModel model, CancellationToken ct = default)
        {
            if(model.EndDate <= model.StartDate) return Result.Validation("End Date Must be After Start Date");
            if(model.StartDate <= DateTime.Now) return Result.Validation("Start Date Must be In Future");
            if(model.Capacity <= 1 || model.Capacity > 25) return Result.Validation("Capacity Must be Between 1 and 25");

            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(model.TrainerId, ct);
            if (trainer == null) return Result.NotFound("Trainer Not Found");

            var category =await _unitOfWork.GetRepository<Category>().GetByIdAsync(model.CategoryId);
            if (category is null) return Result.NotFound("Category Not Found");

            var isValid = Enum.TryParse<Speciality>(category.CategoryName, true,out var categorySpeciality);
            if (!isValid || trainer.Speciality != categorySpeciality) return Result.Validation("Cannot Create This Session To This Trainer");

            var session = _mapper.Map<CreateSessionViewModel, Session>(model);

            _unitOfWork.GetRepository<Session>().Add(session);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.Ok() : Result.Fail("Failed To Create Session");
        }

        public async Task<IEnumerable<SessionViewModel>?> GetAllSessionsAsync(CancellationToken ct)
        {
            var sessionRepository = _unitOfWork.SessionRepository;
            var sessions = await sessionRepository.GetAllSessionsWithTrainerAndCategory(ct:ct);
            if (sessions == null || !sessions.Any()) return null;

            var mappedSessions = sessions.Select(s => new SessionViewModel()
            {
                Id = s.Id,
                Capacity = s.Capacity,
                CategoryName = s.Category.CategoryName ?? "No Category",
                TrainerName = s.Trainer.Name ?? "No Trainer",
                Description = s.Description,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            });

            foreach(var session in mappedSessions)
            {
                session.AvailableSlots = session.Capacity - await sessionRepository.GetCountOfBookedSlotsAsync(session.Id, ct);
                // N + 1 problem 
            }
            return mappedSessions;
        }

        public async Task<IEnumerable<CategorySelectViewModel>> GetCategoriesForDropDownAsync(CancellationToken ct = default)
        {
            var result = await _unitOfWork.GetRepository<Category>().GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<CategorySelectViewModel>>(result);
        }

        public async Task<Result<SessionViewModel>> GetSessionByIdAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetSessionByIdWithTrainerAndCategory(sessionId, ct);
            if (session is null) 
                return Result<SessionViewModel>.NotFound("Session Not Found");
            else
            {
                var mappedSession = _mapper.Map<Session, SessionViewModel>(session);
                mappedSession.AvailableSlots = mappedSession.Capacity - await _unitOfWork.SessionRepository.GetCountOfBookedSlotsAsync(sessionId, ct);
                return Result<SessionViewModel>.Ok(mappedSession);
            }
        }
        public async Task<IEnumerable<TrainerSelectViewModel>> GetTrainersForDropDownAsync(CancellationToken ct = default)
        {
            var result = await _unitOfWork.GetRepository<Trainer>().GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<TrainerSelectViewModel>>(result);
        }

        public async Task<Result<UpdateSessionViewModel>> GetSessionToUpdateAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(sessionId, ct);
            if (session == null)
                return Result<UpdateSessionViewModel>.NotFound("Session Not Found");
            if (session.StartDate <= DateTime.Now)
                return Result<UpdateSessionViewModel>.Fail("Cannot Update Session That Has Already Started");
            var bookingCount = await _unitOfWork.SessionRepository.GetCountOfBookedSlotsAsync(sessionId, ct);
            if (bookingCount > 0)
                return Result<UpdateSessionViewModel>.Fail("Cannot Update Session That Has Bookings");
            var mappedSession = _mapper.Map<Session, UpdateSessionViewModel>(session);
            return Result<UpdateSessionViewModel>.Ok(mappedSession);
        }


        public async Task<Result> UpdateSessionAsync(int id, UpdateSessionViewModel model, CancellationToken ct = default)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(id, ct);
            if (session == null)
                return Result.NotFound("Session Not Found");
            if (session.StartDate <= DateTime.Now)
                return Result.Fail("Cannot Edit Session That Has Already Started");
            if (model.EndDate <= model.StartDate)
                return Result.Validation("End Date Must be After Start Date");
            var bookedCount = await _unitOfWork.SessionRepository.GetCountOfBookedSlotsAsync(id, ct);
            if (bookedCount > 0)
                return Result.Fail("Cannot Update Session That Has Bookings");
            if (model.StartDate <= DateTime.Now)
                return Result.Validation("Start Date Must be In Future");

            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(model.TrainerId, ct);
            if (trainer == null) return Result.NotFound("Trainer Not Found");

            var category = await _unitOfWork.GetRepository<Category>().GetByIdAsync(session.CategoryId);

            var isValid = Enum.TryParse<Speciality>(category?.CategoryName, true, out var categorySpeciality);
            if (!isValid || trainer.Speciality != categorySpeciality) return Result.Validation("Cannot Create This Session To This Trainer");

            _mapper.Map(model, session);
            session.UpdatedAt = DateTime.Now;
            _unitOfWork.SessionRepository.Update(session);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Update Session");
        }

        public async Task<Result> RemoveSessionAsync(int sessionId, CancellationToken ct)
        {
            var session = await _unitOfWork.SessionRepository.GetByIdAsync(sessionId, ct);
            if (session == null)
                return Result.NotFound("Session Not Found");
            if (session.EndDate >= DateTime.Now)
                return Result.Fail("Cannot Delete a Session That Has Not Ended Yet");

            var bookedCount = await _unitOfWork.SessionRepository.GetCountOfBookedSlotsAsync(sessionId, ct);
            if (bookedCount > 0)
                return Result.Fail("Cannot Delete Session That Has Bookings");

            _unitOfWork.SessionRepository.Delete(session);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            return result > 0 ? Result.Ok() : Result.Fail("Failed To Delete Session");
        }
    }
}
