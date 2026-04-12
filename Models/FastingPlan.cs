namespace MyMauiApp.Models;

public class FastingPlan
{
    public string Name { get; set; } = string.Empty;
    public int FastingHours { get; set; }
    public int EatingHours { get; set; }
    public string Description { get; set; } = string.Empty;
    public string DisplayLabel => $"{FastingHours}:{EatingHours}";
}
