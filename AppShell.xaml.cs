using Microsoft.Maui.Controls;
using AumoFinance.Pages;
using AumoFinance.Pages.Coa;
using AumoFinance.Pages.Dashboard;
using AumoFinance.Pages.JournalEntry;
using AumoFinance.Pages.Periods;
using AumoFinance.Pages.Reports.AdjustingJournal;
using AumoFinance.Pages.Reports.ClosingJournal;
using AumoFinance.Pages.Reports.GeneralJournal;
using AumoFinance.Pages.Reports.GeneralLedger;
using AumoFinance.Pages.Reports.IncomeStatement;
using AumoFinance.Pages.Reports.PostClosingTrialBalance;
using AumoFinance.Pages.Reports.RetainedEarnings;
using AumoFinance.Pages.Reports.StatementOfCashFlows;
using AumoFinance.Pages.Reports.StatementOfFinancialPosition;
using AumoFinance.Pages.Reports.TrialBalance;
using AumoFinance.Pages.Reports.Worksheet;

namespace AumoFinance;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // 1. Core & Master Data Routes
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(CoaPage), typeof(CoaPage));
        Routing.RegisterRoute(nameof(PeriodsPage), typeof(PeriodsPage));
        Routing.RegisterRoute(nameof(JournalEntryPage), typeof(JournalEntryPage));

        // 2. Financial Reports Routes
        Routing.RegisterRoute(nameof(GeneralJournalPage), typeof(GeneralJournalPage));
        Routing.RegisterRoute(nameof(GeneralLedgerPermanentPage), typeof(GeneralLedgerPermanentPage));
        Routing.RegisterRoute(nameof(GeneralLedgerTemporaryPage), typeof(GeneralLedgerTemporaryPage));
        Routing.RegisterRoute(nameof(TrialBalancePage), typeof(TrialBalancePage));
        Routing.RegisterRoute(nameof(AdjustingJournalPage), typeof(AdjustingJournalPage));
        Routing.RegisterRoute(nameof(WorksheetPage), typeof(WorksheetPage));
        Routing.RegisterRoute(nameof(IncomeStatementPage), typeof(IncomeStatementPage));
        Routing.RegisterRoute(nameof(RetainedEarningsPage), typeof(RetainedEarningsPage));
        Routing.RegisterRoute(nameof(StatementOfFinancialPositionPage), typeof(StatementOfFinancialPositionPage));
        Routing.RegisterRoute(nameof(ClosingJournalPage), typeof(ClosingJournalPage));
        Routing.RegisterRoute(nameof(PostClosingTrialBalancePage), typeof(PostClosingTrialBalancePage));
        Routing.RegisterRoute(nameof(StatementOfCashFlowsPage), typeof(StatementOfCashFlowsPage));
    }
}
