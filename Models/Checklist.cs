using System;
using System.Collections.Generic;

namespace PRM393_Travel_Planner_BE.Models;

public partial class Checklist
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public string? Title { get; set; }

    public int? SortOrder { get; set; }

    public virtual ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

    public virtual Trip Trip { get; set; } = null!;
}
