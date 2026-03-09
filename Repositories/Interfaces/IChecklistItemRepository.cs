using PRM393_Travel_Planner_BE.Models;

namespace PRM393_Travel_Planner_BE.Repositories.Interfaces
{
    public interface IChecklistItemRepository
    {
        Task<ChecklistItem?> GetByIdAsync(Guid id);
        Task AddAsync(ChecklistItem item);
        Task UpdateAsync(ChecklistItem item);
        Task UpdateRangeAsync(IEnumerable<ChecklistItem> items);
        Task DeleteAsync(ChecklistItem item);
        Task<bool> BelongsToChecklistAsync(Guid itemId, Guid checklistId);
    }
}
