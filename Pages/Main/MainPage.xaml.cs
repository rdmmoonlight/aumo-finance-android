using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using AumoFinance.Services;
using AumoFinance.Pages.JournalEntry;
using AumoFinance.Pages.Dashboard;

namespace AumoFinance.Pages.Main;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        AutoUpdateSwitch.IsToggled = Preferences.Default.Get("AutoUpdateEnabled", true);
        SetTimeBasedGreeting();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Warm-up server Render secara terbelakang (fire-and-forget / non-blocking)
        _ = WarmupServerAsync();
    }

    private async Task WarmupServerAsync()
    {
        try
        {
            Debug.WriteLine("Warming up Render server...");
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
            // Ping endpoint kesehatan / endpoint ringan apapun di Render
            await client.GetAsync("https://aumo.onrender.com/api/mobile/chart-of-accounts");
            Debug.WriteLine("Render server is warm and ready!");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Server warm-up ping finished with notice: {ex.Message}");
        }
    }

    private void SetTimeBasedGreeting()
    {
        var hour = DateTime.Now.Hour;
        if (hour < 12)
            GreetingLabel.Text = "Good Morning,";
        else if (hour < 17)
            GreetingLabel.Text = "Good Afternoon,";
        else
            GreetingLabel.Text = "Good Evening,";
    }

    private async void OnDashboardFabClicked(object? sender, EventArgs e)
    {
        var dashboardPage = Handler?.MauiContext?.Services.GetService<DashboardPage>();
        if (dashboardPage != null)
        {
            await Navigation.PushAsync(dashboardPage);
        }
    }

    private async void OnPrimaryFabClicked(object? sender, EventArgs e)
    {
        var journalEntryPage = Handler?.MauiContext?.Services.GetService<JournalEntryPage>();
        if (journalEntryPage != null)
        {
            await Navigation.PushAsync(journalEntryPage);
        }
    }

    private void OnAutoUpdateToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("AutoUpdateEnabled", e.Value);
    }

    private async void OnCheckUpdateManualClicked(object? sender, EventArgs e)
    {
        try
        {
            var updateService = new UpdateService();
            await updateService.CheckAndInstallUpdateAsync("rdmmoonlight", "aumo-finance-android", isSilent: false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Manual update check error: {ex}");
            await this.DisplayAlertAsync("Error", "Failed to check for updates.", "OK");
        }
    }
}
