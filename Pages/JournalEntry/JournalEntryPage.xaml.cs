using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance.Pages.JournalEntry;

public partial class JournalEntryPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly CultureInfo _idrCulture = new("id-ID");
    private List<AccountLookupModel> _accounts = new();
    private readonly List<JournalLineRow> _rows = new();

    public JournalEntryPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        EntryDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_accounts.Count == 0)
        {
            _accounts = await _apiService.GetAccountsAsync();
            foreach (var row in _rows)
            {
                row.AccountPicker.ItemsSource = _accounts;
            }
        }

        if (_rows.Count == 0)
        {
            AddLine();
            AddLine();
        }
    }

    private void OnAddLineClicked(object? sender, EventArgs e) => AddLine();

    private void AddLine()
    {
        var row = new JournalLineRow(_accounts, RemoveLine, UpdateTotals);
        _rows.Add(row);
        LinesContainer.Children.Add(row.View);
        UpdateTotals();
    }

    private async void RemoveLine(JournalLineRow row)
    {
        if (_rows.Count <= 2)
        {
            await DisplayAlertAsync("Informasi", "Jurnal minimal harus memiliki dua baris.", "OK");
            return;
        }

        _rows.Remove(row);
        LinesContainer.Children.Remove(row.View);
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        decimal totalDebit = _rows.Sum(r => r.Debit);
        decimal totalCredit = _rows.Sum(r => r.Credit);

        TotalDebitLabel.Text = $"Rp {totalDebit.ToString("N0", _idrCulture)}";
        TotalCreditLabel.Text = $"Rp {totalCredit.ToString("N0", _idrCulture)}";

        bool balanced = totalDebit == totalCredit && totalDebit > 0;
        BalanceStatusLabel.Text = balanced ? "Seimbang" : "Belum Seimbang";
        BalanceStatusLabel.TextColor = balanced ? Color.FromArgb("#34D399") : Color.FromArgb("#F87171");
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        SaveBtn.IsEnabled = false;
        SaveBtn.Text = "Menyimpan...";

        try
        {
            var lines = _rows
                .Where(r => r.SelectedAccountId != 0 && (r.Debit != 0 || r.Credit != 0))
                .Select(r => new CreateJournalLineDto
                {
                    AccountId = r.SelectedAccountId,
                    LineDescription = r.Description,
                    Debit = r.Debit,
                    Credit = r.Credit
                })
                .ToList();

            if (lines.Count < 2)
            {
                await DisplayAlertAsync("Peringatan", "Isi minimal dua baris jurnal dengan akun dan nominal yang valid.", "OK");
                return;
            }

            var totalDebit = lines.Sum(l => l.Debit);
            var totalCredit = lines.Sum(l => l.Credit);
            if (totalDebit != totalCredit || totalDebit == 0)
            {
                await DisplayAlertAsync("Peringatan", "Total Debit dan Kredit harus seimbang dan lebih dari 0.", "OK");
                return;
            }

            DateTime selectedDate = EntryDatePicker.Date;

            var dto = new CreateJournalDto
            {
                EntryDate = DateTime.SpecifyKind(selectedDate, DateTimeKind.Utc),
                Lines = lines
            };

            var (success, message) = await _apiService.PostJournalAsync(dto);

            if (success)
            {
                await DisplayAlertAsync("Berhasil", message, "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlertAsync("Gagal Menyimpan", message, "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnSaveClicked Exception: {ex}");
            await DisplayAlertAsync("Error", "Terjadi error saat menyimpan: " + ex.Message, "OK");
        }
        finally
        {
            SaveBtn.IsEnabled = true;
            SaveBtn.Text = "SIMPAN JURNAL";
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

internal class JournalLineRow
{
    public Picker AccountPicker { get; }
    private readonly Entry _debitEntry;
    private readonly Entry _creditEntry;
    private readonly Entry _descEntry;

    public View View { get; }

    public int SelectedAccountId => AccountPicker.SelectedItem is AccountLookupModel acc ? acc.Id : 0;
    public decimal Debit => ParseAmount(_debitEntry.Text);
    public decimal Credit => ParseAmount(_creditEntry.Text);
    public string? Description => string.IsNullOrWhiteSpace(_descEntry.Text) ? null : _descEntry.Text.Trim();

    public JournalLineRow(List<AccountLookupModel> accounts, Action<JournalLineRow> onRemove, Action onChanged)
    {
        AccountPicker = new Picker
        {
            Title = "Pilih Akun",
            ItemsSource = accounts,
            ItemDisplayBinding = new Binding(nameof(AccountLookupModel.DisplayText)),
            TextColor = Colors.White,
            TitleColor = Color.FromArgb("#64748B")
        };

        _debitEntry = new Entry
        {
            Placeholder = "Debit",
            Keyboard = Keyboard.Numeric,
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#64748B")
        };

        _creditEntry = new Entry
        {
            Placeholder = "Kredit",
            Keyboard = Keyboard.Numeric,
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#64748B")
        };

        _descEntry = new Entry
        {
            Placeholder = "Keterangan (opsional)",
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#64748B")
        };

        _debitEntry.TextChanged += (_, _) =>
        {
            if (!string.IsNullOrEmpty(_debitEntry.Text)) _creditEntry.Text = string.Empty;
            onChanged();
        };
        _creditEntry.TextChanged += (_, _) =>
        {
            if (!string.IsNullOrEmpty(_creditEntry.Text)) _debitEntry.Text = string.Empty;
            onChanged();
        };

        var removeBtn = new Button
        {
            Text = "Hapus Baris",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#F87171"),
            BorderColor = Color.FromArgb("#334155"),
            BorderWidth = 1,
            HeightRequest = 36,
            FontSize = 12,
            CornerRadius = 8
        };
        removeBtn.Clicked += (_, _) => onRemove(this);

        var grid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };

        grid.Add(AccountPicker, 0, 0);
        Grid.SetColumnSpan(AccountPicker, 2);
        grid.Add(_debitEntry, 0, 1);
        grid.Add(_creditEntry, 1, 1);
        grid.Add(_descEntry, 0, 2);
        Grid.SetColumnSpan(_descEntry, 2);
        grid.Add(removeBtn, 0, 3);
        Grid.SetColumnSpan(removeBtn, 2);

        View = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1,
            Background = Color.FromArgb("#1E293B"),
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = 12,
            Content = grid
        };
    }

    private static decimal ParseAmount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0m;
        var clean = new string(text.Where(char.IsDigit).ToArray());
        return decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }
}