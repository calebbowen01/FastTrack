using MyMauiApp.ViewModels;

namespace MyMauiApp.Pages;

public partial class CaloriesPage : ContentPage
{
    private readonly CaloriesViewModel _vm;

    public CaloriesPage(CaloriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.RefreshAll();
    }
}
