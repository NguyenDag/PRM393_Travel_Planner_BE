using PRM393_Travel_Planner_BE.DTOs.Trip;

namespace PRM393_Travel_Planner_BE.Services.Interfaces
{
    public interface ITripService
    {
        Task<IEnumerable<TripDto>> GetMyTripsAsync(Guid userId);
        Task<TripDetailDto> GetTripDetailAsync(Guid tripId, Guid userId);
        Task<TripDto> CreateTripAsync(Guid userId, CreateTripRequest request);
        Task<TripDto> UpdateTripAsync(Guid tripId, Guid userId, UpdateTripRequest request);
        Task DeleteTripAsync(Guid tripId, Guid userId);
    }
}
