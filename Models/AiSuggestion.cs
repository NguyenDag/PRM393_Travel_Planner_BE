using System;
using System.Collections.Generic;

namespace PRM393_Travel_Planner_BE.Models;

public partial class AiSuggestion
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid DestinationId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? ItineraryJson { get; set; }

    public string? HighlightsJson { get; set; }

    public string? Label { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Destination Destination { get; set; } = null!;

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    public virtual User User { get; set; } = null!;
}
