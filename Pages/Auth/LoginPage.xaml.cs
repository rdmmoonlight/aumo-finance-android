using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Tampilkan crash terakhir (jika ada) — satu-satunya cara melihat detail
        // crash tanpa akses logcat/PC tools.
        var lastCrash = CrashLogger.ReadAndClearLastCrash();
        if (!string.IsNullOrWhiteSpace(lastCrash))
        {
            var snippet = lastCrash.Length > 900 ? lastCrash[..900] + "\n...(dipotong)" : lastCrash;
            await DisplayAlertAsync("Aplikasi Sempat Crash", snippet, "OK");
        }
    }

    private void OnTogglePasswordClicked(object? sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        TogglePasswordButton.Text = PasswordEntry.IsPassword ? "👁️" : "🙈";
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

        SetLoadingState(true);

        try
        {
            var (success, message, userId) = await _apiService.LoginAsync(username, password);

            if (success && userId != null)
            {
                Preferences.Default.Set("current_user_id", userId);
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ShowError(message);
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
    }
}
