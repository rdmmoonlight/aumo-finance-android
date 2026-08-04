using System.Diagnostics;
using System.Globalization;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class InputJournalPage : ContentPage
{
    private string _selectedType = "Income";
    private readonly CultureInfo _idrCulture = new("id-ID");
    private readonly ApiService _apiService;

    public InputJournalPage()
    {
        InitializeComponent();

        _apiService = new ApiService();
        EntryDatePicker.Date = DateTime.Today;

        SetTypeVisual("Income");
    }

    private void OnIncomeSelected(object? sender, EventArgs e)
    {
        SetTypeVisual("Income");
    }

    private void OnExpenseSelected(object? sender, EventArgs e)
    {
        SetTypeVisual("Expense");
    }

    private void SetTypeVisual(string type)
    {
        _selectedType = type;
        if (type == "Income")
        {
            IncomeBtn.BackgroundColor = Color.FromArgb("#10B981");
            IncomeBtn.TextColor = Colors.White;
            ExpenseBtn.BackgroundColor = Color.FromArgb("#1E293B");
            ExpenseBtn.TextColor = Color.FromArgb("#94A3B8");

            AmountTypeLabel.Text = "Nominal Pemasukan (Rp)";
            AmountTypeLabel.TextColor = Color.FromArgb("#38BDF8");
        }
        else
        {
            ExpenseBtn.BackgroundColor = Color.FromArgb("#EF4444");
            ExpenseBtn.TextColor = Colors.White;
            IncomeBtn.BackgroundColor = Color.FromArgb("#1E293B");
            IncomeBtn.TextColor = Color.FromArgb("#94A3B8");

            AmountTypeLabel.Text = "Nominal Pengeluaran (Rp)";
            AmountTypeLabel.TextColor = Color.FromArgb("#F87171");
        }
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
        SaveBtn.IsEnabled = false;
        SaveBtn.Text = "Menyimpan data...";

        try
        {
            decimal amount = CleanAndParseDecimal(AmountEntry.Text);
            if (amount <= 0)
            {
                AmountFieldBorder.Stroke = _invalidBorderColor;
                AmountValidationLabel.IsVisible = true;
                await this.DisplayAlertAsync("Peringatan", "Isikan nominal transaksi yang valid.", "OK");
                ResetButtonState();
                return;
            }

            string note = NoteEntry.Text?.Trim() ?? string.Empty;
            DateTime rawDate = EntryDatePicker.Date.GetValueOrDefault(DateTime.Today);
            DateTime utcDate = new DateTime(rawDate.Year, rawDate.Month, rawDate.Day, 0, 0, 0, DateTimeKind.Utc);

            var transactionDto = new CreateSimpleTransactionDto
            {
                EntryDate = utcDate,
                Type = _selectedType,
                Amount = amount,
                Note = note
            };

            // TEMBAK LANGSUNG KE APISERVICE (Sama persis seperti halaman tes)
            var (success, message) = await _apiService.PostSimpleTransactionAsync(transactionDto);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (success)
                {
                    await this.DisplayAlertAsync("Berhasil", string.IsNullOrWhiteSpace(message) ? "Data berhasil disimpan." : message, "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await this.DisplayAlertAsync("Gagal Input DB", string.IsNullOrWhiteSpace(message) ? "Terjadi kesalahan saat menyimpan data." : message, "OK");
                    ResetButtonState();
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnSaveClicked Exception: {ex}");
            await this.DisplayAlertAsync("Error", "Terjadi error saat menyimpan: " + ex.Message, "OK");
            ResetButtonState();
        }
    }

    private void ResetButtonState()
    {
        SaveBtn.IsEnabled = true;
        SaveBtn.Text = "SIMPAN TRANSAKSI";
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
