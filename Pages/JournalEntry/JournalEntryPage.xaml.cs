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

[QueryProperty(nameof(EntryId), "entryId")]
public partial class JournalEntryPage : ContentPage
{
    private readonly JournalEntryService _journalEntryService;
    private readonly CoaService _coaService;
    private List<AccountLookupDto> _allAccounts = new();
    private int? _editingEntryId;
    private bool _isLocked;

    public ObservableCollection<JournalLineViewModel> Lines { get; set; } = new();
    private readonly CultureInfo _idCulture = new("id-ID");

    // Diset oleh Shell lewat query string "entryId" saat navigasi ke mode edit,
    // mis. GoToAsync($"{nameof(JournalEntryPage)}?entryId={id}").
    public string? EntryId { get; set; }

    public JournalEntryPage(JournalEntryService journalEntryService, CoaService coaService)
    {
        InitializeComponent();
        _journalEntryService = journalEntryService;
        _coaService = coaService;

        JournalTypePicker.SelectedIndex = 0; // Default: "General"
        EntryDatePicker.Date = DateTime.Today;

        LinesCollectionView.ItemsSource = Lines;

        AddNewLine();
        AddNewLine();

        UpdateTotals();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        bool isEditRequest = int.TryParse(EntryId, out var parsedId) && parsedId > 0;

        if (isEditRequest && _editingEntryId != parsedId)
        {
            _editingEntryId = parsedId;
            await LoadAccountsAsync();
            await LoadEntryForEditAsync(parsedId);
        }
        else if (!isEditRequest && _editingEntryId == null)
        {
            await LoadAccountsAsync();
            await RefreshNextTransactionNumberAsync();
        }
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

                foreach (var line in Lines)
                {
                    line.AvailableAccounts = _allAccounts;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadAccountsAsync Exception: {ex}");
        }
    }

