using System.Windows.Input;
using MyMauiApp.Services;

namespace MyMauiApp.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly FastingService _fastingService;
    private readonly CalorieService _calorieService;
    private readonly WaterService _waterService;

    private string _calorieGoalText = string.Empty;
    private string _waterGoalText = string.Empty;
    private string _weightText = string.Empty;

    public ProfileViewModel(FastingService fastingService, CalorieService calorieService, WaterService waterService)
    {
        _fastingService = fastingService;
        _calorieService = calorieService;
        _waterService = waterService;
        SaveCommand = new Command(Save);
        ResetDataCommand = new Command(ResetData);
        Load();
    }

    public string CalorieGoalText { get => _calorieGoalText; set => SetProperty(ref _calorieGoalText, value); }
    public string WaterGoalText { get => _waterGoalText; set => SetProperty(ref _waterGoalText, value); }
    public string WeightText { get => _weightText; set => SetProperty(ref _weightText, value); }

    public int FastingStreak => _fastingService.CurrentStreak;
    public int TotalFasts => _fastingService.CompletedSessions.Count;

    public double CurrentWeight
    {
        get => Preferences.Get("current_weight", 0.0);
        set => Preferences.Set("current_weight", value);
    }

    public double GoalWeight
    {
        get => Preferences.Get("goal_weight", 0.0);
        set => Preferences.Set("goal_weight", value);
    }

    private string _goalWeightText = string.Empty;
    public string GoalWeightText { get => _goalWeightText; set => SetProperty(ref _goalWeightText, value); }

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    public ICommand SaveCommand { get; }
    public ICommand ResetDataCommand { get; }

    private void Load()
    {
        CalorieGoalText = _calorieService.DailyCalorieGoal.ToString();
        WaterGoalText = _waterService.DailyGoalMl.ToString();
        WeightText = CurrentWeight > 0 ? CurrentWeight.ToString("F1") : "";
        GoalWeightText = GoalWeight > 0 ? GoalWeight.ToString("F1") : "";
    }

    private void Save()
    {
        if (int.TryParse(CalorieGoalText, out int calGoal) && calGoal > 0)
            _calorieService.DailyCalorieGoal = calGoal;
        if (int.TryParse(WaterGoalText, out int waterGoal) && waterGoal > 0)
            _waterService.DailyGoalMl = waterGoal;
        if (double.TryParse(WeightText, out double weight) && weight > 0)
            CurrentWeight = weight;
        if (double.TryParse(GoalWeightText, out double goalWeight) && goalWeight > 0)
            GoalWeight = goalWeight;

        StatusMessage = "✓ Settings saved!";
        OnPropertyChanged(nameof(FastingStreak));
        OnPropertyChanged(nameof(TotalFasts));
    }

    private async void ResetData()
    {
        var confirm = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Reset All Data",
            "This will clear all your fasting, calorie, and water data. This cannot be undone.",
            "Reset", "Cancel");
        if (confirm)
        {
            Preferences.Clear();
            Load();
            StatusMessage = "All data has been reset.";
        }
    }
}
