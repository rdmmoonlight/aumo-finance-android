using System;
using System.Linq;
using System.Threading.Tasks;
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
        await BuildSofpAsync();
    }

    // Overload untuk menerima hingga 4 argumen dari PostClosingTrialBalancePage
    public async Task BuildSofpAsync(bool isPostClosing = false, object? arg2 = null, object? arg3 = null, object? arg4 = null)
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

            var trialBalanceRows = await _accountingService.GetTrialBalanceAsync(_currentUserId, period, includeAdjusting: true);

            if (!trialBalanceRows.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada data laporan pada periode {period.PeriodName}.";
                return;
            }

            var assets = trialBalanceRows
                .Where(r => r.Type.Equals("Asset", StringComparison.OrdinalIgnoreCase) ||
                            r.Type.Equals("CurrentAsset", StringComparison.OrdinalIgnoreCase) ||
                            r.Type.Equals("Cash", StringComparison.OrdinalIgnoreCase) ||
                            r.Type.Equals("NonCurrentAsset", StringComparison.OrdinalIgnoreCase) ||
                            r.Type.Equals("FixedAsset", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel
                {
                    ReferenceNumber = r.ReferenceNumber.ToString(),
                    AccountName = r.AccountName,
                    Amount = r.NetBalance
                }).ToList();

            var liabilities = trialBalanceRows
                .Where(r => r.Type.Equals("Liability", StringComparison.OrdinalIgnoreCase) ||
                            r.Type.Equals("CurrentLiability", StringComparison.OrdinalIgnoreCase) ||
                            r.Type.Equals("NonCurrentLiability", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel
                {
                    ReferenceNumber = r.ReferenceNumber.ToString(),
                    AccountName = r.AccountName,
                    Amount = Math.Abs(r.NetBalance)
                }).ToList();

            var equities = trialBalanceRows
                .Where(r => r.Type.Equals("Equity", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel
                {
                    ReferenceNumber = r.ReferenceNumber.ToString(),
                    AccountName = r.AccountName,
                    Amount = Math.Abs(r.NetBalance)
                }).ToList();

            decimal totalAssets = assets.Sum(r => r.Amount);
            decimal totalLiabilities = liabilities.Sum(r => r.Amount);
            decimal totalEquity = equities.Sum(r => r.Amount);
            decimal totalLiabilitiesAndEquity = totalLiabilities + totalEquity;

            var culture = new System.Globalization.CultureInfo("id-ID");

            AssetsCollectionView.ItemsSource = assets;
            TotalAssetsLabel.Text = $"Rp {totalAssets.ToString("N0", culture)}";

            LiabilitiesCollectionView.ItemsSource = liabilities;
            TotalLiabilitiesLabel.Text = $"Rp {totalLiabilities.ToString("N0", culture)}";

            EquityCollectionView.ItemsSource = equities;
            TotalEquityLabel.Text = $"Rp {totalEquity.ToString("N0", culture)}";

            TotalLiabilitiesAndEquityLabel.Text = $"Rp {totalLiabilitiesAndEquity.ToString("N0", culture)}";

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
}

public class FinancialPositionLineModel
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    private static readonly System.Globalization.CultureInfo Idr = new("id-ID");

    public string FormattedAmount => Amount.ToString("N0", Idr);
}
