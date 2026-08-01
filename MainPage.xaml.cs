using System.Globalization;
using AumoFinance.Services;
using AumoFinance.Models;

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
                // Update label periode melalui komponen TopHeader
                TopHeader.PeriodText = data.ActivePeriod ?? "-";
                
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
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Terjadi kesalahan: {ex.Message}", "OK");
        }
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadDashboardDataAsync();
    }

    private async void OnInputJournalClicked(object? sender, EventArgs e)
    {
        // Navigasi ke halaman input jurnal
        await Navigation.PushAsync(new InputJournalPage());
    }

    /// <summary>
    /// Contoh Method Helper yang dipanggil setelah pengguna selesai menginput jurnal baru 
    /// untuk menjalankan antrean sync 10 detik via TopHeader.
    /// </summary>
    public async Task ProcessNewJournalEntryAsync(JournalEntryModel newEntry)
    {
        // 1. Simpan sementara ke penyimpanan/list lokal (jika ada)
        SaveToLocalMemory(newEntry);

        // 2. Kirim ke antrean TopBar (Queue 10 detik dengan warna Orange Ketela)
        await TopHeader.QueueAndUploadDataAsync(
            data: newEntry,
            uploadTask: async (entry) =>
            {
                // Panggil ApiService untuk simpan ke database Neon/PostgreSQL
                return await _apiService.SaveJournalEntryAsync(entry);
            },
            onDeleteLocalData: (entry) =>
            {
                // Callback jika gagal upload: Hapus data otomatis dari memori lokal
                RemoveFromLocalMemory(entry);
            }
        );

        // 3. Refresh dashboard jika sync berhasil
        await LoadDashboardDataAsync();
    }

    private void SaveToLocalMemory(JournalEntryModel entry)
    {
        // Logika simpan sementara ke SQLite / List Lokal
    }

    private void RemoveFromLocalMemory(JournalEntryModel entry)
    {
        // Logika hapus otomatis dari SQLite / List Lokal saat gagal upload
    }
}
