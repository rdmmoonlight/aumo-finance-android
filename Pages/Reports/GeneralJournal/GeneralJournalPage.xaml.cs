using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;
using AumoFinance.Pages.JournalEntry;

namespace AumoFinance.Pages;

public partial class GeneralJournalPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public GeneralJournalPage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadJournalEntriesAsync();
    }

    private async Task LoadJournalEntriesAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        JournalCollectionView.IsVisible = false;
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
            ClosedBadge.IsVisible = period.IsClosed;

            var entries = await _accountingService.GetGeneralJournalAsync(_currentUserId, period);

            if (!entries.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada entri jurnal pada periode {period.PeriodName}.";
            }
            else
            {
                JournalCollectionView.ItemsSource = entries;
                JournalCollectionView.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlertAsync("Error", $"Gagal memuat database: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddEntryClicked(object? sender, EventArgs e)
    {
        // Catatan: route sebelumnya "JournalEntryCreatePage" tidak pernah terdaftar
        // di AppShell (yang ada "JournalEntryPage") — diperbaiki ke nama yang benar.
        await Shell.Current.GoToAsync(nameof(JournalEntryPage));
    }

    private async void OnEditEntryClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Guid entryId)
        {
            await Shell.Current.GoToAsync($"//JournalEntryEditPage?id={entryId}");
        }
    }
}
