using Astitva.Shared.Services;

using AstitvaLibrary.Data;
using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Exporting;
using AstitvaLibrary.Models;

namespace Astitva.Shared.Pages.Certificate;

public partial class BirthCertificatePage
{
	private UserModel _user;
	private BirthCertificateModel _birthCertificateModel = new();
	private List<MunicipalityModel> _municipalities = new();

	// Form state management
	private bool _isLoading = true;
	private bool _isSaving = false;
	private Dictionary<string, string> _validationErrors = new();
	private bool _showValidationSummary = false;

	// Form field states
	private bool _isFirstNameFocused = false;
	private bool _isMiddleNameFocused = false;
	private bool _isLastNameFocused = false;
	private bool _isFatherNameFocused = false;
	private bool _isMotherNameFocused = false;
	private bool _isSexFocused = false;
	private bool _isBirthPlaceFocused = false;
	private bool _isAddressFocused = false;

	// Voice control fields
	private bool _showVoiceDialog = false;
	private string _transcript = "";
	private BirthCertificateResponseModel _extractedBirthCertificate = new();

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
		_birthCertificateModel.DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
		_birthCertificateModel.RegistrationDate = DateOnly.FromDateTime(DateTime.Now);
	}

	private async Task LoadExistingCertificate()
	{
		try
		{
			var existingCertificate = await BirthCertificateData.LoadBirthCertificateByUser(_user.Id);
			if (existingCertificate != null)
			{
				_birthCertificateModel = existingCertificate;
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
		if (string.IsNullOrWhiteSpace(_birthCertificateModel.FirstName))
			_validationErrors["FirstName"] = "First name is required";
		else if (_birthCertificateModel.FirstName.Length < 2)
			_validationErrors["FirstName"] = "First name must be at least 2 characters";
		else if (_birthCertificateModel.FirstName.Length > 50)
			_validationErrors["FirstName"] = "First name cannot exceed 50 characters";

		if (string.IsNullOrWhiteSpace(_birthCertificateModel.Sex))
			_validationErrors["Sex"] = "Sex is required";
		else if (!new[] { "Male", "Female", "Other" }.Contains(_birthCertificateModel.Sex))
			_validationErrors["Sex"] = "Please select a valid sex";

		if (_birthCertificateModel.DateOfBirth == default)
			_validationErrors["DateOfBirth"] = "Date of birth is required";
		else if (_birthCertificateModel.DateOfBirth > DateOnly.FromDateTime(DateTime.Now))
			_validationErrors["DateOfBirth"] = "Date of birth cannot be in the future";
		else if (_birthCertificateModel.DateOfBirth < DateOnly.FromDateTime(DateTime.Now.AddYears(-150)))
			_validationErrors["DateOfBirth"] = "Please enter a valid date of birth";

		if (_birthCertificateModel.MunicipalityId <= 0)
			_validationErrors["MunicipalityId"] = "Please select a municipality";

		// Optional field validations (if provided)
		if (!string.IsNullOrWhiteSpace(_birthCertificateModel.MiddleName) && _birthCertificateModel.MiddleName.Length > 50)
			_validationErrors["MiddleName"] = "Middle name cannot exceed 50 characters";

		if (!string.IsNullOrWhiteSpace(_birthCertificateModel.LastName) && _birthCertificateModel.LastName.Length > 50)
			_validationErrors["LastName"] = "Last name cannot exceed 50 characters";

		if (!string.IsNullOrWhiteSpace(_birthCertificateModel.FatherName) && _birthCertificateModel.FatherName.Length > 100)
			_validationErrors["FatherName"] = "Father's name cannot exceed 100 characters";

		if (!string.IsNullOrWhiteSpace(_birthCertificateModel.MotherName) && _birthCertificateModel.MotherName.Length > 100)
			_validationErrors["MotherName"] = "Mother's name cannot exceed 100 characters";

		if (!string.IsNullOrWhiteSpace(_birthCertificateModel.BirthPlace) && _birthCertificateModel.BirthPlace.Length > 200)
			_validationErrors["BirthPlace"] = "Birth place cannot exceed 200 characters";

		if (!string.IsNullOrWhiteSpace(_birthCertificateModel.Address) && _birthCertificateModel.Address.Length > 500)
			_validationErrors["Address"] = "Address cannot exceed 500 characters";

		// Registration number validation (optional field)
		if (_birthCertificateModel.RegistrationNo.HasValue && _birthCertificateModel.RegistrationNo.Value <= 0)
			_validationErrors["RegistrationNo"] = "Registration number must be a positive number";

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
		_birthCertificateModel.UserId = _user.Id;
		_birthCertificateModel.Status = true;
		_birthCertificateModel.Approved = false;
		_birthCertificateModel.RegistrationDate = DateOnly.FromDateTime(DateTime.Now);

		// Clean optional fields
		_birthCertificateModel.MiddleName = string.IsNullOrWhiteSpace(_birthCertificateModel.MiddleName) ? null : _birthCertificateModel.MiddleName.Trim();
		_birthCertificateModel.LastName = string.IsNullOrWhiteSpace(_birthCertificateModel.LastName) ? null : _birthCertificateModel.LastName.Trim();
		_birthCertificateModel.FatherName = string.IsNullOrWhiteSpace(_birthCertificateModel.FatherName) ? null : _birthCertificateModel.FatherName.Trim();
		_birthCertificateModel.MotherName = string.IsNullOrWhiteSpace(_birthCertificateModel.MotherName) ? null : _birthCertificateModel.MotherName.Trim();
		_birthCertificateModel.BirthPlace = string.IsNullOrWhiteSpace(_birthCertificateModel.BirthPlace) ? null : _birthCertificateModel.BirthPlace.Trim();
		_birthCertificateModel.Address = string.IsNullOrWhiteSpace(_birthCertificateModel.Address) ? null : _birthCertificateModel.Address.Trim();

		await BirthCertificateData.InsertBirthCertificate(_birthCertificateModel);

		// Save to local storage for backup
		await DataStorageService.LocalSaveAsync(StorageFileNames.BirthCertificateFileName,
			System.Text.Json.JsonSerializer.Serialize(_birthCertificateModel));

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
		_birthCertificateModel = new BirthCertificateModel
		{
			DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-1)),
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
		if (_birthCertificateModel.Id > 0) return "Update Certificate";
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
		var birthCertificate = await BirthCertificateData.LoadBirthCertificateOverviewByUser(_birthCertificateModel.UserId);

		// Generate certificate content
		using var certificateStream = BirthCertificatePDFExport.GenerateBirthCertificate(birthCertificate, _user);

		// Generate filename
		var fileName = BirthCertificatePDFExport.GenerateFileName(birthCertificate, _user);

		// Save certificate
		await SaveAndViewService.SaveAndView(fileName, "text/plain", certificateStream);
	}

	private async Task DownloadCertificate()
	{
		if (_birthCertificateModel.Id <= 0) return;

		var birthCertificate = await BirthCertificateData.LoadBirthCertificateOverviewByUser(_birthCertificateModel.UserId);

		// Generate certificate content
		using var certificateStream = BirthCertificatePDFExport.GenerateBirthCertificate(birthCertificate, _user);

		// Generate filename
		var fileName = BirthCertificatePDFExport.GenerateFileName(birthCertificate, _user);

		// Save certificate
		await SaveAndViewService.SaveAndView(fileName, "text/plain", certificateStream);

		VibrationService.VibrateHapticClick();
	}

	#region Voice Assistance
	private void CloseVoiceDialog() =>
		_showVoiceDialog = false;

	private void OnVoiceAssistantClick()
	{
		_showVoiceDialog = true;
		_transcript = "";
		_extractedBirthCertificate = new();
		StateHasChanged();
	}

	private async Task OnProcessTranscriptClick()
	{
		if (string.IsNullOrWhiteSpace(_transcript))
			return;

		try
		{
			_extractedBirthCertificate = await BirthCertificateAIProcessing.ExtractBirthCertificateDetailsFromTranscript(_transcript);

			if (_extractedBirthCertificate is null)
				return;

			// Apply extracted data to the form
			if (!string.IsNullOrEmpty(_extractedBirthCertificate.firstName))
				_birthCertificateModel.FirstName = _extractedBirthCertificate.firstName;

			if (!string.IsNullOrEmpty(_extractedBirthCertificate.middleName))
				_birthCertificateModel.MiddleName = _extractedBirthCertificate.middleName;

			if (!string.IsNullOrEmpty(_extractedBirthCertificate.lastName))
				_birthCertificateModel.LastName = _extractedBirthCertificate.lastName;

			if (!string.IsNullOrEmpty(_extractedBirthCertificate.sex))
				_birthCertificateModel.Sex = _extractedBirthCertificate.sex;

			if (!string.IsNullOrEmpty(_extractedBirthCertificate.dateOfBirth))
			{
				if (DateTime.TryParse(_extractedBirthCertificate.dateOfBirth, out DateTime parsedDate))
					_birthCertificateModel.DateOfBirth = DateOnly.FromDateTime(parsedDate);
			}

			if (!string.IsNullOrEmpty(_extractedBirthCertificate.birthPlace))
				_birthCertificateModel.BirthPlace = _extractedBirthCertificate.birthPlace;

			if (!string.IsNullOrEmpty(_extractedBirthCertificate.fatherName))
				_birthCertificateModel.FatherName = _extractedBirthCertificate.fatherName;

			if (!string.IsNullOrEmpty(_extractedBirthCertificate.motherName))
				_birthCertificateModel.MotherName = _extractedBirthCertificate.motherName;

			if (!string.IsNullOrEmpty(_extractedBirthCertificate.address))
				_birthCertificateModel.Address = _extractedBirthCertificate.address;

			if (_extractedBirthCertificate.registrationNo.HasValue)
				_birthCertificateModel.RegistrationNo = _extractedBirthCertificate.registrationNo;

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