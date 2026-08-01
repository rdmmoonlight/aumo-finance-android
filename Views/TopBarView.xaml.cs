using Microsoft.Maui.Graphics;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    private CancellationTokenSource? _syncCts;

    // Color Palette
    private readonly Color _orangeKetela = Color.FromArgb("#E67E22"); // Orange Ketela (Queueing)
    private readonly Color _greenSuccess = Color.FromArgb("#10B981"); // Hijau (Success)
    private readonly Color _darkBg = Color.FromArgb("#1E293B");

    public TopBarView()
    {
        InitializeComponent();
    }

    public string PeriodText
    {
        get => PeriodLabel.Text;
        set => PeriodLabel.Text = $"Periode: {value}";
    }

    /// <summary>
    /// Memulai antrean sync 10 detik sebelum mengunggah data ke Database.
    /// </summary>
    public async Task QueueAndUploadDataAsync<T>(
        T data, 
        Func<T, Task<bool>> uploadTask, 
        Action<T> onDeleteLocalData)
    {
        // Batalkan timer sync yang sedang berjalan sebelumnya (jika ada)
        _syncCts?.Cancel();
        _syncCts = new CancellationTokenSource();
        var token = _syncCts.Token;

        try
        {
            // 1. Tampilkan Indikator dengan Warna Orange Ketela
            SyncBadge.IsVisible = true;
            SyncBadge.BackgroundColor = _orangeKetela;
            SyncBadge.Stroke = _orangeKetela;

            // 2. Countdown Queue 10 Detik
            SyncIcon.Text = "🕓"; // menunggu di antrean
            for (int i = 10; i > 0; i--)
            {
                if (token.IsCancellationRequested) return;

                SyncLabel.Text = $"Wait {i}s";
                SyncIcon.Rotation = (10 - i) * 36; // Rotasi perlahan ikon
                await Task.Delay(1000, token);
            }

            // 3. Status Berubah Jadi Syncing
            SyncIcon.Text = "☁️"; // sedang mengunggah
            SyncLabel.Text = "Uploading...";
            await SyncIcon.RotateToAsync(360, 800, Easing.Linear);

            // 4. Eksekusi Upload ke DB Neon/PostgreSQL
            bool isSuccess = await uploadTask(data);

            if (isSuccess)
            {
                // Sukses: Ubah warna jadi hijau sejenak lalu sembunyikan
                SyncBadge.BackgroundColor = _greenSuccess;
                SyncBadge.Stroke = _greenSuccess;
                SyncIcon.Text = "✅"; // berhasil tersinkron
                SyncLabel.Text = "Synced!";
                await Task.Delay(2000);
            }
            else
            {
                // Gagal Upload: Munculkan Notifikasi & Hapus Data
                await HandleUploadFailureAsync(data, onDeleteLocalData, "Gagal terhubung ke database server.");
            }
        }
        catch (Exception ex)
        {
            // Error Exception (Connection timeout / RLS fail / dll)
            await HandleUploadFailureAsync(data, onDeleteLocalData, ex.Message);
        }
        finally
        {
            // Hanya reset tampilan jika task ini belum digantikan oleh task sync berikutnya.
            // Tanpa guard ini, task lama yang dibatalkan tetap sampai ke sini dan menimpa
            // (menyembunyikan) badge milik task baru yang sedang berjalan -> indikator sync
            // terlihat tidak berfungsi / berkedip hilang.
            if (!token.IsCancellationRequested)
            {
                SyncBadge.IsVisible = false;
                SyncBadge.BackgroundColor = _darkBg;
                SyncIcon.Rotation = 0;
                SyncIcon.Text = "🔄";
            }
        }
    }

    private async Task HandleUploadFailureAsync<T>(T data, Action<T> onDeleteLocalData, string errorMessage)
    {
        // A. Hapus Data Otomatis dari Storage Lokal/Memory
        onDeleteLocalData(data);

        // B. Tampilkan Notifikasi Alert / Pop-up ke User (.NET 10 Compatible)
        if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
        {
            await Application.Current.Windows[0].Page!.DisplayAlertAsync(
                "Sync Gagal ❌", 
                $"Data tidak dapat diunggah ({errorMessage}). Data otomatis dibatalkan & dihapus demi konsistensi.", 
                "OK");
        }
    }
}
