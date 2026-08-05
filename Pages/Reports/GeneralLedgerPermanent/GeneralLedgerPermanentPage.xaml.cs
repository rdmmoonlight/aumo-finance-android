using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace AumoFinance.Pages;

public partial class GeneralLedgerPermanentPage : ContentPage
{
    public GeneralLedgerPermanentPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLedgerDataAsync();
    }

    private async Task LoadLedgerDataAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        LedgersCollectionView.IsVisible = false;
        EmptyStateContainer.IsVisible = false;

        try
        {
            await Task.Delay(400); // Simulasi ambil data

            var dummyData = GetDummyPermanentLedgers();

            if (dummyData == null || !dummyData.Any())
            {
                EmptyStateContainer.IsVisible = true;
            }
            else
            {
                LedgersCollectionView.ItemsSource = dummyData;
                LedgersCollectionView.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal memuat General Ledger: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnGeneralJournalClicked(object sender, EventArgs e)
    {
        // Navigasi ke Halaman General Journal
        await DisplayAlert("Navigasi", "Membuka General Journal", "OK");
    }

    private List<LedgerAccountDisplayModel> GetDummyPermanentLedgers()
    {
        return new List<LedgerAccountDisplayModel>
        {
            new LedgerAccountDisplayModel
            {
                AccountId = Guid.NewGuid(),
                ReferenceNumber = "1101",
                AccountName = "Kas & Bank",
                Type = "Asset",
                NormalBalanceIsDebit = true,
                EndingBalance = 12500000,
                Lines = new List<LedgerLineDisplayModel>
                {
                    new LedgerLineDisplayModel { EntryDate = new DateTime(2026, 1, 5), Description = "Saldo Awal", Debit = 10000000, Credit = 0, RunningBalance = 10000000 },
                    new LedgerLineDisplayModel { EntryDate = new DateTime(2026, 1, 15), Description = "Pendapatan Jasa", Debit = 2500000, Credit = 0, RunningBalance = 12500000 }
                }
            },
            new LedgerAccountDisplayModel
            {
                AccountId = Guid.NewGuid(),
                ReferenceNumber = "2101",
                AccountName = "Utang Usaha",
                Type = "Liability",
                NormalBalanceIsDebit = false,
                EndingBalance = 2000000,
                Lines = new List<LedgerLineDisplayModel>
                {
                    new LedgerLineDisplayModel { EntryDate = new DateTime(2026, 1, 10), Description = "Pembelian Perlengkapan Kredit", Debit = 0, Credit = 2000000, RunningBalance = 2000000 }
                }
            }
        };
    }
}
