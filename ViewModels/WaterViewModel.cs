using System.Collections.ObjectModel;
using System.Windows.Input;
using MyMauiApp.Models;
using MyMauiApp.Services;

namespace MyMauiApp.ViewModels;

public class WaterViewModel : BaseViewModel
{
    private readonly WaterService _waterService;
    private readonly NotificationService _notificationService;

    public WaterViewModel(WaterService waterService, NotificationService notificationService)
    {
        _waterService = waterService;
        _notificationService = notificationService;
        Add250Command = new Command(() => AddWater(250));
        Add500Command = new Command(() => AddWater(500));
        AddCustomCommand = new Command(AddCustom);
        UndoCommand = new Command(Undo);
        RemoveEntryCommand = new Command<WaterEntry>(DoRemoveEntry);
        RefreshEntries();
    }

    private string _customAmount = string.Empty;
    public string CustomAmount { get => _customAmount; set => SetProperty(ref _customAmount, value); }

    public int TodayTotalMl => _waterService.TodayTotalMl;
    public int GoalMl => _waterService.DailyGoalMl;
    public double Progress => _waterService.Progress;
    public string ProgressText => $"{TodayTotalMl} ml / {GoalMl} ml";
    public string ProgressLiters => $"{TodayTotalMl / 1000.0:F1}L of {GoalMl / 1000.0:F1}L";
    public int GlassCount => TodayTotalMl / 250;
    public string PercentText => $"{(int)(Progress * 100)}%";
    public bool GoalReached => TodayTotalMl >= GoalMl;

    public ObservableCollection<WaterEntry> TodayEntries { get; } = [];

    public ICommand Add250Command { get; }
    public ICommand Add500Command { get; }
    public ICommand AddCustomCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RemoveEntryCommand { get; }

    private void AddWater(int ml)
    {
        _waterService.AddWater(ml);
        if (!GoalReached)
            _ = _notificationService.ScheduleWaterReminder();
        RefreshAll();
    }

    private void AddCustom()
    {
        if (int.TryParse(CustomAmount, out int ml) && ml > 0)
        {
            _waterService.AddWater(ml);
            CustomAmount = string.Empty;
            RefreshAll();
        }
    }

    private void Undo()
    {
        _waterService.RemoveLast();
        RefreshAll();
    }

    private void DoRemoveEntry(WaterEntry entry)
    {
        _waterService.RemoveEntry(entry.Id);
        RefreshAll();
    }

    private void RefreshEntries()
    {
        TodayEntries.Clear();
        foreach (var e in _waterService.TodayEntries.OrderByDescending(e => e.Timestamp))
            TodayEntries.Add(e);
    }

    public void RefreshAll()
    {
        RefreshEntries();
        OnPropertyChanged(nameof(TodayTotalMl));
        OnPropertyChanged(nameof(GoalMl));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressLiters));
        OnPropertyChanged(nameof(GlassCount));
        OnPropertyChanged(nameof(PercentText));
        OnPropertyChanged(nameof(GoalReached));
    }
}
