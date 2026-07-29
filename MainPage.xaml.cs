using System.Globalization;
using Aumo.Services;

namespace Aumo;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService = new();
    private readonly CultureInfo _idrCulture = new("id-ID");

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        var data = await _apiService.GetDashboardAsync();

        if (data != null)
        {
            PeriodLabel.Text = $"Periode: {data.ActivePeriod}";
            CashLabel.Text = data.TotalCash.ToString("C0", _idrCulture);
            NetIncomeLabel.Text = data.NetIncome.ToString("C0", _idrCulture);
            RevenueLabel.Text = data.Revenue.ToString("C0", _idrCulture);
            ExpenseLabel.Text = data.Expenses.ToString("C0", _idrCulture);
        }
        else
        {
            await DisplayAlert("Koneksi Gagal", "Gagal mengambil data dari server web.", "OK");
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadDashboardDataAsync();
    }

    private async void OnInputJournalClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Fitur", "Navigasi ke Halaman Input Jurnal", "OK");
    }
}
