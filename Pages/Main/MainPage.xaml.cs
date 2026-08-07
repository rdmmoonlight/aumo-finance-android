using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using AumoFinance.Services;
using AumoFinance.Pages.JournalEntry;

namespace AumoFinance.Pages;

public partial class MainPage : ContentPage
{
    private readonly DashboardService _dashboardService;
    private readonly CultureInfo _usdCulture = new("en-US");

    public MainPage(DashboardService dashboardService)
    {
        InitializeComponent();
        _dashboardService = dashboardService;

        // Load Auto-Update switch state from Preferences (default: true)
        AutoUpdateSwitch.IsToggled = Preferences.Default.Get("AutoUpdateEnabled", true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        DashboardContent.IsVisible = false;

        try
        {
            var (data, errorDetail) = await _dashboardService.GetDashboardAsync();

            if (data != null && data.Success)
            {
                // Update Period Header in TopBarView
                TopHeader.PeriodText = string.IsNullOrWhiteSpace(data.SelectedPeriodName)
                    ? "No Period Selected"
                    : data.SelectedPeriodName;

                // Format Financial Figures in US Currency ($#,##0.00)
                CashLabel.Text = data.TotalAssets.ToString("C2", _usdCulture);
                NetIncomeLabel.Text = data.NetIncome.ToString("C2", _usdCulture);
                RevenueLabel.Text = data.TotalRevenue.ToString("C2", _usdCulture);
                ExpenseLabel.Text = data.TotalExpenses.ToString("C2", _usdCulture);
            }
            else
            {
                string detail = string.IsNullOrWhiteSpace(errorDetail) ? "Unknown error occurred." : errorDetail;
                await this.DisplayAlertAsync("Connection Failed", $"Failed to retrieve dashboard data from the server.\n\nDetails: {detail}", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadDashboardDataAsync error: {ex}");
            await this.DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            DashboardContent.IsVisible = true;
        }
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadDashboardDataAsync();
    }

    private async void OnPrimaryFabClicked(object? sender, EventArgs e)
    {
        // Menggunakan ServiceProvider MAUI agar Dependency Injection terinjeksi sempurna
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
