using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using AumoFinance.Services;
using AumoFinance.Pages.JournalEntry;

namespace AumoFinance.Pages.Dashboard;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardService _dashboardService;
    // Menggunakan kultur Indonesia
    private readonly CultureInfo _idCulture = new("id-ID");

    public DashboardPage(DashboardService dashboardService)
    {
        InitializeComponent();
        _dashboardService = dashboardService;

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
                PeriodText.Text = string.IsNullOrWhiteSpace(data.SelectedPeriodName)
                    ? "No Period Selected"
                    : data.SelectedPeriodName;

                // Format ke Rupiah tanpa desimal (N0) dengan simbol "Rp " di depannya
                CashLabel.Text = string.Format(_idCulture, "Rp {0:N0}", data.TotalAssets);
                NetIncomeLabel.Text = string.Format(_idCulture, "Rp {0:N0}", data.NetIncome);
                RevenueLabel.Text = string.Format(_idCulture, "Rp {0:N0}", data.TotalRevenue);
                ExpenseLabel.Text = string.Format(_idCulture, "Rp {0:N0}", data.TotalExpenses);
            }
            else
            {
                string detail = string.IsNullOrWhiteSpace(errorDetail) ? "Unknown error occurred." : errorDetail;
                await this.DisplayAlert("Connection Failed", $"Failed to retrieve dashboard data from the server.\n\nDetails: {detail}", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadDashboardDataAsync error: {ex}");
            await this.DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
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
            await this.DisplayAlert("Error", "Failed to check for updates.", "OK");
        }
    }
}
