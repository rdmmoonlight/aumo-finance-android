using System;
using Microsoft.Maui.Controls;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    public TopBarView()
    {
        InitializeComponent();
    }

    private async void OnGeneralJournalClicked(object sender, EventArgs e)
    {
        // TODO: Navigasi ke General Journal
    }

    private async void OnGlPermanentClicked(object sender, EventArgs e)
    {
        // TODO: Navigasi ke GL Permanen
    }

    private async void OnGlTemporaryClicked(object sender, EventArgs e)
    {
        // TODO: Navigasi ke GL Sementara
    }

    private async void OnCoaClicked(object sender, EventArgs e)
    {
        // TODO: Navigasi ke COA
    }

    private async void OnPeriodClicked(object sender, EventArgs e)
    {
        // TODO: Filter Periode
    }
}
