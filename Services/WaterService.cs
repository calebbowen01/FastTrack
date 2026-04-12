using System.Text.Json;
using MyMauiApp.Models;

namespace MyMauiApp.Services;

public class WaterService
{
    private const string EntriesKey = "water_entries";
    private List<WaterEntry> _entries = [];

    public WaterService()
    {
        Load();
    }

    public int DailyGoalMl
    {
        get => Preferences.Get("water_goal_ml", 2500);
        set => Preferences.Set("water_goal_ml", value);
    }

    public List<WaterEntry> TodayEntries => _entries.Where(e => e.Timestamp.Date == DateTime.Today).ToList();

    public int TodayTotalMl => TodayEntries.Sum(e => e.AmountMl);

    public double Progress => DailyGoalMl > 0 ? Math.Min(1.0, (double)TodayTotalMl / DailyGoalMl) : 0;

    public void AddWater(int amountMl)
    {
        _entries.Add(new WaterEntry { AmountMl = amountMl });
        Save();
    }

    public void RemoveLast()
    {
        var today = TodayEntries;
        if (today.Count > 0)
        {
            var last = today.Last();
            _entries.RemoveAll(e => e.Id == last.Id);
            Save();
        }
    }

    public void RemoveEntry(string id)
    {
        _entries.RemoveAll(e => e.Id == id);
        Save();
    }

    public void ClearAll()
    {
        _entries.Clear();
        Preferences.Remove(EntriesKey);
        Preferences.Remove("water_goal_ml");
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_entries);
        Preferences.Set(EntriesKey, json);
    }

    private void Load()
    {
        var json = Preferences.Get(EntriesKey, "[]");
        _entries = JsonSerializer.Deserialize<List<WaterEntry>>(json) ?? [];
    }
}
