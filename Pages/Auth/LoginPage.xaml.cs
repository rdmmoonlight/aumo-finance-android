using System;
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
    private const string KeySavedUsername = "saved_username";
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

        // Muat Status Checkbox "Ingat Saya" & Kredensial Terhitung
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
                string? savedUsername = await SecureStorage.Default.GetAsync(KeySavedUsername);
                string? savedPassword = await SecureStorage.Default.GetAsync(KeySavedPassword);

                if (!string.IsNullOrEmpty(savedUsername))
                {
                    UsernameEntry.Text = savedUsername;
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
                // Penanganan jika SecureStorage tidak didukung oleh perangkat
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
        string username = UsernameEntry.Text?.Trim() ?? string.Empty;
        string password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Email/Username dan Password tidak boleh kosong.");
            return;
        }

        await ProcessLoginAsync(username, password);
    }

    private async void OnBiometricButtonClicked(object? sender, EventArgs e)
    {
        // 1. Jalankan Verifikasi Biometrik Perangkat
        bool isAuthenticated = await AuthenticateWithBiometricsAsync();

        if (isAuthenticated)
        {
            string? savedUsername = await SecureStorage.Default.GetAsync(KeySavedUsername);
            string? savedPassword = await SecureStorage.Default.GetAsync(KeySavedPassword);

            if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
            {
                await ProcessLoginAsync(savedUsername, savedPassword);
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

    private async Task ProcessLoginAsync(string username, string password)
    {
        SetLoadingState(true);

        try
        {
            var (success, message, userId, fullName) = await _authService.LoginAsync(username, password);

            if (success && userId != null)
            {
                Preferences.Default.Set("current_user_id", userId);
                if (!string.IsNullOrEmpty(fullName))
                {
                    Preferences.Default.Set("current_user_name", fullName);
                }

                // Simpan atau Hapus Kredensial Berdasarkan Checkbox "Ingat Saya"
                if (RememberMeCheckBox.IsChecked)
                {
                    Preferences.Default.Set(KeyRememberMe, true);
                    await SecureStorage.Default.SetAsync(KeySavedUsername, username);
                    await SecureStorage.Default.SetAsync(KeySavedPassword, password);
                }
                else
                {
                    Preferences.Default.Remove(KeyRememberMe);
                    SecureStorage.Default.Remove(KeySavedUsername);
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
        // Jika menggunakan NuGet Plugin.Fingerprint / Plugin.Validation.Biometrics,
        // panggil fungsi otentikasi di sini. 
        // Contoh implementasi dummy/panggilan pustaka:
        
        /* 
        var result = await Plugin.Fingerprint.CrossFingerprint.Current.AuthenticateAsync(
            new Plugin.Fingerprint.Abstractions.AuthenticationRequestConfiguration(
                "Verifikasi Biometrik", "Pindai sidik jari atau wajah Anda untuk login"));
        return result.Authenticated;
        */

        await Task.Delay(300); // Simulasi jeda autentikasi
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
