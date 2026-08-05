using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;

namespace AumoFinance.Pages;

[QueryProperty(nameof(IncludeAdjustingStr), "includeAdjusting")]
public partial class TrialBalancePage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;
    private bool _includeAdjusting;

    public string IncludeAdjustingStr
    {
        set { _includeAdjusting = bool.TryParse(value, out var result) && result; }
    }

    public TrialBalancePage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Sesuaikan Teks UI berdasarkan parameter query
        if (_includeAdjusting)
        {
            PageTitleLabel.Text = "Adjusted Trial Balance";
            PageSubtitleLabel.Text = "Saldo akun setelah jurnal penyesuaian.";
            this.Title = "Adjusted Trial Balance";
        }
        else
        {
            PageTitleLabel.Text = "Trial Balance";
            PageSubtitleLabel.Text = "Saldo akun sebelum penyesuaian.";
            this.Title = "Trial Balance";
        }

        await LoadTrialBalanceAsync();
    }

    private async Task LoadTrialBalanceAsync()
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
                return;
            }

            var rows = await _accountingService.GetTrialBalanceAsync(_currentUserId, period, _includeAdjusting);

            if (!rows.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "Tidak ada riwayat transaksi akun ditemukan.";
            }
            else
            {
                decimal totalDebit = rows.Sum(r => r.Debit);
                decimal totalCredit = rows.Sum(r => r.Credit);
                bool isBalanced = Math.Round(totalDebit - totalCredit, 2) == 0;

                // Update Footer Totals
                var culture = new System.Globalization.CultureInfo("id-ID");
                TotalDebitLabel.Text = totalDebit.ToString("N0", culture);
                TotalCreditLabel.Text = totalCredit.ToString("N0", culture);

                // Update Status Card
                BalanceStatusCard.IsVisible = true;
                BalanceStatusCard.BackgroundColor = isBalanced ? Color.FromArgb("#064E3B") : Color.FromArgb("#7F1D1D");
                BalanceStatusCard.Stroke = isBalanced ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626");
                BalanceStatusIcon.Text = isBalanced ? "✓" : "⚠️";
                BalanceStatusIcon.TextColor = isBalanced ? Color.FromArgb("#34D399") : Color.FromArgb("#FCA5A5");
                BalanceStatusText.TextColor = BalanceStatusIcon.TextColor;
                BalanceStatusText.Text = isBalanced
                    ? "Trial balance seimbang; total Debit sama dengan Kredit."
                    : "Trial balance tidak seimbang! Silakan periksa kembali entri jurnal Anda.";

                TrialBalanceCollectionView.ItemsSource = rows;
                TableContainer.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal memuat Trial Balance: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}