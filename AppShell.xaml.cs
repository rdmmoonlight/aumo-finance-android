using Microsoft.Maui.Controls;
using AumoFinance.Pages;

namespace AumoFinance;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register Rute Master & Laporan
        Routing.RegisterRoute(nameof(CoaPage), typeof(CoaPage));
        Routing.RegisterRoute(nameof(PeriodsPage), typeof(PeriodsPage));
        Routing.RegisterRoute(nameof(InputJournalPage), typeof(InputJournalPage));

        Routing.RegisterRoute(nameof(GeneralJournalPage), typeof(GeneralJournalPage));
        Routing.RegisterRoute(nameof(GeneralLedgerPermanentPage), typeof(GeneralLedgerPermanentPage));
        Routing.RegisterRoute(nameof(GeneralLedgerTemporaryPage), typeof(GeneralLedgerTemporaryPage));
        Routing.RegisterRoute(nameof(TrialBalancePage), typeof(TrialBalancePage));
        Routing.RegisterRoute(nameof(AdjustingJournalPage), typeof(AdjustingJournalPage));
        Routing.RegisterRoute(nameof(WorksheetPage), typeof(WorksheetPage));
        Routing.RegisterRoute(nameof(IncomeStatementPage), typeof(IncomeStatementPage));
        Routing.RegisterRoute(nameof(RetainedEarningsPage), typeof(RetainedEarningsPage));
        Routing.RegisterRoute(nameof(StatementOfFinancialPositionPage), typeof(StatementOfFinancialPositionPage));
        Routing.RegisterRoute(nameof(PostClosingTrialBalancePage), typeof(PostClosingTrialBalancePage));
    }
}
