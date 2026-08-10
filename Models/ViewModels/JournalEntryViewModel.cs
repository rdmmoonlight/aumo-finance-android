using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AumoFinance.Models.Dtos;
using AumoFinance.Services;

namespace AumoFinance.ViewModels;

public class JournalEntryViewModel : BindableObject
{
    private readonly JournalEntryService _journalEntryService;
    private readonly CoaService _coaService;
    private readonly PeriodService _periodService;
    private readonly CultureInfo _idCulture = new("id-ID");

    private List<AccountLookupDto> _allAccounts = new();
    private int? _editingEntryId;
    private bool _isLocked;
    private bool _isBusy;
    private string _pageTitle = "New Journal Entry";
    private string _submitButtonText = "Save Journal Entry";
    private string _selectedJournalType = "General";
    private DateTime _entryDate = DateTime.Today;
    private string _transactionNumber = "Auto-generated";
    private Color _transactionNumberColor = Color.FromArgb("#64748B");
    private string _totalDebitText = "Rp 0";
    private string _totalCreditText = "Rp 0";
    private bool _isBalanced;
    private bool _isSaveVisible;
    private Color _balanceBadgeBg = Color.FromArgb("#7F1D1D");
    private string _balanceStatusText = "UNBALANCED";
    private Color _balanceStatusTextColor = Color.FromArgb("#FCA5A5");

    public JournalEntryViewModel(
        JournalEntryService journalEntryService, 
        CoaService coaService, 
        PeriodService periodService)
    {
        _journalEntryService = journalEntryService;
        _coaService = coaService;
        _periodService = periodService;

        Lines = new ObservableCollection<JournalLineViewModel>();

        AddLineCommand = new Command(AddNewLine);
        RemoveLineCommand = new Command<JournalLineViewModel>(RemoveLine);
        SaveJournalCommand = new Command(async () => await SaveJournalAsync(), () => !IsBusy && IsBalanced && !IsLocked);
    }

    #region Properties

    public ObservableCollection<JournalLineViewModel> Lines { get; }

    public ICommand AddLineCommand { get; }
    public ICommand RemoveLineCommand { get; }
    public Command SaveJournalCommand { get; }

    // Event untuk memberikan signal/notifikasi ke View (misal: DisplayAlert atau PopNavigation)
    public event Func<string, string, string, Task>? RequestAlert;
    public event Func<Task>? RequestNavigationPop;

    public string PageTitle
    {
        get => _pageTitle;
        set { _pageTitle = value; OnPropertyChanged(); }
    }

    public string SubmitButtonText
    {
        get => _submitButtonText;
        set { _submitButtonText = value; OnPropertyChanged(); }
    }

    public string SelectedJournalType
    {
        get => _selectedJournalType;
        set
        {
            if (_selectedJournalType != value)
            {
                _selectedJournalType = value;
                OnPropertyChanged();
                if (!_editingEntryId.HasValue)
                {
                    _ = RefreshNextTransactionNumberAsync();
                }
            }
        }
    }

    public DateTime EntryDate
    {
        get => _entryDate;
        set { _entryDate = value; OnPropertyChanged(); }
    }

    public string TransactionNumber
    {
        get => _transactionNumber;
        set { _transactionNumber = value; OnPropertyChanged(); }
    }

    public Color TransactionNumberColor
    {
        get => _transactionNumberColor;
        set { _transactionNumberColor = value; OnPropertyChanged(); }
    }

    public string TotalDebitText
    {
        get => _totalDebitText;
        set { _totalDebitText = value; OnPropertyChanged(); }
    }

    public string TotalCreditText
    {
        get => _totalCreditText;
        set { _totalCreditText = value; OnPropertyChanged(); }
    }

    public bool IsBalanced
    {
        get => _isBalanced;
        set
        {
            _isBalanced = value;
            OnPropertyChanged();
            SaveJournalCommand.ChangeCanExecute();
        }
    }

