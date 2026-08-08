using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
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

        // Muat Status Checkbox "Ingat Saya" & Email/Password Terimpan
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
                    // Tampilkan tombol biometrik jika ada data login tersimpan di SecureStorage
                    BiometricButton.IsVisible = true;
                }
            }
            catch (Exception)
            {
                // Penanganan jika SecureStorage tidak dapat diakses
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
            ShowError("Email dan Password tidak boleh kosong.");
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
                ShowError("Kredensial tersimpan tidak ditemukan. Silakan login manual.");
            }
        }
        else
        {
            ShowError("Otentikasi biometrik gagal atau dibatalkan.");
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

                // Simpan atau hapus kredensial berdasarkan checkbox "Ingat Saya"
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
                ShowError(string.IsNullOrWhiteSpace(message) ? "Email atau password salah." : message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Gagal terhubung ke server: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task<bool> AuthenticateWithBiometricsAsync()
    {
        // Integrasi dengan plugin biometrik seperti Plugin.Fingerprint jika digunakan
        await Task.Delay(300); // Simulasi panggilan biometrik
        return true;
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
