using Astitva.Shared.Services;

using AstitvaLibrary.Data;
using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Exporting;
using AstitvaLibrary.Models;

namespace Astitva.Shared.Pages.Certificate;

public partial class DeathCertificatePage
{
	private UserModel _user;
	private DeathCertificateModel _deathCertificateModel = new();
	private List<MunicipalityModel> _municipalities = new();

	// Form state management
	private bool _isLoading = true;
	private bool _isSaving = false;
	private Dictionary<string, string> _validationErrors = new();
	private bool _showValidationSummary = false;

	// Voice control fields
	private bool _showVoiceDialog = false;
	private string _transcript = "";
	private DeathCertificateResponseModel _extractedDeathCertificate = new();

	protected override async Task OnInitializedAsync()
	{
		var authResult = await AuthService.ValidateUser(DataStorageService, NavigationManager, NotificationService, VibrationService);
		_user = authResult.User;
		await LoadData();
		await LoadExistingCertificate();

		_isLoading = false;
		StateHasChanged();
	}

	private async Task LoadData()
	{
		_municipalities = await CommonData.LoadTableDataByStatus<MunicipalityModel>(TableNames.Municipality);

		// Initialize default values
		_deathCertificateModel.DateOfDeath = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
		_deathCertificateModel.RegistrationDate = DateOnly.FromDateTime(DateTime.Now);
	}

	private async Task LoadExistingCertificate()
	{
		try
		{
			var existingCertificate = await DeathCertificateData.LoadDeathCertificateByUser(_user.Id);
			if (existingCertificate != null)
			{
				_deathCertificateModel = existingCertificate;
			}
		}
		catch (Exception)
		{
			// If no existing certificate found, continue with new form
		}
	}

	private bool ValidateForm()
	{
		_validationErrors.Clear();
		_showValidationSummary = false;

		// Required field validations
		if (string.IsNullOrWhiteSpace(_deathCertificateModel.FirstName))
			_validationErrors["FirstName"] = "First name is required";
		else if (_deathCertificateModel.FirstName.Length < 2)
			_validationErrors["FirstName"] = "First name must be at least 2 characters";
		else if (_deathCertificateModel.FirstName.Length > 50)
			_validationErrors["FirstName"] = "First name cannot exceed 50 characters";

		if (string.IsNullOrWhiteSpace(_deathCertificateModel.Sex))
			_validationErrors["Sex"] = "Sex is required";
		else if (!new[] { "Male", "Female", "Other" }.Contains(_deathCertificateModel.Sex))
			_validationErrors["Sex"] = "Please select a valid sex";

		if (_deathCertificateModel.DateOfDeath == default)
			_validationErrors["DateOfDeath"] = "Date of death is required";
		else if (_deathCertificateModel.DateOfDeath > DateOnly.FromDateTime(DateTime.Now))
			_validationErrors["DateOfDeath"] = "Date of death cannot be in the future";
		else if (_deathCertificateModel.DateOfDeath < DateOnly.FromDateTime(DateTime.Now.AddYears(-150)))
			_validationErrors["DateOfDeath"] = "Please enter a valid date of death";

		if (_deathCertificateModel.MunicipalityId <= 0)
			_validationErrors["MunicipalityId"] = "Please select a municipality";

		// Optional field validations (if provided)
		if (!string.IsNullOrWhiteSpace(_deathCertificateModel.MiddleName) && _deathCertificateModel.MiddleName.Length > 50)
			_validationErrors["MiddleName"] = "Middle name cannot exceed 50 characters";

		if (!string.IsNullOrWhiteSpace(_deathCertificateModel.LastName) && _deathCertificateModel.LastName.Length > 50)
			_validationErrors["LastName"] = "Last name cannot exceed 50 characters";

		if (!string.IsNullOrWhiteSpace(_deathCertificateModel.FatherName) && _deathCertificateModel.FatherName.Length > 100)
			_validationErrors["FatherName"] = "Father's name cannot exceed 100 characters";

		if (!string.IsNullOrWhiteSpace(_deathCertificateModel.MotherName) && _deathCertificateModel.MotherName.Length > 100)
			_validationErrors["MotherName"] = "Mother's name cannot exceed 100 characters";

		if (!string.IsNullOrWhiteSpace(_deathCertificateModel.DeathPlace) && _deathCertificateModel.DeathPlace.Length > 200)
			_validationErrors["DeathPlace"] = "Place of death cannot exceed 200 characters";

		if (!string.IsNullOrWhiteSpace(_deathCertificateModel.Address) && _deathCertificateModel.Address.Length > 500)
			_validationErrors["Address"] = "Address cannot exceed 500 characters";

		if (_validationErrors.Any())
		{
			_showValidationSummary = true;
			VibrationService.VibrateWithTime(300);
		}

		return !_validationErrors.Any();
	}