    public bool IsSaveVisible
    {
        get => _isSaveVisible;
        set { _isSaveVisible = value; OnPropertyChanged(); }
    }

    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEditable));
            SaveJournalCommand.ChangeCanExecute();
        }
    }

    public bool IsEditable => !IsLocked;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            SaveJournalCommand.ChangeCanExecute();
        }
    }

    public Color BalanceBadgeBg
    {
        get => _balanceBadgeBg;
        set { _balanceBadgeBg = value; OnPropertyChanged(); }
    }

    public string BalanceStatusText
    {
        get => _balanceStatusText;
        set { _balanceStatusText = value; OnPropertyChanged(); }
    }

    public Color BalanceStatusTextColor
    {
        get => _balanceStatusTextColor;
        set { _balanceStatusTextColor = value; OnPropertyChanged(); }
    }

    #endregion

    #region Public Methods & Lifecycle

    public async Task InitializeAsync(string? rawEntryId)
    {
        ResetFormLines();

        bool isEditRequest = int.TryParse(rawEntryId, out var parsedId) && parsedId > 0;

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

    public async Task ApplyPeriodHeaderAsync(object topHeader)
    {
        await SelectedPeriodDisplayHelper.ApplyToTopBarAsync(topHeader, _periodService);
    }

    #endregion

    #region Private Logics

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
        IsSaveVisible = false;

        var (entry, errorDetail) = await _journalEntryService.GetJournalEntryByIdAsync(entryId);

        if (!string.IsNullOrEmpty(errorDetail) || entry == null)
        {
            if (RequestAlert != null)
                await RequestAlert.Invoke("Error", errorDetail ?? "Journal entry not found.", "OK");

            if (RequestNavigationPop != null)
                await RequestNavigationPop.Invoke();
            return;
        }

        PageTitle = "Edit Journal Entry";
        SubmitButtonText = "Update Journal Entry";

        SelectedJournalType = entry.JournalType;
        EntryDate = entry.EntryDate;

        TransactionNumber = entry.TransactionNumber;
        TransactionNumberColor = Colors.White;

        Lines.Clear();
        foreach (var line in entry.Lines.OrderBy(l => l.LineOrder))
        {
            var lineVm = new JournalLineViewModel(_allAccounts, UpdateTotals)
            {
                SelectedAccount = _allAccounts.FirstOrDefault(a => a.Id == line.AccountId),
                DebitText = line.Debit > 0 ? string.Format(_idCulture, "{0:N0}", line.Debit) : string.Empty,
                CreditText = line.Credit > 0 ? string.Format(_idCulture, "{0:N0}", line.Credit) : string.Empty,
                LineDescription = line.LineDescription ?? string.Empty
            };
            Lines.Add(lineVm);
        }

        IsLocked = entry.IsLocked;
        UpdateTotals();
    }

    private async Task RefreshNextTransactionNumberAsync()
    {
        TransactionNumber = "Loading...";
        TransactionNumberColor = Color.FromArgb("#64748B");

        var (nextNumber, _) = await _journalEntryService.GetNextTransactionNumberAsync(SelectedJournalType);

        if (!string.IsNullOrWhiteSpace(nextNumber))
        {
            TransactionNumber = nextNumber;
            TransactionNumberColor = Colors.White;
        }
        else
        {
            TransactionNumber = "Auto-generated";
            TransactionNumberColor = Color.FromArgb("#64748B");
        }
    }

    private void AddNewLine()
    {
        var newLine = new JournalLineViewModel(_allAccounts, UpdateTotals);
        Lines.Add(newLine);
        UpdateTotals();
    }

    private void RemoveLine(JournalLineViewModel? lineVm)
    {
        if (lineVm != null && Lines.Contains(lineVm))
        {
            Lines.Remove(lineVm);
            UpdateTotals();
        }
    }

    private void ResetFormLines()
    {
        Lines.Clear();
        AddNewLine();
        AddNewLine();
    }

    private void UpdateTotals()
    {
        decimal totalDebit = Lines.Sum(l => l.Debit);
        decimal totalCredit = Lines.Sum(l => l.Credit);

        TotalDebitText = string.Format(_idCulture, "Rp {0:N0}", totalDebit);
        TotalCreditText = string.Format(_idCulture, "Rp {0:N0}", totalCredit);

        IsBalanced = Math.Round(totalDebit - totalCredit, 2) == 0 && totalDebit > 0;

        if (IsBalanced)
        {
            BalanceBadgeBg = Color.FromArgb("#14532D");
            BalanceStatusText = "BALANCED";
            BalanceStatusTextColor = Color.FromArgb("#86EFAC");
        }
        else
        {
            BalanceBadgeBg = Color.FromArgb("#7F1D1D");
            BalanceStatusText = "UNBALANCED";
            BalanceStatusTextColor = Color.FromArgb("#FCA5A5");
        }

        IsSaveVisible = IsBalanced && !IsLocked;
    }

    private async Task SaveJournalAsync()
    {
        if (IsLocked) return;

        var isValid = await ValidateFormAsync();
        if (!isValid) return;

        IsBusy = true;

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
            if (RequestAlert != null)
                await RequestAlert.Invoke("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            UpdateTotals();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsCreateAsync()
    {
        var requestDto = new CreateJournalEntryRequest
        {
            JournalType = SelectedJournalType,
            EntryDate = EntryDate,
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

        var (success, message, _, transactionNumber) = await _journalEntryService.CreateJournalEntryAsync(requestDto);

        if (success)
        {
            string successMessage = string.IsNullOrWhiteSpace(transactionNumber)
                ? message
                : $"Journal Entry {transactionNumber} recorded successfully!";

            if (RequestAlert != null)
                await RequestAlert.Invoke("Success", successMessage, "OK");

            ResetFormLines();
            await RefreshNextTransactionNumberAsync();
        }
        else
        {
            if (RequestAlert != null)
                await RequestAlert.Invoke("Posting Failed", message, "OK");
            UpdateTotals();
        }
    }

    private async Task SaveAsUpdateAsync(int entryId)
    {
        var updateDto = new UpdateJournalEntryRequest
        {
            JournalType = SelectedJournalType,
            EntryDate = EntryDate,
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
            if (RequestAlert != null)
                await RequestAlert.Invoke("Success", message, "OK");

            if (RequestNavigationPop != null)
                await RequestNavigationPop.Invoke();
        }
        else
        {
            if (RequestAlert != null)
                await RequestAlert.Invoke("Update Failed", message, "OK");
            UpdateTotals();
        }
    }

    private async Task<bool> ValidateFormAsync()
    {
        var activeLines = Lines.Where(l => l.SelectedAccount != null && (l.Debit > 0 || l.Credit > 0)).ToList();
        decimal totalDebit = activeLines.Sum(l => l.Debit);
        decimal totalCredit = activeLines.Sum(l => l.Credit);

        if (activeLines.Count < 2)
        {
            if (RequestAlert != null)
                await RequestAlert.Invoke("Validation Error", "A journal entry must have at least 2 active transaction lines with valid accounts.", "OK");
            return false;
        }

        foreach (var line in activeLines)
        {
            if (line.Debit > 0 && line.Credit > 0)
            {
                if (RequestAlert != null)
                    await RequestAlert.Invoke("Validation Error", "A single transaction line cannot have both Debit and Credit amounts.", "OK");
                return false;
            }
        }

        if (Math.Round(totalDebit - totalCredit, 2) != 0 || totalDebit == 0)
        {
            if (RequestAlert != null)
                await RequestAlert.Invoke("Validation Error", "Total debits must equal total credits and be greater than zero before saving.", "OK");
            return false;
        }

        return true;
    }

    #endregion
}
