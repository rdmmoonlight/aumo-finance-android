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

    // Catatan: menu navigasi sebelumnya memakai DisplayActionSheetAsync
    // bertingkat (Menu -> Reports & Journals -> pilihan laporan), yang
    // terasa berantakan. Sekarang tombol ☰ membuka Shell Flyout (menu
    // geser) sungguhan yang didefinisikan di AppShell.xaml.
    //
    // PENTING: FlyoutBase.ContextFlyout / MenuFlyout (menu klik-kanan ala
    // desktop) memang TIDAK didukung di Android — itulah sebabnya kode
    // lama memakai ActionSheet. Namun Shell Flyout (drawer geser dari
    // tepi layar) adalah fitur yang BERBEDA dan didukung penuh di Android,
    // sehingga aman dipakai di sini.
    private void OnMenuButtonClicked(object? sender, EventArgs e)
    {
        var shell = Shell.Current;
        if (shell != null)
        {
            shell.FlyoutIsPresented = !shell.FlyoutIsPresented;
        }
    }
}
