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
    private bool _isLocked, _isBusy, _isBalanced, _isSaveVisible;
    private string _pageTitle = "New Journal Entry", _submitButtonText = "Save Journal Entry";
    private string _selectedJournalType = "General", _transactionNumber = "Auto-generated";
    private string _totalDebitText = "Rp 0", _totalCreditText = "Rp 0", _balanceStatusText = "UNBALANCED";
    private DateTime _entryDate = DateTime.Today;
    private Color _transactionNumberColor = Color.FromArgb("#64748B"), _balanceBadgeBg = Color.FromArgb("#7F1D1D"), _balanceStatusTextColor = Color.FromArgb("#FCA5A5");

    public JournalEntryViewModel(JournalEntryService journalEntryService, CoaService coaService, PeriodService periodService)
    {
        _journalEntryService = journalEntryService;
        _coaService = coaService;
        _periodService = periodService;

        Lines = new ObservableCollection<JournalLineViewModel>();
        AddLineCommand = new Command(AddNewLine);
        RemoveLineCommand = new Command<JournalLineViewModel>(RemoveLine);
        SaveJournalCommand = new Command(async () => await SaveJournalAsync(), () => !IsBusy && IsBalanced && !IsLocked);
    }

    #region Properties & Events

    public ObservableCollection<JournalLineViewModel> Lines { get; }
    public ICommand AddLineCommand { get; }
    public ICommand RemoveLineCommand { get; }
    public Command SaveJournalCommand { get; }

    public event Func<string, string, string, Task>? RequestAlert;
    public event Func<Task>? RequestNavigationPop;

    public string PageTitle { => _pageTitle; set => SetProperty(ref _pageTitle, value); }
    public string SubmitButtonText { => _submitButtonText; set => SetProperty(ref _submitButtonText, value); }
    public DateTime EntryDate { => _entryDate; set => SetProperty(ref _entryDate, value); }
    public string TransactionNumber { => _transactionNumber; set => SetProperty(ref _transactionNumber, value); }
    public Color TransactionNumberColor { => _transactionNumberColor; set => SetProperty(ref _transactionNumberColor, value); }
    public string TotalDebitText { => _totalDebitText; set => SetProperty(ref _totalDebitText, value); }
    public string TotalCreditText { => _totalCreditText; set => SetProperty(ref _totalCreditText, value); }
    public bool IsSaveVisible { => _isSaveVisible; set => SetProperty(ref _isSaveVisible, value); }
    public Color BalanceBadgeBg { => _balanceBadgeBg; set => SetProperty(ref _balanceBadgeBg, value); }
    public string BalanceStatusText { => _balanceStatusText; set => SetProperty(ref _balanceStatusText, value); }
    public Color BalanceStatusTextColor { => _balanceStatusTextColor; set => SetProperty(ref _balanceStatusTextColor, value); }
    public bool IsEditable => !IsLocked;

    public string SelectedJournalType
    {
        get => _selectedJournalType;
        set { if (SetProperty(ref _selectedJournalType, value) && !_editingEntryId.HasValue) _ = RefreshNextTransactionNumberAsync(); }
    }

    public bool IsBalanced
    {
        get => _isBalanced;
        set { if (SetProperty(ref _isBalanced, value)) SaveJournalCommand.ChangeCanExecute(); }
    }

    public bool IsLocked
    {
        get => _isLocked;
        set { if (SetProperty(ref _isLocked, value)) { OnPropertyChanged(nameof(IsEditable)); SaveJournalCommand.ChangeCanExecute(); } }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { if (SetProperty(ref _isBusy, value)) SaveJournalCommand.ChangeCanExecute(); }
    }

    #endregion

    #region Public Methods

    public async Task InitializeAsync(string? rawEntryId)
    {
        ResetFormLines();
        bool isEdit = int.TryParse(rawEntryId, out var parsedId) && parsedId > 0;

        if (isEdit && _editingEntryId != parsedId)
        {
            _editingEntryId = parsedId;
            await LoadAccountsAsync();
            await LoadEntryForEditAsync(parsedId);
        }
        else if (!isEdit && _editingEntryId == null)
        {
            await LoadAccountsAsync();
            await RefreshNextTransactionNumberAsync();
        }
    }

    public Task ApplyPeriodHeaderAsync(object topHeader) => SelectedPeriodDisplayHelper.ApplyToTopBarAsync(topHeader, _periodService);

    #endregion

    #region Private Logics

    private async Task LoadAccountsAsync()
    {
        try
        {
            var (accounts, _) = await _coaService.GetAccountsAsync();
            if (accounts?.Any() == true)
            {
                _allAccounts = accounts.Select(a => new AccountLookupDto
                {
                    Id = a.Id, ReferenceNumber = a.ReferenceNumber, AccountName = a.AccountName, DisplayName = $"{a.ReferenceNumber} - {a.AccountName}"
                }).ToList();

                foreach (var line in Lines) line.AvailableAccounts = _allAccounts;
            }
        }
        catch (Exception ex) { Debug.WriteLine($"LoadAccountsAsync Exception: {ex}"); }
    }

    private async Task LoadEntryForEditAsync(int entryId)
    {
        IsSaveVisible = false;
        var (entry, errorDetail) = await _journalEntryService.GetJournalEntryByIdAsync(entryId);

        if (!string.IsNullOrEmpty(errorDetail) || entry == null)
        {
            await ShowAlertAsync("Error", errorDetail ?? "Journal entry not found.");
            if (RequestNavigationPop != null) await RequestNavigationPop.Invoke();
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
            Lines.Add(new JournalLineViewModel(_allAccounts, UpdateTotals)
            {
                SelectedAccount = _allAccounts.FirstOrDefault(a => a.Id == line.AccountId),
                DebitText = line.Debit > 0 ? string.Format(_idCulture, "{0:N0}", line.Debit) : string.Empty,
                CreditText = line.Credit > 0 ? string.Format(_idCulture, "{0:N0}", line.Credit) : string.Empty,
                LineDescription = line.LineDescription ?? string.Empty
            });
        }

        IsLocked = entry.IsLocked;
        UpdateTotals();
    }

    private async Task RefreshNextTransactionNumberAsync()
    {
        TransactionNumber = "Loading...";
        TransactionNumberColor = Color.FromArgb("#64748B");
        var (nextNumber, _) = await _journalEntryService.GetNextTransactionNumberAsync(SelectedJournalType);

        TransactionNumber = !string.IsNullOrWhiteSpace(nextNumber) ? nextNumber : "Auto-generated";
        TransactionNumberColor = !string.IsNullOrWhiteSpace(nextNumber) ? Colors.White : Color.FromArgb("#64748B");
    }

    private void AddNewLine() { Lines.Add(new JournalLineViewModel(_allAccounts, UpdateTotals)); UpdateTotals(); }

    private void RemoveLine(JournalLineViewModel? lineVm) { if (lineVm != null && Lines.Remove(lineVm)) UpdateTotals(); }

    private void ResetFormLines() { Lines.Clear(); AddNewLine(); AddNewLine(); }

    private void UpdateTotals()
    {
        decimal totalDebit = Lines.Sum(l => l.Debit), totalCredit = Lines.Sum(l => l.Credit);
        TotalDebitText = string.Format(_idCulture, "Rp {0:N0}", totalDebit);
        TotalCreditText = string.Format(_idCulture, "Rp {0:N0}", totalCredit);

        IsBalanced = Math.Round(totalDebit - totalCredit, 2) == 0 && totalDebit > 0;
        BalanceBadgeBg = IsBalanced ? Color.FromArgb("#14532D") : Color.FromArgb("#7F1D1D");
        BalanceStatusText = IsBalanced ? "BALANCED" : "UNBALANCED";
        BalanceStatusTextColor = IsBalanced ? Color.FromArgb("#86EFAC") : Color.FromArgb("#FCA5A5");
        IsSaveVisible = IsBalanced && !IsLocked;
    }

    private async Task SaveJournalAsync()
    {
        if (IsLocked || !await ValidateFormAsync()) return;
        IsBusy = true;

        try
        {
            if (_editingEntryId.HasValue) await SaveAsUpdateAsync(_editingEntryId.Value);
            else await SaveAsCreateAsync();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"An unexpected error occurred: {ex.Message}");
            UpdateTotals();
        }
        finally { IsBusy = false; }
    }

    private async Task SaveAsCreateAsync()
    {
        var requestDto = new CreateJournalEntryRequest { JournalType = SelectedJournalType, EntryDate = EntryDate, Lines = GetActiveLineRequests() };
        var (success, message, _, txNum) = await _journalEntryService.CreateJournalEntryAsync(requestDto);

        if (success)
        {
            await ShowAlertAsync("Success", string.IsNullOrWhiteSpace(txNum) ? message : $"Journal Entry {txNum} recorded successfully!");
            ResetFormLines();
            await RefreshNextTransactionNumberAsync();
        }
        else { await ShowAlertAsync("Posting Failed", message); UpdateTotals(); }
    }

    private async Task SaveAsUpdateAsync(int entryId)
    {
        var updateDto = new UpdateJournalEntryRequest { JournalType = SelectedJournalType, EntryDate = EntryDate, Lines = GetActiveLineRequests() };
        var (success, message) = await _journalEntryService.EditJournalEntryAsync(entryId, updateDto);

        if (success)
        {
            await ShowAlertAsync("Success", message);
            if (RequestNavigationPop != null) await RequestNavigationPop.Invoke();
        }
        else { await ShowAlertAsync("Update Failed", message); UpdateTotals(); }
    }

    private List<JournalEntryLineRequest> GetActiveLineRequests() =>
        Lines.Where(l => l.SelectedAccount != null && (l.Debit > 0 || l.Credit > 0))
             .Select(l => new JournalEntryLineRequest { AccountId = l.SelectedAccount!.Id, LineDescription = l.LineDescription, Debit = l.Debit, Credit = l.Credit })
             .ToList();

    private async Task<bool> ValidateFormAsync()
    {
        var activeLines = Lines.Where(l => l.SelectedAccount != null && (l.Debit > 0 || l.Credit > 0)).ToList();
        decimal totalDebit = activeLines.Sum(l => l.Debit), totalCredit = activeLines.Sum(l => l.Credit);

        if (activeLines.Count < 2) return await ShowAlertAndReturnFalse("Validation Error", "A journal entry must have at least 2 active transaction lines with valid accounts.");
        if (activeLines.Any(l => l.Debit > 0 && l.Credit > 0)) return await ShowAlertAndReturnFalse("Validation Error", "A single transaction line cannot have both Debit and Credit amounts.");
        if (Math.Round(totalDebit - totalCredit, 2) != 0 || totalDebit == 0) return await ShowAlertAndReturnFalse("Validation Error", "Total debits must equal total credits and be greater than zero before saving.");

        return true;
    }

    private async Task ShowAlertAsync(string title, string message) { if (RequestAlert != null) await RequestAlert.Invoke(title, message, "OK"); }

    private async Task<bool> ShowAlertAndReturnFalse(string title, string message) { await ShowAlertAsync(title, message); return false; }

    private bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
