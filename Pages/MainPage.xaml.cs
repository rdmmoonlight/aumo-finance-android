using System.Globalization;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance.Pages;

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
        try
        {
            var data = await _apiService.GetDashboardAsync();

            if (data != null)
            {
                TopHeader.PeriodText = data.ActivePeriod ?? "-";
                
                CashLabel.Text = data.TotalCash.ToString("C0", _idrCulture);
                NetIncomeLabel.Text = data.NetIncome.ToString("C0", _idrCulture);
                RevenueLabel.Text = data.Revenue.ToString("C0", _idrCulture);
                ExpenseLabel.Text = data.Expenses.ToString("C0", _idrCulture);
            }
            else
            {
                await this.DisplayAlertAsync("Koneksi Gagal", "Gagal mengambil data dari server web.", "OK");
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlertAsync("Error", $"Terjadi kesalahan: {ex.Message}", "OK");
        }
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadDashboardDataAsync();
    }

    private async void OnInputJournalClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new InputJournalPage());
    }

    /// <summary>
    /// Dipanggil dari InputJournalPage untuk memproses antrean sync 10 detik.
    /// </summary>
    public async Task ProcessNewTransactionAsync(CreateSimpleTransactionDto transactionDto)
    {
        SaveToLocalMemory(transactionDto);

        await TopHeader.QueueAndUploadDataAsync(
            data: transactionDto,
            uploadTask: async (dto) =>
            {
                // Memanggil PostSimpleTransactionAsync sesuai dengan yang ada di ApiService.cs
                var (success, message) = await _apiService.PostSimpleTransactionAsync(dto);
                return success;
            },
            onDeleteLocalData: (dto) =>
            {
                RemoveFromLocalMemory(dto);
            }
        );

        await LoadDashboardDataAsync();
    }

    private void SaveToLocalMemory(CreateSimpleTransactionDto dto)
    {
        // Logika simpan sementara ke SQLite / List Lokal
    }

    private void RemoveFromLocalMemory(CreateSimpleTransactionDto dto)
    {
        // Logika hapus otomatis dari SQLite / List Lokal saat gagal upload
    }
}
