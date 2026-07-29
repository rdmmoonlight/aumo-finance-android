using AumoFinance.Models;
using AumoFinance.Services;

namespace AumoFinance;

public partial class InputJournalPage : ContentPage
{
    private readonly ApiService _apiService = new();
    private List<AccountLookupModel> _accounts = new();

    public InputJournalPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _accounts = await _apiService.GetAccountsAsync();

        AccountPicker1.ItemsSource = _accounts;
        AccountPicker2.ItemsSource = _accounts;
    }

    private async void OnSaveJournalClicked(object? sender, EventArgs e) // CS8622 Fix (object? sender)
    {
        var acc1 = AccountPicker1.SelectedItem as AccountLookupModel;
        var acc2 = AccountPicker2.SelectedItem as AccountLookupModel;

        if (acc1 == null || acc2 == null)
        {
            await DisplayAlertAsync("Peringatan", "Silakan pilih Akun Debit dan Kredit.", "OK");
            return;
        }

        if (!decimal.TryParse(DebitEntry1.Text, out decimal debit) || debit <= 0)
        {
            await DisplayAlertAsync("Peringatan", "Nilai Debit harus lebih besar dari 0.", "OK");
            return;
        }

        if (!decimal.TryParse(CreditEntry2.Text, out decimal credit) || credit <= 0)
        {
            await DisplayAlertAsync("Peringatan", "Nilai Kredit harus lebih besar dari 0.", "OK");
            return;
        }

        if (debit != credit)
        {
            await DisplayAlertAsync("Jurnal Unbalanced", $"Total Debit (Rp {debit:N0}) dan Kredit (Rp {credit:N0}) harus seimbang!", "OK");
            return;
        }

        var dto = new CreateJournalDto
        {
            EntryDate = EntryDatePicker.Date ?? DateTime.Today,
            Lines = new List<CreateJournalLineDto>
            {
                new() { AccountId = acc1.Id, LineDescription = DescEntry1.Text, Debit = debit, Credit = 0 },
                new() { AccountId = acc2.Id, LineDescription = DescEntry2.Text, Debit = 0, Credit = credit }
            }
        };

        var (success, message) = await _apiService.PostJournalAsync(dto);

        if (success)
        {
            await DisplayAlertAsync("Sukses", message, "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Gagal", message, "OK");
        }
    }
}
