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
        // TODO: Navigasi ke Halaman General Journal
        // await Shell.Current.GoToAsync("//GeneralJournalPage");
    }

    private async void OnGlPermanentClicked(object sender, EventArgs e)
    {
        // TODO: Navigasi ke Halaman Buku Besar (Akun Permanen)
    }

    private async void OnGlTemporaryClicked(object sender, EventArgs e)
    {
        // TODO: Navigasi ke Halaman Buku Besar (Akun Sementara)
    }

    private async void OnCoaClicked(object sender, EventArgs e)
    {
        // TODO: Navigasi ke Halaman COA (Chart of Accounts)
    }

    private async void OnPeriodClicked(object sender, EventArgs e)
    {
        // TODO: Aksi Pilihan/Filter Periode
    }
}
