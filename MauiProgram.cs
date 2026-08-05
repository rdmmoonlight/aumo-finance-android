using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AumoFinance.Models;
using AumoFinance.Pages;
using AumoFinance.Services;

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

        // 1. Registrasi AppDbContext (EF Core)
        // Ganti ConnectionString sesuai environment (PostgreSQL / Npgsql atau SQL Server)
        var connectionString = "Host=localhost;Database=aumofinance;Username=postgres;Password=yourpassword";
        
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)); // Gunakan UseSqlServer jika memakai SQL Server

        // 2. Registrasi AccountingService untuk logika transaksi & ledger
        builder.Services.AddScoped<AccountingService>();

        // 3. Registrasi Halaman (Pages) ke DI Container
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<InputJournalPage>();
        builder.Services.AddTransient<GeneralJournalPage>();
        builder.Services.AddTransient<GeneralLedgerPermanentPage>();
        builder.Services.AddTransient<GeneralLedgerTemporaryPage>();

        return builder.Build();
    }
}
