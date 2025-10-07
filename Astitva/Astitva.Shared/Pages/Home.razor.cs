
using Astitva.Shared.Services;

using AstitvaLibrary.Data;
using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Models;
using AstitvaLibrary.Exporting;

namespace Astitva.Shared.Pages;

public partial class Home
{
	private string factor => FormFactor.GetFormFactor();
	private string platform => FormFactor.GetPlatform();

	private UserModel _user;
	private BirthCertificateModel _birthCertificate;
	private DeathCertificateModel _deathCertificate;
	private bool _isLoading = true;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		var authResult = await AuthService.ValidateUser(DataStorageService, NavigationManager, NotificationService, VibrationService);
		_user = authResult.User;

		await LoadData();
		_isLoading = false;
		StateHasChanged();
	}

	private async Task LoadData()
	{
		try
		{
			_birthCertificate = await BirthCertificateData.LoadBirthCertificateByUser(_user.Id);
			_deathCertificate = await DeathCertificateData.LoadDeathCertificateByUser(_user.Id);

			if (_birthCertificate is not null)
				await DataStorageService.LocalSaveAsync(StorageFileNames.BirthCertificateFileName, System.Text.Json.JsonSerializer.Serialize(_birthCertificate));

			if (_deathCertificate is not null)
				await DataStorageService.LocalSaveAsync(StorageFileNames.DeathCertificateFileName, System.Text.Json.JsonSerializer.Serialize(_deathCertificate));
		}
		catch (Exception ex)
		{
			await NotificationService.ShowLocalNotification(1, "Error", "Loading Certificates", $"Error loading certificates: {ex.Message}");
		}
	}

	private bool HasCertificates => _birthCertificate is not null || _deathCertificate is not null;

	private async Task DownloadBirthCertificate()
	{
		if (_birthCertificate is null) return;

		try
		{
			await NotificationService.ShowLocalNotification(2, "Download", "Birth Certificate", "Generating professional birth certificate PDF...");

			// Get municipality details
			var municipality = await CommonData.LoadTableDataById<MunicipalityModel>(TableNames.Municipality, _birthCertificate.MunicipalityId);

			// Generate professional PDF certificate
			using var certificateStream = BirthCertificatePDFExport.GenerateBirthCertificate(_birthCertificate, municipality, _user);

			// Generate filename
			var fileName = BirthCertificatePDFExport.GenerateFileName(_birthCertificate, _user);

			// Save certificate as PDF
			await SaveAndViewService.SaveAndView(fileName, "application/pdf", certificateStream);

			VibrationService.VibrateWithTime(200);
			await NotificationService.ShowLocalNotification(6, "Success", "Download Complete", "Professional birth certificate PDF downloaded successfully!");
		}
		catch (Exception ex)
		{
			await NotificationService.ShowLocalNotification(3, "Error", "Download Failed", $"Failed to download: {ex.Message}");
		}
	}

	private async Task DownloadDeathCertificate()
	{
		if (_deathCertificate is null) return;

		try
		{
			await NotificationService.ShowLocalNotification(4, "Download", "Death Certificate", "Generating professional death certificate PDF...");

			// Get municipality details
			var municipality = await CommonData.LoadTableDataById<MunicipalityModel>(TableNames.Municipality, _deathCertificate.MunicipalityId);

			// Generate professional PDF certificate
			using var certificateStream = DeathCertificatePDFExport.GenerateDeathCertificate(_deathCertificate, municipality, _user);

			// Generate filename
			var fileName = DeathCertificatePDFExport.GenerateFileName(_deathCertificate, _user);

			// Save certificate as PDF
			await SaveAndViewService.SaveAndView(fileName, "application/pdf", certificateStream);

			VibrationService.VibrateWithTime(200);
			await NotificationService.ShowLocalNotification(7, "Success", "Download Complete", "Professional death certificate PDF downloaded successfully!");
		}
		catch (Exception ex)
		{
			await NotificationService.ShowLocalNotification(5, "Error", "Download Failed", $"Failed to download: {ex.Message}");
		}
	}



	private void NavigateToBirthCertificate() => NavigationManager.NavigateTo("/certificate/birth");
	private void NavigateToDeathCertificate() => NavigationManager.NavigateTo("/certificate/death");

	private async Task Logout() =>
		await AuthService.Logout(DataStorageService, NavigationManager, NotificationService, VibrationService);
}