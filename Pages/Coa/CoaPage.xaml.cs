using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class CoaPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<ChartOfAccountDisplayModel> _allAccounts = new();
    private string _searchText = string.Empty;
    private string? _selectedCategory = null;

    private static readonly string[] AccountTypes =
    {
        "Assets", "Liabilities", "Equity", "OperatingIncome",
        "OperatingExpenses", "OtherIncome", "OtherExpenses"
    };

    public CoaPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // Inisialisasi Picker Kategori
        CategoryPicker.ItemsSource = new List<string> { "Semua Kategori" }.Concat(AccountTypes.Select(FormatCategoryLabel)).ToList();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        CoaCollectionView.IsVisible = false;
        EmptyStateContainer.IsVisible = false;

        try
        {
            var (accounts, _, errorDetail) = await _apiService.GetChartOfAccountsFullAsync();

            if (errorDetail != null)
            {
                await DisplayAlertAsync("Koneksi Gagal", $"Gagal memuat Chart of Accounts dari server.\n\nDetail: {errorDetail}", "OK");
                return;
            }

            _allAccounts = accounts.Select(a => new ChartOfAccountDisplayModel
            {
                Id = a.Id,
                ReferenceNumber = a.ReferenceNumber,
                AccountName = a.AccountName,
                Type = a.Type,
                Role = a.Role == "Default" ? string.Empty : a.Role,
                IsActive = a.IsActive,
                Balance = a.Balance
            }).ToList();

            FilterAndDisplayAccounts();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Gagal memuat Chart of Accounts: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private void FilterAndDisplayAccounts()
    {
        var filtered = _allAccounts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(a => a.AccountName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) || a.ReferenceNumber.ToString().Contains(_searchText));
        }

        if (!string.IsNullOrEmpty(_selectedCategory) && _selectedCategory != "Semua Kategori")
        {
            // Ambil kembali raw type key dari format label
            var rawType = AccountTypes.FirstOrDefault(t => FormatCategoryLabel(t) == _selectedCategory);
            if (!string.IsNullOrEmpty(rawType))
            {
                filtered = filtered.Where(a => a.Type == rawType);
            }
        }

        var list = filtered.ToList();
        if (list.Any())
        {
            CoaCollectionView.ItemsSource = list;
            CoaCollectionView.IsVisible = true;
            EmptyStateContainer.IsVisible = false;
        }
        else
        {
            CoaCollectionView.IsVisible = false;
            EmptyStateContainer.IsVisible = true;
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? string.Empty;
        FilterAndDisplayAccounts();
    }

    private void OnCategoryPickerChanged(object? sender, EventArgs e)
    {
        if (CategoryPicker.SelectedIndex >= 0)
        {
            _selectedCategory = CategoryPicker.SelectedItem?.ToString();
            FilterAndDisplayAccounts();
        }
    }

    private static string FormatCategoryLabel(string type) => type switch
    {
        "Assets" => "Assets (100 - 199)",
        "Liabilities" => "Liabilities (200 - 299)",
        "Equity" => "Equity (300 - 399)",
        "OperatingIncome" => "Operating Income (400 - 499)",
        "OperatingExpenses" => "Operating Expenses (500 - 599)",
        "OtherIncome" => "Other Income (600 - 799)",
        "OtherExpenses" => "Other Expenses (800 - 999)",
        _ => type
    };

    private async void OnOpenAddModalClicked(object? sender, EventArgs e)
    {
        // Navigasi atau popup tambah akun (bisa diarahkan ke halaman CreateCoaPage jika dibuat terpisah)
        await DisplayAlertAsync("Informasi", "Fitur form tambah akun baru dapat dibuatkan halaman khusus.", "OK");
    }

    private async void OnEditAccountClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int accountId)
        {
            await DisplayAlertAsync("Informasi", $"Edit akun dengan ID: {accountId}", "OK");
        }
    }

    private async void OnDeleteAccountClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int accountId)
        {
            bool confirm = await DisplayAlertAsync("Konfirmasi", "Hapus akun ini? Tindakan ini tidak dapat dibatalkan.", "Ya", "Batal");
            if (confirm)
            {
                var (success, message) = await _apiService.DeleteAccountAsync(accountId);
                if (success)
                {
                    await LoadAccountsAsync();
                }
                else
                {
                    await DisplayAlertAsync("Gagal", message, "OK");
                }
            }
        }
    }
}

public class ChartOfAccountDisplayModel
{
    public int Id { get; set; }
    public int ReferenceNumber { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal Balance { get; set; }

    public bool HasRole => !string.IsNullOrEmpty(Role);
    public string StatusText => IsActive ? "Active" : "Inactive";
    public Color StatusBackgroundColor => IsActive ? Color.FromArgb("#064E3B") : Color.FromArgb("#7F1D1D");
    public Color StatusTextColor => IsActive ? Color.FromArgb("#34D399") : Color.FromArgb("#FCA5A5");
    public Color BalanceColor => Balance >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

    private static readonly System.Globalization.CultureInfo Idr = new("id-ID");
    public string FormattedBalance => $"Rp {Balance.ToString("N0", Idr)}";
}
