using AumoFinance.Pages;
using AumoFinance.Pages.JournalEntry;
using AumoFinance.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AumoFinance;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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

        // Ambil connection string dari Environment Variable atau set string default
        string connectionString = Environment.GetEnvironmentVariable("NEON_CONNECTION_STRING") 
            ?? "Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD";

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddTransient<AccountingService>();

        // Bungkus Guid ke dalam UserContext (Reference Type)
        builder.Services.AddSingleton(new UserContext(CurrentUser.Id));

        // Pendaftaran Halaman
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<JournalEntryPage>();

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
