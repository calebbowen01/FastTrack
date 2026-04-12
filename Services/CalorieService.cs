using System.Text.Json;
using MyMauiApp.Models;

namespace MyMauiApp.Services;

public class CalorieService
{
    private const string EntriesKey = "calorie_entries";
    private List<CalorieEntry> _entries = [];

    public static List<MealPreset> DefaultPresets =>
    [
        new() { Name = "Grilled Chicken Breast", Calories = 280, ProteinGrams = 42, CarbsGrams = 0, FatGrams = 6, Icon = "🍗" },
        new() { Name = "Brown Rice (1 cup)", Calories = 215, ProteinGrams = 5, CarbsGrams = 45, FatGrams = 2, Icon = "🍚" },
        new() { Name = "Scrambled Eggs (2)", Calories = 180, ProteinGrams = 12, CarbsGrams = 2, FatGrams = 14, Icon = "🥚" },
        new() { Name = "Protein Shake", Calories = 150, ProteinGrams = 30, CarbsGrams = 5, FatGrams = 2, Icon = "🥤" },
        new() { Name = "Greek Yogurt", Calories = 130, ProteinGrams = 15, CarbsGrams = 10, FatGrams = 4, Icon = "🥛" },
        new() { Name = "Mixed Salad", Calories = 120, ProteinGrams = 4, CarbsGrams = 12, FatGrams = 6, Icon = "🥗" },
        new() { Name = "Banana", Calories = 105, ProteinGrams = 1, CarbsGrams = 27, FatGrams = 0, Icon = "🍌" },
        new() { Name = "Almonds (1 oz)", Calories = 164, ProteinGrams = 6, CarbsGrams = 6, FatGrams = 14, Icon = "🥜" },
        new() { Name = "Oatmeal (1 cup)", Calories = 154, ProteinGrams = 5, CarbsGrams = 27, FatGrams = 3, Icon = "🥣" },
        new() { Name = "Salmon Fillet", Calories = 367, ProteinGrams = 34, CarbsGrams = 0, FatGrams = 22, Icon = "🐟" },
    ];

    public CalorieService()
    {
        Load();
    }

    public int DailyCalorieGoal
    {
        get => Preferences.Get("calorie_goal", 2000);
        set => Preferences.Set("calorie_goal", value);
    }

    public List<CalorieEntry> TodayEntries => _entries.Where(e => e.Timestamp.Date == DateTime.Today).OrderByDescending(e => e.Timestamp).ToList();

    public int TodayCalories => TodayEntries.Sum(e => e.Calories);
    public double TodayProtein => TodayEntries.Sum(e => e.ProteinGrams);
    public double TodayCarbs => TodayEntries.Sum(e => e.CarbsGrams);
    public double TodayFat => TodayEntries.Sum(e => e.FatGrams);

    public double CalorieProgress => DailyCalorieGoal > 0 ? Math.Min(1.0, (double)TodayCalories / DailyCalorieGoal) : 0;

    public void AddEntry(CalorieEntry entry)
    {
        _entries.Add(entry);
        Save();
    }

    public void RemoveEntry(string id)
    {
        _entries.RemoveAll(e => e.Id == id);
        Save();
    }

    public CalorieEntry QuickAdd(string name, int calories, MealType mealType)
    {
        var entry = new CalorieEntry
        {
            Name = name,
            Calories = calories,
            MealType = mealType,
            Method = EntryMethod.QuickAdd
        };
        AddEntry(entry);
        return entry;
    }

    public CalorieEntry AddFromMacros(string name, double protein, double carbs, double fat, MealType mealType)
    {
        int calories = (int)(protein * 4 + carbs * 4 + fat * 9);
        var entry = new CalorieEntry
        {
            Name = name,
            Calories = calories,
            ProteinGrams = protein,
            CarbsGrams = carbs,
            FatGrams = fat,
            MealType = mealType,
            Method = EntryMethod.ManualMacros
        };
        AddEntry(entry);
        return entry;
    }

    public CalorieEntry AddFromPreset(MealPreset preset, MealType mealType)
    {
        var entry = new CalorieEntry
        {
            Name = preset.Name,
            Calories = preset.Calories,
            ProteinGrams = preset.ProteinGrams,
            CarbsGrams = preset.CarbsGrams,
            FatGrams = preset.FatGrams,
            MealType = mealType,
            Method = EntryMethod.MealPreset
        };
        AddEntry(entry);
        return entry;
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_entries);
        Preferences.Set(EntriesKey, json);
    }

    private void Load()
    {
        var json = Preferences.Get(EntriesKey, "[]");
        _entries = JsonSerializer.Deserialize<List<CalorieEntry>>(json) ?? [];
    }
}
