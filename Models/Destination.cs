using System;
using System.Collections.Generic;

namespace PRM393_Travel_Planner_BE.Models;

public partial class Destination
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Location { get; set; }

    public string? Country { get; set; }

    public string? Description { get; set; }

    public string? ShortDesc { get; set; }

    public string? ImageUrl { get; set; }

    public string? Tag { get; set; }

    public int? DurationDays { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool? IsTrending { get; set; }

    public int? ViewCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AiSuggestion> AiSuggestions { get; set; } = new List<AiSuggestion>();
}
