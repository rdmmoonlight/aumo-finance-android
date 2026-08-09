using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Models.Reports;
using AumoFinance.Services.Reports;

namespace AumoFinance.Pages.Reports.GeneralJournal;

public partial class GeneralJournalPage : ContentPage
{
    private readonly GeneralJournalService _generalJournalService;
    private readonly CultureInfo _usdCulture = new("en-US");

    public GeneralJournalPage(GeneralJournalService generalJournalService)
    {
        InitializeComponent();
        _generalJournalService = generalJournalService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadGeneralJournalAsync();
    }

    private async Task LoadGeneralJournalAsync()
    {
        SetLoadingState(true);

        try
        {
            var (data, errorDetail) = await _generalJournalService.GetGeneralJournalReportAsync();

            if (!string.IsNullOrEmpty(errorDetail))
            {
                await this.DisplayAlertAsync("Error", errorDetail, "OK");
                return;
            }

            if (data != null)
            {
                SelectedPeriodHeaderLabel.Text = string.IsNullOrWhiteSpace(data.SelectedPeriodName)
                    ? "No Active Period"
                    : data.SelectedPeriodName;

                var viewModels = (data.Entries ?? new List<GeneralJournalEntryReportDto>()).Select(e => new GeneralJournalEntryViewModel
                {
                    Id = e.Id,
                    EntryDate = e.EntryDate,
                    JournalType = e.JournalType ?? "General",
                    TransactionNumber = e.TransactionNumber ?? string.Empty,
                    Lines = (e.Lines ?? new List<GeneralJournalLineReportDto>()).Select(l => new GeneralJournalLineViewModel
                    {
                        AccountReferenceNumber = l.ReferenceNumber ?? 0,
                        AccountName = l.AccountName ?? string.Empty,
                        LineDescription = l.LineDescription,
                        Debit = l.Debit,
                        Credit = l.Credit,
                        UsdCulture = _usdCulture
                    }).ToList(),
                    UsdCulture = _usdCulture
                }).ToList();

                JournalCollectionView.ItemsSource = viewModels;
                EmptyStateView.IsVisible = !viewModels.Any();
                JournalCollectionView.IsVisible = viewModels.Any();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadGeneralJournalAsync error: {ex}");
            await this.DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            SetLoadingState(false);
            JournalRefreshView.IsRefreshing = false;
        }
    }

    public async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadGeneralJournalAsync();
    }

    public async void OnRefreshViewRefreshing(object? sender, EventArgs e)
    {
        await LoadGeneralJournalAsync();
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }
}
