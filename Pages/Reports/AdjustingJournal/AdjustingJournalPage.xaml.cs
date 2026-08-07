using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Models;
using AumoFinance.Services.Reports;
using AumoFinance.Pages.JournalEntry;

namespace AumoFinance.Pages;

public partial class AdjustingJournalPage : ContentPage
{
    private readonly AdjustingJournalService _adjusturingJournalService;

    public AdjustingJournalPage(AdjustingJournalService adjusturingJournalService)
    {
        InitializeComponent();
        _adjusturingJournalService = adjusturingJournalService;
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
            var (response, errorDetail) = await _adjusturingJournalService.GetAdjustingJournalReportAsync();

            if (response == null || !response.Success)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = errorDetail ?? "Gagal memuat data adjusting journal.";
                return;
            }

            if (!response.HasPeriodSelected)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "Belum ada periode aktif yang dipilih.";
                return;
            }

            PeriodNameLabel.Text = response.SelectedPeriodName;
            ClosedBadge.IsVisible = false; // Disesuaikan dengan API response

            var entries = response.Entries;

            if (entries == null || !entries.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada adjusting entries pada periode {response.SelectedPeriodName}.";
            }
            else
            {
                var displayList = entries.Select(e => new JournalEntryDisplayModel
                {
                    Id = e.Id,
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
            await this.DisplayAlertAsync("Error", $"Gagal memuat data: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddAdjustingEntryClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(JournalEntryPage)}?type=Adjusting");
    }

    private async void OnEditEntryClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int entryId)
        {
            await Shell.Current.GoToAsync($"//JournalEntryEditPage?id={entryId}");
        }
    }
}
