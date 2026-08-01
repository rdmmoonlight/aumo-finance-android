using System.Globalization;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance;

public partial class InputJournalPage : ContentPage
{
    private readonly ApiService _apiService = new();
    private string _selectedType = "Income";

    private static readonly Color SelectedColor = Color.FromArgb("#22C55E");
    private static readonly Color UnselectedColor = Color.FromArgb("#334155");

    public InputJournalPage()
    {
        InitializeComponent();
        UpdateToggleUi();
    }

    private void OnIncomeSelected(object? sender, EventArgs e)
    {
        _selectedType = "Income";
        UpdateToggleUi();
    }

    private void OnExpenseSelected(object? sender, EventArgs e)
    {
        _selectedType = "Expense";
        UpdateToggleUi();
    }

    private void UpdateToggleUi()
    {
        bool isIncome = _selectedType == "Income";
        IncomeButton.BackgroundColor = isIncome ? SelectedColor : UnselectedColor;
        ExpenseButton.BackgroundColor = !isIncome ? Color.FromArgb("#EF4444") : UnselectedColor;
    }

    private void OnAmountTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry) return;

        // Unsubscribe temporary to prevent recursive calls
        entry.TextChanged -= OnAmountTextChanged;

        // Ambil hanya digit angka
        string rawInput = new string(e.NewTextValue?.Where(char.IsDigit).ToArray());

        if (ulong.TryParse(rawInput, out ulong amount))
        {
            // Format menggunakan kultur Indonesia (pemisah ribuan titik)
            var cultureInfo = new CultureInfo("id-ID");
            string formattedText = amount.ToString("N0", cultureInfo);

            entry.Text = formattedText;
            entry.CursorPosition = formattedText.Length;
        }
        else
        {
            entry.Text = string.Empty;
        }

        // Subscribe kembali
        entry.TextChanged += OnAmountTextChanged;
    }

    private decimal GetParsedAmount()
    {
        // Bersihkan tanda titik pemisah ribuan sebelum parsing ke decimal
        string rawText = new string(AmountEntry.Text?.Where(char.IsDigit).ToArray());
        return decimal.TryParse(rawText, out decimal result) ? result : 0m;
    }

    private async void OnSaveJournalClicked(object? sender, EventArgs e)
    {
        decimal amount = GetParsedAmount();

        if (amount <= 0)
        {
            await DisplayAlert("Peringatan", "Nominal harus lebih besar dari 0.", "OK");
            return;
        }

        var dto = new CreateSimpleTransactionDto
        {
            EntryDate = EntryDatePicker.Date.GetValueOrDefault(DateTime.Today),
            Type = _selectedType,
            Amount = amount,
            Note = string.IsNullOrWhiteSpace(NoteEntry.Text) ? null : NoteEntry.Text
        };

        var (success, message) = await _apiService.PostSimpleTransactionAsync(dto);

        if (success)
        {
            await DisplayAlert("Sukses", message, "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Gagal", message, "OK");
        }
    }
}
