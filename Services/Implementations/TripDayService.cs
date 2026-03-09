using PRM393_Travel_Planner_BE.DTOs.Trip;
using PRM393_Travel_Planner_BE.DTOs.TripDay;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Services.Implementations
{
    public class TripDayService(
    ITripDayRepository dayRepo,
    ITripRepository tripRepo) : ITripDayService
    {
        public async Task<TripDayDto> GetTripDayAsync(Guid tripId, Guid dayId, Guid userId)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            var day = await dayRepo.GetWithActivitiesAsync(dayId)
                ?? throw new KeyNotFoundException("Không tìm thấy ngày này.");

            if (day.TripId != tripId)
                throw new UnauthorizedAccessException();

            return MapToDto(day);
        }

        public async Task<TripDayDto> CreateTripDayAsync(Guid tripId, Guid userId, CreateTripDayRequest req)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            var day = new TripDay
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                DayNumber = req.DayNumber,
                Date = req.Date,
                Title = req.Title,
            };

            await dayRepo.AddAsync(day);
            return MapToDto(day);
        }

        public async Task<TripDayDto> UpdateTripDayAsync(Guid tripId, Guid dayId, Guid userId, UpdateTripDayRequest req)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            var day = await dayRepo.GetWithActivitiesAsync(dayId)
                ?? throw new KeyNotFoundException("Không tìm thấy ngày này.");

            if (day.TripId != tripId) throw new UnauthorizedAccessException();

            if (req.Title is not null) day.Title = req.Title;
            if (req.Date.HasValue) day.Date = req.Date.Value;

            await dayRepo.UpdateAsync(day);
            return MapToDto(day);
        }

        public async Task DeleteTripDayAsync(Guid tripId, Guid dayId, Guid userId)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            var day = await dayRepo.GetByIdAsync(dayId)
                ?? throw new KeyNotFoundException("Không tìm thấy ngày này.");

            if (day.TripId != tripId) throw new UnauthorizedAccessException();

            await dayRepo.DeleteAsync(day);
        }

        private async Task EnsureTripOwnerAsync(Guid tripId, Guid userId)
        {
            if (!await tripRepo.BelongsToUserAsync(tripId, userId))
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập chuyến đi này.");
        }

        private static TripDayDto MapToDto(TripDay d) => new(
            d.Id, d.DayNumber, d.Date, d.Title,
            d.TripActivities.Select(TripService.MapActivityDto));
    }
}
