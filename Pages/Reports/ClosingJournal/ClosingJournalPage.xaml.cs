using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services.Reports;

namespace AumoFinance.Pages.Reports.ClosingJournal;

public partial class ClosingJournalPage : ContentPage
{
    private readonly ClosingJournalService _closingJournalService;

    public ClosingJournalPage(ClosingJournalService closingJournalService)
    {
        InitializeComponent();
        _closingJournalService = closingJournalService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReportAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadReportAsync();
    }

    private async void OnRefreshViewRefreshing(object sender, EventArgs e)
    {
        await LoadReportAsync();
        JournalRefreshView.IsRefreshing = false;
    }

    private async Task LoadReportAsync()
    {
        SetLoadingState(true);

        var (data, errorDetail) = await _closingJournalService.GetClosingJournalReportAsync();

        SetLoadingState(false);

        if (errorDetail != null)
        {
            await DisplayAlert("Error Loading Report", errorDetail, "OK");
            ShowEmptyState(true);
            return;
        }

        if (data == null || !data.Success || data.Entries == null || data.Entries.Count == 0)
        {
            SelectedPeriodHeaderLabel.Text = data?.SelectedPeriodName ?? "No Period Selected";
            ShowEmptyState(true);
            return;
        }

        SelectedPeriodHeaderLabel.Text = data.SelectedPeriodName ?? "Active Period";
        ShowEmptyState(false);

        // Bind data dengan wrapper ViewModel untuk format tampilan akuntansi
        var uiModels = data.Entries.Select(e => new ClosingJournalEntryViewModel(e)).ToList();
        JournalCollectionView.ItemsSource = uiModels;
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        
        if (isLoading)
        {
            EmptyStateView.IsVisible = false;
        }
    }

    private void ShowEmptyState(bool show)
    {
        EmptyStateView.IsVisible = show;
        JournalRefreshView.IsVisible = !show;
    }
}

// =========================================================================
// VIEW MODELS / DISPLAY WRAPPERS FOR UI FORMATTING
// =========================================================================

public class ClosingJournalEntryViewModel
{
    private readonly ClosingJournalEntryDto _dto;

    public ClosingJournalEntryViewModel(ClosingJournalEntryDto dto)
    {
        _dto = dto;
        Lines = dto.Lines.Select(l => new ClosingJournalLineViewModel(l)).ToList();
    }

    public string? ReferenceNumber => _dto.ReferenceNumber;
    public string? JournalType => string.IsNullOrWhiteSpace(_dto.JournalType) ? "CLOSING" : _dto.JournalType;
    public string FormattedDate => _dto.EntryDate.ToString("dd MMM yyyy", new CultureInfo("id-ID"));
    
    public List<ClosingJournalLineViewModel> Lines { get; }

    public string FormattedTotalDebit => _dto.TotalDebit == 0 ? "-" : _dto.TotalDebit.ToString("C0", new CultureInfo("id-ID"));
    public string FormattedTotalCredit => _dto.TotalCredit == 0 ? "-" : _dto.TotalCredit.ToString("C0", new CultureInfo("id-ID"));
}

public class ClosingJournalLineViewModel
{
    private readonly ClosingJournalLineDto _dto;

    public ClosingJournalLineViewModel(ClosingJournalLineDto dto)
    {
        _dto = dto;
    }

    public string ReferenceNumber => _dto.ReferenceNumber.ToString();
    public string? AccountName => _dto.AccountName;
    public string? LineDescription => _dto.LineDescription;
    
    public string FormattedDebit => _dto.Debit == 0 ? "-" : _dto.Debit.ToString("N0", new CultureInfo("id-ID"));
    public string FormattedCredit => _dto.Credit == 0 ? "-" : _dto.Credit.ToString("N0", new CultureInfo("id-ID"));
}
