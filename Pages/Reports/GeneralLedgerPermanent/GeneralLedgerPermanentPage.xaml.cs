using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services.Reports;
using AumoFinance.Pages.Reports.GeneralJournal;

namespace AumoFinance.Pages;

public partial class GeneralLedgerPermanentPage : ContentPage
{
    private readonly GeneralLedgerService _generalLedgerService;

    public GeneralLedgerPermanentPage(GeneralLedgerService generalLedgerService)
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

        try
        {
            // isTemporary = false for General Ledger Permanent (Real Accounts: Assets, Liabilities, Equity)
            var (response, errorDetail) = await _generalLedgerService.GetGeneralLedgerReportAsync(isTemporary: false);

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
                LedgersCollectionView.ItemsSource = ledgers;
                LedgersCollectionView.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlertAsync("Error", $"Failed to connect to the database: {ex.Message}", "OK");
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
