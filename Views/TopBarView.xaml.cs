using System;
using Microsoft.Maui.Controls;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    public TopBarView()
    {
        InitializeComponent();
    }

    private void OnGeneralJournalClicked(object sender, EventArgs e)
    {
        // Navigasi ke General Journal
    }

    private void OnGlPermanentClicked(object sender, EventArgs e)
    {
        // Navigasi ke GL Permanen
    }

    private void OnGlTemporaryClicked(object sender, EventArgs e)
    {
        // Navigasi ke GL Sementara
    }

    private void OnCoaClicked(object sender, EventArgs e)
    {
        // Navigasi ke COA
    }

    private void OnPeriodClicked(object sender, EventArgs e)
    {
        // Navigasi ke Periode
    }
}
