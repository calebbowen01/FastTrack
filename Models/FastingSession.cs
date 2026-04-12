namespace MyMauiApp.Models;

public class FastingSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TargetHours { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public bool IsCompleted => EndTime.HasValue;
    public TimeSpan Elapsed => (EndTime ?? DateTime.Now) - StartTime;
    public double ProgressPercent => Math.Min(1.0, Elapsed.TotalHours / TargetHours);
}
