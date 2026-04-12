using System.Collections.ObjectModel;
using System.Windows.Input;
using MyMauiApp.Models;
using MyMauiApp.Services;

namespace MyMauiApp.ViewModels;

public class FastingViewModel : BaseViewModel
{
    private readonly FastingService _fastingService;
    private IDispatcherTimer? _timer;
    private FastingPlan _selectedPlan;

    public FastingViewModel(FastingService fastingService)
    {
        _fastingService = fastingService;
        _selectedPlan = Plans[0];
        StartFastCommand = new Command(StartFast, () => !IsFasting);
        StopFastCommand = new Command(StopFast, () => IsFasting);
        RefreshHistory();
    }

    public List<FastingPlan> Plans => FastingService.AvailablePlans;

    private string _customFastingHours = string.Empty;
    private string _customEatingHours = string.Empty;

    public FastingPlan SelectedPlan
    {
        get => _selectedPlan;
        set
        {
            if (SetProperty(ref _selectedPlan, value))
                OnPropertyChanged(nameof(IsCustomPlan));
        }
    }

    public bool IsCustomPlan => SelectedPlan?.Name == "Custom";

    public string CustomFastingHours
    {
        get => _customFastingHours;
        set => SetProperty(ref _customFastingHours, value);
    }

    public string CustomEatingHours
    {
        get => _customEatingHours;
        set => SetProperty(ref _customEatingHours, value);
    }

    public bool IsFasting => _fastingService.IsFasting;
    public bool IsNotFasting => !IsFasting;

    public string ElapsedDisplay => IsFasting ? FormatTimeSpan(_fastingService.ActiveSession!.Elapsed) : "00:00:00";

    public string TargetDisplay => IsFasting ? $"Goal: {_fastingService.ActiveSession!.TargetHours}h" : "";

    public string RemainingDisplay
    {
        get
        {
            if (!IsFasting) return "";
            var remaining = TimeSpan.FromHours(_fastingService.ActiveSession!.TargetHours) - _fastingService.ActiveSession.Elapsed;
            if (remaining.TotalSeconds <= 0) return "🎉 Goal reached!";
            return $"{FormatTimeSpan(remaining)} remaining";
        }
    }

    public double Progress => IsFasting ? _fastingService.ActiveSession!.ProgressPercent : 0;
    public string ProgressPercent => $"{(int)(Progress * 100)}%";

    public string StartTimeDisplay => IsFasting ? _fastingService.ActiveSession!.StartTime.ToString("h:mm tt") : "";

    public string EndTimeEstimate
    {
        get
        {
            if (!IsFasting) return "";
            var end = _fastingService.ActiveSession!.StartTime.AddHours(_fastingService.ActiveSession.TargetHours);
            return end.ToString("h:mm tt");
        }
    }

    public ObservableCollection<FastingSession> History { get; } = [];

    public Command StartFastCommand { get; }
    public Command StopFastCommand { get; }

    public int Streak => _fastingService.CurrentStreak;

    public void StartFast()
    {
        var plan = SelectedPlan;
        if (plan.Name == "Custom")
        {
            if (!int.TryParse(CustomFastingHours, out int fh) || fh <= 0 || fh > 24) return;
            int eh = 24 - fh;
            if (int.TryParse(CustomEatingHours, out int parsedEh) && parsedEh > 0 && parsedEh < 24)
                eh = parsedEh;
            plan = new FastingPlan
            {
                Name = $"Custom {fh}:{eh}",
                FastingHours = fh,
                EatingHours = eh,
                Description = $"Custom — {fh}h fast, {eh}h eating window"
            };
        }
        _fastingService.StartFast(plan);
        RefreshAll();
        StartTimer();
    }

    public void StopFast()
    {
        _fastingService.StopFast();
        StopTimer();
        RefreshAll();
    }

    public void StartTimer()
    {
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer != null)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => RefreshTimerDisplay();
            _timer.Start();
        }
    }

    public void StopTimer()
    {
        _timer?.Stop();
    }

    public void RefreshAll()
    {
        OnPropertyChanged(nameof(IsFasting));
        OnPropertyChanged(nameof(IsNotFasting));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ProgressPercent));
        OnPropertyChanged(nameof(StartTimeDisplay));
        OnPropertyChanged(nameof(EndTimeEstimate));
        OnPropertyChanged(nameof(TargetDisplay));
        OnPropertyChanged(nameof(Streak));
        OnPropertyChanged(nameof(IsCustomPlan));
        StartFastCommand.ChangeCanExecute();
        StopFastCommand.ChangeCanExecute();
        RefreshTimerDisplay();
        RefreshHistory();
    }

    private void RefreshTimerDisplay()
    {
        OnPropertyChanged(nameof(ElapsedDisplay));
        OnPropertyChanged(nameof(RemainingDisplay));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ProgressPercent));
    }

    private void RefreshHistory()
    {
        History.Clear();
        foreach (var s in _fastingService.GetHistory().Take(20))
            History.Add(s);
    }

    private static string FormatTimeSpan(TimeSpan ts) => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
}
