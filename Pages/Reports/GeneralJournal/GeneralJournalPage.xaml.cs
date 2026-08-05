using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class GeneralJournalPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId; // Ambil dari session login/auth

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
            await DisplayAlert("Error", $"Gagal memuat database: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddEntryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//JournalEntryCreatePage");
    }

    private async void OnEditEntryClicked(object sender, EventArgs e)
    {
        // Menggunakan "Button" (karena di XAML sebelumnya ImageButton sudah diganti menjadi Button)
        if (sender is Button btn && btn.CommandParameter is Guid entryId)
        {
            await Shell.Current.GoToAsync($"//JournalEntryEditPage?id={entryId}");
        }
    }
}
