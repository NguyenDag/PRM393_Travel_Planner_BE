using PRM393_Travel_Planner_BE.Models;

namespace PRM393_Travel_Planner_BE.Repositories.Interfaces
{
    public interface ITripDayRepository
    {
        Task<IEnumerable<TripDay>> GetByTripIdAsync(Guid tripId);
        Task<TripDay?> GetByIdAsync(Guid id);
        Task<TripDay?> GetWithActivitiesAsync(Guid id);
        Task AddAsync(TripDay day);
        Task UpdateAsync(TripDay day);
        Task DeleteAsync(TripDay day);
        Task<bool> BelongsToTripAsync(Guid dayId, Guid tripId);
    }
}
