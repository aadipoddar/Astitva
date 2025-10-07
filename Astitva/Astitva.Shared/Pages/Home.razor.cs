using Astitva.Shared.Services;

using AstitvaLibrary.Models.Common;

namespace Astitva.Shared.Pages;

public partial class Home
{
	private string factor => FormFactor.GetFormFactor();
	private string platform => FormFactor.GetPlatform();

	private UserModel _user;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		var authResult = await AuthService.ValidateUser(DataStorageService, NavigationManager, NotificationService, VibrationService);
		if (authResult.IsAuthenticated && authResult.User != null)
		{
			_user = authResult.User;
			StateHasChanged();
		}
		// If authentication failed, the user will be redirected to phone-number page by the AuthService
	}

	private async Task Logout() =>
		await AuthService.Logout(DataStorageService, NavigationManager, NotificationService, VibrationService);
}