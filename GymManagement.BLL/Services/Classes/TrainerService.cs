using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GymManagement.BLL.Common;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.BLL.ViewModels.MemberViewModels;
using GymManagement.BLL.ViewModels.TrainerViewModels;
using GymManagement.DAL.Data.Models;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymManagement.BLL.Services.Classes
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrainerService(IUnitOfWork unitOfWork,
                              IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default)
        {
            // check email
            var emailExist = await _unitOfWork.GetRepository<Trainer>().AnyAsync(t => t.Email == model.Email, ct);
            //if (emailExist) return Result.Validation("Trainer Email Already Exists");
            // check phone number
            var phoneExist = await _unitOfWork.GetRepository<Trainer>().AnyAsync(t => t.Phone == model.Phone, ct);
            //if (phoneExist) return Result.Validation("Trainer Phone Number Already Exists");
            // if phone or email exists return false
            if (emailExist || phoneExist) return Result.Validation("Trainer Email or Phone Number Already Exists");
            // Map trainer to the create trainer view model and return true
            var trainer = _mapper.Map<CreateTrainerViewModel,Trainer>(model);
            _unitOfWork.GetRepository<Trainer>().Add(trainer);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Create a Trainer");
        }

        public async Task<IEnumerable<TrainerViewModel?>> GetAllTrainersAsync(CancellationToken ct = default)
        {
            var trainers = await _unitOfWork.GetRepository<Trainer>().GetAllAsync();
            if (!trainers.Any()) return [];
            var trainersViewModel = _mapper.Map<IEnumerable<TrainerViewModel>>(trainers);
            return trainersViewModel;
        }

        public async Task<TrainerToUpdateViewModel?> GetTrainerToUpdate(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(trainerId, ct);
            if (trainer == null) return null;
            else
                return _mapper.Map<TrainerToUpdateViewModel>(trainer);
        }

        public async Task<TrainerViewModel?> GetTrainerDetailsAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(trainerId, ct);
            if (trainer == null) return null;
            var model = _mapper.Map<TrainerViewModel>(trainer);
            return model;
        }

        public async Task<Result> UpdateTrainerAsync(int id, TrainerToUpdateViewModel model, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(id, ct);
            if (trainer == null) return Result.NotFound("Trainer Not Found");
            var emailExist = await _unitOfWork.GetRepository<Trainer>().AnyAsync(t => t.Email == model.Email && t.Id != id, ct);
            // check phone number
            var phoneExist = await _unitOfWork.GetRepository<Trainer>().AnyAsync(t => t.Phone == model.Phone && t.Id != id, ct);
            // if phone or email exists return false
            if (emailExist || phoneExist) return Result.Validation("Trainer Email or Phone Number Already Exists");
            _mapper.Map(model, trainer);
            trainer.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Trainer>().Update(trainer);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Update a Trainer");
        }

        public async Task<Result> RemoveTrainerAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(trainerId);
            if (trainer == null) return Result.NotFound("Trainer Is Not Found");

            //Cannot delete a trainer with scheduled sessions 
            // check scheduled sessions
            var hasSessions = await _unitOfWork.GetRepository<Session>().AnyAsync(s => s.TrainerId == trainer.Id, ct);
            if (hasSessions) return Result.Validation("Cannot Remove a Trainer with Scheduled Sessions");

            _unitOfWork.GetRepository<Trainer>().Delete(trainer);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0 ? Result.Ok() : Result.Fail("Failed To Remove a Trainer");
        }
    }
}