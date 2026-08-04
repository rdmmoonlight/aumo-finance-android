using System.Globalization;
using System.Text.Json;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly CultureInfo _idrCulture = new("id-ID");

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

    public async Task<bool> ProcessNewTransactionAsync(CreateSimpleTransactionDto transactionDto)
    {
        SaveToLocalMemory(transactionDto);

        bool isSuccess = false;
        string resultMessage = string.Empty;

        await TopHeader.QueueAndUploadDataAsync(
            data: transactionDto,
            uploadTask: async (dto) =>
            {
                var (success, message) = await _apiService.PostSimpleTransactionAsync(dto);
                isSuccess = success;
                resultMessage = message;

                if (success)
                {
                    RemoveFromLocalMemory(dto);
                }
                return success;
            },
            onDeleteLocalData: (dto) =>
            {
                RemoveFromLocalMemory(dto);
            }
        );

        if (isSuccess)
        {
            await LoadDashboardDataAsync();
        }
        else
        {
            await this.DisplayAlertAsync("Gagal Input DB", string.IsNullOrEmpty(resultMessage) ? "Terjadi kesalahan saat menyimpan transaksi." : resultMessage, "OK");
        }

        return isSuccess;
    }

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
