namespace MyMauiApp.Models;

public class MealPreset
{
    public string Name { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public string Icon { get; set; } = "🍽️";
}
