using System.Text.Json;
using MyMauiApp.Models;

namespace MyMauiApp.Services;

public class FavoriteMealService
{
    private const string FavoritesKey = "favorite_meals";
    private List<MealPreset> _favorites = [];

    public FavoriteMealService()
    {
        Load();
    }

    public List<MealPreset> Favorites => [.. _favorites];

    public void AddFavorite(MealPreset preset)
    {
        _favorites.Add(preset);
        Save();
    }

    public void RemoveFavorite(string name)
    {
        _favorites.RemoveAll(f => f.Name == name);
        Save();
    }

    public void ClearAll()
    {
        _favorites.Clear();
        Preferences.Remove(FavoritesKey);
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_favorites);
        Preferences.Set(FavoritesKey, json);
    }

    private void Load()
    {
        var json = Preferences.Get(FavoritesKey, "[]");
        _favorites = JsonSerializer.Deserialize<List<MealPreset>>(json) ?? [];
    }
}
