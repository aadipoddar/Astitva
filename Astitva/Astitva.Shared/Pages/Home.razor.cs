
using Astitva.Shared.Services;

using AstitvaLibrary.Data;
using AstitvaLibrary.Exporting;
using AstitvaLibrary.Models;

namespace Astitva.Shared.Pages;

public partial class Home
{
	private string factor => FormFactor.GetFormFactor();
	private string platform => FormFactor.GetPlatform();

	private UserModel _user;
	private BirthCertificateOverviewModel _birthCertificate;
	private DeathCertificateOverviewModel _deathCertificate;
	private bool _isLoading = true;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		var authResult = await AuthService.ValidateUser(DataStorageService, NavigationManager, NotificationService, VibrationService);
		_user = authResult.User;

		if (_user is null)
			return;

		await LoadData();
		_isLoading = false;
		StateHasChanged();
	}

	private async Task LoadData()
	{
		try
		{
			_birthCertificate = await BirthCertificateData.LoadBirthCertificateOverviewByUser(_user.Id);
			_deathCertificate = await DeathCertificateData.LoadDeathCertificateOverviewByUser(_user.Id);

			if (_birthCertificate is not null)
				await DataStorageService.LocalSaveAsync(StorageFileNames.BirthCertificateFileName, System.Text.Json.JsonSerializer.Serialize(_birthCertificate));

			if (_deathCertificate is not null)
				await DataStorageService.LocalSaveAsync(StorageFileNames.DeathCertificateFileName, System.Text.Json.JsonSerializer.Serialize(_deathCertificate));
		}
		catch (Exception ex)
		{
			if (await DataStorageService.LocalExists(StorageFileNames.BirthCertificateFileName))
				_birthCertificate = System.Text.Json.JsonSerializer.Deserialize<BirthCertificateOverviewModel>(await DataStorageService.LocalGetAsync(StorageFileNames.BirthCertificateFileName));

			if (await DataStorageService.LocalExists(StorageFileNames.DeathCertificateFileName))
				_deathCertificate = System.Text.Json.JsonSerializer.Deserialize<DeathCertificateOverviewModel>(await DataStorageService.LocalGetAsync(StorageFileNames.DeathCertificateFileName));
		}
	}

	private bool HasCertificates => _birthCertificate is not null || _deathCertificate is not null;

	private async Task DownloadBirthCertificate()
	{
		if (_birthCertificate is null) return;

		// Generate professional PDF certificate
		using var certificateStream = BirthCertificatePDFExport.GenerateBirthCertificate(_birthCertificate, _user);

		// Generate filename
		var fileName = BirthCertificatePDFExport.GenerateFileName(_birthCertificate, _user);

		// Save certificate as PDF
		await SaveAndViewService.SaveAndView(fileName, "application/pdf", certificateStream);

		VibrationService.VibrateWithTime(200);
	}

	private async Task DownloadDeathCertificate()
	{
		if (_deathCertificate is null) return;

		// Generate professional PDF certificate
		using var certificateStream = DeathCertificatePDFExport.GenerateDeathCertificate(_deathCertificate, _user);

		// Generate filename
		var fileName = DeathCertificatePDFExport.GenerateFileName(_deathCertificate, _user);

		// Save certificate as PDF
		await SaveAndViewService.SaveAndView(fileName, "application/pdf", certificateStream);

		VibrationService.VibrateWithTime(200);
	}

	private void NavigateToBirthCertificate() => NavigationManager.NavigateTo("/certificate/birth");
	private void NavigateToDeathCertificate() => NavigationManager.NavigateTo("/certificate/death");

	private async Task Logout() =>
		await AuthService.Logout(DataStorageService, NavigationManager, NotificationService, VibrationService);
}