using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using PRM393_Travel_Planner_BE.Commons.Enums;

namespace PRM393_Travel_Planner_BE.DTOs.Trip
{
    // ── Requests ──────────────────────────────────────────────────────────────────

    public record CreateTripRequest(
        [Required, StringLength(200)] string Title,
        [Required, StringLength(200)] string Location,
        [Required, StringLength(200)] string Country,
        [Required] DateOnly DateFrom,
        [Required] DateOnly DateTo,
        string? CoverImageUrl,
        Guid? SourceAiSuggestionId
    );

    public record UpdateTripRequest(
        [StringLength(200)] string? Title,
        [StringLength(200)] string? Location,
        [StringLength(200)] string? Country,
        DateOnly? DateFrom,
        DateOnly? DateTo,
        TripStatus? Status,
        string? CoverImageUrl
    );

    // ── Responses ─────────────────────────────────────────────────────────────────

    public record TripDto(
        Guid Id,
        string Title,
        string Location,
        string Country,
        DateOnly? DateFrom,
        DateOnly? DateTo,
        string Status,
        string? CoverImageUrl,
        int? TotalDays,
        DateTime? CreatedAt,
        DateTime? UpdatedAt
    );

    public record TripDetailDto(
        Guid Id,
        string Title,
        string Location,
        string Country,
        DateOnly? DateFrom,
        DateOnly? DateTo,
        string? Status,
        string? CoverImageUrl,
        int? TotalDays,
        DateTime? CreatedAt,
        DateTime? UpdatedAt,
        IEnumerable<TripDayDto> Days
    );

    public record TripDayDto(
        Guid Id,
        int DayNumber,
        DateOnly? Date,
        string? Title,
        IEnumerable<TripActivityDto> Activities
    );

    public record TripActivityDto(
        Guid Id,
        string Time,
        string Title,
        string LocationName,
        decimal? Latitude,
        decimal? Longitude,
        string? Note,
        int? SortOrder,
        DateTime? CreatedAt
    );
}
