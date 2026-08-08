using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using AumoFinance.Services;
using AumoFinance.Pages.Log;

namespace AumoFinance.Pages;

public partial class LoginPage : ContentPage
{
    // Glyph Unicode Material Icons: E8F4 = visibility, E8F5 = visibility_off
    private const string IconEyeVisible = "\uE8F4";
    private const string IconEyeHidden = "\uE8F5";

    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Pastikan Flyout drawer tetap terkunci saat berada di Halaman Login
        if (Shell.Current is Shell shell)
        {
            Shell.SetFlyoutBehavior(shell, FlyoutBehavior.Disabled);
        }

        string? lastCrash = CrashLogger.ReadAndClearLastCrash();
        if (!string.IsNullOrWhiteSpace(lastCrash))
        {
            await Navigation.PushModalAsync(new CrashLogPage(lastCrash));
        }
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
            ShowError("Email/Username and Password cannot be empty.");
            return;
        }

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

                // Kunci aman terhadap null reference
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
    }
}
