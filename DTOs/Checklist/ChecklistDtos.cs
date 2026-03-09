using System.ComponentModel.DataAnnotations;

namespace PRM393_Travel_Planner_BE.DTOs.Checklist
{
    // ── Checklist ─────────────────────────────────────────────────────────────────

    public record CreateChecklistRequest(
        [Required, StringLength(100)] string Title,
        int SortOrder = 0
    );

    public record UpdateChecklistRequest(
        [StringLength(100)] string? Title,
        int? SortOrder
    );

    public record ChecklistDto(
        Guid Id,
        string Title,
        int? SortOrder,
        IEnumerable<ChecklistItemDto> Items
    );

    // ── ChecklistItem ─────────────────────────────────────────────────────────────

    public record CreateChecklistItemRequest(
        [Required, StringLength(200)] string Label,
        [StringLength(50)] string? Category,
        int SortOrder = 0
    );

    public record UpdateChecklistItemRequest(
        [StringLength(200)] string? Label,
        bool? IsChecked,
        [StringLength(50)] string? Category,
        int? SortOrder
    );

    public record ToggleItemRequest(
        [Required] bool IsChecked
    );

    public record ChecklistItemDto(
        Guid Id,
        string Label,
        bool? IsChecked,
        string? Category,
        int? SortOrder,
        DateTime? CreatedAt
    );

    public record BulkToggleRequest(
        [Required] List<Guid> ItemIds,
        [Required] bool IsChecked
    );
}
