using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.ViewModels;

namespace AumoFinance.Pages.JournalEntry;

[QueryProperty(nameof(EntryId), "entryId")]
public partial class JournalEntryPage : ContentPage
{
    private readonly JournalEntryViewModel _viewModel;
    private static readonly CultureInfo IdCulture = new("id-ID");

    // Guard anti-rekursi: mencegah Entry.Text yang kita set sendiri di
    // bawah memicu ulang handler ini. Dikunci per instance Entry (bukan
    // satu flag global) supaya baris jurnal lain (Debit/Kredit baris lain)
    // tidak ikut terpengaruh saat satu baris sedang diformat.
    private readonly HashSet<Entry> _formattingInProgress = new();

    public string? EntryId { get; set; }

    public JournalEntryPage(JournalEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Hubungkan event UI (DisplayAlert & Navigation) dari ViewModel
        _viewModel.RequestAlert += DisplayAlertAsync;
        _viewModel.RequestNavigationPop += SafePopAsync;
    }

    // Pop relatif (".."). Kalau Shell tetap melempar "Ambiguous routes matched"
    // (mis. sisa cache rute lama sebelum rebuild), fallback ke navigasi absolut
    // langsung ke GeneralJournalPage supaya tombol Cancel/Update tidak "diam saja".
    private async Task SafePopAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Relative pop failed, falling back to absolute route: {ex}");
            try
            {
                await Shell.Current.GoToAsync($"//{nameof(Pages.Main.MainPage)}/{nameof(Pages.Reports.GeneralJournal.GeneralJournalPage)}");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"Absolute fallback pop also failed: {fallbackEx}");
                await DisplayAlertAsync("Navigation Error", "Couldn't return to the previous page. Please use the app menu.", "OK");
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.ApplyPeriodHeaderAsync(TopHeader);
        await _viewModel.InitializeAsync(EntryId);
    }

    // ==========================================
    // Pemisah ribuan Debit/Kredit — format + posisi kursor dalam SATU
    // operasi sinkron per keystroke, dengan guard anti-rekursi.
    //
    // Sebelumnya format dilakukan lewat setter di JournalLineViewModel
    // (View -> ViewModel -> View, dua putaran binding per keystroke), yang
    // membuat digit baru bisa mendarat di posisi kursor yang salah saat
    // panjang teks berubah karena titik ribuan ditambahkan (mis. mengetik
    // "237500" menghasilkan "237.005" atau "237.050", bukan "237.500").
    // Dengan handler ini, tepat satu Entry.Text + Entry.CursorPosition
    // di-set per keystroke, jadi tidak ada ruang untuk race/urutan yang
    // salah. Binding TwoWay yang sudah ada tetap mendorong nilai akhirnya
    // ke DebitText/CreditText seperti biasa (setter di ViewModel bersifat
    // idempoten terhadap teks yang sudah terformat).
    // ==========================================

    private void OnDebitTextChanged(object? sender, TextChangedEventArgs e)
        => HandleAmountTextChanged(sender, e, _formattingInProgress);

    private void OnCreditTextChanged(object? sender, TextChangedEventArgs e)
        => HandleAmountTextChanged(sender, e, _formattingInProgress);

    private static void HandleAmountTextChanged(object? sender, TextChangedEventArgs e, HashSet<Entry> formattingInProgress)
    {
        if (sender is not Entry entry) return;
        if (formattingInProgress.Contains(entry)) return;

        string digitsOnly = Regex.Replace(e.NewTextValue ?? string.Empty, @"[^\d]", "");
        string formatted = string.Empty;

        if (digitsOnly.Length > 0 && decimal.TryParse(digitsOnly, NumberStyles.None, CultureInfo.InvariantCulture, out decimal value))
        {
            formatted = value.ToString("N0", IdCulture);
        }

        formattingInProgress.Add(entry);
        try
        {
            if (entry.Text != formatted)
            {
                entry.Text = formatted;
            }
            entry.CursorPosition = formatted.Length;
        }
        finally
        {
            formattingInProgress.Remove(entry);
        }
    }
}
