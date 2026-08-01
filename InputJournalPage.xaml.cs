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

    private void OnAmountTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isFormatting || sender is not Entry entry) return;

        _isFormatting = true;

        try
        {
            string rawText = new string((e.NewTextValue ?? string.Empty).Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(rawText))
            {
                entry.Text = string.Empty;
                return;
            }

            if (decimal.TryParse(rawText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
            {
                entry.Text = string.Format(_idrCulture, "{0:N0}", value);
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
        string description = DescriptionEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(description))
        {
            await this.DisplayAlertAsync("Peringatan", "Keterangan transaksi tidak boleh kosong.", "OK");
            return;
        }

        decimal debitValue = CleanAndParseDecimal(DebitEntry.Text);
        decimal creditValue = CleanAndParseDecimal(CreditEntry.Text);

        if (debitValue <= 0 && creditValue <= 0)
        {
            await this.DisplayAlertAsync("Peringatan", "Isikan setidaknya nominal Debit atau Kredit.", "OK");
            return;
        }

        var transactionDto = new CreateSimpleTransactionDto
        {
            EntryDate = DateTime.Today,
            Type = debitValue > 0 ? "Expense" : "Income",
            Amount = debitValue > 0 ? debitValue : creditValue,
            Note = description
        };

        if (Navigation.NavigationStack.FirstOrDefault(p => p is MainPage) is MainPage mainPage)
        {
            await Navigation.PopAsync();
            
            // Panggil method ProcessNewTransactionAsync
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

    private static decimal CleanAndParseDecimal(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0m;
        string cleanText = new string(input.Where(char.IsDigit).ToArray());
        return decimal.TryParse(cleanText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result) ? result : 0m;
    }
}
