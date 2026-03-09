using PRM393_Travel_Planner_BE.DTOs.Trip;
using PRM393_Travel_Planner_BE.DTOs.TripDay;

namespace PRM393_Travel_Planner_BE.Services.Interfaces
{
    public interface ITripDayService
    {
        Task<TripDayDto> GetTripDayAsync(Guid tripId, Guid dayId, Guid userId);
        Task<TripDayDto> CreateTripDayAsync(Guid tripId, Guid userId, CreateTripDayRequest request);
        Task<TripDayDto> UpdateTripDayAsync(Guid tripId, Guid dayId, Guid userId, UpdateTripDayRequest request);
        Task DeleteTripDayAsync(Guid tripId, Guid dayId, Guid userId);
    }
}
