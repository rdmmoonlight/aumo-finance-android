using Microsoft.Maui.Graphics;

namespace AumoFinance.Views;

public partial class TopBarView : ContentView
{
    private CancellationTokenSource? _syncCts;

    // Color Palette
    private readonly Color _idleBg = Color.FromArgb("#10B981");   // Idle / siap -> disamakan dengan warna sukses
    private readonly Color _orangeKetela = Color.FromArgb("#E67E22"); // Queueing
    private readonly Color _blueUploading = Color.FromArgb("#3B82F6"); // Uploading
    private readonly Color _greenSuccess = Color.FromArgb("#10B981"); // Success
    private readonly Color _redFailed = Color.FromArgb("#EF4444"); // Gagal

    public TopBarView()
    {
        InitializeComponent();
    }

    public string PeriodText
    {
        get => PeriodLabel.Text;
        set => PeriodLabel.Text = value;
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
            // 1. Status Queueing -> Orange Ketela
            SyncBadge.BackgroundColor = _orangeKetela;
            SyncBadge.Stroke = _orangeKetela;

            // 2. Countdown Queue 10 Detik (tanpa teks, hanya warna oranye selama menunggu)
            for (int i = 10; i > 0; i--)
            {
                if (token.IsCancellationRequested) return;
                await Task.Delay(1000, token);
            }

            // 3. Status Berubah Jadi Uploading -> Biru
            SyncBadge.BackgroundColor = _blueUploading;
            SyncBadge.Stroke = _blueUploading;

            // 4. Eksekusi Upload ke DB Neon/PostgreSQL
            bool isSuccess = await uploadTask(data);

            if (isSuccess)
            {
                // Sukses: Hijau, lalu kembali idle (idle juga hijau, jadi tetap mulus)
                SyncBadge.BackgroundColor = _greenSuccess;
                SyncBadge.Stroke = _greenSuccess;
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
            // badge milik task baru yang sedang berjalan -> indikator sync terlihat
            // tidak berfungsi / berkedip hilang.
            if (!token.IsCancellationRequested)
            {
                SyncBadge.BackgroundColor = _idleBg;
                SyncBadge.Stroke = _idleBg;
            }
        }
    }

    private async Task HandleUploadFailureAsync<T>(T data, Action<T> onDeleteLocalData, string errorMessage)
    {
        // A. Tampilkan status Gagal -> Merah, lalu kembali ke idle
        SyncBadge.BackgroundColor = _redFailed;
        SyncBadge.Stroke = _redFailed;
        // B. Hapus Data Otomatis dari Storage Lokal/Memory
        onDeleteLocalData(data);

        // C. Tampilkan Notifikasi Alert / Pop-up ke User (.NET 10 Compatible)
        if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
        {
            await Application.Current.Windows[0].Page!.DisplayAlertAsync(
                "Sync Gagal",
                $"Data tidak dapat diunggah ({errorMessage}). Data otomatis dibatalkan & dihapus demi konsistensi.",
                "OK");
        }

        await Task.Delay(2000);
    }
}
