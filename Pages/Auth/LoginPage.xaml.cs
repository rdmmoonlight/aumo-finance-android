using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using AumoFinance.Services;
using AumoFinance.Pages.Log;

namespace AumoFinance.Pages;

public partial class LoginPage : ContentPage
{
    private const string IconEyeVisible = "\uE8F4";
    private const string IconEyeHidden = "\uE8F5";

    private const string KeyRememberMe = "remember_me";
    private const string KeySavedEmail = "saved_email";
    private const string KeySavedPassword = "saved_password";

    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Shell.Current is Shell shell)
        {
            Shell.SetFlyoutBehavior(shell, FlyoutBehavior.Disabled);
        }

        string? lastCrash = CrashLogger.ReadAndClearLastCrash();
        if (!string.IsNullOrWhiteSpace(lastCrash))
        {
            await Navigation.PushModalAsync(new CrashLogPage(lastCrash));
        }

        // Load "Keep Me Signed In" status and saved credentials
        await LoadSavedCredentialsAsync();
    }

    private async Task LoadSavedCredentialsAsync()
    {
        bool isRemembered = Preferences.Default.Get(KeyRememberMe, false);
        RememberMeCheckBox.IsChecked = isRemembered;

        if (isRemembered)
        {
            try
            {
                string? savedEmail = await SecureStorage.Default.GetAsync(KeySavedEmail);
                string? savedPassword = await SecureStorage.Default.GetAsync(KeySavedPassword);

                if (!string.IsNullOrEmpty(savedEmail))
                {
                    EmailEntry.Text = savedEmail;
                }

                if (!string.IsNullOrEmpty(savedPassword))
                {
                    PasswordEntry.Text = savedPassword;
                    // Show biometric button only if saved credentials exist in SecureStorage
                    BiometricButton.IsVisible = true;
                }
            }
            catch (Exception)
            {
                // Handle cases where SecureStorage is unavailable on the device
            }
        }
    }

    private void OnRememberMeLabelTapped(object? sender, TappedEventArgs e)
    {
        RememberMeCheckBox.IsChecked = !RememberMeCheckBox.IsChecked;
    }

    private void OnTogglePasswordClicked(object? sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        TogglePasswordButton.Text = PasswordEntry.IsPassword ? IconEyeHidden : IconEyeVisible;
    }

    private async void OnLoginButtonClicked(object? sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim() ?? string.Empty;
        string password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Email and Password cannot be empty.");
            return;
        }

        await ProcessLoginAsync(email, password);
    }

    private async void OnBiometricButtonClicked(object? sender, EventArgs e)
    {
        bool isAuthenticated = await AuthenticateWithBiometricsAsync();

        if (isAuthenticated)
        {
            string? savedEmail = await SecureStorage.Default.GetAsync(KeySavedEmail);
            string? savedPassword = await SecureStorage.Default.GetAsync(KeySavedPassword);

            if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
            {
                await ProcessLoginAsync(savedEmail, savedPassword);
            }
            else
            {
                ShowError("Saved credentials not found. Please log in manually.");
            }
        }
    }

    private async Task<bool> AuthenticateWithBiometricsAsync()
    {
        try
        {
            // 1. Check if biometric hardware/permissions are available on device
            var isAvailable = await CrossFingerprint.Current.IsAvailableAsync();
            if (!isAvailable)
            {
                ShowError("Biometric authentication is not available on this device.");
                return false;
            }

            // 2. Configure system biometric prompt dialog
            var request = new AuthenticationRequestConfiguration(
                "Biometric Authentication",
                "Scan your fingerprint or face to sign in to AumoFinance")
            {
                CancelTitle = "Cancel",
                FallbackTitle = "Use Password"
            };

            // 3. Trigger OS native biometric prompt
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);

            if (result.Authenticated)
            {
                return true;
            }
            else if (result.Status == FingerprintAuthenticationResultStatus.Canceled)
            {
                // User explicitly canceled prompt; do not display error message
                return false;
            }
            else
            {
                ShowError("Biometric authentication failed. Please try again.");
                return false;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Biometric error: {ex.Message}");
            return false;
        }
    }

    private async Task ProcessLoginAsync(string email, string password)
    {
        SetLoadingState(true);

        try
        {
            var (success, message, userId, fullName) = await _authService.LoginAsync(email, password);

            if (success && userId != null)
            {
                Preferences.Default.Set("current_user_id", userId);
                if (!string.IsNullOrEmpty(fullName))
                {
                    Preferences.Default.Set("current_user_name", fullName);
                }

                // Save or clear encrypted credentials based on "Keep Me Signed In" checkbox
                if (RememberMeCheckBox.IsChecked)
                {
                    Preferences.Default.Set(KeyRememberMe, true);
                    await SecureStorage.Default.SetAsync(KeySavedEmail, email);
                    await SecureStorage.Default.SetAsync(KeySavedPassword, password);
                }
                else
                {
                    Preferences.Default.Remove(KeyRememberMe);
                    SecureStorage.Default.Remove(KeySavedEmail);
                    SecureStorage.Default.Remove(KeySavedPassword);
                }

                if (Shell.Current is Shell shell)
                {
                    Shell.SetFlyoutBehavior(shell, FlyoutBehavior.Flyout);
                    await shell.GoToAsync("//MainPage");
                }
            }
            else
            {
                ShowError(string.IsNullOrWhiteSpace(message) ? "Invalid email or password." : message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to connect to server: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorCard.IsVisible = true;
    }

    private void SetLoadingState(bool isLoading)
    {
        ErrorCard.IsVisible = false;
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoginButton.IsEnabled = !isLoading;
        LoginButton.IsVisible = !isLoading;
        BiometricButton.IsEnabled = !isLoading;
    }
}
