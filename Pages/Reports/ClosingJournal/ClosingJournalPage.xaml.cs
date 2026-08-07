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

        if (data == null || !data.Success || data.ClosingJournal == null || data.ClosingJournal.Groups.Count == 0)
        {
            SelectedPeriodHeaderLabel.Text = data?.SelectedPeriodName ?? "No Period Selected";
            ShowEmptyState(true);
            return;
        }

        SelectedPeriodHeaderLabel.Text = data.SelectedPeriodName ?? "Active Period";
        ShowEmptyState(false);

        // Bind Net Income Card
        var culture = new CultureInfo("id-ID");
        NetIncomeCard.IsVisible = true;
        NetIncomeLabel.Text = data.ClosingJournal.NetIncome.ToString("C0", culture);
        RetainedEarningsAccountLabel.Text = $"Tujuan: {data.ClosingJournal.RetainedEarningsAccountName ?? "Laba Ditahan"}";

        // Bind Group Items
        var groupViewModels = data.ClosingJournal.Groups
            .Select(g => new ClosingJournalGroupViewModel(g))
            .ToList();

        JournalCollectionView.ItemsSource = groupViewModels;
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;

        if (isLoading)
        {
            EmptyStateView.IsVisible = false;
            NetIncomeCard.IsVisible = false;
        }
    }

    private void ShowEmptyState(bool show)
    {
        EmptyStateView.IsVisible = show;
        JournalRefreshView.IsVisible = !show;
    }
}

// =========================================================================
// VIEW MODELS / DISPLAY WRAPPERS FOR NEW JSON
// =========================================================================

public class ClosingJournalGroupViewModel
{
    private readonly ClosingJournalGroupDto _dto;

    public ClosingJournalGroupViewModel(ClosingJournalGroupDto dto)
    {
        _dto = dto;
        Lines = dto.Lines.Select(l => new ClosingJournalLineViewModel(l)).ToList();
    }

    public string? Description => _dto.Description;
    public List<ClosingJournalLineViewModel> Lines { get; }

    public string FormattedTotalDebit => _dto.TotalDebit == 0 ? "-" : _dto.TotalDebit.ToString("N0", new CultureInfo("id-ID"));
    public string FormattedTotalCredit => _dto.TotalCredit == 0 ? "-" : _dto.TotalCredit.ToString("N0", new CultureInfo("id-ID"));
}

public class ClosingJournalLineViewModel
{
    private readonly ClosingJournalLineDto _dto;

    public ClosingJournalLineViewModel(ClosingJournalLineDto dto)
    {
        _dto = dto;
    }

    public string FormattedRef => _dto.ReferenceNumber == 0 ? "-" : _dto.ReferenceNumber.ToString();
    public string? AccountName => _dto.AccountName;

    public string FormattedDebit => _dto.Debit == 0 ? "-" : _dto.Debit.ToString("N0", new CultureInfo("id-ID"));
    public string FormattedCredit => _dto.Credit == 0 ? "-" : _dto.Credit.ToString("N0", new CultureInfo("id-ID"));
}