	private async Task SaveDetails()
	{
		if (_isSaving) return;

		if (!ValidateForm())
		{
			StateHasChanged();
			return;
		}

		_isSaving = true;
		StateHasChanged();

		// Set system fields
		_deathCertificateModel.UserId = _user.Id;
		_deathCertificateModel.Status = true;
		_deathCertificateModel.Approved = false;
		_deathCertificateModel.RegistrationDate = DateOnly.FromDateTime(DateTime.Now);

		// Clean optional fields
		_deathCertificateModel.MiddleName = string.IsNullOrWhiteSpace(_deathCertificateModel.MiddleName) ? null : _deathCertificateModel.MiddleName.Trim();
		_deathCertificateModel.LastName = string.IsNullOrWhiteSpace(_deathCertificateModel.LastName) ? null : _deathCertificateModel.LastName.Trim();
		_deathCertificateModel.FatherName = string.IsNullOrWhiteSpace(_deathCertificateModel.FatherName) ? null : _deathCertificateModel.FatherName.Trim();
		_deathCertificateModel.MotherName = string.IsNullOrWhiteSpace(_deathCertificateModel.MotherName) ? null : _deathCertificateModel.MotherName.Trim();
		_deathCertificateModel.DeathPlace = string.IsNullOrWhiteSpace(_deathCertificateModel.DeathPlace) ? null : _deathCertificateModel.DeathPlace.Trim();
		_deathCertificateModel.Address = string.IsNullOrWhiteSpace(_deathCertificateModel.Address) ? null : _deathCertificateModel.Address.Trim();

		await DeathCertificateData.InsertDeathCertificate(_deathCertificateModel);

		// Save to local storage for backup
		await DataStorageService.LocalSaveAsync(StorageFileNames.DeathCertificateFileName,
			System.Text.Json.JsonSerializer.Serialize(_deathCertificateModel));

		// Generate and save certificate
		await GenerateAndSaveCertificate();

		VibrationService.VibrateHapticClick();

		// Navigate back to home
		NavigationManager.NavigateTo("/");

		_isSaving = false;
		StateHasChanged();
	}

	private void CancelForm()
	{
		NavigationManager.NavigateTo("/");
	}

	private void ClearForm()
	{
		_deathCertificateModel = new DeathCertificateModel
		{
			DateOfDeath = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
			RegistrationDate = DateOnly.FromDateTime(DateTime.Now)
		};
		_validationErrors.Clear();
		_showValidationSummary = false;
		StateHasChanged();
	}

	private string GetValidationClass(string fieldName)
	{
		return _validationErrors.ContainsKey(fieldName) ? "has-error" : string.Empty;
	}

	private string GetValidationMessage(string fieldName)
	{
		return _validationErrors.TryGetValue(fieldName, out var message) ? message : string.Empty;
	}

	private string GetButtonContent()
	{
		if (_isSaving) return "Saving...";
		if (_deathCertificateModel.Id > 0) return "Update Certificate";
		return "Save Certificate";
	}

