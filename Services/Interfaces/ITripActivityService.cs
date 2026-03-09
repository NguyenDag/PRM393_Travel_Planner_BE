using PRM393_Travel_Planner_BE.DTOs.Trip;
using PRM393_Travel_Planner_BE.DTOs.TripActivity;

namespace PRM393_Travel_Planner_BE.Services.Interfaces
{
    public interface ITripActivityService
    {
        Task<TripActivityDto> GetActivityAsync(Guid tripId, Guid dayId, Guid activityId, Guid userId);
        Task<TripActivityDto> CreateActivityAsync(Guid tripId, Guid dayId, Guid userId, CreateTripActivityRequest request);
        Task<TripActivityDto> UpdateActivityAsync(Guid tripId, Guid dayId, Guid activityId, Guid userId, UpdateTripActivityRequest request);
        Task DeleteActivityAsync(Guid tripId, Guid dayId, Guid activityId, Guid userId);
        Task ReorderActivitiesAsync(Guid tripId, Guid dayId, Guid userId, ReorderActivitiesRequest request);
    }
}
