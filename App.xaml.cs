using Microsoft.Extensions.DependencyInjection;
using AumoFinance.Services; // Tambahkan namespace Services

namespace AumoFinance;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Jalankan pengecekan update secara asynchronous di background saat window dibuat
        Task.Run(async () =>
        {
            var updateService = new UpdateService();

            // GANTI "USERNAME_GITHUB" dengan username / nama organisasi GitHub Anda
            // Contoh: await updateService.CheckAndInstallUpdateAsync("ghofur", "AumoFinance");
            await updateService.CheckAndInstallUpdateAsync("rdmmoonlight", "aumo-finance-android");
        });

        return new Window(new AppShell());
    }
}
