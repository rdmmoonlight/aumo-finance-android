using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class PostClosingTrialBalancePage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public PostClosingTrialBalancePage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPostClosingDataAsync();
    }

    private async Task LoadPostClosingDataAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        TableContainer.IsVisible = false;
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
            SubtitleLabel.Text = `Menampilkan akun permanen per ${period.EndDate:MMMM dd, yyyy}`;

            // Ambil SOFP post-closing melalui helper/service
            var vm = await StatementOfFinancialPositionPage.BuildSofpAsync(_accountingService.DbContext, _currentUserId, period, isPostClosing: true);

            var rows = new List<PostClosingRowModel>();
            var culture = new System.Globalization.CultureInfo("id-ID");

            // 1. Assets
            foreach (var asset in vm.Assets)
            {
                rows.Add(new PostClosingRowModel
                {
                    ReferenceNumber = asset.ReferenceNumber,
                    AccountName = asset.AccountName,
                    TypeLabel = "Asset",
                    DebitAmount = asset.Amount,
                    CreditAmount = 0
                });
            }

            // 2. Liabilities
            foreach (var liab in vm.Liabilities)
            {
                rows.Add(new PostClosingRowModel
                {
                    ReferenceNumber = liab.ReferenceNumber,
                    AccountName = liab.AccountName,
                    TypeLabel = "Liability",
                    DebitAmount = 0,
                    CreditAmount = liab.Amount
                });
            }

            // 3. Equity excluding Retained Earnings
            foreach (var eq in vm.EquityExcludingRetainedEarnings)
            {
                rows.Add(new PostClosingRowModel
                {
                    ReferenceNumber = eq.ReferenceNumber,
                    AccountName = eq.AccountName,
                    TypeLabel = "Equity",
                    DebitAmount = 0,
                    CreditAmount = eq.Amount
                });
            }

            // 4. Retained Earnings (Ending)
            rows.Add(new PostClosingRowModel
            {
                ReferenceNumber = "-",
                AccountName = $"Retained earnings, {period.EndDate:MMMM d}",
                TypeLabel = "Equity",
                DebitAmount = vm.RetainedEarningsEnding < 0 ? Math.Abs(vm.RetainedEarningsEnding) : 0,
                CreditAmount = vm.RetainedEarningsEnding >= 0 ? vm.RetainedEarningsEnding : 0
            });

            if (!rows.Any())
            {
                EmptyStateContainer.IsVisible = true;
                return;
            }

            decimal totalDebit = rows.Sum(r => r.DebitAmount);
            decimal totalCredit = rows.Sum(r => r.CreditAmount);
            bool isBalanced = Math.Round(totalDebit - totalCredit, 2) == 0;

            TotalDebitLabel.Text = totalDebit.ToString("N0", culture);
            TotalCreditLabel.Text = totalCredit.ToString("N0", culture);

            // Status Alert
            BalanceStatusCard.IsVisible = true;
            BalanceStatusCard.BackgroundColor = isBalanced ? Color.FromArgb("#064E3B") : Color.FromArgb("#7F1D1D");
            BalanceStatusCard.Stroke = isBalanced ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626");
            BalanceStatusIcon.Text = isBalanced ? "✓" : "⚠️";
            BalanceStatusIcon.TextColor = isBalanced ? Color.FromArgb("#34D399") : Color.FromArgb("#FCA5A5");
            BalanceStatusText.TextColor = BalanceStatusIcon.TextColor;
            BalanceStatusText.Text = isBalanced 
                ? "Post-closing trial balance seimbang; buku siap untuk periode berikutnya." 
                : "Post-closing trial balance tidak seimbang! Periksa kembali closing entries.";

            TrialBalanceCollectionView.ItemsSource = rows;
            TableContainer.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal memuat post-closing trial balance: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}

public class PostClosingRowModel
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string TypeLabel { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }

    private static readonly System.Globalization.CultureInfo Idr = new("id-ID");

    public string FormattedDebit => DebitAmount > 0 ? DebitAmount.ToString("N0", Idr) : "-";
    public string FormattedCredit => CreditAmount > 0 ? CreditAmount.ToString("N0", Idr) : "-";
}
