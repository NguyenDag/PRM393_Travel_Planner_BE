using System;
using System.Collections.Generic;

namespace PRM393_Travel_Planner_BE.Models;

public partial class TripDay
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public int DayNumber { get; set; }

    public DateOnly? Date { get; set; }

    public string? Title { get; set; }

    public virtual Trip Trip { get; set; } = null!;

    public virtual ICollection<TripActivity> TripActivities { get; set; } = new List<TripActivity>();
}
