using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AumoFinance.Services;

namespace AumoFinance.Pages.JournalEntry;

public partial class JournalEntryPage : ContentPage
{
    private readonly JournalEntryService _journalEntryService;
    private readonly CoaService _coaService;
    private List<AccountLookupDto> _allAccounts = new();

    public ObservableCollection<JournalLineViewModel> Lines { get; set; } = new();
    private readonly CultureInfo _usdCulture = new("en-US");

    public JournalEntryPage(JournalEntryService journalEntryService, CoaService coaService)
    {
        InitializeComponent();
        _journalEntryService = journalEntryService;
        _coaService = coaService;

        JournalTypePicker.SelectedIndex = 0; // Default: "General"
        EntryDatePicker.Date = DateTime.Today;

        LinesCollectionView.ItemsSource = Lines;

        // Tambahkan 2 baris awal untuk kemudahan pengguna
        AddNewLine();
        AddNewLine();

        UpdateTotals();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            var (accounts, errorDetail) = await _coaService.GetAccountsAsync();
            if (accounts != null && accounts.Any())
            {
                _allAccounts = accounts.Select(a => new AccountLookupDto
                {
                    Id = a.Id,
                    ReferenceNumber = a.ReferenceNumber,
                    AccountName = a.AccountName,
                    DisplayName = $"{a.ReferenceNumber} - {a.AccountName}"
                }).ToList();

                // Perbarui daftar akun pada setiap baris jurnal yang sudah ada
                foreach (var line in Lines)
                {
                    line.AvailableAccounts = _allAccounts;
                }
            }
            else if (!string.IsNullOrEmpty(errorDetail))
            {
                Debug.WriteLine($"LoadAccountsAsync failed: {errorDetail}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadAccountsAsync Exception: {ex}");
        }
    }

    private void OnAddLineClicked(object? sender, EventArgs e)
    {
        AddNewLine();
    }

    private void AddNewLine()
    {
        var newLine = new JournalLineViewModel(_allAccounts, () => UpdateTotals());
        Lines.Add(newLine);
        UpdateTotals();
    }

    private void OnRemoveLineClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is JournalLineViewModel lineVm)
        {
            Lines.Remove(lineVm);
            UpdateTotals();
        }
    }

    private void UpdateTotals()
    {
        decimal totalDebit = Lines.Sum(l => l.Debit);
        decimal totalCredit = Lines.Sum(l => l.Credit);

        TotalDebitLabel.Text = totalDebit.ToString("C2", _usdCulture);
        TotalCreditLabel.Text = totalCredit.ToString("C2", _usdCulture);

        bool isBalanced = Math.Round(totalDebit - totalCredit, 2) == 0 && totalDebit > 0;

        if (isBalanced)
        {
            BalanceBadge.BackgroundColor = Color.FromArgb("#14532D");
            BalanceStatusLabel.Text = "BALANCED";
            BalanceStatusLabel.TextColor = Color.FromArgb("#86EFAC");
        }
        else
        {
            BalanceBadge.BackgroundColor = Color.FromArgb("#7F1D1D");
            BalanceStatusLabel.Text = "UNBALANCED";
            BalanceStatusLabel.TextColor = Color.FromArgb("#FCA5A5");
        }
    }

    private async void OnSaveJournalClicked(object? sender, EventArgs e)
    {
        if (!ValidateForm(out decimal totalDebit, out decimal totalCredit))
            return;

        SubmitButton.IsEnabled = false;

        try
        {
            var requestDto = new CreateJournalEntryRequest
            {
                JournalType = JournalTypePicker.SelectedItem?.ToString() ?? "General",
                EntryDate = EntryDatePicker.Date,
                Lines = Lines
                    .Where(l => l.SelectedAccount != null && (l.Debit > 0 || l.Credit > 0))
                    .Select(l => new JournalEntryLineRequest
                    {
                        AccountId = l.SelectedAccount!.Id,
                        LineDescription = l.LineDescription,
                        Debit = l.Debit,
                        Credit = l.Credit
                    }).ToList()
            };

            var (success, message, entryId, refNumber) = await _journalEntryService.CreateJournalEntryAsync(requestDto);

            if (success)
            {
                string successMessage = string.IsNullOrWhiteSpace(refNumber)
                    ? message
                    : $"Journal Entry {refNumber} recorded successfully!";

                await this.DisplayAlertAsync("Success", successMessage, "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await this.DisplayAlertAsync("Posting Failed", message, "OK");
                SubmitButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            SubmitButton.IsEnabled = true;
        }
    }

    private bool ValidateForm(out decimal totalDebit, out decimal totalCredit)
    {
        var activeLines = Lines.Where(l => l.SelectedAccount != null && (l.Debit > 0 || l.Credit > 0)).ToList();
        totalDebit = activeLines.Sum(l => l.Debit);
        totalCredit = activeLines.Sum(l => l.Credit);

        if (activeLines.Count < 2)
        {
            this.DisplayAlertAsync("Validation Error", "A journal entry must have at least 2 active transaction lines with valid accounts.", "OK");
            return false;
        }

        foreach (var line in activeLines)
        {
            if (line.Debit > 0 && line.Credit > 0)
            {
                this.DisplayAlertAsync("Validation Error", "A single transaction line cannot have both Debit and Credit amounts.", "OK");
                return false;
            }
        }

        if (Math.Round(totalDebit - totalCredit, 2) != 0 || totalDebit == 0)
        {
            this.DisplayAlertAsync("Validation Error", "Total debits must equal total credits and be greater than zero before saving.", "OK");
            return false;
        }

        return true;
    }
}

// ==========================================
// VIEW MODELS & DTO HELPERS
// ==========================================
public class AccountLookupDto
{
    public int Id { get; set; }
    public int ReferenceNumber { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class JournalLineViewModel : BindableObject
{
    private readonly Action _onChanged;
    private List<AccountLookupDto> _availableAccounts;
    private AccountLookupDto? _selectedAccount;
    private string _debitText = string.Empty;
    private string _creditText = string.Empty;
    private string _lineDescription = string.Empty;

    public JournalLineViewModel(List<AccountLookupDto> availableAccounts, Action onChanged)
    {
        _availableAccounts = availableAccounts;
        _onChanged = onChanged;
    }

    public List<AccountLookupDto> AvailableAccounts
    {
        get => _availableAccounts;
        set { _availableAccounts = value; OnPropertyChanged(); }
    }

    public AccountLookupDto? SelectedAccount
    {
        get => _selectedAccount;
        set { _selectedAccount = value; OnPropertyChanged(); }
    }

    public string DebitText
    {
        get => _debitText;
        set
        {
            _debitText = value;
            OnPropertyChanged();
            _onChanged?.Invoke();
        }
    }

    public string CreditText
    {
        get => _creditText;
        set
        {
            _creditText = value;
            OnPropertyChanged();
            _onChanged?.Invoke();
        }
    }

    public string LineDescription
    {
        get => _lineDescription;
        set { _lineDescription = value; OnPropertyChanged(); }
    }

    public decimal Debit => decimal.TryParse(DebitText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val) ? val : 0m;
    public decimal Credit => decimal.TryParse(CreditText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val) ? val : 0m;
}
