using AumoFinance.Pages;
using AumoFinance.Pages.JournalEntry;
using AumoFinance.Pages.Reports;
using AumoFinance.Pages.Reports.AdjustingJournal;
using AumoFinance.Pages.Reports.GeneralJournal;
using AumoFinance.Pages.Reports.GeneralLedger;
using AumoFinance.Pages.Reports.IncomeStatement;
using AumoFinance.Pages.Reports.PostClosingTrialBalance;
using AumoFinance.Pages.Reports.RetainedEarnings;
using AumoFinance.Pages.Reports.StatementOfFinancialPosition;
using AumoFinance.Pages.Reports.TrialBalance;
using AumoFinance.Pages.Reports.Worksheet;
using AumoFinance.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        // Ambil connection string dari Environment Variable atau fallback string
        string connectionString = Environment.GetEnvironmentVariable("NEON_CONNECTION_STRING")
            ?? "Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD";

        // Registrasi Services
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddTransient<AccountingService>();

        // Registrasi User Context
        builder.Services.AddSingleton(new UserContext(CurrentUser.Id));

        // Registrasi Core Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<JournalEntryPage>();

        // Registrasi Report & Management Pages
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
        builder.Services.AddTransient<CoaPage>();
        builder.Services.AddTransient<PeriodsPage>();

        return builder.Build();
    }
}
