using System.Windows.Input;
using MyMauiApp.Models;
using MyMauiApp.Services;

namespace MyMauiApp.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly FastingService _fastingService;
    private readonly CalorieService _calorieService;
    private readonly WaterService _waterService;
    private IDispatcherTimer? _timer;

    public DashboardViewModel(FastingService fastingService, CalorieService calorieService, WaterService waterService)
    {
        _fastingService = fastingService;
        _calorieService = calorieService;
        _waterService = waterService;
    }

    public bool IsFasting => _fastingService.IsFasting;
    public string FastingStatus => IsFasting ? "FASTING" : "NOT FASTING";
    public string FastingStatusColor => IsFasting ? "#00C853" : "#FF6D00";
    public string FastingElapsed => IsFasting ? FormatTimeSpan(_fastingService.ActiveSession!.Elapsed) : "--:--:--";
    public string FastingPlan => IsFasting ? _fastingService.ActiveSession!.PlanName : "No active fast";
    public double FastingProgress => IsFasting ? _fastingService.ActiveSession!.ProgressPercent : 0;

    public int TodayCalories => _calorieService.TodayCalories;
    public int CalorieGoal => _calorieService.DailyCalorieGoal;
    public int CaloriesRemaining => Math.Max(0, CalorieGoal - TodayCalories);
    public double CalorieProgress => _calorieService.CalorieProgress;
    public double TodayProtein => _calorieService.TodayProtein;
    public double TodayCarbs => _calorieService.TodayCarbs;
    public double TodayFat => _calorieService.TodayFat;

    public int WaterTotalMl => _waterService.TodayTotalMl;
    public int WaterGoalMl => _waterService.DailyGoalMl;
    public double WaterProgress => _waterService.Progress;
    public string WaterDisplay => $"{WaterTotalMl / 1000.0:F1}L / {WaterGoalMl / 1000.0:F1}L";

    public int FastingStreak => _fastingService.CurrentStreak;

    public string Greeting
    {
        get
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                < 12 => "Good Morning",
                < 17 => "Good Afternoon",
                _ => "Good Evening"
            };
        }
    }

    public string TodayDate => DateTime.Today.ToString("dddd, MMMM d");

    public void StartTimer()
    {
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer != null)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => RefreshFastingDisplay();
            _timer.Start();
        }
    }

    public void StopTimer()
    {
        _timer?.Stop();
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(IsFasting));
        OnPropertyChanged(nameof(FastingStatus));
        OnPropertyChanged(nameof(FastingStatusColor));
        OnPropertyChanged(nameof(FastingPlan));
        OnPropertyChanged(nameof(FastingProgress));
        OnPropertyChanged(nameof(TodayCalories));
        OnPropertyChanged(nameof(CalorieGoal));
        OnPropertyChanged(nameof(CaloriesRemaining));
        OnPropertyChanged(nameof(CalorieProgress));
        OnPropertyChanged(nameof(TodayProtein));
        OnPropertyChanged(nameof(TodayCarbs));
        OnPropertyChanged(nameof(TodayFat));
        OnPropertyChanged(nameof(WaterTotalMl));
        OnPropertyChanged(nameof(WaterGoalMl));
        OnPropertyChanged(nameof(WaterProgress));
        OnPropertyChanged(nameof(WaterDisplay));
        OnPropertyChanged(nameof(FastingStreak));
        OnPropertyChanged(nameof(Greeting));
        RefreshFastingDisplay();
    }

    private void RefreshFastingDisplay()
    {
        OnPropertyChanged(nameof(FastingElapsed));
        OnPropertyChanged(nameof(FastingProgress));
    }

    private static string FormatTimeSpan(TimeSpan ts) => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
}
