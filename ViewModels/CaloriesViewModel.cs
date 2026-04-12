using System.Collections.ObjectModel;
using System.Windows.Input;
using MyMauiApp.Models;
using MyMauiApp.Services;

namespace MyMauiApp.ViewModels;

public class CaloriesViewModel : BaseViewModel
{
    private readonly CalorieService _calorieService;

    private string _quickName = string.Empty;
    private string _quickCalories = string.Empty;
    private MealType _selectedMealType = MealType.Lunch;

    private string _macroName = string.Empty;
    private string _macroProtein = string.Empty;
    private string _macroCarbs = string.Empty;
    private string _macroFat = string.Empty;

    private int _selectedTabIndex;

    public CaloriesViewModel(CalorieService calorieService)
    {
        _calorieService = calorieService;
        QuickAddCommand = new Command(DoQuickAdd);
        MacroAddCommand = new Command(DoMacroAdd);
        PresetAddCommand = new Command<MealPreset>(DoPresetAdd);
        RemoveEntryCommand = new Command<CalorieEntry>(DoRemoveEntry);
        RefreshEntries();
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public string QuickName { get => _quickName; set => SetProperty(ref _quickName, value); }
    public string QuickCalories { get => _quickCalories; set => SetProperty(ref _quickCalories, value); }

    public string MacroName { get => _macroName; set => SetProperty(ref _macroName, value); }
    public string MacroProtein { get => _macroProtein; set => SetProperty(ref _macroProtein, value); }
    public string MacroCarbs { get => _macroCarbs; set => SetProperty(ref _macroCarbs, value); }
    public string MacroFat { get => _macroFat; set => SetProperty(ref _macroFat, value); }

    public MealType SelectedMealType { get => _selectedMealType; set => SetProperty(ref _selectedMealType, value); }
    public List<MealType> MealTypes => Enum.GetValues<MealType>().ToList();

    public int TodayCalories => _calorieService.TodayCalories;
    public int CalorieGoal => _calorieService.DailyCalorieGoal;
    public int CaloriesRemaining => Math.Max(0, CalorieGoal - TodayCalories);
    public double CalorieProgress => _calorieService.CalorieProgress;
    public string CalorieProgressText => $"{TodayCalories} / {CalorieGoal} kcal";
    public double TodayProtein => _calorieService.TodayProtein;
    public double TodayCarbs => _calorieService.TodayCarbs;
    public double TodayFat => _calorieService.TodayFat;

    public ObservableCollection<CalorieEntry> TodayEntries { get; } = [];
    public List<MealPreset> Presets => CalorieService.DefaultPresets;

    public ICommand QuickAddCommand { get; }
    public ICommand MacroAddCommand { get; }
    public ICommand PresetAddCommand { get; }
    public ICommand RemoveEntryCommand { get; }

    private void DoQuickAdd()
    {
        if (string.IsNullOrWhiteSpace(QuickName) || !int.TryParse(QuickCalories, out int cal) || cal <= 0) return;
        _calorieService.QuickAdd(QuickName, cal, SelectedMealType);
        QuickName = string.Empty;
        QuickCalories = string.Empty;
        RefreshAll();
    }

    private void DoMacroAdd()
    {
        if (string.IsNullOrWhiteSpace(MacroName)) return;
        double.TryParse(MacroProtein, out double p);
        double.TryParse(MacroCarbs, out double c);
        double.TryParse(MacroFat, out double f);
        if (p <= 0 && c <= 0 && f <= 0) return;
        _calorieService.AddFromMacros(MacroName, p, c, f, SelectedMealType);
        MacroName = string.Empty;
        MacroProtein = string.Empty;
        MacroCarbs = string.Empty;
        MacroFat = string.Empty;
        RefreshAll();
    }

    private void DoPresetAdd(MealPreset preset)
    {
        _calorieService.AddFromPreset(preset, SelectedMealType);
        RefreshAll();
    }

    private void DoRemoveEntry(CalorieEntry entry)
    {
        _calorieService.RemoveEntry(entry.Id);
        RefreshAll();
    }

    public void RefreshAll()
    {
        RefreshEntries();
        OnPropertyChanged(nameof(TodayCalories));
        OnPropertyChanged(nameof(CalorieGoal));
        OnPropertyChanged(nameof(CaloriesRemaining));
        OnPropertyChanged(nameof(CalorieProgress));
        OnPropertyChanged(nameof(CalorieProgressText));
        OnPropertyChanged(nameof(TodayProtein));
        OnPropertyChanged(nameof(TodayCarbs));
        OnPropertyChanged(nameof(TodayFat));
    }

    private void RefreshEntries()
    {
        TodayEntries.Clear();
        foreach (var e in _calorieService.TodayEntries)
            TodayEntries.Add(e);
    }
}
