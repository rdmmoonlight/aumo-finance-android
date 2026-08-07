using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class PeriodsPage : ContentPage
{
    private readonly ApiService _apiService;

    public PeriodsPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPeriodsAsync();
    }

    private async Task LoadPeriodsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        PeriodsCollectionView.IsVisible = false;
        EmptyStateContainer.IsVisible = false;
        AlertCard.IsVisible = false;
        StopViewingButton.IsVisible = false;

        try
        {
            var (periods, selectedPeriodId, errorDetail) = await _apiService.GetPeriodsAsync();

            if (errorDetail != null)
            {
                await DisplayAlertAsync("Koneksi Gagal", $"Gagal memuat periode dari server.\n\nDetail: {errorDetail}", "OK");
                return;
            }

            if (selectedPeriodId != null)
            {
                StopViewingButton.IsVisible = true;
            }

            if (!periods.Any())
            {
                EmptyStateContainer.IsVisible = true;
                return;
            }

            var displayList = new List<PeriodDisplayModel>();
            foreach (var p in periods)
            {
                bool isSelected = selectedPeriodId == p.Id;

                // Cek apakah ada periode sebelumnya yang belum ditutup
                bool hasEarlierOpenPeriod = periods.Any(x => x.Id != p.Id && x.StartDate < p.StartDate && !x.IsClosed);
                bool canClose = !p.IsClosed && !hasEarlierOpenPeriod;

                displayList.Add(new PeriodDisplayModel
                {
                    Id = p.Id,
                    PeriodName = p.PeriodName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsClosed = p.IsClosed,
                    IsSelected = isSelected,
                    CanClose = canClose
                });
            }

            PeriodsCollectionView.ItemsSource = displayList;
            PeriodsCollectionView.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Gagal memuat periode: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnSelectPeriodClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int periodId)
        {
            var (success, message) = await _apiService.SelectPeriodAsync(periodId);
            ShowAlert(message, success);
            if (success)
            {
                await LoadPeriodsAsync();
            }
        }
    }

    private async void OnStopViewingClicked(object? sender, EventArgs e)
    {
        var (success, message) = await _apiService.ClearPeriodSelectionAsync();
        ShowAlert(message, success);
        if (success)
        {
            await LoadPeriodsAsync();
        }
    }

    private async void OnClosePeriodClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int periodId)
        {
            bool confirm = await DisplayAlertAsync("Konfirmasi Tutup Buku", "Apakah Anda yakin ingin menutup periode ini? Tindakan ini akan mengunci transaksi.", "Ya, Tutup", "Batal");
            if (confirm)
            {
                var (success, message) = await _apiService.ClosePeriodAsync(periodId);
                ShowAlert(message, success);
                if (success)
                {
                    await LoadPeriodsAsync();
                }
            }
        }
    }

    private async void OnOpenNewPeriodClicked(object? sender, EventArgs e)
    {
        string? periodName = await DisplayPromptAsync("Periode Baru", "Nama periode (mis. Agustus 2026):");
        if (string.IsNullOrWhiteSpace(periodName))
        {
            return;
        }

        string? startText = await DisplayPromptAsync("Tanggal Mulai", "Format: yyyy-MM-dd", initialValue: DateTime.Today.ToString("yyyy-MM-01"));
        if (!DateTime.TryParse(startText, out DateTime startDate))
        {
            await DisplayAlertAsync("Error", "Format tanggal mulai tidak valid.", "OK");
            return;
        }

        string? endText = await DisplayPromptAsync("Tanggal Selesai", "Format: yyyy-MM-dd", initialValue: DateTime.Today.ToString("yyyy-MM-dd"));
        if (!DateTime.TryParse(endText, out DateTime endDate))
        {
            await DisplayAlertAsync("Error", "Format tanggal selesai tidak valid.", "OK");
            return;
        }

        var (success, message) = await _apiService.CreatePeriodAsync(periodName.Trim(), startDate, endDate);
        ShowAlert(message, success);
        if (success)
        {
            await LoadPeriodsAsync();
        }
    }

    private void ShowAlert(string message, bool success)
    {
        AlertCard.BackgroundColor = success ? Color.FromArgb("#064E3B") : Color.FromArgb("#7F1D1D");
        AlertCard.Stroke = success ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626");
        AlertIcon.Text = success ? "✓" : "⚠️";
        AlertText.TextColor = success ? Color.FromArgb("#34D399") : Color.FromArgb("#FCA5A5");
        AlertText.Text = message;
        AlertCard.IsVisible = true;
    }
}

public class PeriodDisplayModel
{
    public int Id { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public bool IsSelected { get; set; }
    public bool CanClose { get; set; }

    public string FormattedStartDate => StartDate.ToString("dd MMM yyyy");
    public string FormattedEndDate => EndDate.ToString("dd MMM yyyy");

    public string StatusText => IsClosed ? "Closed" : "Active";
    public Color StatusBackgroundColor => IsClosed ? Color.FromArgb("#334155") : Color.FromArgb("#064E3B");
    public Color StatusTextColor => IsClosed ? Color.FromArgb("#CBD5E1") : Color.FromArgb("#34D399");

    public Color CardBackgroundColor => IsSelected ? Color.FromArgb("#1E3A8A") : Color.FromArgb("#1E293B");
    public Color CardBorderColor => IsSelected ? Color.FromArgb("#3B82F6") : Color.FromArgb("#334155");
    public Color ViewButtonBackgroundColor => IsSelected ? Color.FromArgb("#2563EB") : Color.FromArgb("#334155");
}
