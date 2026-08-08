using System;
using Microsoft.Maui.Controls;

// Import Namespace Sesuai Subfolder — satu using per folder, mengikuti struktur fisik file
using AumoFinance.Pages.Auth;
using AumoFinance.Pages.Main;
using AumoFinance.Pages.Dashboard;
using AumoFinance.Pages.Coa;
using AumoFinance.Pages.Periods;
using AumoFinance.Pages.JournalEntry;
using AumoFinance.Pages.Settings;
using AumoFinance.Pages.Reports.GeneralJournal;
using AumoFinance.Pages.Reports.ClosingJournal;
using AumoFinance.Pages.Reports.StatementOfCashFlows;
using AumoFinance.Pages.Reports.GeneralLedgerPermanent;
using AumoFinance.Pages.Reports.GeneralLedgerTemporary;
using AumoFinance.Pages.Reports.TrialBalance;
using AumoFinance.Pages.Reports.AdjustingJournal;
using AumoFinance.Pages.Reports.Worksheet;
using AumoFinance.Pages.Reports.IncomeStatement;
using AumoFinance.Pages.Reports.RetainedEarnings;
using AumoFinance.Pages.Reports.StatementOfFinancialPosition;
using AumoFinance.Pages.Reports.PostClosingTrialBalance;

namespace AumoFinance;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // 1. Auth & Account Routes
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(LogoutPage), typeof(LogoutPage));

        // 2. Core & Master Data Routes
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(CoaPage), typeof(CoaPage));
        Routing.RegisterRoute(nameof(PeriodsPage), typeof(PeriodsPage));
        Routing.RegisterRoute(nameof(JournalEntryPage), typeof(JournalEntryPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

        // 3. Financial Reports Routes
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

        // Lock Flyout drawer by default
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
    }

    private async void NavigateAndCloseFlyout(string route)
    {
        FlyoutIsPresented = false;
        await GoToAsync(route);
    }

    // ================= FLYOUT MENU HANDLERS =================

    private void OnDashboardMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(DashboardPage));

    private void OnCoaMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(CoaPage));

    private void OnPeriodsMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(PeriodsPage));

    private void OnJournalEntryMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(JournalEntryPage));

    private void OnGeneralJournalMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(GeneralJournalPage));

    private void OnAdjustingJournalMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(AdjustingJournalPage));

    private void OnGlPermanentMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(GeneralLedgerPermanentPage));

    private void OnGlTemporaryMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(GeneralLedgerTemporaryPage));

    private void OnTrialBalanceMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout($"{nameof(TrialBalancePage)}?includeAdjusting=false");

    private void OnAdjustedTrialBalanceMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout($"{nameof(TrialBalancePage)}?includeAdjusting=true");

    private void OnWorksheetMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(WorksheetPage));

    private void OnIncomeStatementMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(IncomeStatementPage));

    private void OnRetainedEarningsMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(RetainedEarningsPage));

    private void OnSofpMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(StatementOfFinancialPositionPage));

    private void OnClosingJournalMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(ClosingJournalPage));

    private void OnPostClosingMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(PostClosingTrialBalancePage));

    private void OnCashFlowMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(StatementOfCashFlowsPage));

    private void OnSettingsMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(SettingsPage));

    private void OnLogoutMenuItemClicked(object? sender, EventArgs e)
        => NavigateAndCloseFlyout(nameof(LogoutPage));
}
