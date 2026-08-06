using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using AumoFinance.Services;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class StatementOfFinancialPositionPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public StatementOfFinancialPositionPage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPageDataAsync();
    }

    private async Task LoadPageDataAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        SheetContainer.IsVisible = false;
        EmptyStateContainer.IsVisible = false;

        try
        {
            var period = await _accountingService.GetCurrentPeriodAsync(_currentUserId);
            if (period == null)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "Belum ada periode aktif yang dipilih.";
                return;
            }

            PeriodNameLabel.Text = period.PeriodName;

            var vm = await BuildSofpAsync(_accountingService.DbContext, _currentUserId, period, isPostClosing: false);

            if (!vm.Assets.Any() && !vm.Liabilities.Any() && !vm.EquityExcludingRetainedEarnings.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada data laporan pada periode {period.PeriodName}.";
                return;
            }

            var culture = new System.Globalization.CultureInfo("id-ID");

            // Update UI Koleksi
            AssetsCollectionView.ItemsSource = vm.Assets;
            TotalAssetsLabel.Text = $"Rp {vm.TotalAssets.ToString("N0", culture)}";

            LiabilitiesCollectionView.ItemsSource = vm.Liabilities;
            TotalLiabilitiesLabel.Text = $"Rp {vm.TotalLiabilities.ToString("N0", culture)}";

            EquityCollectionView.ItemsSource = vm.EquityExcludingRetainedEarnings;
            TotalEquityLabel.Text = $"Rp {vm.TotalEquity.ToString("N0", culture)}";

            TotalLiabilitiesAndEquityLabel.Text = $"Rp {vm.TotalLiabilitiesAndEquity.ToString("N0", culture)}";

            SheetContainer.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Gagal memuat neraca keuangan: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    // METHOD STATIC YANG DIPANGGIL OLEH PostClosingTrialBalancePage
    public static async Task<StatementOfFinancialPositionViewModel> BuildSofpAsync(
        AppDbContext dbContext, 
        Guid currentUserId, 
        Period period, 
        bool isPostClosing = false)
    {
        // Ambil COA aktif milik user
        var accounts = await dbContext.ChartOfAccounts
            .Where(a => a.IsActive && a.UserId == currentUserId)
            .OrderBy(a => a.ReferenceNumber)
            .ToListAsync();

        var accountIds = accounts.Select(a => a.Id).ToList();

        // Ambil transaksi baris jurnal hingga akhir periode
        var lines = await dbContext.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => accountIds.Contains(l.AccountId) 
                     && l.JournalEntry!.UserId == currentUserId
                     && l.JournalEntry!.EntryDate <= period.EndDate)
            .ToListAsync();

        var assets = new List<FinancialPositionLineModel>();
        var liabilities = new List<FinancialPositionLineModel>();
        var equities = new List<FinancialPositionLineModel>();

        foreach (var acc in accounts)
        {
            var accLines = lines.Where(l => l.AccountId == acc.Id).ToList();
            
            // Penanganan null aman untuk acc.Type
            string accountType = acc.Type ?? string.Empty;
            bool normalDebit = AccountClassification.NormalBalanceIsDebit(accountType);

            decimal net = normalDebit
                ? accLines.Sum(l => l.Debit - l.Credit)
                : accLines.Sum(l => l.Credit - l.Debit);

            if (!accLines.Any() && net == 0) continue;

            var item = new FinancialPositionLineModel
            {
                ReferenceNumber = acc.ReferenceNumber.ToString(),
                AccountName = acc.AccountName,
                Amount = Math.Abs(net)
            };

            // Pengecekan Kategori berdasarkan String accountType
            if (accountType.Contains("Asset", StringComparison.OrdinalIgnoreCase) || accountType.Equals("Cash", StringComparison.OrdinalIgnoreCase))
            {
                assets.Add(item);
            }
            else if (accountType.Contains("Liabilit", StringComparison.OrdinalIgnoreCase) || accountType.Equals("Payable", StringComparison.OrdinalIgnoreCase))
            {
                liabilities.Add(item);
            }
            else if (accountType.Contains("Equity", StringComparison.OrdinalIgnoreCase) || accountType.Equals("Capital", StringComparison.OrdinalIgnoreCase))
            {
                equities.Add(item);
            }
        }

        decimal totalAssets = assets.Sum(a => a.Amount);
        decimal totalLiab = liabilities.Sum(l => l.Amount);
        decimal totalEq = equities.Sum(e => e.Amount);

        return new StatementOfFinancialPositionViewModel
        {
            Assets = assets,
            Liabilities = liabilities,
            EquityExcludingRetainedEarnings = equities,
            RetainedEarningsEnding = 0m,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiab,
            TotalEquity = totalEq,
            TotalLiabilitiesAndEquity = totalLiab + totalEq
        };
    }
}

// MODEL VIEW DATA SOFP
public class StatementOfFinancialPositionViewModel
{
    public List<FinancialPositionLineModel> Assets { get; set; } = new();
    public List<FinancialPositionLineModel> Liabilities { get; set; } = new();
    public List<FinancialPositionLineModel> EquityExcludingRetainedEarnings { get; set; } = new();
    public decimal RetainedEarningsEnding { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity { get; set; }
}

public class FinancialPositionLineModel
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    private static readonly System.Globalization.CultureInfo Idr = new("id-ID");

    public string FormattedAmount => Amount.ToString("N0", Idr);
}
