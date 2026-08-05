using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace AumoFinance.Pages;

public partial class GeneralJournalPage : ContentPage
{
    private static readonly CultureInfo IdrCulture = new("id-ID");

    public GeneralJournalPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadJournalEntriesAsync();
    }

    private async Task LoadJournalEntriesAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        JournalCollectionView.IsVisible = false;
        EmptyStateContainer.IsVisible = false;

        try
        {
            // Simulation/Fetch Data - Sesuaikan dengan DbContext atau API Service kamu
            await Task.Delay(500); // Dummy delay

            // Contoh Model DTO untuk Tampilan MAUI
            var periodName = "Januari 2026";
            var isPeriodClosed = false;

            PeriodNameLabel.Text = periodName;
            ClosedBadge.IsVisible = isPeriodClosed;

            // Sample data mapping
            var sampleEntries = GetDummyJournalEntries();

            if (sampleEntries == null || !sampleEntries.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada entri jurnal pada periode {periodName}.";
            }
            else
            {
                JournalCollectionView.ItemsSource = sampleEntries;
                JournalCollectionView.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal memuat data jurnal: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddEntryClicked(object sender, EventArgs e)
    {
        // Navigasi ke halaman Tambah Jurnal (misal: /JournalEntry/Create)
        await DisplayAlert("Navigasi", "Membuka Form Tambah Entri Jurnal", "OK");
    }

    private async void OnEditEntryClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is Guid entryId)
        {
            // Navigasi ke halaman Edit Jurnal berdasarkan ID
            await DisplayAlert("Edit", $"Edit entri jurnal ID: {entryId}", "OK");
        }
    }

    #region Dummy Data Helpers (Sesuaikan ViewModel Anda)
    private List<JournalEntryDisplayModel> GetDummyJournalEntries()
    {
        return new List<JournalEntryDisplayModel>
        {
            new JournalEntryDisplayModel
            {
                Id = Guid.NewGuid(),
                EntryDate = new DateTime(2026, 1, 15),
                Lines = new List<JournalLineDisplayModel>
                {
                    new JournalLineDisplayModel
                    {
                        AccountName = "Kas & Bank",
                        RefNumber = "1101",
                        LineDescription = "Penerimaan Pendapatan Jasa",
                        Debit = 2500000,
                        Credit = 0
                    },
                    new JournalLineDisplayModel
                    {
                        AccountName = "Pendapatan Usaha",
                        RefNumber = "4101",
                        LineDescription = "",
                        Debit = 0,
                        Credit = 2500000
                    }
                }
            },
            new JournalEntryDisplayModel
            {
                Id = Guid.NewGuid(),
                EntryDate = new DateTime(2026, 1, 18),
                Lines = new List<JournalLineDisplayModel>
                {
                    new JournalLineDisplayModel
                    {
                        AccountName = "Beban Sewa Tempat",
                        RefNumber = "5201",
                        LineDescription = "Pembayaran sewa kantor Januari",
                        Debit = 750000,
                        Credit = 0
                    },
                    new JournalLineDisplayModel
                    {
                        AccountName = "Kas & Bank",
                        RefNumber = "1101",
                        LineDescription = "",
                        Debit = 0,
                        Credit = 750000
                    }
                }
            }
        };
    }
    #endregion
}

#region Display Models
public class JournalEntryDisplayModel
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public List<JournalLineDisplayModel> Lines { get; set; } = new();
}

public class JournalLineDisplayModel
{
    public string AccountName { get; set; } = string.Empty;
    public string RefNumber { get; set; } = string.Empty;
    public string LineDescription { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    public bool IsCredit => Credit > 0;
    public bool HasDescription => !string.IsNullOrWhiteSpace(LineDescription);

    public string AccountTextColor => IsCredit ? "#CBD5E1" : "#F8FAFC";
    public string AmountColor => IsCredit ? "#F87171" : "#4ADE80"; // Red for Credit, Green for Debit

    public string FormattedAmount
    {
        get
        {
            var culture = new CultureInfo("id-ID");
            return Debit > 0 
                ? $"Rp {Debit.ToString("N0", culture)}" 
                : $"Rp {Credit.ToString("N0", culture)}";
        }
    }
}
#endregion
