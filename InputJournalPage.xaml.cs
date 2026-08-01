using System.Globalization;
using AumoFinance.Models;

namespace AumoFinance;

public partial class InputJournalPage : ContentPage
{
    private bool _isFormatting = false;
    private readonly CultureInfo _idrCulture = new("id-ID");

    public InputJournalPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event handler aman anti-crash untuk format ribuan otomatis saat mengetik.
    /// Memperbaiki warning CS8622 dengan menambahkan nullability handler (object? sender).
    /// </summary>
    private void OnAmountTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Mencegah infinite loop saat nilai Entry diubah dari kode
        if (_isFormatting || sender is not Entry entry) return;

        _isFormatting = true;

        try
        {
            // 1. Ekstrak hanya karakter angka murni
            string rawText = new string((e.NewTextValue ?? string.Empty).Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(rawText))
            {
                entry.Text = string.Empty;
                return;
            }

            // 2. Parse nilai angka
            if (decimal.TryParse(rawText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
            {
                // 3. Format dengan titik pemisah ribuan (tanpa simbol Rp)
                entry.Text = string.Format(_idrCulture, "{0:N0}", value);

                // Set posisi kursor tetap di paling akhir
                entry.CursorPosition = entry.Text.Length;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Format Error: {ex.Message}");
        }
        finally
        {
            _isFormatting = false;
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        // Validasi Keterangan
        string description = DescriptionEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(description))
        {
            // Memperbaiki warning CS0618: Menggunakan DisplayAlertAsync untuk .NET 10
            await this.DisplayAlertAsync("Peringatan", "Keterangan transaksi tidak boleh kosong.", "OK");
            return;
        }

        // Ambil nominal angka secara aman
        decimal debitValue = CleanAndParseDecimal(DebitEntry.Text);
        decimal creditValue = CleanAndParseDecimal(CreditEntry.Text);

        if (debitValue <= 0 && creditValue <= 0)
        {
            // Memperbaiki warning CS0618: Menggunakan DisplayAlertAsync untuk .NET 10
            await this.DisplayAlertAsync("Peringatan", "Isikan setidaknya nominal Debit atau Kredit.", "OK");
            return;
        }

        // Buat DTO Transaksi Sederhana sesuai spesifikasi ApiService
        var transactionDto = new CreateSimpleTransactionDto
        {
            EntryDate = DateTime.Today,
            Type = debitValue > 0 ? "Expense" : "Income",
            Amount = debitValue > 0 ? debitValue : creditValue,
            Note = description
        };

        // Kembali ke halaman utama & eksekusi antrean Sync 10 Detik
        if (Navigation.NavigationStack.FirstOrDefault(p => p is MainPage) is MainPage mainPage)
        {
            await Navigation.PopAsync();
            
            // Panggil antrean sync di MainPage via TopBarView
            _ = mainPage.ProcessNewTransactionAsync(transactionDto);
        }
        else
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Helper method untuk membersihkan string format ribuan menjadi tipe decimal murni.
    /// </summary>
    private static decimal CleanAndParseDecimal(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0m;

        // Ambil karakter digit saja
        string cleanText = new string(input.Where(char.IsDigit).ToArray());

        return decimal.TryParse(cleanText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result)
            ? result
            : 0m;
    }
}
