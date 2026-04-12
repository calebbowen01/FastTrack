using MyMauiApp.ViewModels;

namespace MyMauiApp.Pages;

public partial class WaterPage : ContentPage
{
    private readonly WaterViewModel _vm;

    public WaterPage(WaterViewModel vm)
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
