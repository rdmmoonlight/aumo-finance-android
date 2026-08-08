using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AumoFinance.Services;

namespace AumoFinance.Pages.Coa;

public partial class CoaPage : ContentPage
{
    private readonly CoaService _coaService;
    private List<CoaItemViewModel> _allAccounts = new();
    private readonly CultureInfo _idrCulture = new("id-ID");

    public CoaPage(CoaService coaService)
    {
        InitializeComponent();
        _coaService = coaService;
        SetupCategoryPicker();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAccountsAsync();
    }

    private void SetupCategoryPicker()
    {
        CategoryPicker.ItemsSource = new List<string>
        {
            "All Categories",
            "Asset",
            "Liability",
            "Equity",
            "Revenue",
            "Expense"
        };
        CategoryPicker.SelectedIndex = 0;
    }

    private async Task LoadAccountsAsync()
    {
        SetLoadingState(true);

        try
        {
            var (accounts, errorDetail) = await _coaService.GetAccountsAsync();

            if (!string.IsNullOrEmpty(errorDetail))
            {
                ShowAlert(errorDetail, isError: true);
                return;
            }

            // REVISI: Mengantisipasi null pada accounts dan ReferenceNumber
            _allAccounts = (accounts ?? new List<CoaAccountDto>()).Select(a => new CoaItemViewModel
            {
                Id = a.Id,
                ReferenceNumber = a.ReferenceNumber ?? 0, // REVISI: Default ke 0 jika null
                AccountName = a.AccountName ?? string.Empty,
                Type = a.Type ?? string.Empty,
                Role = a.Role ?? string.Empty,
                IsActive = a.IsActive,
                CurrentBalance = a.CurrentBalance,
                IdrCulture = _idrCulture
            }).ToList();

            ApplyFilterAndSearch();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadAccountsAsync error: {ex}");
            ShowAlert($"An unexpected error occurred: {ex.Message}", isError: true);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void ApplyFilterAndSearch()
    {
        string searchQuery = SearchField.Text?.Trim().ToLower() ?? string.Empty;
        string selectedCategory = CategoryPicker.SelectedItem?.ToString() ?? "All Categories";

        var filtered = _allAccounts.Where(a =>
        {
            bool matchesSearch = string.IsNullOrEmpty(searchQuery) ||
                                 a.AccountName.ToLower().Contains(searchQuery) ||
                                 a.ReferenceNumber.ToString().Contains(searchQuery);

            bool matchesCategory = selectedCategory == "All Categories" ||
                                   a.Type.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase);

            return matchesSearch && matchesCategory;
        }).ToList();

        CoaCollectionView.ItemsSource = filtered;
        EmptyStateContainer.IsVisible = !filtered.Any();
        CoaCollectionView.IsVisible = filtered.Any();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilterAndSearch();
    }

    private void OnCategoryPickerChanged(object? sender, EventArgs e)
    {
        ApplyFilterAndSearch();
    }

    private async void OnOpenAddModalClicked(object? sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("New Account", "Enter account name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        string refStr = await DisplayPromptAsync("Reference Code", "Enter reference code (e.g. 1010):");
        if (!int.TryParse(refStr, out int refNum))
        {
            await this.DisplayAlertAsync("Invalid Input", "Reference code must be a valid number.", "OK");
            return;
        }

        string type = await DisplayActionSheetAsync("Select Account Type", "Cancel", null, "Asset", "Liability", "Equity", "Revenue", "Expense");
        if (type == "Cancel" || string.IsNullOrEmpty(type)) return;

        var dto = new CreateAccountDto
        {
            AccountName = name.Trim(),
            ReferenceNumber = refNum,
            Type = type,
            Role = ""
        };

        SetLoadingState(true);
        var (success, message) = await _coaService.CreateAccountAsync(dto);

        if (success)
        {
            ShowAlert("Account created successfully!", isError: false);
            await LoadAccountsAsync();
        }
        else
        {
            ShowAlert(message, isError: true);
            SetLoadingState(false);
        }
    }

    private async void OnEditAccountClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int accountId)
        {
            var account = _allAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null) return;

            string newName = await DisplayPromptAsync("Edit Account", "Update account name:", initialValue: account.AccountName);
            if (string.IsNullOrWhiteSpace(newName)) return;

            var dto = new UpdateAccountDto
            {
                AccountName = newName.Trim(),
                ReferenceNumber = account.ReferenceNumber,
                Type = account.Type,
                Role = account.Role,
                IsActive = account.IsActive
            };

            SetLoadingState(true);
            var (success, message) = await _coaService.UpdateAccountAsync(accountId, dto);

            if (success)
            {
                ShowAlert("Account updated successfully!", isError: false);
                await LoadAccountsAsync();
            }
            else
            {
                ShowAlert(message, isError: true);
                SetLoadingState(false);
            }
        }
    }

    private async void OnDeleteAccountClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int accountId)
        {
            bool confirm = await this.DisplayAlertAsync(
                "Delete Confirmation",
                "Are you sure you want to delete this account? Accounts with transaction entries cannot be deleted.",
                "Yes, Delete",
                "Cancel");

            if (!confirm) return;

            SetLoadingState(true);
            var (success, message) = await _coaService.DeleteAccountAsync(accountId);

            if (success)
            {
                ShowAlert("Account deleted successfully!", isError: false);
                await LoadAccountsAsync();
            }
            else
            {
                ShowAlert(message, isError: true);
                SetLoadingState(false);
            }
        }
    }

    private void ShowAlert(string message, bool isError)
    {
        AlertText.Text = message;
        AlertIcon.Text = isError ? "⚠️" : "✅";
        AlertCard.BackgroundColor = isError ? Color.FromArgb("#7F1D1D") : Color.FromArgb("#14532D");
        AlertCard.Stroke = isError ? Color.FromArgb("#EF4444") : Color.FromArgb("#22C55E");
        AlertText.TextColor = isError ? Color.FromArgb("#FCA5A5") : Color.FromArgb("#86EFAC");
        AlertCard.IsVisible = true;
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        if (isLoading) AlertCard.IsVisible = false;
    }
}

// ==========================================
// VIEW MODEL ITEM CHART OF ACCOUNTS
// ==========================================
public class CoaItemViewModel
{
    public int Id { get; set; }
    public int ReferenceNumber { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal CurrentBalance { get; set; }
    public CultureInfo IdrCulture { get; set; } = new("id-ID");

    public bool HasRole => !string.IsNullOrWhiteSpace(Role);
    public string StatusText => IsActive ? "ACTIVE" : "INACTIVE";
    public Color StatusBackgroundColor => IsActive ? Color.FromArgb("#14532D") : Color.FromArgb("#7F1D1D");
    public Color StatusTextColor => IsActive ? Color.FromArgb("#86EFAC") : Color.FromArgb("#FCA5A5");

    public string FormattedBalance => "Rp" + Math.Round(CurrentBalance, 0, MidpointRounding.AwayFromZero).ToString("N0", IdrCulture);
    public Color BalanceColor => CurrentBalance >= 0 ? Color.FromArgb("#38BDF8") : Color.FromArgb("#EF4444");
}
