using System;
using System.Collections.Generic;

namespace PRM393_Travel_Planner_BE.Models;

public partial class TripActivity
{
    public Guid Id { get; set; }

    public Guid TripDayId { get; set; }

    public TimeOnly? Time { get; set; }

    public string? Title { get; set; }

    public string? LocationName { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? Note { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TripDay TripDay { get; set; } = null!;
}
