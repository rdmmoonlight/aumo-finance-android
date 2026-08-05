using AumoFinance.Pages;
using AumoFinance.Services;
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

        // ApiService dibagikan sebagai singleton agar NpgsqlDataSource (connection
        // pool) miliknya dipakai bersama oleh seluruh halaman.
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<AccountingService>();

        // Didaftarkan agar Shell dapat membuat instance-nya lewat DI container
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<InputJournalPage>();

        // Pendaftaran Seluruh Halaman Laporan (Reports)
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
