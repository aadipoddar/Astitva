using Plugin.LocalNotification;

namespace Astitva.Services;

public class NotificationService : Shared.Services.INotificationService
{
	public async Task DeregisterDevicePushNotification()
	{
	}

	public async Task RegisterDevicePushNotification(string tag)
	{
	}

	public async Task ShowLocalNotification(int id, string title, string subTitle, string description)
	{
		if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
			await LocalNotificationCenter.Current.RequestNotificationPermission();

		var request = new NotificationRequest
		{
			NotificationId = id,
			Title = title,
			Subtitle = subTitle,
			Description = description,
			Schedule = new NotificationRequestSchedule
			{
				NotifyTime = DateTime.Now.AddSeconds(5)
			}
		};

		await LocalNotificationCenter.Current.Show(request);
	}
}
