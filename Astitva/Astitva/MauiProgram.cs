using Astitva.Services;
using Astitva.Shared.Services;

using AstitvaLibrary.DataAccess;

using Microsoft.Extensions.Logging;

using Plugin.LocalNotification;

using Syncfusion.Blazor;

namespace Astitva;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Secrets.SyncfusionLicense);

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseLocalNotification()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		// Add device-specific services used by the Astitva.Shared project
		builder.Services.AddSingleton<IFormFactor, FormFactor>();
		builder.Services.AddSingleton<ISaveAndViewService, SaveAndViewService>();
		builder.Services.AddSingleton<IUpdateService, UpdateService>();
		builder.Services.AddSingleton<IDataStorageService, DataStorageService>();
		builder.Services.AddSingleton<IVibrationService, VibrationService>();
		builder.Services.AddSingleton<ISoundService, SoundService>();
		builder.Services.AddScoped<Shared.Services.INotificationService, NotificationService>();

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSyncfusionBlazor();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
