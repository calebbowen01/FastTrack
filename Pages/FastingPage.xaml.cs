using MyMauiApp.ViewModels;

namespace MyMauiApp.Pages;

public partial class FastingPage : ContentPage
{
    private readonly FastingViewModel _vm;

    public FastingPage(FastingViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.RefreshAll();
        if (_vm.IsFasting)
            _vm.StartTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.StopTimer();
    }
}
