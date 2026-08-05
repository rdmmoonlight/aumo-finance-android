using System;
using Microsoft.Maui.Controls;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    public TopBarView()
    {
        InitializeComponent();
    }

    private void OnMenuButtonClicked(object sender, EventArgs e)
    {
        FlyoutBase.ShowAttachedFlyout(MenuButton);
    }

    private async void OnGeneralJournalClicked(object sender, EventArgs e)
    {
        // Navigasi ke General Journal
    }

    private async void OnGlPermanentClicked(object sender, EventArgs e)
    {
        // Navigasi ke GL Permanen
    }

    private async void OnGlTemporaryClicked(object sender, EventArgs e)
    {
        // Navigasi ke GL Sementara
    }

    private async void OnCoaClicked(object sender, EventArgs e)
    {
        // Navigasi ke COA
    }

    private async void OnPeriodClicked(object sender, EventArgs e)
    {
        // Navigasi ke Periode
    }
}
