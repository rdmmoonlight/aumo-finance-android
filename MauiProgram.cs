using Microsoft.Extensions.Logging;
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

		// ApiService dibagikan sebagai singleton agar NpgsqlDataSource (connection
		// pool) miliknya dipakai bersama oleh seluruh halaman, bukan dibuat ulang
		// setiap kali sebuah halaman baru dibuka.
		builder.Services.AddSingleton<ApiService>();

		// Didaftarkan agar Shell dapat membuat instance-nya lewat DI container
		// (constructor injection), bukan lewat "new MainPage()" manual.
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<InputJournalPage>();

		return builder.Build();
	}
}
