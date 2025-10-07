
using Astitva.Shared.Services;

using AstitvaLibrary.Data;
using AstitvaLibrary.Models;

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
			await NotificationService.ShowLocalNotification(2, "Download", "Birth Certificate", "Downloading birth certificate...");

			// Generate certificate content
			var certificateContent = GenerateBirthCertificateContent();
			var certificateBytes = System.Text.Encoding.UTF8.GetBytes(certificateContent);

			using var memoryStream = new MemoryStream(certificateBytes);
			var fileName = $"BirthCertificate_{_birthCertificate.FirstName}_{_birthCertificate.RegistrationNo}.txt";

			// For now we'll save as text, but you can integrate with a PDF library later
			await SaveAndViewService.SaveAndView(fileName, "text/plain", memoryStream);

			VibrationService.VibrateWithTime(200);
			await NotificationService.ShowLocalNotification(6, "Success", "Download Complete", "Birth certificate downloaded successfully!");
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
			await NotificationService.ShowLocalNotification(4, "Download", "Death Certificate", "Downloading death certificate...");

			// Generate certificate content
			var certificateContent = GenerateDeathCertificateContent();
			var certificateBytes = System.Text.Encoding.UTF8.GetBytes(certificateContent);

			using var memoryStream = new MemoryStream(certificateBytes);
			var fileName = $"DeathCertificate_{_deathCertificate.FirstName}_{_deathCertificate.RegistrationNo}.txt";

			// For now we'll save as text, but you can integrate with a PDF library later
			await SaveAndViewService.SaveAndView(fileName, "text/plain", memoryStream);

			VibrationService.VibrateWithTime(200);
			await NotificationService.ShowLocalNotification(7, "Success", "Download Complete", "Death certificate downloaded successfully!");
		}
		catch (Exception ex)
		{
			await NotificationService.ShowLocalNotification(5, "Error", "Download Failed", $"Failed to download: {ex.Message}");
		}
	}

	private string GenerateBirthCertificateContent()
	{
		return $@"
BIRTH CERTIFICATE
==================

Registration No: {_birthCertificate.RegistrationNo}
Registration Date: {_birthCertificate.RegistrationDate:dd MMMM yyyy}

PERSONAL INFORMATION
--------------------
Full Name: {GetFullName(_birthCertificate.FirstName, _birthCertificate.MiddleName, _birthCertificate.LastName)}
Date of Birth: {_birthCertificate.DateOfBirth:dd MMMM yyyy}
Sex: {_birthCertificate.Sex}

PARENT INFORMATION
------------------
Father's Name: {_birthCertificate.FatherName ?? "Not specified"}
Mother's Name: {_birthCertificate.MotherName ?? "Not specified"}

REGISTRATION DETAILS
--------------------
Municipality ID: {_birthCertificate.MunicipalityId}
Status: {(_birthCertificate.Status ? "Active" : "Inactive")}

Generated on: {DateTime.Now:dd MMMM yyyy HH:mm:ss}
";
	}

	private string GenerateDeathCertificateContent()
	{
		return $@"
DEATH CERTIFICATE
=================

Registration No: {_deathCertificate.RegistrationNo?.ToString() ?? "Not assigned"}
Registration Date: {_deathCertificate.RegistrationDate:dd MMMM yyyy}

PERSONAL INFORMATION
--------------------
Full Name: {GetFullName(_deathCertificate.FirstName, _deathCertificate.MiddleName, _deathCertificate.LastName)}
Date of Death: {_deathCertificate.DateOfDeath:dd MMMM yyyy}
Sex: {_deathCertificate.Sex}

PARENT INFORMATION
------------------
Father's Name: {_deathCertificate.FatherName ?? "Not specified"}
Mother's Name: {_deathCertificate.MotherName ?? "Not specified"}

DEATH DETAILS
-------------
Place of Death: {_deathCertificate.DeathPlace ?? "Not specified"}
Address: {_deathCertificate.Address ?? "Not specified"}

REGISTRATION DETAILS
--------------------
Municipality ID: {_deathCertificate.MunicipalityId}
User ID: {_deathCertificate.UserId}
Approved: {(_deathCertificate.Approved ? "Yes" : "No")}
Status: {(_deathCertificate.Status ? "Active" : "Inactive")}

Generated on: {DateTime.Now:dd MMMM yyyy HH:mm:ss}
";
	}

	private void NavigateToBirthCertificate() => NavigationManager.NavigateTo("/birth-certificate");
	private void NavigateToDeathCertificate() => NavigationManager.NavigateTo("/death-certificate");

	private async Task Logout() =>
		await AuthService.Logout(DataStorageService, NavigationManager, NotificationService, VibrationService);
}