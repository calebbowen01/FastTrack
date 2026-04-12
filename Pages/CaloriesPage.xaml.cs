using MyMauiApp.ViewModels;
using ZXing.Net.Maui;

namespace MyMauiApp.Pages;

public partial class CaloriesPage : ContentPage
{
    private readonly CaloriesViewModel _vm;

    public CaloriesPage(CaloriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.RefreshAll();
    }

    private void BarcodeReader_BarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results?.FirstOrDefault();
        if (first == null) return;

        Dispatcher.Dispatch(() =>
        {
            _vm.OnBarcodeDetected(first.Value);
        });
    }
}
