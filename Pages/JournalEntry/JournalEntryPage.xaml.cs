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
        // Pop HARUS lewat Shell juga (bukan Navigation.PopAsync), karena push-nya
        // dilakukan lewat Shell.Current.GoToAsync(relative route). Mencampur kedua
        // API ini membuat internal stack Shell desync, dan navigasi Shell berikutnya
        // (mis. buka General Journal dari menu) gagal dengan "Ambiguous routes matched".
        _viewModel.RequestNavigationPop += async () => await Shell.Current.GoToAsync("..");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.ApplyPeriodHeaderAsync(TopHeader);
        await _viewModel.InitializeAsync(EntryId);
    }
}
