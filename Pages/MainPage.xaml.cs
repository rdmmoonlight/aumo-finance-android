using System.Diagnostics;
using System.Globalization;
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
            var data = await _apiService.GetDashboardAsync();

            if (data != null)
            {
                bool isClosedPeriod = data.IsClosed ||
                    (!string.IsNullOrEmpty(data.ActivePeriod) && data.ActivePeriod.Contains("CLOSED", StringComparison.OrdinalIgnoreCase));

                if (isClosedPeriod)
                {
                    string basePeriod = data.ActivePeriod?.Replace("(CLOSED)", "", StringComparison.OrdinalIgnoreCase).Trim() ?? "Periode";
                    TopHeader.PeriodText = $"{basePeriod} (CLOSED)";

                    CashLabel.Text = "-";
                    NetIncomeLabel.Text = "-";
                    RevenueLabel.Text = "-";
                    ExpenseLabel.Text = "-";
                }
                else
                {
                    TopHeader.PeriodText = string.IsNullOrWhiteSpace(data.ActivePeriod) ? "-" : data.ActivePeriod;

                    CashLabel.Text = data.TotalCash.ToString("C0", _idrCulture);
                    NetIncomeLabel.Text = data.NetIncome.ToString("C0", _idrCulture);
                    RevenueLabel.Text = data.Revenue.ToString("C0", _idrCulture);
                    ExpenseLabel.Text = data.Expenses.ToString("C0", _idrCulture);
                }
            }
            else
            {
                await DisplayAlert("Koneksi Gagal", "Gagal mengambil data dari server web.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadDashboardDataAsync error: {ex}");
            await DisplayAlert("Error", $"Terjadi kesalahan: {ex.Message}", "OK");
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

    public async Task<(bool success, string message)> ProcessNewTransactionAsync(CreateSimpleTransactionDto transactionDto)
    {
        try
        {
            var (success, message) = await _apiService.PostSimpleTransactionAsync(transactionDto);

            if (success)
            {
                await LoadDashboardDataAsync();
            }

            return (success, message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ProcessNewTransactionAsync error: {ex}");
            return (false, "Terjadi kesalahan di MainPage: " + ex.Message);
        }
    }
}
