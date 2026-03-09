using PRM393_Travel_Planner_BE.Models;

namespace PRM393_Travel_Planner_BE.Repositories.Interfaces
{
    public interface IChecklistRepository
    {
        Task<IEnumerable<Checklist>> GetByTripIdAsync(Guid tripId);
        Task<Checklist?> GetByIdAsync(Guid id);
        Task<Checklist?> GetWithItemsAsync(Guid id);
        Task AddAsync(Checklist checklist);
        Task UpdateAsync(Checklist checklist);
        Task DeleteAsync(Checklist checklist);
        Task<bool> BelongsToTripAsync(Guid checklistId, Guid tripId);
    }
}
