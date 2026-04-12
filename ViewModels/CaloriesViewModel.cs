using System.Collections.ObjectModel;
using System.Windows.Input;
using MyMauiApp.Models;
using MyMauiApp.Services;

namespace MyMauiApp.ViewModels;

public class CaloriesViewModel : BaseViewModel
{
    private readonly CalorieService _calorieService;
    private readonly BarcodeService _barcodeService;
    private readonly FavoriteMealService _favoriteMealService;

    private string _quickName = string.Empty;
    private string _quickCalories = string.Empty;
    private string _quickProtein = string.Empty;
    private string _quickCarbs = string.Empty;
    private string _quickFat = string.Empty;
    private MealType _selectedMealType = MealType.Lunch;

    private string _macroName = string.Empty;
    private string _macroProtein = string.Empty;
    private string _macroCarbs = string.Empty;
    private string _macroFat = string.Empty;

    private string _barcodeText = string.Empty;
    private string _barcodeStatus = string.Empty;
    private bool _isBarcodeLoading;
    private FoodProduct? _scannedProduct;
    private bool _useServingSize = true;
    private string _servingsText = "1";

    private int _selectedTabIndex;
    private bool _isScannerVisible;

    public CaloriesViewModel(CalorieService calorieService, BarcodeService barcodeService, FavoriteMealService favoriteMealService)
    {
        _calorieService = calorieService;
        _barcodeService = barcodeService;
        _favoriteMealService = favoriteMealService;
        QuickAddCommand = new Command(DoQuickAdd);
        MacroAddCommand = new Command(DoMacroAdd);
        PresetAddCommand = new Command<MealPreset>(DoPresetAdd);
        RemoveEntryCommand = new Command<CalorieEntry>(DoRemoveEntry);
        BarcodeLookupCommand = new Command(async () => await DoBarcodeLookup());
        BarcodeAddCommand = new Command(DoAddScannedProduct);
        SaveAsFavoriteCommand = new Command(DoSaveAsFavorite);
        FavoriteAddCommand = new Command<MealPreset>(DoFavoriteAdd);
        FavoriteRemoveCommand = new Command<MealPreset>(DoFavoriteRemove);
        ToggleScannerCommand = new Command(ToggleScanner);
        ClearScannedProductCommand = new Command(DoClearScannedProduct);
        RefreshEntries();
        RefreshFavorites();
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    // Quick Add
    public string QuickName { get => _quickName; set => SetProperty(ref _quickName, value); }
    public string QuickCalories { get => _quickCalories; set => SetProperty(ref _quickCalories, value); }
    public string QuickProtein { get => _quickProtein; set => SetProperty(ref _quickProtein, value); }
    public string QuickCarbs { get => _quickCarbs; set => SetProperty(ref _quickCarbs, value); }
    public string QuickFat { get => _quickFat; set => SetProperty(ref _quickFat, value); }

    // Manual Macros
    public string MacroName { get => _macroName; set => SetProperty(ref _macroName, value); }
    public string MacroProtein { get => _macroProtein; set => SetProperty(ref _macroProtein, value); }
    public string MacroCarbs { get => _macroCarbs; set => SetProperty(ref _macroCarbs, value); }
    public string MacroFat { get => _macroFat; set => SetProperty(ref _macroFat, value); }

    // Scanner
    public bool IsScannerVisible { get => _isScannerVisible; set => SetProperty(ref _isScannerVisible, value); }
    public string ScannerButtonText => IsScannerVisible ? "Close Scanner" : "📷 Open Camera Scanner";

    // Barcode
    public string BarcodeText { get => _barcodeText; set => SetProperty(ref _barcodeText, value); }
    public string BarcodeStatus { get => _barcodeStatus; set => SetProperty(ref _barcodeStatus, value); }
    public bool IsBarcodeLoading { get => _isBarcodeLoading; set => SetProperty(ref _isBarcodeLoading, value); }
    public FoodProduct? ScannedProduct { get => _scannedProduct; set { SetProperty(ref _scannedProduct, value); OnPropertyChanged(nameof(HasScannedProduct)); OnPropertyChanged(nameof(ScannedProductDisplay)); OnPropertyChanged(nameof(ScannedCaloriesDisplay)); OnPropertyChanged(nameof(ScannedMacrosDisplay)); } }
    public bool HasScannedProduct => ScannedProduct != null;
    public string ScannedProductDisplay => ScannedProduct?.DisplayName ?? "";
    public string ScannedCaloriesDisplay => ScannedProduct == null ? "" : ScannedProduct.HasServingData ? $"{ScannedProduct.CaloriesPerServing} kcal/serving ({ScannedProduct.ServingSize})" : $"{ScannedProduct.CaloriesPer100g} kcal/100g";
    public string ScannedMacrosDisplay => ScannedProduct == null ? "" : ScannedProduct.HasServingData ? $"P: {ScannedProduct.ProteinPerServing:F1}g · C: {ScannedProduct.CarbsPerServing:F1}g · F: {ScannedProduct.FatPerServing:F1}g" : $"P: {ScannedProduct.ProteinPer100g:F1}g · C: {ScannedProduct.CarbsPer100g:F1}g · F: {ScannedProduct.FatPer100g:F1}g";

    public bool UseServingSize { get => _useServingSize; set { SetProperty(ref _useServingSize, value); OnPropertyChanged(nameof(ScannedCaloriesDisplay)); OnPropertyChanged(nameof(ScannedMacrosDisplay)); } }
    public string ServingsText { get => _servingsText; set => SetProperty(ref _servingsText, value); }

    // Meal type
    public MealType SelectedMealType { get => _selectedMealType; set => SetProperty(ref _selectedMealType, value); }
    public List<MealType> MealTypes => Enum.GetValues<MealType>().ToList();

    // Progress
    public int TodayCalories => _calorieService.TodayCalories;
    public int CalorieGoal => _calorieService.DailyCalorieGoal;
    public int CaloriesRemaining => Math.Max(0, CalorieGoal - TodayCalories);
    public double CalorieProgress => _calorieService.CalorieProgress;
    public string CalorieProgressText => $"{TodayCalories} / {CalorieGoal} kcal";
    public double TodayProtein => _calorieService.TodayProtein;
    public double TodayCarbs => _calorieService.TodayCarbs;
    public double TodayFat => _calorieService.TodayFat;

    public int ProteinGoal => _calorieService.DailyProteinGoal;
    public int CarbsGoal => _calorieService.DailyCarbsGoal;
    public int FatGoal => _calorieService.DailyFatGoal;
    public double ProteinProgress => ProteinGoal > 0 ? Math.Min(1.0, TodayProtein / ProteinGoal) : 0;
    public double CarbsProgress => CarbsGoal > 0 ? Math.Min(1.0, TodayCarbs / CarbsGoal) : 0;
    public double FatProgress => FatGoal > 0 ? Math.Min(1.0, TodayFat / FatGoal) : 0;

    // Collections
    public ObservableCollection<CalorieEntry> TodayEntries { get; } = [];
    public List<MealPreset> Presets => CalorieService.DefaultPresets;
    public ObservableCollection<MealPreset> Favorites { get; } = [];
    public bool HasFavorites => Favorites.Count > 0;

    // Commands
    public ICommand QuickAddCommand { get; }
    public ICommand MacroAddCommand { get; }
    public ICommand PresetAddCommand { get; }
    public ICommand RemoveEntryCommand { get; }
    public ICommand BarcodeLookupCommand { get; }
    public ICommand BarcodeAddCommand { get; }
    public ICommand SaveAsFavoriteCommand { get; }
    public ICommand FavoriteAddCommand { get; }
    public ICommand FavoriteRemoveCommand { get; }
    public ICommand ToggleScannerCommand { get; }
    public ICommand ClearScannedProductCommand { get; }

    private void DoQuickAdd()
    {
        if (string.IsNullOrWhiteSpace(QuickName) || !int.TryParse(QuickCalories, out int cal) || cal <= 0) return;

        double.TryParse(QuickProtein, out double p);
        double.TryParse(QuickCarbs, out double c);
        double.TryParse(QuickFat, out double f);

        var entry = new CalorieEntry
        {
            Name = QuickName,
            Calories = cal,
            ProteinGrams = p,
            CarbsGrams = c,
            FatGrams = f,
            MealType = SelectedMealType,
            Method = EntryMethod.QuickAdd
        };
        _calorieService.AddEntry(entry);

        QuickName = string.Empty;
        QuickCalories = string.Empty;
        QuickProtein = string.Empty;
        QuickCarbs = string.Empty;
        QuickFat = string.Empty;
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

    private void DoFavoriteAdd(MealPreset preset)
    {
        _calorieService.AddFromPreset(preset, SelectedMealType);
        RefreshAll();
    }

    private void DoFavoriteRemove(MealPreset preset)
    {
        _favoriteMealService.RemoveFavorite(preset.Name);
        RefreshFavorites();
    }

    private void DoRemoveEntry(CalorieEntry entry)
    {
        _calorieService.RemoveEntry(entry.Id);
        RefreshAll();
    }

    private void DoClearScannedProduct()
    {
        ScannedProduct = null;
        BarcodeText = string.Empty;
        BarcodeStatus = string.Empty;
        ServingsText = "1";
    }

    private void ToggleScanner()
    {
        IsScannerVisible = !IsScannerVisible;
        OnPropertyChanged(nameof(ScannerButtonText));
    }

    public async void OnBarcodeDetected(string barcode)
    {
        IsScannerVisible = false;
        OnPropertyChanged(nameof(ScannerButtonText));
        BarcodeText = barcode;
        await DoBarcodeLookup();
    }

    private async Task DoBarcodeLookup()
    {
        var code = BarcodeText?.Trim();
        if (string.IsNullOrWhiteSpace(code)) return;

        IsBarcodeLoading = true;
        BarcodeStatus = "Looking up product...";
        ScannedProduct = null;

        var product = await _barcodeService.LookupBarcodeAsync(code);

        IsBarcodeLoading = false;

        if (product != null)
        {
            ScannedProduct = product;
            BarcodeStatus = "✓ Product found!";
            ServingsText = "1";
        }
        else
        {
            BarcodeStatus = "✗ Product not found. Enter details manually below.";
        }
    }

    private void DoAddScannedProduct()
    {
        if (ScannedProduct == null) return;
        double.TryParse(ServingsText, out double servings);
        if (servings <= 0) servings = 1;

        int cal;
        double p, c, f;
        if (ScannedProduct.HasServingData && UseServingSize)
        {
            cal = ScannedProduct.CaloriesPerServing;
            p = ScannedProduct.ProteinPerServing;
            c = ScannedProduct.CarbsPerServing;
            f = ScannedProduct.FatPerServing;
        }
        else
        {
            cal = ScannedProduct.CaloriesPer100g;
            p = ScannedProduct.ProteinPer100g;
            c = ScannedProduct.CarbsPer100g;
            f = ScannedProduct.FatPer100g;
        }

        _calorieService.AddFromBarcode(ScannedProduct.DisplayName, cal, p, c, f, ScannedProduct.Barcode, SelectedMealType, servings);

        BarcodeText = string.Empty;
        BarcodeStatus = "✓ Added!";
        ScannedProduct = null;
        RefreshAll();
    }

    private void DoSaveAsFavorite()
    {
        if (ScannedProduct == null) return;
        var preset = new MealPreset
        {
            Name = ScannedProduct.DisplayName,
            Calories = ScannedProduct.HasServingData ? ScannedProduct.CaloriesPerServing : ScannedProduct.CaloriesPer100g,
            ProteinGrams = ScannedProduct.HasServingData ? ScannedProduct.ProteinPerServing : ScannedProduct.ProteinPer100g,
            CarbsGrams = ScannedProduct.HasServingData ? ScannedProduct.CarbsPerServing : ScannedProduct.CarbsPer100g,
            FatGrams = ScannedProduct.HasServingData ? ScannedProduct.FatPerServing : ScannedProduct.FatPer100g,
            Icon = "⭐"
        };
        _favoriteMealService.AddFavorite(preset);
        RefreshFavorites();
        BarcodeStatus = "✓ Saved to favorites!";
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
        OnPropertyChanged(nameof(ProteinProgress));
        OnPropertyChanged(nameof(CarbsProgress));
        OnPropertyChanged(nameof(FatProgress));
    }

    private void RefreshEntries()
    {
        TodayEntries.Clear();
        foreach (var e in _calorieService.TodayEntries)
            TodayEntries.Add(e);
    }

    private void RefreshFavorites()
    {
        Favorites.Clear();
        foreach (var f in _favoriteMealService.Favorites)
            Favorites.Add(f);
        OnPropertyChanged(nameof(HasFavorites));
    }
}
