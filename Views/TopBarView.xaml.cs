using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    private int _pendingQueueCount = 0;

    public TopBarView()
    {
        InitializeComponent();
    }

    public string PeriodText
    {
        get => PeriodLabel.Text;
        set => PeriodLabel.Text = string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    /// <summary>
    /// Properti untuk meng-update jumlah antrean transaksi lokal yang belum tersinkron.
    /// Panggil properti ini setiap kali ada jurnal baru yang disimpan lokal atau sukses di-upload.
    /// </summary>
    public int PendingQueueCount
    {
        get => _pendingQueueCount;
        set
        {
            _pendingQueueCount = Math.Max(0, value);
            UpdateNetworkAndQueueUI();
        }
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Mulai memantau perubahan jaringan saat komponen dipasang
        if (Parent != null)
        {
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
            UpdateNetworkAndQueueUI();
        }
        else
        {
            // Lepas event handler saat komponen di-unmount dari layar
            Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        // Jalankan update UI di Main/UI Thread agar aman
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateNetworkAndQueueUI();
        });
    }

    private void UpdateNetworkAndQueueUI()
    {
        bool isConnected = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        if (!isConnected)
        {
            // Tampilan saat OFFLINE
            ConnectionStatusDot.Fill = new SolidColorBrush(Color.Parse("#EF4444")); // Red
            NetworkQueueLabel.Text = _pendingQueueCount > 0 
                ? $"Offline ({_pendingQueueCount} pending)" 
                : "Offline";
            NetworkQueueLabel.TextColor = Color.Parse("#FCA5A5");
        }
        else
        {
            // Tampilan saat ONLINE
            if (_pendingQueueCount > 0)
            {
                // Ada antrean yang sedang/akan di-sync
                ConnectionStatusDot.Fill = new SolidColorBrush(Color.Parse("#F59E0B")); // Amber / Yellow
                NetworkQueueLabel.Text = $"Syncing ({_pendingQueueCount})...";
                NetworkQueueLabel.TextColor = Color.Parse("#FCD34D");
            }
            else
            {
                // Online & semua data tersinkron sempurna
                ConnectionStatusDot.Fill = new SolidColorBrush(Color.Parse("#10B981")); // Emerald Green
                NetworkQueueLabel.Text = "Online";
                NetworkQueueLabel.TextColor = Color.Parse("#CBD5E1");
            }
        }
    }

    private void OnMenuButtonClicked(object? sender, EventArgs e)
    {
        var shell = Shell.Current;
        if (shell != null)
        {
            shell.FlyoutIsPresented = !shell.FlyoutIsPresented;
        }
    }
}
