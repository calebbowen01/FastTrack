using MyMauiApp.Services;

namespace MyMauiApp;

public partial class App : Application
{
	public App(NotificationService notificationService)
	{
		InitializeComponent();
		_ = notificationService.RequestPermissionAsync();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		window.Width = 400;
		window.Height = 850;
		return window;
	}
}