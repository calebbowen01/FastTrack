using System.Windows.Input;
using MyMauiApp.Services;

namespace MyMauiApp.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly FastingService _fastingService;
    private readonly CalorieService _calorieService;
    private readonly WaterService _waterService;
    private readonly FavoriteMealService _favoriteMealService;
    private readonly NotificationService _notificationService;

    private string _calorieGoalText = string.Empty;
    private string _waterGoalText = string.Empty;
    private string _weightText = string.Empty;
    private string _goalWeightText = string.Empty;
    private string _proteinGoalText = string.Empty;
    private string _carbsGoalText = string.Empty;
    private string _fatGoalText = string.Empty;
    private string _heightText = string.Empty;
    private string _ageText = string.Empty;
    private string _selectedGender = "Male";
    private string _statusMessage = string.Empty;
    private string _bmrDisplay = string.Empty;

    public ProfileViewModel(FastingService fastingService, CalorieService calorieService, WaterService waterService, FavoriteMealService favoriteMealService, NotificationService notificationService)
    {
        _fastingService = fastingService;
        _calorieService = calorieService;
        _waterService = waterService;
        _favoriteMealService = favoriteMealService;
        _notificationService = notificationService;
        SaveCommand = new Command(Save);
        ResetDataCommand = new Command(ResetData);
        CalculateBmrCommand = new Command(CalculateBmr);
        Load();
    }

    public string CalorieGoalText { get => _calorieGoalText; set => SetProperty(ref _calorieGoalText, value); }
    public string WaterGoalText { get => _waterGoalText; set => SetProperty(ref _waterGoalText, value); }
    public string WeightText { get => _weightText; set => SetProperty(ref _weightText, value); }
    public string GoalWeightText { get => _goalWeightText; set => SetProperty(ref _goalWeightText, value); }
    public string ProteinGoalText { get => _proteinGoalText; set => SetProperty(ref _proteinGoalText, value); }
    public string CarbsGoalText { get => _carbsGoalText; set => SetProperty(ref _carbsGoalText, value); }
    public string FatGoalText { get => _fatGoalText; set => SetProperty(ref _fatGoalText, value); }
    public string HeightText { get => _heightText; set => SetProperty(ref _heightText, value); }
    public string AgeText { get => _ageText; set => SetProperty(ref _ageText, value); }
    public string SelectedGender { get => _selectedGender; set => SetProperty(ref _selectedGender, value); }
    public List<string> Genders => ["Male", "Female"];
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public string BmrDisplay { get => _bmrDisplay; set => SetProperty(ref _bmrDisplay, value); }

    public int FastingStreak => _fastingService.CurrentStreak;
    public int LongestStreak => _fastingService.LongestStreak;
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

    public double HeightInches
    {
        get => Preferences.Get("height_inches", 0.0);
        set => Preferences.Set("height_inches", value);
    }

    public int Age
    {
        get => Preferences.Get("user_age", 0);
        set => Preferences.Set("user_age", value);
    }

    public string Gender
    {
        get => Preferences.Get("user_gender", "Male");
        set => Preferences.Set("user_gender", value);
    }

    public bool NotificationsEnabled
    {
        get => _notificationService.NotificationsEnabled;
        set
        {
            _notificationService.NotificationsEnabled = value;
            OnPropertyChanged();
            if (!value)
                _notificationService.CancelAll();
        }
    }

    public bool WaterRemindersEnabled
    {
        get => _notificationService.WaterRemindersEnabled;
        set
        {
            _notificationService.WaterRemindersEnabled = value;
            OnPropertyChanged();
            if (!value)
                _notificationService.CancelWaterReminder();
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand ResetDataCommand { get; }
    public ICommand CalculateBmrCommand { get; }

    private void Load()
    {
        CalorieGoalText = _calorieService.DailyCalorieGoal.ToString();
        WaterGoalText = _waterService.DailyGoalMl.ToString();
        WeightText = CurrentWeight > 0 ? CurrentWeight.ToString("F1") : "";
        GoalWeightText = GoalWeight > 0 ? GoalWeight.ToString("F1") : "";
        ProteinGoalText = _calorieService.DailyProteinGoal.ToString();
        CarbsGoalText = _calorieService.DailyCarbsGoal.ToString();
        FatGoalText = _calorieService.DailyFatGoal.ToString();
        HeightText = HeightInches > 0 ? HeightInches.ToString("F1") : "";
        AgeText = Age > 0 ? Age.ToString() : "";
        SelectedGender = Gender;
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
        if (int.TryParse(ProteinGoalText, out int proteinGoal) && proteinGoal > 0)
            _calorieService.DailyProteinGoal = proteinGoal;
        if (int.TryParse(CarbsGoalText, out int carbsGoal) && carbsGoal > 0)
            _calorieService.DailyCarbsGoal = carbsGoal;
        if (int.TryParse(FatGoalText, out int fatGoal) && fatGoal > 0)
            _calorieService.DailyFatGoal = fatGoal;
        if (double.TryParse(HeightText, out double height) && height > 0)
            HeightInches = height;
        if (int.TryParse(AgeText, out int age) && age > 0)
            Age = age;
        Gender = SelectedGender;

        StatusMessage = "✓ Settings saved!";
        OnPropertyChanged(nameof(FastingStreak));
        OnPropertyChanged(nameof(LongestStreak));
        OnPropertyChanged(nameof(TotalFasts));
    }

    private void CalculateBmr()
    {
        if (!double.TryParse(WeightText, out double weightLbs) || weightLbs <= 0)
        { BmrDisplay = "Enter your weight first."; return; }
        if (!double.TryParse(HeightText, out double heightIn) || heightIn <= 0)
        { BmrDisplay = "Enter your height first."; return; }
        if (!int.TryParse(AgeText, out int age) || age <= 0)
        { BmrDisplay = "Enter your age first."; return; }

        double weightKg = weightLbs * 0.453592;
        double heightCm = heightIn * 2.54;

        double bmr;
        if (SelectedGender == "Male")
            bmr = 10 * weightKg + 6.25 * heightCm - 5 * age + 5;
        else
            bmr = 10 * weightKg + 6.25 * heightCm - 5 * age - 161;

        double sedentary = bmr * 1.2;
        double light = bmr * 1.375;
        double moderate = bmr * 1.55;
        double active = bmr * 1.725;

        BmrDisplay = $"BMR: {bmr:F0} kcal/day\n" +
                     $"Sedentary: {sedentary:F0} kcal\n" +
                     $"Light Active: {light:F0} kcal\n" +
                     $"Moderate: {moderate:F0} kcal\n" +
                     $"Very Active: {active:F0} kcal";
    }

    private async void ResetData()
    {
        var confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
            "Reset All Data",
            "This will permanently clear ALL your data:\n\n" +
            "• Fasting sessions & streaks\n" +
            "• Calorie entries & goals\n" +
            "• Water intake logs\n" +
            "• Saved favorites\n" +
            "• Profile settings\n\n" +
            "This cannot be undone!",
            "Reset Everything", "Cancel");
        if (confirm)
        {
            _fastingService.ClearAll();
            _calorieService.ClearAll();
            _waterService.ClearAll();
            _favoriteMealService.ClearAll();
            Preferences.Remove("current_weight");
            Preferences.Remove("goal_weight");
            Preferences.Remove("height_inches");
            Preferences.Remove("user_age");
            Preferences.Remove("user_gender");
            Preferences.Remove("notifications_enabled");
            Preferences.Remove("water_reminders_enabled");
            _notificationService.CancelAll();
            Load();
            OnPropertyChanged(nameof(FastingStreak));
            OnPropertyChanged(nameof(LongestStreak));
            OnPropertyChanged(nameof(TotalFasts));
            OnPropertyChanged(nameof(NotificationsEnabled));
            OnPropertyChanged(nameof(WaterRemindersEnabled));
            BmrDisplay = string.Empty;
            StatusMessage = "All data has been reset.";
        }
    }
}
