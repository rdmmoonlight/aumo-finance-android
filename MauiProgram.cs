using AumoFinance.Pages;
using AumoFinance.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace AumoFinance;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Pasang crash logger sedini mungkin
        CrashLogger.Install();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification() // <--- Injeksi Plugin Local Notification
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ==========================================
        // 1. REGISTRASI CORE SERVICES
        // ==========================================
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<DashboardService>();
        builder.Services.AddSingleton<PeriodService>();
        builder.Services.AddSingleton<NotificationService>(); // <--- Registrasi NotificationService
        builder.Services.AddTransient<JournalEntryService>();
        builder.Services.AddTransient<CoaService>();

        // ==========================================
        // 2. REGISTRASI REPORT SERVICES (Folder Services/Reports)
        // ==========================================
        builder.Services.AddTransient<GeneralJournalService>();
        builder.Services.AddTransient<GeneralLedgerService>();
        builder.Services.AddTransient<TrialBalanceService>();
        builder.Services.AddTransient<PostClosingTrialBalanceService>();
        builder.Services.AddTransient<AdjustingJournalService>();
        builder.Services.AddTransient<WorksheetService>();
        builder.Services.AddTransient<IncomeStatementService>();
        builder.Services.AddTransient<RetainedEarningsService>();
        builder.Services.AddTransient<StatementOfFinancialPositionService>();
        builder.Services.AddTransient<ClosingJournalService>();
        builder.Services.AddTransient<StatementOfCashFlowsService>();

        // ==========================================
        // 3. REGISTRASI PAGES (VIEWS)
        // ==========================================
        // Core & Authentication Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<JournalEntryPage>();

        // Master Data & Period Management Pages
        builder.Services.AddTransient<CoaPage>();
        builder.Services.AddTransient<PeriodsPage>();

        // Financial Report Pages
        builder.Services.AddTransient<GeneralJournalPage>();
        builder.Services.AddTransient<GeneralLedgerPermanentPage>();
        builder.Services.AddTransient<GeneralLedgerTemporaryPage>();
        builder.Services.AddTransient<TrialBalancePage>();
        builder.Services.AddTransient<AdjustingJournalPage>();
        builder.Services.AddTransient<WorksheetPage>();
        builder.Services.AddTransient<IncomeStatementPage>();
        builder.Services.AddTransient<RetainedEarningsPage>();
        builder.Services.AddTransient<StatementOfFinancialPositionPage>();
        builder.Services.AddTransient<ClosingJournalPage>();
        builder.Services.AddTransient<PostClosingTrialBalancePage>();
        builder.Services.AddTransient<StatementOfCashFlowsPage>();

        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
