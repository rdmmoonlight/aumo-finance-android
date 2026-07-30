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

    private async void OnSaveJournalClicked(object? sender, EventArgs e)
    {
        if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlertAsync("Peringatan", "Nominal harus lebih besar dari 0.", "OK");
            return;
        }

        var dto = new CreateSimpleTransactionDto
        {
            EntryDate = EntryDatePicker.Date ?? DateTime.Today,
            Type = _selectedType,
            Amount = amount,
            Note = string.IsNullOrWhiteSpace(NoteEntry.Text) ? null : NoteEntry.Text
        };

        var (success, message) = await _apiService.PostSimpleTransactionAsync(dto);

        if (success)
        {
            await DisplayAlertAsync("Sukses", message, "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Gagal", message, "OK");
        }
    }
}
