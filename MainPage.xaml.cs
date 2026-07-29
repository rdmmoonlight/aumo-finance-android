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
        UpdateServerLabel();
        await LoadDashboardDataAsync();
    }

    // Mengubah tampilan indikator server di header XAML
    private void UpdateServerLabel()
    {
        var currentUrl = ApiService.CurrentBaseUrl;
        
        if (currentUrl.Contains("aumo-preview"))
        {
            ServerLabel.Text = "Server: Preview";
            ServerLabel.TextColor = Color.FromArgb("#F59E0B"); // Warna Orange
        }
        else if (currentUrl.Contains("aumo.up"))
        {
            ServerLabel.Text = "Server: Production";
            ServerLabel.TextColor = Color.FromArgb("#38BDF8"); // Warna Biru
        }
        else
        {
            ServerLabel.Text = "Server: Custom";
            ServerLabel.TextColor = Color.FromArgb("#A855F7"); // Warna Ungu
        }
    }

    // Ambil data dari Backend
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
            await DisplayAlert("Koneksi Gagal", "Gagal mengambil data dari server web aktif.", "OK");
        }
    }

    // Handler Tombol Refresh
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadDashboardDataAsync();
    }

    // Handler Tombol Switch Environment (⚙️)
    private async void OnSwitchEnvironmentClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet(
            "Pilih Backend Server:",
            "Batal",
            null,
            "1. Production (aumo.up.railway.app)",
            "2. Preview (aumo-preview.up.railway.app)",
            "3. Custom URL..."
        );

        if (action == null || action == "Batal") return;

        if (action.StartsWith("1."))
        {
            ApiService.CurrentBaseUrl = ApiService.UrlProduction;
        }
        else if (action.StartsWith("2."))
        {
            ApiService.CurrentBaseUrl = ApiService.UrlPreview;
        }
        else if (action.StartsWith("3."))
        {
            string custom = await DisplayPromptAsync("Custom Backend", "Masukkan Base URL:", initialValue: ApiService.CurrentBaseUrl);
            if (!string.IsNullOrWhiteSpace(custom))
            {
                ApiService.CurrentBaseUrl = custom.Trim();
            }
        }

        UpdateServerLabel();
        await LoadDashboardDataAsync();
    }

    // Handler Tombol Input Jurnal Baru
    private async void OnInputJournalClicked(object sender, EventArgs e)
    {
        // Tempat navigasi ke Halaman 2 (Input Jurnal)
        await DisplayAlert("Fitur", "Navigasi ke Halaman Input Jurnal", "OK");
    }
}
