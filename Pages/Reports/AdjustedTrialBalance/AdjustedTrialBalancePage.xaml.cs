using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AumoFinance.Services.Reports;

namespace AumoFinance.Pages.Reports.AdjustedTrialBalance;

public partial class AdjustedTrialBalancePage : ContentPage
{
    private readonly TrialBalanceService _trialBalanceService;
    private bool _isDataLoaded;

    public AdjustedTrialBalancePage(TrialBalanceService trialBalanceService)
    {
        InitializeComponent();
        _trialBalanceService = trialBalanceService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isDataLoaded)
        {
            await LoadAdjustedTrialBalanceAsync();
        }
    }

    private async Task LoadAdjustedTrialBalanceAsync()
    {
        SetLoadingState(true);

        try
        {
            // Mengirim parameter "adjusted" untuk mengambil data Trial Balance yang menyertakan Jurnal Penyesuaian
            var (response, errorDetail) = await _trialBalanceService.GetTrialBalanceReportAsync("adjusted");

            if (response == null || !response.Success)
            {
                ShowEmptyState(errorDetail ?? "Failed to load adjusted trial balance report.");
                return;
            }

            if (!response.HasPeriodSelected)
            {
                ShowEmptyState("No active period selected.");
                return;
            }

            // Update nama periode aktif di TopHeader
            if (TopHeader != null)
            {
                TopHeader.PeriodText = string.IsNullOrWhiteSpace(response.SelectedPeriodName)
                    ? "No Active Period"
                    : response.SelectedPeriodName;
            }

            var rows = response.Rows;

            if (rows == null || !rows.Any())
            {
                ShowEmptyState("No account transaction history found.");
            }
            else
            {
                decimal totalDebit = response.TotalDebit;
                decimal totalCredit = response.TotalCredit;
                bool isBalanced = response.IsBalanced;

                var culture = new CultureInfo("id-ID");
                TotalDebitLabel.Text = totalDebit.ToString("N0", culture);
                TotalCreditLabel.Text = totalCredit.ToString("N0", culture);

                // Update Status Keseimbangan
                BalanceStatusCard.IsVisible = true;
                BalanceStatusCard.BackgroundColor = Color.Parse(isBalanced ? "#064E3B" : "#7F1D1D");
                BalanceStatusCard.Stroke = Color.Parse(isBalanced ? "#059669" : "#DC2626");

                var accentColor = Color.Parse(isBalanced ? "#34D399" : "#FCA5A5");
                BalanceStatusIcon.Text = isBalanced ? "✓" : "⚠️";
                BalanceStatusIcon.TextColor = accentColor;
                BalanceStatusText.TextColor = accentColor;
                
                BalanceStatusText.Text = isBalanced
                    ? "Adjusted trial balance is balanced; total Debits equal Credits."
                    : "Adjusted trial balance is unbalanced! Please check your adjusting entries.";

                TrialBalanceCollectionView.ItemsSource = rows;
                TableContainer.IsVisible = true;
                _isDataLoaded = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to load Adjusted Trial Balance: {ex.Message}", "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;

        if (isLoading)
        {
            TableContainer.IsVisible = false;
            EmptyStateContainer.IsVisible = false;
            BalanceStatusCard.IsVisible = false;
        }
    }

    private void ShowEmptyState(string message)
    {
        EmptyStateContainer.IsVisible = true;
        EmptyStateLabel.Text = message;
        TableContainer.IsVisible = false;
        BalanceStatusCard.IsVisible = false;
    }
}
