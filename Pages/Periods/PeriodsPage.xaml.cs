using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.EntityFrameworkCore;
using AumoFinance.Services;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class PeriodsPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public PeriodsPage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
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
            var periods = await _accountingService.DbContext.Periods
                .Where(p => p.UserId == _currentUserId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            var selectedPeriodId = (await SelectedPeriodHelper.GetSelectedPeriodAsync(_accountingService.DbContext, _currentUserId))?.Id;

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
        if (sender is Button btn && btn.CommandParameter is Guid periodId)
        {
            try
            {
                var entity = await _accountingService.DbContext.Periods.FirstOrDefaultAsync(p => p.Id == periodId && p.UserId == _currentUserId);
                if (entity != null)
                {
                    await SelectedPeriodHelper.SelectPeriodAsync(_accountingService.DbContext, _currentUserId, entity.Id);
                    ShowAlert($"Berhasil melihat periode {entity.PeriodName}", success: true);
                    await LoadPeriodsAsync();
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Gagal memilih periode: {ex.Message}", success: false);
            }
        }
    }

    private async void OnStopViewingClicked(object? sender, EventArgs e)
    {
        try
        {
            await SelectedPeriodHelper.ClearSelectionAsync(_accountingService.DbContext, _currentUserId);
            ShowAlert("Berhenti melihat periode. Laporan disembunyikan.", success: true);
            await LoadPeriodsAsync();
        }
        catch (Exception ex)
        {
            ShowAlert($"Gagal: {ex.Message}", success: false);
        }
    }

    private async void OnClosePeriodClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Guid periodId)
        {
            var entity = await _accountingService.DbContext.Periods.FirstOrDefaultAsync(p => p.Id == periodId && p.UserId == _currentUserId);
            if (entity == null || entity.IsClosed) return;

            bool confirm = await DisplayAlertAsync("Konfirmasi Tutup Buku", $"Apakah Anda yakin ingin menutup periode {entity.PeriodName}? Tindakan ini akan mengunci transaksi.", "Ya, Tutup", "Batal");
            if (confirm)
            {
                try
                {
                    entity.IsClosed = true;
                    await _accountingService.DbContext.SaveChangesAsync();
                    ShowAlert($"Periode {entity.PeriodName} berhasil ditutup.", success: true);
                    await LoadPeriodsAsync();
                }
                catch (Exception ex)
                {
                    ShowAlert($"Gagal menutup periode: {ex.Message}", success: false);
                }
            }
        }
    }

    private async void OnOpenNewPeriodClicked(object? sender, EventArgs e)
    {
        // Arahkan ke halaman pembuatan periode baru jika sudah dibuat
        await DisplayAlertAsync("Informasi", "Form untuk membuka periode baru dapat dihubungkan ke halaman CreatePeriod.", "OK");
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
    public Guid Id { get; set; }
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