	private string GetButtonClass()
	{
		var baseClass = "save-button";
		if (_isSaving) return $"{baseClass} saving";
		return $"{baseClass} primary";
	}

	private async Task GenerateAndSaveCertificate()
	{
		var certificate = await DeathCertificateData.LoadDeathCertificateOverviewByUser(_deathCertificateModel.UserId);

		// Generate professional PDF certificate
		using var certificateStream = DeathCertificatePDFExport.GenerateDeathCertificate(certificate, _user);

		// Generate filename
		var fileName = DeathCertificatePDFExport.GenerateFileName(certificate, _user);

		// Save certificate as PDF
		await SaveAndViewService.SaveAndView(fileName, "application/pdf", certificateStream);
	}

	private async Task DownloadCertificate()
	{
		if (_deathCertificateModel.Id <= 0) return;

		var certificate = await DeathCertificateData.LoadDeathCertificateOverviewByUser(_deathCertificateModel.UserId);

		// Generate professional PDF certificate
		using var certificateStream = DeathCertificatePDFExport.GenerateDeathCertificate(certificate, _user);

		// Generate filename
		var fileName = DeathCertificatePDFExport.GenerateFileName(certificate, _user);

		// Save certificate as PDF
		await SaveAndViewService.SaveAndView(fileName, "application/pdf", certificateStream);

		VibrationService.VibrateHapticClick();
	}

	#region Voice Assistance
	private void CloseVoiceDialog() =>
		_showVoiceDialog = false;

	private void OnVoiceAssistantClick()
	{
		_showVoiceDialog = true;
		_transcript = "";
		_extractedDeathCertificate = new();
		StateHasChanged();
	}

	private async Task OnProcessTranscriptClick()
	{
		if (string.IsNullOrWhiteSpace(_transcript))
			return;

		try
		{
			_extractedDeathCertificate = await DeathCertificateAIProcessing.ExtractDeathCertificateDetailsFromTranscript(_transcript);

			if (_extractedDeathCertificate is null)
				return;

			// Apply extracted data to the form
			if (!string.IsNullOrEmpty(_extractedDeathCertificate.firstName))
				_deathCertificateModel.FirstName = _extractedDeathCertificate.firstName;

			if (!string.IsNullOrEmpty(_extractedDeathCertificate.middleName))
				_deathCertificateModel.MiddleName = _extractedDeathCertificate.middleName;

			if (!string.IsNullOrEmpty(_extractedDeathCertificate.lastName))
				_deathCertificateModel.LastName = _extractedDeathCertificate.lastName;

			if (!string.IsNullOrEmpty(_extractedDeathCertificate.sex))
				_deathCertificateModel.Sex = _extractedDeathCertificate.sex;

			if (!string.IsNullOrEmpty(_extractedDeathCertificate.dateOfDeath))
			{
				if (DateTime.TryParse(_extractedDeathCertificate.dateOfDeath, out DateTime parsedDate))
					_deathCertificateModel.DateOfDeath = DateOnly.FromDateTime(parsedDate);
			}

			if (!string.IsNullOrEmpty(_extractedDeathCertificate.deathPlace))
				_deathCertificateModel.DeathPlace = _extractedDeathCertificate.deathPlace;

			if (!string.IsNullOrEmpty(_extractedDeathCertificate.fatherName))
				_deathCertificateModel.FatherName = _extractedDeathCertificate.fatherName;

			if (!string.IsNullOrEmpty(_extractedDeathCertificate.motherName))
				_deathCertificateModel.MotherName = _extractedDeathCertificate.motherName;

			if (!string.IsNullOrEmpty(_extractedDeathCertificate.address))
				_deathCertificateModel.Address = _extractedDeathCertificate.address;

			// Clear validation errors after applying voice data
			_validationErrors.Clear();
			_showValidationSummary = false;

			_showVoiceDialog = false;
			VibrationService.VibrateHapticClick();
			StateHasChanged();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error extracting information: {ex.Message}");
		}
	}
	#endregion
}