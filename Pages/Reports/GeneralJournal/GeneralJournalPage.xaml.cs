using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;
using AumoFinance.Pages.JournalEntry;

namespace AumoFinance.Pages;

public partial class GeneralJournalPage : ContentPage
{
    private readonly ApiService _apiService;

    public GeneralJournalPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
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
            var (entries, selectedPeriodName, isPeriodClosed, errorDetail) = await _apiService.GetGeneralJournalAsync();

            if (errorDetail != null)
            {
                await DisplayAlertAsync("Koneksi Gagal", $"Gagal memuat General Journal dari server.\n\nDetail: {errorDetail}", "OK");
                return;
            }

            if (string.IsNullOrEmpty(selectedPeriodName))
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "Belum ada periode aktif yang dipilih.";
                return;
            }

            PeriodNameLabel.Text = selectedPeriodName;
            ClosedBadge.IsVisible = isPeriodClosed;

            if (!entries.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada entri jurnal pada periode {selectedPeriodName}.";
            }
            else
            {
                JournalCollectionView.ItemsSource = entries;
                JournalCollectionView.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlertAsync("Error", $"Terjadi kesalahan: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddEntryClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(JournalEntryPage));
    }

    private async void OnEditEntryClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int entryId)
        {
            // Catatan: JournalEntryEditPage belum diimplementasikan di sisi mobile
            // maupun terdaftar sebagai route — fitur edit entri jurnal belum tersedia.
            await DisplayAlertAsync("Informasi", $"Fitur edit entri jurnal (ID: {entryId}) belum diimplementasikan.", "OK");
        }
    }
}
