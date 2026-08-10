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
        _viewModel.RequestNavigationPop += async () => await Navigation.PopAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.ApplyPeriodHeaderAsync(TopHeader);
        await _viewModel.InitializeAsync(EntryId);
    }
}
