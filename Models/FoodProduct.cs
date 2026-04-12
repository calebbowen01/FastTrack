namespace MyMauiApp.Models;

public class FoodProduct
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int CaloriesPer100g { get; set; }
    public double ProteinPer100g { get; set; }
    public double CarbsPer100g { get; set; }
    public double FatPer100g { get; set; }
    public int CaloriesPerServing { get; set; }
    public double ProteinPerServing { get; set; }
    public double CarbsPerServing { get; set; }
    public double FatPerServing { get; set; }
    public string ServingSize { get; set; } = string.Empty;
    public bool HasServingData => CaloriesPerServing > 0;
    public string DisplayName => string.IsNullOrWhiteSpace(Brand) ? Name : $"{Brand} — {Name}";
}
