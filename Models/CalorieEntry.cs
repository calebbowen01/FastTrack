namespace MyMauiApp.Models;

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    Snack
}

public enum EntryMethod
{
    QuickAdd,
    ManualMacros,
    MealPreset,
    BarcodeScanned
}

public class CalorieEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public MealType MealType { get; set; }
    public EntryMethod Method { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
