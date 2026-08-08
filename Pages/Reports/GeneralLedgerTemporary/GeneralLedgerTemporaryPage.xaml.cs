using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services.Reports;
using AumoFinance.Pages.Reports.GeneralJournal;

namespace AumoFinance.Pages;

public partial class GeneralLedgerTemporaryPage : ContentPage
{
    private readonly GeneralLedgerService _generalLedgerService;

    public GeneralLedgerTemporaryPage(GeneralLedgerService generalLedgerService)
    {
        InitializeComponent();
        _generalLedgerService = generalLedgerService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLedgerDataAsync();
    }

    private async Task LoadLedgerDataAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        LedgersCollectionView.IsVisible = false;
        EmptyStateContainer.IsVisible = false;
        NetIncomeCard.IsVisible = false;

        try
        {
            // isTemporary = true for General Ledger Temporary (Nominal Accounts: Revenue, Expenses)
            var (response, errorDetail) = await _generalLedgerService.GetGeneralLedgerReportAsync(isTemporary: true);

            if (response == null || !response.Success)
            {
                EmptyStateContainer.IsVisible = true;
                return;
            }

            if (!response.HasPeriodSelected)
            {
                EmptyStateContainer.IsVisible = true;
                return;
            }

            var ledgers = response.Accounts;

            if (ledgers == null || !ledgers.Any())
            {
                EmptyStateContainer.IsVisible = true;
            }
            else
            {
                decimal netTotal = ledgers.Sum(l => l.NormalBalance.Equals("Debit", StringComparison.OrdinalIgnoreCase) ? -l.EndingBalance : l.EndingBalance);

                NetTotalLabel.Text = $"${netTotal:N2}";
                NetTotalLabel.TextColor = netTotal >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

                NetIncomeCard.IsVisible = true;
                LedgersCollectionView.ItemsSource = ledgers;
                LedgersCollectionView.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlert("Error", $"Failed to connect to the database: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnGeneralJournalClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(GeneralJournalPage));
    }
}
