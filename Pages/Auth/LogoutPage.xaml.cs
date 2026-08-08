using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace AumoFinance.Pages;

public partial class LogoutPage : ContentPage
{
    public LogoutPage()
    {
        InitializeComponent();
    }

    private async void OnLogoutButtonClicked(object? sender, EventArgs e)
    {
        SetLoadingState(true);

        try
        {
            // 1. Hapus Data Sesi User dari Preferences
            Preferences.Default.Remove("current_user_id");
            Preferences.Default.Remove("current_user_name");

            // Optional: Hapus seluruh preferences jika ingin reset total saat logout
            // Preferences.Default.Clear();

            // 2. Kunci Kembali Flyout Menu Drawer (Agar tidak bisa di-swipe/dibuka)
            if (Shell.Current != null)
            {
                Shell.SetFlyoutBehavior(Shell.Current, FlyoutBehavior.Disabled);
            }

            // 3. Arahkan User Kembali ke LoginPage & Reset Tumpukan Navigasi
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal melakukan logout: {ex.Message}", "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async void OnCancelButtonClicked(object? sender, EventArgs e)
    {
        // Kembali ke halaman sebelumnya
        await Shell.Current.GoToAsync("..");
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LogoutButton.IsEnabled = !isLoading;
        LogoutButton.IsVisible = !isLoading;
        CancelButton.IsEnabled = !isLoading;
    }
}
