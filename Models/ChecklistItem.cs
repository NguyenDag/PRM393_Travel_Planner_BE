using System;
using System.Collections.Generic;

namespace PRM393_Travel_Planner_BE.Models;

public partial class ChecklistItem
{
    public Guid Id { get; set; }

    public Guid ChecklistId { get; set; }

    public string? Label { get; set; }

    public bool? IsChecked { get; set; }

    public string? Category { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Checklist Checklist { get; set; } = null!;
}
