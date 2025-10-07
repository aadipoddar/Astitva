using AstitvaLibrary.Data;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Astitva.Shared.Pages.Authentication;

public partial class OtpPage : IDisposable
{
	[Parameter] public string Number { get; set; } = string.Empty;

	private string[] _otpDigits = new string[6];
	private bool _isVerifying = false;
	private bool _isSuccess = false;
	private bool _hasError = false;
	private int _timeRemaining = 300; // 5 minutes in seconds
	private Timer _timer;
	private bool _canResend = false;
	private int _currentFocus = 0;

	protected override void OnInitialized()
	{
		if (string.IsNullOrWhiteSpace(Number))
		{
			NavigationManager.NavigateTo("/phone-number", replace: true);
			return;
		}

		// Start the countdown timer
		_timer = new Timer(UpdateTimer, null, 0, 1000);
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		await DataStorageService.SecureRemoveAll();
	}

	private async Task HandleInput(ChangeEventArgs e, int index)
	{
		var value = e.Value?.ToString() ?? "";

		// Only allow digits
		if (!string.IsNullOrEmpty(value) && !char.IsDigit(value[0]))
			return;

		_hasError = false;
		_otpDigits[index] = string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);

		// Move to next input if digit entered
		if (!string.IsNullOrEmpty(_otpDigits[index]) && index < 5)
		{
			_currentFocus = index + 1;
			await FocusNextInput(index + 1);
		}

		StateHasChanged();

		// Check if all digits are filled
		if (_otpDigits.All(d => !string.IsNullOrEmpty(d)))
		{
			await VerifyOTP();
		}
	}

	private async Task HandleKeyDown(KeyboardEventArgs e, int index)
	{
		if (e.Key == "Backspace" && string.IsNullOrEmpty(_otpDigits[index]) && index > 0)
		{
			_currentFocus = index - 1;
			_otpDigits[index - 1] = "";
			await FocusNextInput(index - 1);
			StateHasChanged();
		}
		else if (e.Key == "ArrowLeft" && index > 0)
		{
			_currentFocus = index - 1;
			await FocusNextInput(index - 1);
		}
		else if (e.Key == "ArrowRight" && index < 5)
		{
			_currentFocus = index + 1;
			await FocusNextInput(index + 1);
		}
	}

	private void HandleFocus(int index)
	{
		_currentFocus = index;
	}

	private async Task FocusNextInput(int index)
	{
		await Task.Delay(10); // Small delay to ensure DOM is updated
		await JSRuntime.InvokeVoidAsync("focusElement", $"otp-inputs input:nth-child({index + 1})");
	}

	private string GetDigitValue(int index)
	{
		return _otpDigits[index] ?? "";
	}

	private async Task VerifyOTP()
	{
		if (_isVerifying)
			return;

		var code = string.Join("", _otpDigits);
		if (code.Length != 6)
			return;

		_isVerifying = true;
		StateHasChanged();

		try
		{
			// Add a small delay for better UX
			await Task.Delay(1000);

			if (code != "000000")
			{
				_hasError = true;
				_isVerifying = false;
				ClearOTP();
				StateHasChanged();

				// Clear error after 3 seconds
				await Task.Delay(3000);
				_hasError = false;
				StateHasChanged();
				return;
			}

			var user = await UserData.LoadUserByNumber(Number);

			if (user is null)
			{
				await UserData.InsertUser(new()
				{
					Id = 0,
					Number = Number,
					Name = null,
					Status = true
				});

				// Reload the user after insertion to get the complete data
				user = await UserData.LoadUserByNumber(Number);
			}
			else if (!user.Status)
			{
				_hasError = true;
				_isVerifying = false;
				StateHasChanged();
				return;
			}

			if (user != null)
			{
				// Show success state
				_isSuccess = true;
				_isVerifying = false;
				StateHasChanged();

				// Save user data
				await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(user));
				VibrationService.VibrateWithTime(500);

				// Wait a bit to show success animation
				await Task.Delay(2500);

				NavigationManager.NavigateTo("/");
			}
		}
		catch (Exception)
		{
			// Handle any errors during authentication process
			_hasError = true;
			_isVerifying = false;
			ClearOTP();
			StateHasChanged();
		}
	}

	private void ClearOTP()
	{
		_otpDigits = new string[6];
		_currentFocus = 0;
	}

	private async Task ResendOTP()
	{
		if (_isVerifying || !_canResend)
			return;

		ClearOTP();
		_hasError = false;
		_canResend = false;
		_timeRemaining = 300; // Reset to 5 minutes

		// Restart the timer
		_timer?.Dispose();
		_timer = new Timer(UpdateTimer, null, 0, 1000);

		StateHasChanged();

		// Add haptic feedback
		VibrationService.VibrateHapticClick();

		// Simulate resend delay
		await Task.Delay(500);

		// In a real app, you would call an API to resend the OTP
		// For now, just show feedback that it was sent
	}

	private void GoBack()
	{
		if (_isVerifying)
			return;

		VibrationService.VibrateHapticClick();
		_timer?.Dispose();
		NavigationManager.NavigateTo("/phone-number", replace: true);
	}

	private void UpdateTimer(object state)
	{
		if (_timeRemaining > 0)
		{
			_timeRemaining--;
			InvokeAsync(StateHasChanged);
		}
		else
		{
			_canResend = true;
			_timer?.Dispose();
			InvokeAsync(StateHasChanged);
		}
	}

	private string GetFormattedTime()
	{
		var minutes = _timeRemaining / 60;
		var seconds = _timeRemaining % 60;
		return $"{minutes:D2}:{seconds:D2}";
	}

	public void Dispose()
	{
		_timer?.Dispose();
	}
}