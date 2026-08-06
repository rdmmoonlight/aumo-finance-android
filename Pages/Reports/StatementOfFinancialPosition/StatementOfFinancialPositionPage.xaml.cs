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
        await LoadFinancialPositionAsync();
    }

    private async Task LoadFinancialPositionAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        StatementContainer.IsVisible = false;
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
            AsOfDateLabel.Text = $"Statement of Financial Position (IAS 1) — per {period.EndDate:dd MMMM yyyy}";

            var trialBalanceRows = await _accountingService.GetTrialBalanceAsync(_currentUserId, period, includeAdjusting: true);

            if (!trialBalanceRows.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada data laporan pada periode {period.PeriodName}.";
                return;
            }

            // PERBAIKAN LINE 60, 61, 62: Tambahkan .ToString()
            var currentAssets = trialBalanceRows.Where(r => r.Type.Equals("CurrentAsset", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber.ToString(), AccountName = r.AccountName, Amount = r.NetBalance }).ToList();

            var nonCurrentAssets = trialBalanceRows.Where(r => r.Type.Equals("NonCurrentAsset", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("FixedAsset", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber.ToString(), AccountName = r.AccountName, Amount = r.NetBalance }).ToList();

            var currentLiabilities = trialBalanceRows.Where(r => r.Type.Equals("CurrentLiability", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Liability", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber.ToString(), AccountName = r.AccountName, Amount = Math.Abs(r.NetBalance) }).ToList();

            var nonCurrentLiabilities = trialBalanceRows.Where(r => r.Type.Equals("NonCurrentLiability", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber.ToString(), AccountName = r.AccountName, Amount = Math.Abs(r.NetBalance) }).ToList();

            var equities = trialBalanceRows.Where(r => r.Type.Equals("Equity", StringComparison.OrdinalIgnoreCase))
                .Select(r => new FinancialPositionLineModel { ReferenceNumber = r.ReferenceNumber.ToString(), AccountName = r.AccountName, Amount = Math.Abs(r.NetBalance) }).ToList();

            decimal totalCurrentAsset = currentAssets.Sum(r => r.Amount);
            decimal totalNonCurrentAsset = nonCurrentAssets.Sum(r => r.Amount);
            decimal totalAssets = totalCurrentAsset + totalNonCurrentAsset;

            decimal totalCurrentLiability = currentLiabilities.Sum(r => r.Amount);
            decimal totalNonCurrentLiability = nonCurrentLiabilities.Sum(r => r.Amount);
            decimal totalLiabilities = totalCurrentLiability + totalNonCurrentLiability;

            decimal totalEquity = equities.Sum(r => r.Amount);
            decimal totalLiabilitiesAndEquity = totalLiabilities + totalEquity;

            var culture = new System.Globalization.CultureInfo("id-ID");

            CurrentAssetsCollectionView.ItemsSource = currentAssets;
            TotalCurrentAssetsLabel.Text = totalCurrentAsset.ToString("N0", culture);

            NonCurrentAssetsCollectionView.ItemsSource = nonCurrentAssets;
            TotalNonCurrentAssetsLabel.Text = totalNonCurrentAsset.ToString("N0", culture);

            TotalAssetsLabel.Text = totalAssets.ToString("N0", culture);

            CurrentLiabilitiesCollectionView.ItemsSource = currentLiabilities;
            TotalCurrentLiabilitiesLabel.Text = totalCurrentLiability.ToString("N0", culture);

            NonCurrentLiabilitiesCollectionView.ItemsSource = nonCurrentLiabilities;
            TotalNonCurrentLiabilitiesLabel.Text = totalNonCurrentLiability.ToString("N0", culture);

            TotalLiabilitiesLabel.Text = totalLiabilities.ToString("N0", culture);

            EquitiesCollectionView.ItemsSource = equities;
            TotalEquityLabel.Text = totalEquity.ToString("N0", culture);

            TotalLiabilitiesAndEquityLabel.Text = totalLiabilitiesAndEquity.ToString("N0", culture);

            StatementContainer.IsVisible = true;
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
