using System.ComponentModel.DataAnnotations;

namespace PRM393_Travel_Planner_BE.DTOs.TripDay
{
    public record CreateTripDayRequest(
    [Required] DateOnly Date,
    [Required] int DayNumber,
    [StringLength(200)] string? Title
    );

    public record UpdateTripDayRequest(
        [StringLength(200)] string? Title,
        DateOnly? Date
    );
}
