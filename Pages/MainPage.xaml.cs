using System.Globalization;
using System.Text.Json;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly CultureInfo _idrCulture = new("id-ID");

    // Berkas lokal untuk menyimpan transaksi yang sedang menunggu (queued)
    // upload, agar tidak hilang jika aplikasi ditutup sebelum proses sync
    // 10 detik selesai. Disimpan sebagai JSON sederhana di penyimpanan
    // aplikasi (tidak butuh dependency SQLite tambahan).
    private static readonly string PendingFilePath =
        Path.Combine(FileSystem.AppDataDirectory, "pending_transactions.json");

    public MainPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RecoverPendingTransactionsAsync();
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
                if (success)
                {
                    // Sudah tersimpan di server: catatan lokal tidak diperlukan lagi.
                    RemoveFromLocalMemory(dto);
                }
                return success;
            },
            onDeleteLocalData: (dto) =>
            {
                RemoveFromLocalMemory(dto);
            }
        );

        await LoadDashboardDataAsync();
    }

    // Setiap transaksi yang sedang diqueue diberi Id lokal sendiri (terpisah
    // dari Id database) supaya bisa dicocokkan lagi saat proses hapus.
    private static readonly Dictionary<CreateSimpleTransactionDto, Guid> _pendingIds = new();

    private void SaveToLocalMemory(CreateSimpleTransactionDto dto)
    {
        try
        {
            var pending = ReadPendingFile();
            var localId = Guid.NewGuid();
            _pendingIds[dto] = localId;
            pending.Add(new PendingTransaction(localId, dto));
            WritePendingFile(pending);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gagal menyimpan transaksi lokal: {ex.Message}");
        }
    }

    private void RemoveFromLocalMemory(CreateSimpleTransactionDto dto)
    {
        try
        {
            if (!_pendingIds.TryGetValue(dto, out var localId)) return;

            var pending = ReadPendingFile();
            pending.RemoveAll(p => p.Id == localId);
            WritePendingFile(pending);
            _pendingIds.Remove(dto);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gagal menghapus transaksi lokal: {ex.Message}");
        }
    }

    /// <summary>
    /// Dipanggil saat aplikasi dibuka kembali: transaksi yang masih tersisa di
    /// berkas lokal berarti aplikasi ditutup sebelum sync 10 detik selesai
    /// atau sebelum upload berhasil, jadi perlu dicoba kirim ulang.
    /// </summary>
    private async Task RecoverPendingTransactionsAsync()
    {
        List<PendingTransaction> pending;
        try
        {
            pending = ReadPendingFile();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gagal membaca transaksi lokal: {ex.Message}");
            return;
        }

        if (pending.Count == 0) return;

        foreach (var item in pending.ToList())
        {
            _pendingIds[item.Dto] = item.Id;
            await TopHeader.QueueAndUploadDataAsync(
                data: item.Dto,
                uploadTask: async (dto) =>
                {
                    var (success, _) = await _apiService.PostSimpleTransactionAsync(dto);
                    if (success)
                    {
                        RemoveFromLocalMemory(dto);
                    }
                    return success;
                },
                onDeleteLocalData: RemoveFromLocalMemory
            );
        }
    }

    private static List<PendingTransaction> ReadPendingFile()
    {
        if (!File.Exists(PendingFilePath)) return new List<PendingTransaction>();
        var json = File.ReadAllText(PendingFilePath);
        if (string.IsNullOrWhiteSpace(json)) return new List<PendingTransaction>();
        return JsonSerializer.Deserialize<List<PendingTransaction>>(json) ?? new List<PendingTransaction>();
    }

    private static void WritePendingFile(List<PendingTransaction> pending)
    {
        var json = JsonSerializer.Serialize(pending);
        File.WriteAllText(PendingFilePath, json);
    }

    private sealed record PendingTransaction(Guid Id, CreateSimpleTransactionDto Dto);
}
