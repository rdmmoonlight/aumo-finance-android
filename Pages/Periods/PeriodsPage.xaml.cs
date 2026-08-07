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
        SetLoading(true);

        try
        {
            var (data, errorDetail) = await _periodService.GetPeriodsAsync();

            if (!string.IsNullOrEmpty(errorDetail))
            {
                await this.DisplayAlertAsync("Error", errorDetail, "OK");
                return;
            }

            if (data != null && data.Periods != null)
            {
                PeriodsCollectionView.ItemsSource = data.Periods;
                EmptyStateView.IsVisible = !data.Periods.Any();
                PeriodsCollectionView.IsVisible = data.Periods.Any();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadPeriodsAsync error: {ex}");
            await this.DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
            PeriodsRefreshView.IsRefreshing = false;
        }
    }

    private async void OnSelectPeriodClicked(object? sender, EventArgs e)
    {
        int? periodId = ExtractPeriodId(sender);
        if (periodId.HasValue)
        {
            var (success, message) = await _periodService.SetActivePeriodAsync(periodId.Value.ToString());

            if (success)
            {
                await LoadPeriodsAsync();
            }
            else
            {
                await this.DisplayAlertAsync("Failed", message, "OK");
            }
        }
    }

    private async void OnClosePeriodClicked(object? sender, EventArgs e)
    {
        int? periodId = ExtractPeriodId(sender);
        if (periodId.HasValue)
        {
            bool confirm = await this.DisplayAlertAsync(
                "Close Period",
                "Are you sure you want to close this accounting period? This action will lock all transactions in this period.",
                "Yes, Close",
                "Cancel");

            if (!confirm) return;

            var (success, message) = await _periodService.ClosePeriodAsync(periodId.Value);

            if (success)
            {
                await this.DisplayAlertAsync("Success", message, "OK");
                await LoadPeriodsAsync();
            }
            else
            {
                await this.DisplayAlertAsync("Failed", message, "OK");
            }
        }
    }

    private async void OnReopenPeriodClicked(object? sender, EventArgs e)
    {
        int? periodId = ExtractPeriodId(sender);
        if (periodId.HasValue)
        {
            var (success, message) = await _periodService.ReopenPeriodAsync(periodId.Value);

            if (success)
            {
                await this.DisplayAlertAsync("Success", message, "OK");
                await LoadPeriodsAsync();
            }
            else
            {
                await this.DisplayAlertAsync("Failed", message, "OK");
            }
        }
    }

    public async void OnAddPeriodClicked(object? sender, EventArgs e)
    {
        string name = await this.DisplayPromptAsync("New Period", "Enter period name (e.g. FY 2026 / March 2026):");
        if (string.IsNullOrWhiteSpace(name)) return;

        var (success, message) = await _periodService.CreatePeriodAsync(name.Trim(), DateTime.Today, DateTime.Today.AddMonths(1));

        if (success)
        {
            await this.DisplayAlertAsync("Success", "Accounting period created successfully.", "OK");
            await LoadPeriodsAsync();
        }
        else
        {
            await this.DisplayAlertAsync("Failed", message, "OK");
        }
    }

    public async void OnRefreshClicked(object? sender, EventArgs e) => await LoadPeriodsAsync();
    public async void OnRefreshViewRefreshing(object? sender, EventArgs e) => await LoadPeriodsAsync();

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }

    private static int? ExtractPeriodId(object? sender)
    {
        if (sender is Button button)
        {
            if (button.CommandParameter is int intVal) return intVal;
            if (button.CommandParameter is PeriodDto dto) return dto.Id;
        }
        return null;
    }
}
