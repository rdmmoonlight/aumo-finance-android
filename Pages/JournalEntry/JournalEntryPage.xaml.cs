using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.ViewModels;

namespace AumoFinance.Pages.JournalEntry;

[QueryProperty(nameof(EntryId), "entryId")]
public partial class JournalEntryPage : ContentPage
{
    private readonly JournalEntryViewModel _viewModel;

    public string? EntryId { get; set; }

    public JournalEntryPage(JournalEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Hubungkan event UI (DisplayAlert & Navigation) dari ViewModel
        _viewModel.RequestAlert += DisplayAlertAsync;
        _viewModel.RequestNavigationPop += SafePopAsync;
    }

    // Pop relatif (".."). Kalau Shell tetap melempar "Ambiguous routes matched"
    // (mis. sisa cache rute lama sebelum rebuild), fallback ke navigasi absolut
    // langsung ke GeneralJournalPage supaya tombol Cancel/Update tidak "diam saja".
    private async Task SafePopAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Relative pop failed, falling back to absolute route: {ex}");
            try
            {
                await Shell.Current.GoToAsync($"//{nameof(Pages.Main.MainPage)}/{nameof(Pages.Reports.GeneralJournal.GeneralJournalPage)}");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"Absolute fallback pop also failed: {fallbackEx}");
                await DisplayAlertAsync("Navigation Error", "Couldn't return to the previous page. Please use the app menu.", "OK");
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.ApplyPeriodHeaderAsync(TopHeader);
        await _viewModel.InitializeAsync(EntryId);
    }
}
