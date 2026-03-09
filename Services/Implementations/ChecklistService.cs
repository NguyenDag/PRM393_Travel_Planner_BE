using PRM393_Travel_Planner_BE.DTOs.Checklist;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Services.Implementations
{
    public class ChecklistService(
    IChecklistRepository checklistRepo,
    IChecklistItemRepository itemRepo,
    ITripRepository tripRepo) : IChecklistService
    {
        // ── Checklist CRUD ────────────────────────────────────────────────────────

        public async Task<IEnumerable<ChecklistDto>> GetChecklistsAsync(Guid tripId, Guid userId)
        {
            await EnsureTripOwnerAsync(tripId, userId);
            var lists = await checklistRepo.GetByTripIdAsync(tripId);
            return lists.Select(MapToDto);
        }

        public async Task<ChecklistDto> GetChecklistAsync(Guid tripId, Guid checklistId, Guid userId)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            var checklist = await checklistRepo.GetWithItemsAsync(checklistId)
                ?? throw new KeyNotFoundException("Không tìm thấy checklist.");

            if (checklist.TripId != tripId) throw new UnauthorizedAccessException();

            return MapToDto(checklist);
        }

        public async Task<ChecklistDto> CreateChecklistAsync(Guid tripId, Guid userId, CreateChecklistRequest req)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            var checklist = new Checklist
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                Title = req.Title,
                SortOrder = req.SortOrder,
            };

            await checklistRepo.AddAsync(checklist);
            return MapToDto(checklist);
        }

        public async Task<ChecklistDto> UpdateChecklistAsync(Guid tripId, Guid checklistId, Guid userId, UpdateChecklistRequest req)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            var checklist = await checklistRepo.GetWithItemsAsync(checklistId)
                ?? throw new KeyNotFoundException("Không tìm thấy checklist.");

            if (checklist.TripId != tripId) throw new UnauthorizedAccessException();

            if (req.Title is not null) checklist.Title = req.Title;
            if (req.SortOrder.HasValue) checklist.SortOrder = req.SortOrder.Value;

            await checklistRepo.UpdateAsync(checklist);
            return MapToDto(checklist);
        }

        public async Task DeleteChecklistAsync(Guid tripId, Guid checklistId, Guid userId)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            var checklist = await checklistRepo.GetByIdAsync(checklistId)
                ?? throw new KeyNotFoundException("Không tìm thấy checklist.");

            if (checklist.TripId != tripId) throw new UnauthorizedAccessException();

            await checklistRepo.DeleteAsync(checklist);
        }

        // ── Item CRUD ─────────────────────────────────────────────────────────────

        public async Task<ChecklistItemDto> AddItemAsync(Guid tripId, Guid checklistId, Guid userId, CreateChecklistItemRequest req)
        {
            await EnsureChecklistAccessAsync(tripId, checklistId, userId);

            var item = new ChecklistItem
            {
                Id = Guid.NewGuid(),
                ChecklistId = checklistId,
                Label = req.Label,
                IsChecked = false,
                Category = req.Category,
                SortOrder = req.SortOrder,
                CreatedAt = DateTime.UtcNow,
            };

            await itemRepo.AddAsync(item);
            return MapItemDto(item);
        }

        public async Task<ChecklistItemDto> UpdateItemAsync(Guid tripId, Guid checklistId, Guid itemId, Guid userId, UpdateChecklistItemRequest req)
        {
            await EnsureChecklistAccessAsync(tripId, checklistId, userId);

            var item = await itemRepo.GetByIdAsync(itemId)
                ?? throw new KeyNotFoundException("Không tìm thấy item.");

            if (item.ChecklistId != checklistId) throw new UnauthorizedAccessException();

            if (req.Label is not null) item.Label = req.Label;
            if (req.IsChecked.HasValue) item.IsChecked = req.IsChecked.Value;
            if (req.Category is not null) item.Category = req.Category;
            if (req.SortOrder.HasValue) item.SortOrder = req.SortOrder.Value;

            await itemRepo.UpdateAsync(item);
            return MapItemDto(item);
        }

        public async Task DeleteItemAsync(Guid tripId, Guid checklistId, Guid itemId, Guid userId)
        {
            await EnsureChecklistAccessAsync(tripId, checklistId, userId);

            var item = await itemRepo.GetByIdAsync(itemId)
                ?? throw new KeyNotFoundException("Không tìm thấy item.");

            if (item.ChecklistId != checklistId) throw new UnauthorizedAccessException();

            await itemRepo.DeleteAsync(item);
        }

        public async Task BulkToggleAsync(Guid tripId, Guid checklistId, Guid userId, BulkToggleRequest req)
        {
            await EnsureChecklistAccessAsync(tripId, checklistId, userId);

            var checklist = await checklistRepo.GetWithItemsAsync(checklistId)
                ?? throw new KeyNotFoundException("Không tìm thấy checklist.");

            var targets = checklist.ChecklistItems
                .Where(i => req.ItemIds.Contains(i.Id))
                .ToList();

            foreach (var item in targets)
                item.IsChecked = req.IsChecked;

            await itemRepo.UpdateRangeAsync(targets);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private async Task EnsureTripOwnerAsync(Guid tripId, Guid userId)
        {
            if (!await tripRepo.BelongsToUserAsync(tripId, userId))
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập chuyến đi này.");
        }

        private async Task EnsureChecklistAccessAsync(Guid tripId, Guid checklistId, Guid userId)
        {
            await EnsureTripOwnerAsync(tripId, userId);

            if (!await checklistRepo.BelongsToTripAsync(checklistId, tripId))
                throw new KeyNotFoundException("Checklist không thuộc chuyến đi này.");
        }

        private static ChecklistDto MapToDto(Checklist c) => new(
            c.Id, c.Title, c.SortOrder, c.ChecklistItems.Select(MapItemDto));

        private static ChecklistItemDto MapItemDto(ChecklistItem i) => new(
            i.Id, i.Label, i.IsChecked, i.Category, i.SortOrder, i.CreatedAt);
    }
}
