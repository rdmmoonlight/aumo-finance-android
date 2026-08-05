using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class GeneralLedgerPermanentPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public GeneralLedgerPermanentPage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
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
            var period = await _accountingService.GetCurrentPeriodAsync(_currentUserId);
            if (period == null)
            {
                EmptyStateContainer.IsVisible = true;
                return;
            }

            var ledgers = await _accountingService.GetGeneralLedgerAsync(_currentUserId, period, isTemporary: false);

            if (!ledgers.Any())
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
            await this.DisplayAlertAsync("Error", $"Gagal terhubung ke database: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnGeneralJournalClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GeneralJournalPage");
    }
}
