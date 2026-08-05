using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace AumoFinance.Pages;

public partial class GeneralLedgerTemporaryPage : ContentPage
{
    private static readonly CultureInfo IdrCulture = new("id-ID");

    public GeneralLedgerTemporaryPage()
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
        NetIncomeCard.IsVisible = false;

        try
        {
            await Task.Delay(400); // Simulasi ambil data

            var dummyData = GetDummyTemporaryLedgers();

            if (dummyData == null || !dummyData.Any())
            {
                EmptyStateContainer.IsVisible = true;
            }
            else
            {
                // Hitung Net Income/Loss
                decimal netTotal = dummyData.Sum(l => l.NormalBalanceIsDebit ? -l.EndingBalance : l.EndingBalance);
                
                NetTotalLabel.Text = $"Rp {netTotal.ToString("N0", IdrCulture)}";
                NetTotalLabel.TextColor = netTotal >= 0 
                    ? Color.FromArgb("#4ADE80") 
                    : Color.FromArgb("#F87171");

                NetIncomeCard.IsVisible = true;
                LedgersCollectionView.ItemsSource = dummyData;
                LedgersCollectionView.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal memuat General Ledger Sementara: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnGeneralJournalClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Navigasi", "Membuka General Journal", "OK");
    }

    private List<LedgerAccountDisplayModel> GetDummyTemporaryLedgers()
    {
        return new List<LedgerAccountDisplayModel>
        {
            new LedgerAccountDisplayModel
            {
                AccountId = Guid.NewGuid(),
                ReferenceNumber = "4101",
                AccountName = "Pendapatan Usaha",
                Type = "Revenue",
                NormalBalanceIsDebit = false,
                EndingBalance = 2500000,
                Lines = new List<LedgerLineDisplayModel>
                {
                    new LedgerLineDisplayModel { EntryDate = new DateTime(2026, 1, 15), Description = "Penerimaan Pendapatan Jasa", Debit = 0, Credit = 2500000, RunningBalance = 2500000 }
                }
            },
            new LedgerAccountDisplayModel
            {
                AccountId = Guid.NewGuid(),
                ReferenceNumber = "5201",
                AccountName = "Beban Sewa",
                Type = "Expense",
                NormalBalanceIsDebit = true,
                EndingBalance = 750000,
                Lines = new List<LedgerLineDisplayModel>
                {
                    new LedgerLineDisplayModel { EntryDate = new DateTime(2026, 1, 18), Description = "Pembayaran sewa kantor", Debit = 750000, Credit = 0, RunningBalance = 750000 }
                }
            }
        };
    }
}
