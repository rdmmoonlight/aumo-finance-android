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
    private readonly JournalService _journalService;
    private readonly CoaService _coaService;
    private List<AccountLookupDto> _allAccounts = new();

    public ObservableCollection<JournalLineViewModel> Lines { get; set; } = new();
    private readonly CultureInfo _usdCulture = new("en-US");

    public JournalEntryPage(JournalService journalService, CoaService coaService)
    {
        InitializeComponent();
        _journalService = journalService;
        _coaService = coaService;

        JournalTypePicker.SelectedIndex = 0; // Default to "General"
        EntryDatePicker.Date = DateTime.Today;

        LinesCollectionView.ItemsSource = Lines;

        // Add 2 initial empty lines for convenience
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
            if (accounts != null)
            {
                _allAccounts = accounts.Select(a => new AccountLookupDto
                {
                    Id = a.Id,
                    ReferenceNumber = a.ReferenceNumber,
                    AccountName = a.AccountName,
                    DisplayName = $"{a.ReferenceNumber} - {a.AccountName}"
                }).ToList();

                // Refresh existing lines with account choices
                foreach (var line in Lines)
                {
                    line.AvailableAccounts = _allAccounts;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
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
            var dto = new CreateJournalEntryDto
            {
                JournalType = JournalTypePicker.SelectedItem?.ToString() ?? "General",
                EntryDate = EntryDatePicker.Date,
                ReferenceNumber = ReferenceEntry.Text?.Trim(),
                Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
                Lines = Lines.Select(l => new CreateJournalLineDto
                {
                    AccountId = l.SelectedAccount?.Id ?? 0,
                    LineDescription = l.LineDescription,
                    Debit = l.Debit,
                    Credit = l.Credit
                }).ToList()
            };

            var (success, message) = await _journalService.CreateJournalEntryAsync(dto);

            if (success)
            {
                await this.DisplayAlertAsync("Success", "Journal entry recorded successfully!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await this.DisplayAlertAsync("Failed to Save", message, "OK");
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
        totalDebit = Lines.Sum(l => l.Debit);
        totalCredit = Lines.Sum(l => l.Credit);

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        {
            this.DisplayAlertAsync("Validation Error", "Please enter a transaction description.", "OK");
            return false;
        }

        if (Lines.Count < 2)
        {
            this.DisplayAlertAsync("Validation Error", "A journal entry must have at least 2 transaction lines.", "OK");
            return false;
        }

        foreach (var line in Lines)
        {
            if (line.SelectedAccount == null)
            {
                this.DisplayAlertAsync("Validation Error", "All lines must have a valid account selected.", "OK");
                return false;
            }
            if (line.Debit > 0 && line.Credit > 0)
            {
                this.DisplayAlertAsync("Validation Error", "A line cannot have both debit and credit amounts greater than zero.", "OK");
                return false;
            }
        }

        if (Math.Round(totalDebit - totalCredit, 2) != 0)
        {
            this.DisplayAlertAsync("Validation Error", "Total debits and credits must be balanced before saving.", "OK");
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
