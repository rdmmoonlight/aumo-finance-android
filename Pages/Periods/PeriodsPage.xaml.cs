using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;

namespace AumoFinance.Pages.Periods;

public partial class PeriodsPage : ContentPage
{
    private readonly PeriodService _periodService;

    public PeriodsPage(PeriodService periodService)
    {
        InitializeComponent();
        _periodService = periodService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPeriodsAsync();
    }

    private async Task LoadPeriodsAsync()
    {
        SetLoadingState(true);

        try
        {
            var (periods, activePeriodName, errorDetail) = await _periodService.GetPeriodsAsync();

            if (!string.IsNullOrEmpty(errorDetail))
            {
                await this.DisplayAlertAsync("Error Loading Periods", errorDetail, "OK");
                return;
            }

            ActivePeriodHeaderLabel.Text = string.IsNullOrWhiteSpace(activePeriodName)
                ? "No Active Period Selected"
                : $"Selected: {activePeriodName}";

            var viewModels = periods.Select(p => new PeriodItemViewModel
            {
                Id = p.Id,
                PeriodName = p.PeriodName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsClosed = p.IsClosed,
                IsSelected = p.IsSelected
            }).ToList();

            PeriodsCollectionView.ItemsSource = viewModels;
            EmptyStateView.IsVisible = !viewModels.Any();
            PeriodsCollectionView.IsVisible = viewModels.Any();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadPeriodsAsync error: {ex}");
            await this.DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            SetLoadingState(false);
            PeriodsRefreshView.IsRefreshing = false;
        }
    }

    private async void OnRefreshViewRefreshing(object? sender, EventArgs e)
    {
        await LoadPeriodsAsync();
    }

    private async void OnSelectPeriodClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is PeriodItemViewModel vm)
        {
            SetLoadingState(true);
            var (success, message) = await _periodService.SelectPeriodAsync(vm.Id);

            if (success)
            {
                await LoadPeriodsAsync();
            }
            else
            {
                await this.DisplayAlertAsync("Selection Failed", message, "OK");
                SetLoadingState(false);
            }
        }
    }

    private async void OnClosePeriodClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is PeriodItemViewModel vm)
        {
            bool confirm = await this.DisplayAlertAsync(
                "Close Period Confirmation",
                $"Are you sure you want to close the period '{vm.PeriodName}'? Closed periods cannot be modified.",
                "Yes, Close Period",
                "Cancel");

            if (!confirm) return;

            SetLoadingState(true);
            var (success, message) = await _periodService.ClosePeriodAsync(vm.Id);

            if (success)
            {
                await this.DisplayAlertAsync("Success", message, "OK");
                await LoadPeriodsAsync();
            }
            else
            {
                await this.DisplayAlertAsync("Close Failed", message, "OK");
                SetLoadingState(false);
            }
        }
    }

    private async void OnCreatePeriodClicked(object? sender, EventArgs e)
    {
        string periodName = await DisplayPromptAsync("New Period", "Enter accounting period name (e.g. August 2026):");
        if (string.IsNullOrWhiteSpace(periodName)) return;

        string startDateStr = await DisplayPromptAsync("Start Date", "Enter start date (YYYY-MM-DD):", initialValue: DateTime.Now.ToString("yyyy-MM-01"));
        if (!DateTime.TryParse(startDateStr, out DateTime startDate))
        {
            await this.DisplayAlertAsync("Invalid Date", "Please enter a valid start date.", "OK");
            return;
        }

        string endDateStr = await DisplayPromptAsync("End Date", "Enter end date (YYYY-MM-DD):", initialValue: DateTime.Now.ToString("yyyy-MM-31"));
        if (!DateTime.TryParse(endDateStr, out DateTime endDate))
        {
            await this.DisplayAlertAsync("Invalid Date", "Please enter a valid end date.", "OK");
            return;
        }

        SetLoadingState(true);

        var dto = new CreatePeriodDto
        {
            PeriodName = periodName.Trim(),
            StartDate = startDate,
            EndDate = endDate
        };

        var (success, message) = await _periodService.CreatePeriodAsync(dto);

        if (success)
        {
            await this.DisplayAlertAsync("Success", message, "OK");
            await LoadPeriodsAsync();
        }
        else
        {
            await this.DisplayAlertAsync("Create Failed", message, "OK");
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }
}

// ==========================================
// VIEW MODEL UNTUK ITEM LIST PERIODE
// ==========================================
public class PeriodItemViewModel
{
    public int Id { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public bool IsSelected { get; set; }

    public string DateRangeDisplay => $"{StartDate:MMM dd, yyyy} — {EndDate:MMM dd, yyyy}";
    public bool CanSelect => !IsSelected;
    public bool CanClose => IsSelected && !IsClosed;
}
