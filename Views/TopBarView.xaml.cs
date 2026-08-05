using System;
using Microsoft.Maui.Controls;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    public TopBarView()
    {
        InitializeComponent();
    }

    public string PeriodText
    {
        get => PeriodLabel.Text;
        set => PeriodLabel.Text = string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private async void OnGeneralJournalClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GeneralJournalPage");
    }

    private async void OnGlPermanentClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GeneralLedgerPermanentPage");
    }

    private async void OnGlTemporaryClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GeneralLedgerTemporaryPage");
    }

    private async void OnCoaClicked(object? sender, EventArgs e)
    {
    await Shell.Current.GoToAsync("//CoaPage");
    }

    private async void OnPeriodClicked(object? sender, EventArgs e)
    {
    await Shell.Current.GoToAsync("//PeriodsPage");
    }
}
