using System.Globalization;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class InputJournalPage : ContentPage
{
    private bool _isFormatting = false;
    private string _selectedType = "Expense"; // Default Pengeluaran
    private readonly CultureInfo _idrCulture = new("id-ID");

    public InputJournalPage()
    {
        InitializeComponent();
        EntryDatePicker.Date = DateTime.Today;
    }

    private void OnExpenseTypeSelected(object? sender, EventArgs e)
    {
        _selectedType = "Expense";
        
        // Visual Button Status
        ExpenseBtn.BackgroundColor = Color.FromArgb("#EF4444");
        ExpenseBtn.TextColor = Colors.White;
        ExpenseBtn.BorderWidth = 0;

        IncomeBtn.BackgroundColor = Color.FromArgb("#1E293B");
        IncomeBtn.TextColor = Color.FromArgb("#94A3B8");
        IncomeBtn.BorderWidth = 1;

        // Visual Label
        AmountTypeLabel.Text = "Nominal Pengeluaran (Rp)";
        AmountTypeLabel.TextColor = Color.FromArgb("#F87171");
    }

    private void OnIncomeTypeSelected(object? sender, EventArgs e)
    {
        _selectedType = "Income";

        // Visual Button Status
        IncomeBtn.BackgroundColor = Color.FromArgb("#10B981");
        IncomeBtn.TextColor = Colors.White;
        IncomeBtn.BorderWidth = 0;

        ExpenseBtn.BackgroundColor = Color.FromArgb("#1E293B");
        ExpenseBtn.TextColor = Color.FromArgb("#94A3B8");
        ExpenseBtn.BorderWidth = 1;

        // Visual Label
        AmountTypeLabel.Text = "Nominal Pemasukan (Rp)";
        AmountTypeLabel.TextColor = Color.FromArgb("#38BDF8");
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
        decimal amount = CleanAndParseDecimal(AmountEntry.Text);
        if (amount <= 0)
        {
            await this.DisplayAlertAsync("Peringatan", "Isikan nominal transaksi yang valid.", "OK");
            return;
        }

        string note = NoteEntry.Text?.Trim() ?? string.Empty;

        var transactionDto = new CreateSimpleTransactionDto
        {
            EntryDate = EntryDatePicker.Date,
            Type = _selectedType,
            Amount = amount,
            Note = note
        };

        if (Navigation.NavigationStack.FirstOrDefault(p => p is MainPage) is MainPage mainPage)
        {
            await Navigation.PopAsync();
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
