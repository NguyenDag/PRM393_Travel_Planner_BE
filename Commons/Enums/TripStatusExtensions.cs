namespace PRM393_Travel_Planner_BE.Commons.Enums
{
    public enum TripStatus
    {
        Preparing,
        InProgress,
        Done
    }

    public static class TripStatusExtensions
    {
        public static string ToDbString(this TripStatus status) => status switch
        {
            TripStatus.Preparing => "preparing",
            TripStatus.InProgress => "in_progress",
            TripStatus.Done => "done",
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };

        public static TripStatus FromDbString(string? value) => value switch
        {
            "preparing" => TripStatus.Preparing,
            "in_progress" => TripStatus.InProgress,
            "done" => TripStatus.Done,
            _ => TripStatus.Preparing  // default fallback
        };
    }
}
