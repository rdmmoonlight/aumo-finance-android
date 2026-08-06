using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Pages;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    private const string ReportsMenuLabel = "Reports & Journals";
    private const string CoaMenuLabel = "Chart of Accounts (COA)";
    private const string PeriodMenuLabel = "Accounting Periods";
    private const string CancelLabel = "Batal";

    private const string GeneralJournalLabel = "General Journal";
    private const string AdjustingJournalLabel = "Adjusting Journal";
    private const string GlPermanentLabel = "General Ledger (Permanent)";
    private const string GlTemporaryLabel = "General Ledger (Temporary)";
    private const string TrialBalanceLabel = "Trial Balance";
    private const string AdjustedTrialBalanceLabel = "Adjusted Trial Balance";
    private const string WorksheetLabel = "Worksheet (10-Column)";
    private const string IncomeStatementLabel = "Income Statement";
    private const string RetainedEarningsLabel = "Retained Earnings Statement";
    private const string SofpLabel = "Statement of Financial Position";
    private const string PostClosingLabel = "Post-Closing Trial Balance";

    public TopBarView()
    {
        InitializeComponent();
    }

    public string PeriodText
    {
        get => PeriodLabel.Text;
        set => PeriodLabel.Text = string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    // Catatan: FlyoutBase.ContextFlyout / MenuFlyout hanya didukung MAUI
    // di Windows dan MacCatalyst, TIDAK di Android. Karena target proyek
    // ini hanya net10.0-android, menu memakai DisplayActionSheetAsync
    // (didukung penuh di semua platform, termasuk Android) sebagai ganti.
    private async void OnMenuButtonClicked(object? sender, EventArgs e)
    {
        var page = Shell.Current;
        if (page == null)
        {
            return;
        }

        var choice = await page.DisplayActionSheetAsync(
            "Menu",
            CancelLabel,
            null,
            ReportsMenuLabel,
            CoaMenuLabel,
            PeriodMenuLabel);

        switch (choice)
        {
            case ReportsMenuLabel:
                await ShowReportsMenuAsync(page);
                break;
            case CoaMenuLabel:
                await Shell.Current.GoToAsync(nameof(CoaPage));
                break;
            case PeriodMenuLabel:
                await Shell.Current.GoToAsync(nameof(PeriodsPage));
                break;
        }
    }

    private async Task ShowReportsMenuAsync(Page page)
    {
        var choice = await page.DisplayActionSheetAsync(
            ReportsMenuLabel,
            CancelLabel,
            null,
            GeneralJournalLabel,
            AdjustingJournalLabel,
            GlPermanentLabel,
            GlTemporaryLabel,
            TrialBalanceLabel,
            AdjustedTrialBalanceLabel,
            WorksheetLabel,
            IncomeStatementLabel,
            RetainedEarningsLabel,
            SofpLabel,
            PostClosingLabel);

        switch (choice)
        {
            case GeneralJournalLabel:
                await Shell.Current.GoToAsync(nameof(GeneralJournalPage));
                break;
            case AdjustingJournalLabel:
                await Shell.Current.GoToAsync(nameof(AdjustingJournalPage));
                break;
            case GlPermanentLabel:
                await Shell.Current.GoToAsync(nameof(GeneralLedgerPermanentPage));
                break;
            case GlTemporaryLabel:
                await Shell.Current.GoToAsync(nameof(GeneralLedgerTemporaryPage));
                break;
            case TrialBalanceLabel:
                await Shell.Current.GoToAsync($"{nameof(TrialBalancePage)}?includeAdjusting=false");
                break;
            case AdjustedTrialBalanceLabel:
                await Shell.Current.GoToAsync($"{nameof(TrialBalancePage)}?includeAdjusting=true");
                break;
            case WorksheetLabel:
                await Shell.Current.GoToAsync(nameof(WorksheetPage));
                break;
            case IncomeStatementLabel:
                await Shell.Current.GoToAsync(nameof(IncomeStatementPage));
                break;
            case RetainedEarningsLabel:
                await Shell.Current.GoToAsync(nameof(RetainedEarningsPage));
                break;
            case SofpLabel:
                await Shell.Current.GoToAsync(nameof(StatementOfFinancialPositionPage));
                break;
            case PostClosingLabel:
                await Shell.Current.GoToAsync(nameof(PostClosingTrialBalancePage));
                break;
        }
    }
}
