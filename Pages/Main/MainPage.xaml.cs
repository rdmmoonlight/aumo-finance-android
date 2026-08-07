using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using AumoFinance.Services;
using AumoFinance.Pages.JournalEntry;
using AumoFinance.Pages.Dashboard;

namespace AumoFinance.Pages;

public partial class MainPage : ContentPage
{
    private readonly DashboardService _dashboardService;
    // Mengubah CultureInfo menjadi Indonesia (id-ID)
    private readonly CultureInfo _idCulture = new("id-ID");

    public MainPage(DashboardService dashboardService)
    {
        InitializeComponent();
        _dashboardService = dashboardService;

        AutoUpdateSwitch.IsToggled = Preferences.Default.Get("AutoUpdateEnabled", true);
        SetTimeBasedGreeting();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHomeDataAsync();
    }

    private void SetTimeBasedGreeting()
    {
        var hour = DateTime.Now.Hour;
        if (hour < 12)
            GreetingLabel.Text = "Good Morning,";
        else if (hour < 17)
            GreetingLabel.Text = "Good Afternoon,";
        else
            GreetingLabel.Text = "Good Evening,";
    }

    private async Task LoadHomeDataAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        HomeContent.IsVisible = false;

        try
        {
            var (data, errorDetail) = await _dashboardService.GetDashboardAsync();

            if (data != null && data.Success)
            {
                // Format angka menggunakan format Rupiah (id-ID)
                // Opsi 1: "C0" untuk format Rp tanpa sen (misal: Rp150.000)
                CashLabel.Text = data.TotalAssets.ToString("C0", _idCulture);

                // Opsi 2 (Opsional): Jika ingin memakai sen/desimal (misal: Rp150.000,00), 
                // ubah "C0" di atas menjadi "C2"

                TopHeader.PeriodText = string.IsNullOrWhiteSpace(data.SelectedPeriodName)
                    ? "Welcome to AumoFinance"
                    : data.SelectedPeriodName;
            }
            else
            {
                string detail = string.IsNullOrWhiteSpace(errorDetail) ? "An unknown error occurred." : errorDetail;
                await this.DisplayAlertAsync("Connection Failed", $"Failed to retrieve balance data.\n\nDetails: {detail}", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadHomeDataAsync error: {ex}");
            await this.DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            HomeContent.IsVisible = true;
        }
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadHomeDataAsync();
    }

    private async void OnDashboardFabClicked(object? sender, EventArgs e)
    {
        var dashboardPage = Handler?.MauiContext?.Services.GetService<DashboardPage>();
        if (dashboardPage != null)
        {
            await Navigation.PushAsync(dashboardPage);
        }
    }

    private async void OnPrimaryFabClicked(object? sender, EventArgs e)
    {
        var journalEntryPage = Handler?.MauiContext?.Services.GetService<JournalEntryPage>();
        if (journalEntryPage != null)
        {
            await Navigation.PushAsync(journalEntryPage);
        }
    }

    private void OnAutoUpdateToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("AutoUpdateEnabled", e.Value);
    }

    private async void OnCheckUpdateManualClicked(object? sender, EventArgs e)
    {
        try
        {
            var updateService = new UpdateService();
            await updateService.CheckAndInstallUpdateAsync("rdmmoonlight", "aumo-finance-android", isSilent: false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Manual update check error: {ex}");
            await this.DisplayAlertAsync("Error", "Failed to check for updates.", "OK");
        }
    }
}
