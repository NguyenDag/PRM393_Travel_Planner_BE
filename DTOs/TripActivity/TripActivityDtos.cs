using System.ComponentModel.DataAnnotations;

namespace PRM393_Travel_Planner_BE.DTOs.TripActivity
{
    public record CreateTripActivityRequest(
    [Required] TimeOnly Time,
    [Required, StringLength(200)] string Title,
    [Required, StringLength(200)] string LocationName,
    decimal? Latitude,
    decimal? Longitude,
    string? Note,
    int SortOrder = 0
);

    public record UpdateTripActivityRequest(
        TimeOnly? Time,
        [StringLength(200)] string? Title,
        [StringLength(200)] string? LocationName,
        decimal? Latitude,
        decimal? Longitude,
        string? Note,
        int? SortOrder
    );

    public record ReorderActivitiesRequest(
        [Required] List<ActivityOrderItem> Items
    );

    public record ActivityOrderItem(
        [Required] Guid Id,
        [Required] int SortOrder
    );

}
