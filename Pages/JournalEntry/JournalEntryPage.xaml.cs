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
using AumoFinance.ViewModels;
using AumoFinance.Models.Dtos;

namespace AumoFinance.Pages.JournalEntry;

[QueryProperty(nameof(EntryId), "entryId")]
public partial class JournalEntryPage : ContentPage
{
    private readonly JournalEntryService _journalEntryService;
    private readonly CoaService _coaService;
    private readonly PeriodService _periodService;
    private List<AccountLookupDto> _allAccounts = new();
    private int? _editingEntryId;
    private bool _isLocked;

    public ObservableCollection<JournalLineViewModel> Lines { get; set; } = new();
    private readonly CultureInfo _idCulture = new("id-ID");

    public string? EntryId { get; set; }

    public JournalEntryPage(JournalEntryService journalEntryService, CoaService coaService, PeriodService periodService)
    {
        InitializeComponent();
        _journalEntryService = journalEntryService;
        _coaService = coaService;
        _periodService = periodService;

        JournalTypePicker.SelectedIndex = 0;
        EntryDatePicker.Date = DateTime.Today;

        LinesCollectionView.ItemsSource = Lines;

        ResetFormLines();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _ = SelectedPeriodDisplayHelper.ApplyToTopBarAsync(TopHeader, _periodService);

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
        SubmitButton.IsVisible = false;

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

        int typeIndex = JournalTypePicker.ItemsSource
            .Cast<string>()
            .ToList()
            .FindIndex(t => string.Equals(t, entry.JournalType, StringComparison.OrdinalIgnoreCase));
        JournalTypePicker.SelectedIndex = typeIndex >= 0 ? typeIndex : 0;

        EntryDatePicker.Date = entry.EntryDate;

        TransactionNumberLabel.Text = entry.TransactionNumber;
        TransactionNumberLabel.TextColor = Colors.White;

        Lines.Clear();
        foreach (var line in entry.Lines.OrderBy(l => l.LineOrder))
        {
            var lineVm = new JournalLineViewModel(_allAccounts, () => UpdateTotals())
            {
                SelectedAccount = _allAccounts.FirstOrDefault(a => a.Id == line.AccountId),
                DebitText = line.Debit > 0 ? string.Format(_idCulture, "{0:N0}", line.Debit) : string.Empty,
                CreditText = line.Credit > 0 ? string.Format(_idCulture, "{0:N0}", line.Credit) : string.Empty,
                LineDescription = line.LineDescription ?? string.Empty
            };
            Lines.Add(lineVm);
        }

        _isLocked = entry.IsLocked;
        SetLockedState(_isLocked);

        UpdateTotals();
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

        SubmitButton.IsVisible = isBalanced && !_isLocked;
        SubmitButton.IsEnabled = isBalanced && !_isLocked;
    }

    private async void OnSaveJournalClicked(object? sender, EventArgs e)
    {
        if (_isLocked) return;

        var (isValid, totalDebit, totalCredit) = await ValidateFormAsync();
        if (!isValid) return;

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
            UpdateTotals();
        }
    }

    private async Task SaveAsCreateAsync()
    {
        var requestDto = new CreateJournalEntryRequest
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

        var (success, message, entryId, transactionNumber) = await _journalEntryService.CreateJournalEntryAsync(requestDto);

        if (success)
        {
            string successMessage = string.IsNullOrWhiteSpace(transactionNumber)
                ? message
                : $"Journal Entry {transactionNumber} recorded successfully!";

            await DisplayAlertAsync("Success", successMessage, "OK");

            ResetFormLines();
            await RefreshNextTransactionNumberAsync();
        }
        else
        {
            await DisplayAlertAsync("Posting Failed", message, "OK");
            UpdateTotals();
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
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Update Failed", message, "OK");
            UpdateTotals();
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
