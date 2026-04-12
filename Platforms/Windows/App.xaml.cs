using Microsoft.UI.Xaml;

namespace MyMauiApp.WinUI;

public partial class App : MauiWinUIApplication
{
	public App()
	{
		this.InitializeComponent();
		this.UnhandledException += (s, e) =>
		{
			System.Diagnostics.Debug.WriteLine($"*** UNHANDLED: {e.Exception}");
			e.Handled = true;
		};
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

