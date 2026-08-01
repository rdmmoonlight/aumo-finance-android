using System.Globalization;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance;

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
                // Menggunakan DisplayAlertAsync untuk .NET 10
                await this.DisplayAlertAsync("Koneksi Gagal", "Gagal mengambil data dari server web.", "OK");
            }
        }
        catch (Exception ex)
        {
            // Menggunakan DisplayAlertAsync untuk .NET 10
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
    /// Dipanggil dari InputJournalPage saat user klik Simpan
    /// </summary>
    public async Task ProcessNewTransactionAsync(CreateSimpleTransactionDto transactionDto)
    {
        SaveToLocalMemory(transactionDto);

        await TopHeader.QueueAndUploadDataAsync(
            data: transactionDto,
            uploadTask: async (dto) =>
            {
                // Memanggil method yang ADA di ApiService.cs Anda
                return await _apiService.CreateSimpleTransactionAsync(dto);
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
        // Simpan sementara
    }

    private void RemoveFromLocalMemory(CreateSimpleTransactionDto dto)
    {
        // Hapus jika gagal
    }
}
