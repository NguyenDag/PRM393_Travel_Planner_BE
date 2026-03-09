using PRM393_Travel_Planner_BE.DTOs.Trip;
using PRM393_Travel_Planner_BE.DTOs.TripActivity;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Services.Implementations
{
    public class TripActivityService(
    ITripActivityRepository activityRepo,
    ITripDayRepository dayRepo,
    ITripRepository tripRepo) : ITripActivityService
    {
        public async Task<TripActivityDto> GetActivityAsync(Guid tripId, Guid dayId, Guid activityId, Guid userId)
        {
            await EnsureAccessAsync(tripId, dayId, userId);

            var activity = await activityRepo.GetByIdAsync(activityId)
                ?? throw new KeyNotFoundException("Không tìm thấy hoạt động này.");

            if (activity.TripDayId != dayId) throw new UnauthorizedAccessException();

            return TripService.MapActivityDto(activity);
        }

        public async Task<TripActivityDto> CreateActivityAsync(Guid tripId, Guid dayId, Guid userId, CreateTripActivityRequest req)
        {
            await EnsureAccessAsync(tripId, dayId, userId);

            var activity = new TripActivity
            {
                Id = Guid.NewGuid(),
                TripDayId = dayId,
                Time = req.Time,
                Title = req.Title,
                LocationName = req.LocationName,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Note = req.Note,
                SortOrder = req.SortOrder,
                CreatedAt = DateTime.UtcNow,
            };

            await activityRepo.AddAsync(activity);
            return TripService.MapActivityDto(activity);
        }

        public async Task<TripActivityDto> UpdateActivityAsync(Guid tripId, Guid dayId, Guid activityId, Guid userId, UpdateTripActivityRequest req)
        {
            await EnsureAccessAsync(tripId, dayId, userId);

            var activity = await activityRepo.GetByIdAsync(activityId)
                ?? throw new KeyNotFoundException("Không tìm thấy hoạt động này.");

            if (activity.TripDayId != dayId) throw new UnauthorizedAccessException();

            if (req.Time.HasValue) activity.Time = req.Time.Value;
            if (req.Title is not null) activity.Title = req.Title;
            if (req.LocationName is not null) activity.LocationName = req.LocationName;
            if (req.Latitude.HasValue) activity.Latitude = req.Latitude;
            if (req.Longitude.HasValue) activity.Longitude = req.Longitude;
            if (req.Note is not null) activity.Note = req.Note;
            if (req.SortOrder.HasValue) activity.SortOrder = req.SortOrder.Value;

            await activityRepo.UpdateAsync(activity);
            return TripService.MapActivityDto(activity);
        }

        public async Task DeleteActivityAsync(Guid tripId, Guid dayId, Guid activityId, Guid userId)
        {
            await EnsureAccessAsync(tripId, dayId, userId);

            var activity = await activityRepo.GetByIdAsync(activityId)
                ?? throw new KeyNotFoundException("Không tìm thấy hoạt động này.");

            if (activity.TripDayId != dayId) throw new UnauthorizedAccessException();

            await activityRepo.DeleteAsync(activity);
        }

        public async Task ReorderActivitiesAsync(Guid tripId, Guid dayId, Guid userId, ReorderActivitiesRequest req)
        {
            await EnsureAccessAsync(tripId, dayId, userId);

            var activities = (await activityRepo.GetByDayIdAsync(dayId)).ToList();
            var orderMap = req.Items.ToDictionary(x => x.Id, x => x.SortOrder);

            foreach (var a in activities)
                if (orderMap.TryGetValue(a.Id, out var order))
                    a.SortOrder = order;

            await activityRepo.UpdateRangeAsync(activities);
        }

        private async Task EnsureAccessAsync(Guid tripId, Guid dayId, Guid userId)
        {
            if (!await tripRepo.BelongsToUserAsync(tripId, userId))
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập chuyến đi này.");

            if (!await dayRepo.BelongsToTripAsync(dayId, tripId))
                throw new KeyNotFoundException("Ngày không thuộc chuyến đi này.");
        }
    }
}
