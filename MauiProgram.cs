using AumoFinance.Pages;
using AumoFinance.Pages.JournalEntry;
using AumoFinance.Services;
using AumoFinance.Services.Reports;
using Microsoft.Extensions.Logging;
using AumoFinance.Pages.Coa;
using AumoFinance.Pages.Periods;

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
        builder.Services.AddTransient<JournalEntryService>();
        builder.Services.AddTransient<CoaService>();

        // Legacy / General ApiService (jika masih digunakan di beberapa komponen lama)
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<AccountingService>();

        // Registrasi User Context
        builder.Services.AddSingleton(new UserContext(CurrentUser.Id));

        // ==========================================
        // 2. REGISTRASI REPORT SERVICES (Folder Services/Reports)
        // ==========================================
        builder.Services.AddTransient<GeneralJournalService>();
        builder.Services.AddTransient<GeneralLedgerService>();
        builder.Services.AddTransient<TrialBalanceService>(); // Menangani Unadjusted, Adjusted, & Post-Closing TB
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
        builder.Services.AddTransient<PostClosingTrialBalancePage>();

        return builder.Build();
    }
}
