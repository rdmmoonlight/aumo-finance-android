using System;
using Microsoft.Maui.Controls;
using AumoFinance.Pages;
using AumoFinance.Pages.Coa;
using AumoFinance.Pages.Dashboard;
using AumoFinance.Pages.JournalEntry;
using AumoFinance.Pages.Periods;
using AumoFinance.Pages.Reports; // Namespace flat untuk sebagian besar halaman laporan
using AumoFinance.Pages.Reports.GeneralJournal;
using AumoFinance.Pages.Reports.ClosingJournal;
using AumoFinance.Pages.Reports.StatementOfCashFlows;
using AumoFinance.Pages.Settings;

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

        // KUNCI FLYOUT DRAWER SECARA DEFAULT
        // Menjamin drawer tidak bisa dibuka via gesture/swipe sebelum user berhasil login
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
    }

    // ================= FLYOUT MENU HANDLERS =================
    // Shell otomatis menutup Flyout saat sebuah MenuItem diklik.
    // Setiap handler melakukan push navigasi (GoToAsync) ke rute yang sudah didaftarkan.

    private async void OnDashboardMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(DashboardPage));

    private async void OnCoaMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(CoaPage));

    private async void OnPeriodsMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(PeriodsPage));

    private async void OnJournalEntryMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(JournalEntryPage));

    private async void OnGeneralJournalMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(GeneralJournalPage));

    private async void OnAdjustingJournalMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(AdjustingJournalPage));

    private async void OnGlPermanentMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(GeneralLedgerPermanentPage));

    private async void OnGlTemporaryMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(GeneralLedgerTemporaryPage));

    private async void OnTrialBalanceMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync($"{nameof(TrialBalancePage)}?includeAdjusting=false");

    private async void OnAdjustedTrialBalanceMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync($"{nameof(TrialBalancePage)}?includeAdjusting=true");

    private async void OnWorksheetMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(WorksheetPage));

    private async void OnIncomeStatementMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(IncomeStatementPage));

    private async void OnRetainedEarningsMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(RetainedEarningsPage));

    private async void OnSofpMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(StatementOfFinancialPositionPage));

    private async void OnClosingJournalMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(ClosingJournalPage));

    private async void OnPostClosingMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(PostClosingTrialBalancePage));

    private async void OnCashFlowMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(StatementOfCashFlowsPage));

    private async void OnSettingsMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(SettingsPage));

    private async void OnLogoutMenuItemClicked(object? sender, EventArgs e)
        => await GoToAsync(nameof(LogoutPage));
}
