using MyMauiApp.ViewModels;

namespace MyMauiApp.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;

    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
        _vm.StartTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.StopTimer();
    }
}
