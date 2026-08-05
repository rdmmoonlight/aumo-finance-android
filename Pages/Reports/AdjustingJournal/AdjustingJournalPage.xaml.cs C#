using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class AdjustingJournalPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public AdjustingJournalPage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
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
            var period = await _accountingService.GetCurrentPeriodAsync(_currentUserId);

            if (period == null)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "Belum ada periode aktif yang dipilih.";
                return;
            }

            PeriodNameLabel.Text = period.PeriodName;
            ClosedBadge.IsVisible = period.IsClosed;

            // Ambil general journal dengan filter JournalType == "Adjusting" via AccountingService
            // Atau Anda bisa menambahkan method khusus GetAdjustingJournalAsync di AccountingService jika diperlukan.
            var entries = await _accountingService.GetGeneralJournalAsync(_currentUserId, period);
            var adjustingEntries = entries.Where(j => j.JournalType == "Adjusting").ToList();

            if (!adjustingEntries.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada adjusting entries pada periode {period.PeriodName}.";
            }
            else
            {
                // Mapping ke struktur tampilan card (menggunakan ViewModel yang serupa dengan GeneralJournal)
                var displayList = adjustingEntries.Select(e => new JournalEntryDisplayModel
                {
                    Id = e.Id,
                    EntryDate = e.EntryDate,
                    Lines = e.Lines.OrderBy(l => l.LineOrder).Select(l => new JournalLineDisplayModel
                    {
                        AccountName = l.Account?.AccountName ?? "-",
                        RefNumber = l.Account?.ReferenceNumber ?? "-",
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
            await DisplayAlert("Error", $"Gagal memuat data: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddAdjustingEntryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//JournalEntryCreatePage?type=Adjusting");
    }

    private async void OnEditEntryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Guid entryId)
        {
            await Shell.Current.GoToAsync($"//JournalEntryEditPage?id={entryId}");
        }
    }
}