    private async Task LoadEntryForEditAsync(int entryId)
    {
        SubmitButton.IsEnabled = false;

        var (entry, errorDetail) = await _journalEntryService.GetJournalEntryByIdAsync(entryId);

        if (!string.IsNullOrEmpty(errorDetail) || entry == null)
        {
            await DisplayAlertAsync("Error", errorDetail ?? "Journal entry not found.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        PageHeaderLabel.Text = "Edit Journal Entry";
        Title = "Edit Journal Entry";
        SubmitButton.Text = "Update Journal Entry";

        // Journal type & date
        int typeIndex = JournalTypePicker.ItemsSource
            .Cast<string>()
            .ToList()
            .FindIndex(t => string.Equals(t, entry.JournalType, StringComparison.OrdinalIgnoreCase));
        JournalTypePicker.SelectedIndex = typeIndex >= 0 ? typeIndex : 0;

        EntryDatePicker.Date = entry.EntryDate;

        // Transaction number sudah ada dari server — tampilkan apa adanya, jangan di-generate ulang.
        TransactionNumberLabel.Text = entry.TransactionNumber;
        TransactionNumberLabel.TextColor = Colors.White;

        // Rebuild lines dari data server, cocokkan AccountId dengan daftar akun yang sudah dimuat.
        Lines.Clear();
        foreach (var line in entry.Lines.OrderBy(l => l.LineOrder))
        {
            var lineVm = new JournalLineViewModel(_allAccounts, () => UpdateTotals())
            {
                SelectedAccount = _allAccounts.FirstOrDefault(a => a.Id == line.AccountId),
                DebitText = line.Debit > 0 ? line.Debit.ToString(CultureInfo.InvariantCulture) : string.Empty,
                CreditText = line.Credit > 0 ? line.Credit.ToString(CultureInfo.InvariantCulture) : string.Empty,
                LineDescription = line.LineDescription ?? string.Empty
            };
            Lines.Add(lineVm);
        }
        UpdateTotals();

        _isLocked = entry.IsLocked;
        SetLockedState(_isLocked);

        SubmitButton.IsEnabled = !_isLocked;
    }

    private void SetLockedState(bool isLocked)
    {
        LockedWarningBanner.IsVisible = isLocked;
        JournalTypePicker.IsEnabled = !isLocked;
        EntryDatePicker.IsEnabled = !isLocked;
        LinesCollectionView.IsEnabled = !isLocked;
        AddLineButton.IsEnabled = !isLocked;
    }

    private async void OnJournalTypeChanged(object? sender, EventArgs e)
    {
        // Nomor transaksi hanya di-preview untuk entry baru; saat edit, nomor yang sudah ada dipertahankan.
        if (_editingEntryId == null)
        {
            await RefreshNextTransactionNumberAsync();
        }
    }

    private async Task RefreshNextTransactionNumberAsync()
    {
        string journalType = JournalTypePicker.SelectedItem?.ToString() ?? "General";

        TransactionNumberLabel.Text = "Loading...";
        TransactionNumberLabel.TextColor = Color.FromArgb("#64748B");

        var (nextNumber, _) = await _journalEntryService.GetNextTransactionNumberAsync(journalType);

        if (!string.IsNullOrWhiteSpace(nextNumber))
        {
            TransactionNumberLabel.Text = nextNumber;
            TransactionNumberLabel.TextColor = Colors.White;
        }
        else
        {
            TransactionNumberLabel.Text = "Auto-generated";
            TransactionNumberLabel.TextColor = Color.FromArgb("#64748B");
        }
    }

    private void OnAddLineClicked(object? sender, EventArgs e) => AddNewLine();

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

        TotalDebitLabel.Text = string.Format(_idCulture, "Rp {0:N0}", totalDebit);
        TotalCreditLabel.Text = string.Format(_idCulture, "Rp {0:N0}", totalCredit);

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
        if (_isLocked) return;

        var (isValid, totalDebit, totalCredit) = await ValidateFormAsync();
        if (!isValid)
            return;

        SubmitButton.IsEnabled = false;

        try
        {
            if (_editingEntryId.HasValue)
            {
                await SaveAsUpdateAsync(_editingEntryId.Value);
            }
            else
            {
                await SaveAsCreateAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            SubmitButton.IsEnabled = true;
        }
    }

    private async Task SaveAsCreateAsync()
    {
        var requestDto = new CreateJournalEntryRequest
        {
            JournalType = JournalTypePicker.SelectedItem?.ToString() ?? "General",
            EntryDate = EntryDatePicker.Date ?? DateTime.Today, // EntryDatePicker.Date bertipe DateTime? di versi MAUI ini
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

        var (success, message, entryId, transactionNumber) = await _journalEntryService.CreateJournalEntryAsync(requestDto);

        if (success)
        {
            if (!string.IsNullOrWhiteSpace(transactionNumber))
            {
                TransactionNumberLabel.Text = transactionNumber;
                TransactionNumberLabel.TextColor = Colors.White;
            }

            string successMessage = string.IsNullOrWhiteSpace(transactionNumber)
                ? message
                : $"Journal Entry {transactionNumber} recorded successfully!";

            await DisplayAlertAsync("Success", successMessage, "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Posting Failed", message, "OK");
            SubmitButton.IsEnabled = true;
        }
    }

    private async Task SaveAsUpdateAsync(int entryId)
    {
        var updateDto = new UpdateJournalEntryRequest
        {
            JournalType = JournalTypePicker.SelectedItem?.ToString() ?? "General",
            EntryDate = EntryDatePicker.Date ?? DateTime.Today,
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

        var (success, message) = await _journalEntryService.EditJournalEntryAsync(entryId, updateDto);

        if (success)
        {
            await DisplayAlertAsync("Success", message, "OK");
            // Kembali ke General Journal; OnAppearing halaman itu akan me-refresh daftarnya sendiri.
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Update Failed", message, "OK");
            SubmitButton.IsEnabled = true;
        }
    }

    private async Task<(bool IsValid, decimal TotalDebit, decimal TotalCredit)> ValidateFormAsync()
    {
        var activeLines = Lines.Where(l => l.SelectedAccount != null && (l.Debit > 0 || l.Credit > 0)).ToList();
        decimal totalDebit = activeLines.Sum(l => l.Debit);
        decimal totalCredit = activeLines.Sum(l => l.Credit);

        if (activeLines.Count < 2)
        {
            await DisplayAlertAsync("Validation Error", "A journal entry must have at least 2 active transaction lines with valid accounts.", "OK");
            return (false, totalDebit, totalCredit);
        }

        foreach (var line in activeLines)
        {
            if (line.Debit > 0 && line.Credit > 0)
            {
                await DisplayAlertAsync("Validation Error", "A single transaction line cannot have both Debit and Credit amounts.", "OK");
                return (false, totalDebit, totalCredit);
            }
        }

        if (Math.Round(totalDebit - totalCredit, 2) != 0 || totalDebit == 0)
        {
            await DisplayAlertAsync("Validation Error", "Total debits must equal total credits and be greater than zero before saving.", "OK");
            return (false, totalDebit, totalCredit);
        }

        return (true, totalDebit, totalCredit);
    }
}

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
        set { _debitText = value; OnPropertyChanged(); _onChanged?.Invoke(); }
    }

    public string CreditText
    {
        get => _creditText;
        set { _creditText = value; OnPropertyChanged(); _onChanged?.Invoke(); }
    }

    public string LineDescription
    {
        get => _lineDescription;
        set { _lineDescription = value; OnPropertyChanged(); }
    }

    public decimal Debit => decimal.TryParse(DebitText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val) ? val : 0m;
    public decimal Credit => decimal.TryParse(CreditText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val) ? val : 0m;
}
