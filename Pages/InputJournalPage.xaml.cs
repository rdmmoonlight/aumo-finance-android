using System.Globalization;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class InputJournalPage : ContentPage
{
    private string _selectedType = "Income"; // Default ke Pemasukan
    private readonly CultureInfo _idrCulture = new("id-ID");

    public InputJournalPage()
    {
        InitializeComponent();
        EntryDatePicker.Date = DateTime.Today;

        // Buka tab Pemasukan secara otomatis saat pertama kali halaman dibuka
        SetIncomeTypeVisual();
    }

    private void OnExpenseTypeSelected(object? sender, EventArgs e)
    {
        SetExpenseTypeVisual();
    }

    private void OnIncomeTypeSelected(object? sender, EventArgs e)
    {
        SetIncomeTypeVisual();
    }

    private void SetExpenseTypeVisual()
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

    private void SetIncomeTypeVisual()
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
        UpdateAmountValidationState();
    }

    private void OnAmountFocused(object? sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;

        string rawText = new string((entry.Text ?? string.Empty).Where(char.IsDigit).ToArray());
        entry.Text = rawText;
    }

    private void OnAmountUnfocused(object? sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;

        string rawText = new string((entry.Text ?? string.Empty).Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(rawText))
        {
            entry.Text = string.Empty;
        }
        else if (decimal.TryParse(rawText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
        {
            entry.Text = string.Format(_idrCulture, "{0:N0}", value);
        }

        UpdateAmountValidationState();
    }

    private static readonly Color _validBorderColor = Color.FromArgb("#334155");
    private static readonly Color _invalidBorderColor = Color.FromArgb("#F87171");

    private void UpdateAmountValidationState()
    {
        bool isInvalid = CleanAndParseDecimal(AmountEntry.Text) <= 0 && !string.IsNullOrEmpty(AmountEntry.Text);

        AmountFieldBorder.Stroke = isInvalid ? _invalidBorderColor : _validBorderColor;
        AmountValidationLabel.IsVisible = isInvalid;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        decimal amount = CleanAndParseDecimal(AmountEntry.Text);
        if (amount <= 0)
        {
            AmountFieldBorder.Stroke = _invalidBorderColor;
            AmountValidationLabel.IsVisible = true;
            await this.DisplayAlertAsync("Peringatan", "Isikan nominal transaksi yang valid.", "OK");
            return;
        }

        string note = NoteEntry.Text?.Trim() ?? string.Empty;

        DateTime rawDate = EntryDatePicker.Date.GetValueOrDefault(DateTime.Today);
        DateTime utcDate = DateTime.SpecifyKind(rawDate.Date, DateTimeKind.Utc);

        var transactionDto = new CreateSimpleTransactionDto
        {
            EntryDate = utcDate,
            Type = _selectedType,
            Amount = amount,
            Note = note
        };

        // Matikan tombol simpan sementara
        SaveBtn.IsEnabled = false;

        if (Navigation.NavigationStack.FirstOrDefault(p => p is MainPage) is MainPage mainPage)
        {
            var (success, message) = await mainPage.ProcessNewTransactionAsync(transactionDto);

            if (success)
            {
                await this.DisplayAlertAsync("Berhasil", message, "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await this.DisplayAlertAsync("Gagal Input DB", string.IsNullOrWhiteSpace(message) ? "Terjadi kesalahan saat menyimpan data." : message, "OK");
                SaveBtn.IsEnabled = true;
            }
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
