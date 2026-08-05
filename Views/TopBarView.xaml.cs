using System;
using Microsoft.Maui.Controls;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    public TopBarView()
    {
        InitializeComponent();
    }

    public string PeriodText
    {
        get => PeriodLabel.Text;
        set => PeriodLabel.Text = string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private void OnMenuButtonClicked(object? sender, EventArgs e)
    {
        // Catatan: Microsoft.Maui.Controls.FlyoutBase tidak memiliki API
        // ShowAttachedFlyout (itu API WinUI, bukan MAUI). Di MAUI,
        // FlyoutBase.ContextFlyout yang terpasang pada MenuButton (lihat XAML)
        // sudah otomatis terbuka lewat tap/long-press pada Android, jadi
        // tidak perlu dipanggil manual di sini.
    }

    private async void OnGeneralJournalClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GeneralJournalPage");
    }

    private async void OnAdjustingJournalClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AdjustingJournalPage");
    }

    private async void OnGlPermanentClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GeneralLedgerPermanentPage");
    }

    private async void OnGlTemporaryClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GeneralLedgerTemporaryPage");
    }

    private async void OnTrialBalanceClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//TrialBalancePage?includeAdjusting=false");
    }

    private async void OnAdjustedTrialBalanceClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//TrialBalancePage?includeAdjusting=true");
    }

    private async void OnWorksheetClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//WorksheetPage");
    }

    private async void OnIncomeStatementClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//IncomeStatementPage");
    }

    private async void OnRetainedEarningsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RetainedEarningsPage");
    }

    private async void OnSofpClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//StatementOfFinancialPositionPage");
    }

    private async void OnPostClosingClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PostClosingTrialBalancePage");
    }

    private async void OnCoaClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//CoaPage");
    }

    private async void OnPeriodClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PeriodsPage");
    }
}
