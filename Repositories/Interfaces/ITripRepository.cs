using PRM393_Travel_Planner_BE.Models;

namespace PRM393_Travel_Planner_BE.Repositories.Interfaces
{
    public interface ITripRepository
    {
        Task<IEnumerable<Trip>> GetByUserIdAsync(Guid userId);
        Task<Trip?> GetByIdAsync(Guid id);
        Task<Trip?> GetDetailAsync(Guid id);   // include TripDays + Activities
        Task AddAsync(Trip trip);
        Task UpdateAsync(Trip trip);
        Task DeleteAsync(Trip trip);
        Task<bool> BelongsToUserAsync(Guid tripId, Guid userId);
    }
}
