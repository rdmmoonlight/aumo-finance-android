namespace AumoFinance;

public partial class AppShell : Shell
{
    public AppShell()
    {
        Routing.RegisterRoute(nameof(Pages.CoaPage), typeof(Pages.CoaPage));
        Routing.RegisterRoute(nameof(Pages.PeriodsPage), typeof(Pages.PeriodsPage));
        Routing.RegisterRoute(nameof(Pages.InputJournalPage), typeof(Pages.InputJournalPage));
        Routing.RegisterRoute(nameof(Pages.GeneralJournalPage), typeof(Pages.GeneralJournalPage));
        Routing.RegisterRoute(nameof(Pages.GeneralLedgerPermanentPage), typeof(Pages.GeneralLedgerPermanentPage));
        Routing.RegisterRoute(nameof(Pages.GeneralLedgerTemporaryPage), typeof(Pages.GeneralLedgerTemporaryPage));
        Routing.RegisterRoute(nameof(Pages.TrialBalancePage), typeof(Pages.TrialBalancePage));
        Routing.RegisterRoute(nameof(Pages.AdjustingJournalPage), typeof(Pages.AdjustingJournalPage));
        Routing.RegisterRoute(nameof(Pages.WorksheetPage), typeof(Pages.WorksheetPage));
        Routing.RegisterRoute(nameof(Pages.IncomeStatementPage), typeof(Pages.IncomeStatementPage));
        Routing.RegisterRoute(nameof(Pages.RetainedEarningsPage), typeof(Pages.RetainedEarningsPage));
        Routing.RegisterRoute(nameof(Pages.StatementOfFinancialPositionPage), typeof(Pages.StatementOfFinancialPositionPage));
        Routing.RegisterRoute(nameof(Pages.PostClosingTrialBalancePage), typeof(Pages.PostClosingTrialBalancePage));

        InitializeComponent();
    }
}
