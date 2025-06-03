// In TruckFreightSystem.Application.Services/RatingService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
using TruckFreightSystem.Application.Common.Exceptions;
using TruckFreightSystem.Application.DTOs.Rating;
using TruckFreightSystem.Application.Interfaces.Persistence;
using TruckFreightSystem.Application.Interfaces.Services;
using TruckFreightSystem.Domain.Entities;
using TruckFreightSystem.Domain.Enums;

namespace TruckFreightSystem.Application.Services
{
    public class RatingService : IRatingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RatingService> _logger;

        public RatingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RatingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RatingDto?> CreateRatingAsync(Guid raterUserId, CreateRatingRequest request)
        {
            // Ensure both rater and rated users exist
            var raterUser = await _unitOfWork.Users.GetByIdAsync(raterUserId);
            var ratedUser = await _unitOfWork.Users.GetByIdAsync(request.RatedUserId);
            var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);

            if (raterUser == null || ratedUser == null || trip == null)
            {
                throw new NotFoundException("Rater, Rated User, or Trip not found.");
            }

            // Business logic for rating:
            // 1. Trip must be completed
            if (trip.Status != TripStatus.Completed)
            {
                throw new BusinessLogicException("Cannot rate an incomplete trip.");
            }
            // 2. A user can only rate after a relevant trip
            // 3. Prevent duplicate ratings for the same trip by the same rater
            var existingRating = (await _unitOfWork.Ratings.GetAllAsync()) // Optimize this if many ratings
                                 .FirstOrDefault(r => r.TripId == request.TripId && r.RaterUserId == raterUserId);
            if (existingRating != null)
            {
                throw new BusinessLogicException("You have already rated this trip.");
            }

            // 4. Ensure Rater and Rated are correct roles for the trip
            if (request.Type == RatingType.CargoOwnerToDriver)
            {
                if (raterUser.Role != UserRole.CargoOwner || trip.Cargo.CargoOwnerId != raterUserId)
                    throw new UnauthorizedAccessException("Only the cargo owner of this trip can rate the driver.");
                if (ratedUser.Role != UserRole.Driver || trip.DriverId != request.RatedUserId)
                    throw new BusinessLogicException("The rated user must be the driver of this trip.");
            }
            else if (request.Type == RatingType.DriverToCargoOwner)
            {
                if (raterUser.Role != UserRole.Driver || trip.DriverId != raterUserId)
                    throw new UnauthorizedAccessException("Only the driver of this trip can rate the cargo owner.");
                if (ratedUser.Role != UserRole.CargoOwner || trip.Cargo.CargoOwnerId != request.RatedUserId)
                    throw new BusinessLogicException("The rated user must be the cargo owner of this trip.");
            }
            else
            {
                throw new ValidationException("Invalid rating type.");
            }

            var rating = _mapper.Map<Rating>(request);
            rating.RaterUserId = raterUserId; // Ensure raterUserId is set from authenticated user

            await _unitOfWork.Ratings.AddAsync(rating);
            await _unitOfWork.CompleteAsync();

            // Recalculate and update average rating for the rated user
            var newAverageRating = await _unitOfWork.Ratings.CalculateAverageRatingForUserAsync(request.RatedUserId);
            if (ratedUser.Role == UserRole.Driver)
            {
                var driverProfile = await _unitOfWork.Drivers.GetDriverByUserIdAsync(ratedUser.Id);
                if (driverProfile != null)
                {
                    driverProfile.AverageRating = newAverageRating;
                    driverProfile.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Drivers.UpdateAsync(driverProfile);
                }
            }
            else if (ratedUser.Role == UserRole.CargoOwner)
            {
                var cargoOwnerProfile = await _unitOfWork.CargoOwners.GetCargoOwnerByUserIdAsync(ratedUser.Id);
                if (cargoOwnerProfile != null)
                {
                    cargoOwnerProfile.AverageRating = newAverageRating;
                    cargoOwnerProfile.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.CargoOwners.UpdateAsync(cargoOwnerProfile);
                }
            }
            await _unitOfWork.CompleteAsync(); // Save average rating update

            _logger.LogInformation("Rating created by {RaterUserId} for {RatedUserId} on trip {TripId}.", raterUserId, request.RatedUserId, request.TripId);

            // Eager load for DTO mapping
            var createdRating = await _unitOfWork.Ratings.GetByIdAsync(rating.Id);
            await _unitOfWork.Ratings.LoadReferenceAsync(createdRating!, r => r.RaterUser);
            await _unitOfWork.Ratings.LoadReferenceAsync(createdRating!, r => r.RatedUser);
            return _mapper.Map<RatingDto>(createdRating);
        }

        public async Task<IEnumerable<RatingDto>> GetRatingsForUserAsync(Guid userId)
        {
            var ratings = await _unitOfWork.Ratings.GetRatingsForUserAsync(userId);
            var ratingDtos = new List<RatingDto>();
            foreach (var rating in ratings)
            {
                await _unitOfWork.Ratings.LoadReferenceAsync(rating, r => r.RaterUser);
                await _unitOfWork.Ratings.LoadReferenceAsync(rating, r => r.RatedUser);
                ratingDtos.Add(_mapper.Map<RatingDto>(rating));
            }
            return ratingDtos;
        }

        public async Task<double> GetAverageRatingForUserAsync(Guid userId)
        {
            // This method simply retrieves the already calculated average rating from the profile.
            // If the average rating is not stored in the profile, it would recalculate here.
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            if (user.Role == UserRole.Driver)
            {
                var driverProfile = await _unitOfWork.Drivers.GetDriverByUserIdAsync(userId);
                return driverProfile?.AverageRating ?? 0.0;
            }
            else if (user.Role == UserRole.CargoOwner)
            {
                var cargoOwnerProfile = await _unitOfWork.CargoOwners.GetCargoOwnerByUserIdAsync(userId);
                return cargoOwnerProfile?.AverageRating ?? 0.0;
            }
            return 0.0;
        }
    }
}