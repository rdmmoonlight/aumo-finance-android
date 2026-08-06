using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly CultureInfo _idrCulture = new("id-ID");

    public MainPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
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
            var rawData = await _apiService.GetDashboardAsync();

            if (rawData is JsonElement element)
            {
                bool isClosed = element.TryGetProperty("isClosed", out var isClosedProp) && isClosedProp.GetBoolean();
                string activePeriod = element.TryGetProperty("activePeriod", out var periodProp) ? periodProp.GetString() ?? "" : "";

                bool isClosedPeriod = isClosed ||
                    (!string.IsNullOrEmpty(activePeriod) && activePeriod.Contains("CLOSED", StringComparison.OrdinalIgnoreCase));

                if (isClosedPeriod)
                {
                    string basePeriod = activePeriod.Replace("(CLOSED)", "", StringComparison.OrdinalIgnoreCase).Trim();
                    TopHeader.PeriodText = string.IsNullOrEmpty(basePeriod) ? "Periode (CLOSED)" : $"{basePeriod} (CLOSED)";

                    CashLabel.Text = "-";
                    NetIncomeLabel.Text = "-";
                    RevenueLabel.Text = "-";
                    ExpenseLabel.Text = "-";
                }
                else
                {
                    TopHeader.PeriodText = string.IsNullOrWhiteSpace(activePeriod) ? "-" : activePeriod;

                    decimal totalCash = element.TryGetProperty("totalCash", out var p1) ? p1.GetDecimal() : 0m;
                    decimal netIncome = element.TryGetProperty("netIncome", out var p2) ? p2.GetDecimal() : 0m;
                    decimal revenue = element.TryGetProperty("revenue", out var p3) ? p3.GetDecimal() : 0m;
                    decimal expenses = element.TryGetProperty("expenses", out var p4) ? p4.GetDecimal() : 0m;

                    CashLabel.Text = totalCash.ToString("C0", _idrCulture);
                    NetIncomeLabel.Text = netIncome.ToString("C0", _idrCulture);
                    RevenueLabel.Text = revenue.ToString("C0", _idrCulture);
                    ExpenseLabel.Text = expenses.ToString("C0", _idrCulture);
                }
            }
            else
            {
                await DisplayAlertAsync("Koneksi Gagal", "Gagal mengambil data dari server web.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadDashboardDataAsync error: {ex}");
            await DisplayAlertAsync("Error", $"Terjadi kesalahan: {ex.Message}", "OK");
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
        await Navigation.PushAsync(new InputJournalPage());
    }
}
