using PRM393_Travel_Planner_BE.Models;

namespace PRM393_Travel_Planner_BE.Repositories.Interfaces
{
    public interface ITripActivityRepository
    {
        Task<IEnumerable<TripActivity>> GetByDayIdAsync(Guid tripDayId);
        Task<TripActivity?> GetByIdAsync(Guid id);
        Task AddAsync(TripActivity activity);
        Task UpdateAsync(TripActivity activity);
        Task DeleteAsync(TripActivity activity);
        Task UpdateRangeAsync(IEnumerable<TripActivity> activities);
        Task<bool> BelongsToDayAsync(Guid activityId, Guid tripDayId);
    }
}
