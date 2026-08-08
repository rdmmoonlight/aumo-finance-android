using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services.Reports;

namespace AumoFinance.Pages.Reports.TrialBalance;

[QueryProperty(nameof(IncludeAdjustingStr), "includeAdjusting")]
public partial class TrialBalancePage : ContentPage
{
    private readonly TrialBalanceService _trialBalanceService;
    private bool _includeAdjusting;

    public string IncludeAdjustingStr
    {
        set { _includeAdjusting = bool.TryParse(value, out var result) && result; }
    }

    public TrialBalancePage(TrialBalanceService trialBalanceService)
    {
        InitializeComponent();
        _trialBalanceService = trialBalanceService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Update UI text based on query parameter
        if (_includeAdjusting)
        {
            PageTitleLabel.Text = "Adjusted Trial Balance";
            PageSubtitleLabel.Text = "Account balances after adjusting entries.";
            this.Title = "Adjusted Trial Balance";
        }
        else
        {
            PageTitleLabel.Text = "Trial Balance";
            PageSubtitleLabel.Text = "Account balances before adjusting entries.";
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
            string reportType = _includeAdjusting ? "adjusted" : "unadjusted";
            var (response, errorDetail) = await _trialBalanceService.GetTrialBalanceReportAsync(reportType);

            if (response == null || !response.Success)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = errorDetail ?? "Failed to load trial balance report.";
                return;
            }

            if (!response.HasPeriodSelected)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "No active period selected.";
                return;
            }

            var rows = response.Rows;

            if (rows == null || !rows.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "No account transaction history found.";
            }
            else
            {
                decimal totalDebit = response.TotalDebit;
                decimal totalCredit = response.TotalCredit;
                bool isBalanced = response.IsBalanced;

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
                    ? "Trial balance is balanced; total Debits equal Credits."
                    : "Trial balance is unbalanced! Please check your journal entries.";

                TrialBalanceCollectionView.ItemsSource = rows;
                TableContainer.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load Trial Balance: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
