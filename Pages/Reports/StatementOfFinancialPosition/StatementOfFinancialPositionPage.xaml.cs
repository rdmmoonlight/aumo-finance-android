using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AumoFinance;
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
        await LoadBalanceSheetAsync();
    }

    private async Task LoadBalanceSheetAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        SheetContainer.IsVisible = false;
        EmptyStateContainer.IsVisible = false;
        BalanceStatusCard.IsVisible = false;

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
            SubtitleLabel.Text = $"Balance Sheet (IAS 1) — per {period.EndDate:MMMM dd, yyyy}";

            // Ambil data Trial Balance & Retained Earnings
            var rows = await _accountingService.GetTrialBalanceAsync(_currentUserId, period, includeAdjusting: true);

            // Hitung Retained Earnings
            var incomeStatement = IncomeStatementPageViewModelHelper(rows, period);
            var reAccount = rows.FirstOrDefault(r => r.Type.Equals("RetainedEarnings", StringComparison.OrdinalIgnoreCase) || r.Role?.Equals("RetainedEarnings", StringComparison.OrdinalIgnoreCase) == true);
            decimal beginningRetained = reAccount?.NetBalance ?? 0;
            decimal retainedEarningsEnding = beginningRetained + incomeStatement.NetIncome;

            var assets = rows.Where(r => r.Type.Equals("Assets", StringComparison.OrdinalIgnoreCase)).Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = r.NetBalance }).ToList();
            var liabilities = rows.Where(r => r.Type.Equals("Liabilities", StringComparison.OrdinalIgnoreCase)).Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = r.NetBalance }).ToList();
            var equity = rows.Where(r => r.Type.Equals("Equity", StringComparison.OrdinalIgnoreCase) && !r.Type.Equals("RetainedEarnings", StringComparison.OrdinalIgnoreCase) && r.Role != "RetainedEarnings").Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = r.NetBalance }).ToList();

            decimal totalAssets = assets.Sum(a => a.Amount);
            decimal totalLiabilities = liabilities.Sum(l => l.Amount);
            decimal totalEquity = equity.Sum(e => e.Amount) + retainedEarningsEnding;
            decimal totalLiabAndEquity = totalLiabilities + totalEquity;

            bool isBalanced = Math.Round(totalAssets - totalLiabAndEquity, 2) == 0;

            var culture = new System.Globalization.CultureInfo("id-ID");

            // Update UI Bindings
            AssetsCollectionView.ItemsSource = assets;
            TotalAssetsLabel.Text = totalAssets.ToString("N0", culture);

            LiabilitiesCollectionView.ItemsSource = liabilities;
            TotalLiabilitiesLabel.Text = totalLiabilities.ToString("N0", culture);

            EquityCollectionView.ItemsSource = equity;
            RetainedEarningsRowLabel.Text = etàTextRetainedEarnings(period);
            RetainedEarningsEndingLabel.Text = retainedEarningsEnding.ToString("N0", culture);
            TotalEquityLabel.Text = totalEquity.ToString("N0", culture);

            TotalLiabilitiesAndEquityLabel.Text = totalLiabAndEquity.ToString("N0", culture);

            // Status Card
            BalanceStatusCard.IsVisible = true;
            BalanceStatusCard.BackgroundColor = isBalanced ? Color.FromArgb("#064E3B") : Color.FromArgb("#7F1D1D");
            BalanceStatusCard.Stroke = isBalanced ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626");
            BalanceStatusIcon.Text = isBalanced ? "✓" : "⚠️";
            BalanceStatusIcon.TextColor = isBalanced ? Color.FromArgb("#34D399") : Color.FromArgb("#FCA5A5");
            BalanceStatusText.TextColor = BalanceStatusIcon.TextColor;
            BalanceStatusText.Text = isBalanced
                ? "Total Assets = Total Liabilities + Equity. Statement of Financial Position seimbang."
                : "Total Assets tidak sama dengan Total Liabilities + Equity. Periksa kembali entri jurnal.";

            SheetContainer.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal memuat Balance Sheet: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    public static async Task<StatementOfFinancialPositionViewModel> BuildSofpAsync(AppDbContext dbContext, Guid userId, Period period, bool isPostClosing)
    {
        var accountingService = new AccountingService(dbContext);
        var rows = await accountingService.GetTrialBalanceAsync(userId, period, includeAdjusting: true);
        var incomeStatement = CalculateIncomeStatement(rows);
        var reAccount = rows.FirstOrDefault(r => r.Type.Equals("RetainedEarnings", StringComparison.OrdinalIgnoreCase) || r.Role.Equals("RetainedEarnings", StringComparison.OrdinalIgnoreCase));
        decimal beginningRetained = reAccount?.NetBalance ?? 0;
        decimal retainedEarningsEnding = beginningRetained + incomeStatement.NetIncome;

        var equityRows = rows
            .Where(r => r.Type.Equals("Equity", StringComparison.OrdinalIgnoreCase) && !r.Role.Equals("RetainedEarnings", StringComparison.OrdinalIgnoreCase))
            .Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = r.NetBalance })
            .ToList();

        return new StatementOfFinancialPositionViewModel
        {
            Assets = rows
                .Where(r => r.Type.Equals("Asset", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Assets", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = r.NetBalance })
                .ToList(),
            Liabilities = rows
                .Where(r => r.Type.Equals("Liability", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Liabilities", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = r.NetBalance })
                .ToList(),
            EquityExcludingRetainedEarnings = equityRows,
            RetainedEarningsEnding = retainedEarningsEnding
        };
    }

    private static IncomeStatementModelHelper CalculateIncomeStatement(List<TrialBalanceRowViewModel> rows)
    {
        decimal totalRevenue = rows.Where(r => r.Type.Equals("OperatingIncome", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Revenue", StringComparison.OrdinalIgnoreCase)).Sum(r => r.NetBalance);
        decimal totalExpense = rows.Where(r => r.Type.Equals("OperatingExpenses", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("OperatingExpense", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase)).Sum(r => Math.Abs(r.NetBalance));
        decimal operatingIncome = totalRevenue - totalExpense;

        decimal otherInc = rows.Where(r => r.Type.Equals("OtherIncome", StringComparison.OrdinalIgnoreCase)).Sum(r => r.NetBalance);
        decimal otherExp = rows.Where(r => r.Type.Equals("OtherExpenses", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("OtherExpense", StringComparison.OrdinalIgnoreCase)).Sum(r => Math.Abs(r.NetBalance));

        return new IncomeStatementModelHelper { NetIncome = operatingIncome + otherInc - otherExp };
    }

    private string etàTextRetainedEarnings(Period period) => $"Retained earnings, {period.EndDate:MMMM d}";

    private IncomeStatementModelHelper IncomeStatementPageViewModelHelper(List<TrialBalanceRowViewModel> rows, Period period) => CalculateIncomeStatement(rows);
}

public class FinancialPositionLineModel
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    private static readonly System.Globalization.CultureInfo Idr = new("id-ID");

    public string FormattedAmount => Amount.ToString("N0", Idr);
}


public class StatementOfFinancialPositionViewModel
{
    public List<FinancialPositionLineModel> Assets { get; set; } = new();
    public List<FinancialPositionLineModel> Liabilities { get; set; } = new();
    public List<FinancialPositionLineModel> EquityExcludingRetainedEarnings { get; set; } = new();
    public decimal RetainedEarningsEnding { get; set; }
}