using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Models;
using AumoFinance.Services.Reports;
using AumoFinance.Pages.JournalEntry;

namespace AumoFinance.Pages.Reports.AdjustingJournal;

public partial class AdjustingJournalPage : ContentPage
{
    private readonly AdjustingJournalService _adjustingJournalService;
    private readonly JournalEntryService _journalEntryService;

    public AdjustingJournalPage(AdjustingJournalService adjustingJournalService, JournalEntryService journalEntryService)
    {
        InitializeComponent();
        _adjustingJournalService = adjustingJournalService;
        _journalEntryService = journalEntryService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAdjustingEntriesAsync();
    }

    private async Task LoadAdjustingEntriesAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        AdjustingJournalCollectionView.IsVisible = false;
        EmptyStateContainer.IsVisible = false;

        try
        {
            var (response, errorDetail) = await _adjustingJournalService.GetAdjustingJournalReportAsync();

            if (response == null || !response.Success)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = errorDetail ?? "Failed to load adjusting journal data.";
                return;
            }

            if (!response.HasPeriodSelected)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "No active period selected.";
                return;
            }

            PeriodNameLabel.Text = response.SelectedPeriodName;
            ClosedBadge.IsVisible = response.IsPeriodClosed;

            var entries = response.Entries;

            if (entries == null || !entries.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"No adjusting entries found for period {response.SelectedPeriodName}.";
            }
            else
            {
                var displayList = entries.Select(e => new JournalEntryDisplayModel
                {
                    Id = e.Id,
                    TransactionNumber = e.TransactionNumber ?? string.Empty,
                    EntryDate = e.EntryDate,
                    Lines = e.Lines.Select(l => new JournalEntryLineDisplayModel
                    {
                        AccountName = l.AccountName ?? "-",
                        RefNumber = l.ReferenceNumber.ToString(),
                        LineDescription = l.LineDescription ?? string.Empty,
                        Debit = l.Debit,
                        Credit = l.Credit
                    }).ToList()
                }).ToList();

                AdjustingJournalCollectionView.ItemsSource = displayList;
                AdjustingJournalCollectionView.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlertAsync("Error", $"Failed to load data: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            AdjustingJournalRefreshView.IsRefreshing = false;
        }
    }

    private async void OnAddAdjustingEntryClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(JournalEntryPage)}?type=Adjusting");
    }

    public async void OnRefreshViewRefreshing(object? sender, EventArgs e)
    {
        await LoadAdjustingEntriesAsync();
    }

    private async void OnEditEntryClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int entryId)
        {
            await Shell.Current.GoToAsync($"{nameof(JournalEntryPage)}?entryId={entryId}");
        }
    }

    private async void OnDeleteEntryClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not JournalEntryDisplayModel entry)
            return;

        var label = string.IsNullOrWhiteSpace(entry.TransactionNumber)
            ? entry.Id.ToString()
            : entry.TransactionNumber;

        bool confirm = await this.DisplayAlertAsync(
            "Delete Adjusting Entry",
            $"Delete transaction {label}? This action cannot be undone.",
            "Yes, Delete",
            "Cancel");

        if (!confirm) return;

        var (success, message) = await _journalEntryService.DeleteJournalEntryAsync(entry.Id);

        if (success)
        {
            await LoadAdjustingEntriesAsync();
        }
        else
        {
            await this.DisplayAlertAsync("Delete Failed", message, "OK");
        }
    }
}
