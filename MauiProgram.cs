using Microsoft.Extensions.Logging;
using MyMauiApp.Services;
using MyMauiApp.ViewModels;
using MyMauiApp.Pages;
using Plugin.LocalNotification;
using ZXing.Net.Maui.Controls;

namespace MyMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.UseLocalNotification()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Services
		builder.Services.AddSingleton<FastingService>();
		builder.Services.AddSingleton<CalorieService>();
		builder.Services.AddSingleton<WaterService>();
		builder.Services.AddSingleton<BarcodeService>();
		builder.Services.AddSingleton<FavoriteMealService>();
		builder.Services.AddSingleton<NotificationService>();

		// ViewModels
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<FastingViewModel>();
		builder.Services.AddTransient<CaloriesViewModel>();
		builder.Services.AddTransient<WaterViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();

		// Pages
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<FastingPage>();
		builder.Services.AddTransient<CaloriesPage>();
		builder.Services.AddTransient<WaterPage>();
		builder.Services.AddTransient<ProfilePage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
