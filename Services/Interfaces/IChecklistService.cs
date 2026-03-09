using PRM393_Travel_Planner_BE.DTOs.Checklist;

namespace PRM393_Travel_Planner_BE.Services.Interfaces
{
    public interface IChecklistService
    {
        Task<IEnumerable<ChecklistDto>> GetChecklistsAsync(Guid tripId, Guid userId);
        Task<ChecklistDto> GetChecklistAsync(Guid tripId, Guid checklistId, Guid userId);
        Task<ChecklistDto> CreateChecklistAsync(Guid tripId, Guid userId, CreateChecklistRequest request);
        Task<ChecklistDto> UpdateChecklistAsync(Guid tripId, Guid checklistId, Guid userId, UpdateChecklistRequest request);
        Task DeleteChecklistAsync(Guid tripId, Guid checklistId, Guid userId);

        Task<ChecklistItemDto> AddItemAsync(Guid tripId, Guid checklistId, Guid userId, CreateChecklistItemRequest request);
        Task<ChecklistItemDto> UpdateItemAsync(Guid tripId, Guid checklistId, Guid itemId, Guid userId, UpdateChecklistItemRequest request);
        Task DeleteItemAsync(Guid tripId, Guid checklistId, Guid itemId, Guid userId);
        Task BulkToggleAsync(Guid tripId, Guid checklistId, Guid userId, BulkToggleRequest request);
    }
}
