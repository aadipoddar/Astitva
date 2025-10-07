using System.Linq;

namespace Astitva.Shared.Pages.Authentication;

public partial class PhoneNumberPage
{
	private string _phoneNumber = "";
	private bool _isProcessing = false;
	private string _errorMessage = "";

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		await DataStorageService.SecureRemoveAll();
	}

	private string GetButtonContent()
	{
		if (_isProcessing)
			return "Processing...";
		return "Get OTP";
	}

	private string GetButtonClass()
	{
		var baseClass = "auth-button";
		if (_isProcessing)
			return $"{baseClass} processing";
		return $"{baseClass} primary";
	}

	private async Task GenerateOTP()
	{
		// Reset error message
		_errorMessage = "";

		// Validate phone number
		if (string.IsNullOrWhiteSpace(_phoneNumber))
		{
			_errorMessage = "Please enter a phone number";
			StateHasChanged();
			return;
		}

		if (_phoneNumber.Length != 10)
		{
			_errorMessage = "Phone number must be exactly 10 digits";
			StateHasChanged();
			return;
		}

		if (!_phoneNumber.All(char.IsDigit))
		{
			_errorMessage = "Phone number must contain only digits";
			StateHasChanged();
			return;
		}

		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();

			// Add a small delay to show processing state
			await Task.Delay(1500);

			NavigationManager.NavigateTo($"/otp/{_phoneNumber}");
		}
		catch (Exception)
		{
			_errorMessage = "An error occurred. Please try again.";
			_isProcessing = false;
			StateHasChanged();
		}
	}
}