using System;
using System.Collections.Generic;

namespace PRM393_Travel_Planner_BE.Models;

public partial class Trip
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? Title { get; set; }

    public string? Location { get; set; }

    public string? Country { get; set; }

    public DateOnly? DateFrom { get; set; }

    public DateOnly? DateTo { get; set; }

    public string? Status { get; set; }

    public string? CoverImageUrl { get; set; }

    public int? TotalDays { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? SourceAiSuggestionId { get; set; }

    public virtual ICollection<Checklist> Checklists { get; set; } = new List<Checklist>();

    public virtual AiSuggestion? SourceAiSuggestion { get; set; }

    public virtual ICollection<TripDay> TripDays { get; set; } = new List<TripDay>();

    public virtual User User { get; set; } = null!;
}
