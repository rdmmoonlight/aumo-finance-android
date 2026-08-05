using AumoFinance.Pages;
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

        // ApiService dibagikan sebagai singleton agar NpgsqlDataSource (connection
        // pool) miliknya dipakai bersama oleh seluruh halaman.
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(ApiService.ConnectionString));
        builder.Services.AddTransient<AccountingService>();

        // Aplikasi single-user: daftarkan Guid tetap agar Shell/DI dapat
        // mengisi otomatis parameter "Guid currentUserId" di constructor
        // Page (CoaPage, PeriodsPage, semua halaman Reports, dll). Tanpa
        // ini, navigasi ke halaman tersebut akan crash karena DI tidak
        // tahu cara menyediakan nilai Guid.
        builder.Services.AddSingleton(CurrentUser.Id);

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
        builder.Services.AddTransient<CoaPage>();
        builder.Services.AddTransient<PeriodsPage>();

        return builder.Build();
    }
}
