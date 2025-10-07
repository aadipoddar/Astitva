using AstitvaLibrary.Data;
using AstitvaLibrary.Models;

using Microsoft.AspNetCore.Components;

namespace Astitva.Shared.Services;

public static class AuthService
{
	public static async Task<AuthenticationResult> ValidateUser(IDataStorageService dataStorageService, NavigationManager navigationManager, INotificationService notificationService, IVibrationService vibrationService)
	{
		var userData = await dataStorageService.SecureGetAsync(StorageFileNames.UserDataFileName);
		if (string.IsNullOrEmpty(userData))
		{
			await Logout(dataStorageService, navigationManager, notificationService, vibrationService);
			return new AuthenticationResult(false, null, "No user data found");
		}

		try
		{
			var user = System.Text.Json.JsonSerializer.Deserialize<UserModel>(userData);
			if (user is null)
			{
				await Logout(dataStorageService, navigationManager, notificationService, vibrationService);
				return new AuthenticationResult(false, null, "Invalid user data");
			}

			if (!user.Status)
			{
				await Logout(dataStorageService, navigationManager, notificationService, vibrationService);
				return new AuthenticationResult(false, null, "User account is inactive");
			}

			return new AuthenticationResult(true, user, null);
		}
		catch (System.Text.Json.JsonException)
		{
			await Logout(dataStorageService, navigationManager, notificationService, vibrationService);
			return new AuthenticationResult(false, null, "Corrupted user data");
		}
	}

	public static async Task Logout(IDataStorageService dataStorageService, NavigationManager navigationManager, INotificationService notificationService, IVibrationService vibrationService)
	{
		await dataStorageService.SecureRemoveAll();
		await notificationService.DeregisterDevicePushNotification();
		vibrationService.VibrateWithTime(500);
		navigationManager.NavigateTo("/phone-number", forceLoad: true);
	}
}
