using System.Diagnostics;
using System.Globalization;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class TestInputPage : ContentPage
{
    private string _selectedType = "Income";
    private readonly ApiService _apiService;

    public TestInputPage()
    {
        InitializeComponent();

        _apiService = new ApiService();
        TestDatePicker.Date = DateTime.Today;

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
        }
        else
        {
            ExpenseBtn.BackgroundColor = Color.FromArgb("#EF4444");
            ExpenseBtn.TextColor = Colors.White;
            IncomeBtn.BackgroundColor = Color.FromArgb("#1E293B");
            IncomeBtn.TextColor = Color.FromArgb("#94A3B8");
        }
    }

    private async void OnExecuteTestClicked(object? sender, EventArgs e)
    {
        ExecuteTestBtn.IsEnabled = false;
        ExecuteTestBtn.Text = "Mengirim data...";

        try
        {
            string rawAmount = new string((TestAmountEntry.Text ?? string.Empty).Where(char.IsDigit).ToArray());
            if (!decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("Validasi Tes", "Isikan angka nominal yang valid di atas 0.", "OK");
                ResetButtonState();
                return;
            }

            DateTime rawDate = TestDatePicker.Date.GetValueOrDefault(DateTime.Today);
            DateTime utcDate = new DateTime(rawDate.Year, rawDate.Month, rawDate.Day, 0, 0, 0, DateTimeKind.Utc);

            var testDto = new CreateSimpleTransactionDto
            {
                EntryDate = utcDate,
                Type = _selectedType,
                Amount = amount,
                Note = TestNoteEntry.Text?.Trim() ?? "Tes Input Langsung"
            };

            var (success, message) = await _apiService.PostSimpleTransactionAsync(testDto);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (success)
                {
                    await DisplayAlert("TES SUKSES!", $"PostgreSQL Merespons:\n{message}", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("TES GAGAL!", $"Detail Penolakan DB:\n{message}", "OK");
                    ResetButtonState();
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TestInputPage Exception: {ex}");
            await DisplayAlert("TES ERROR!", $"Exception Terdeteksi:\n{ex.Message}", "OK");
            ResetButtonState();
        }
    }

    private void ResetButtonState()
    {
        ExecuteTestBtn.IsEnabled = true;
        ExecuteTestBtn.Text = "KIRIM TES INPUT SEKARANG";
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
