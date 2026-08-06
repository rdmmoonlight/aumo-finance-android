using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace AumoFinance.Services;

// Menangkap crash (unhandled exception) dan menuliskannya ke file lokal,
// karena environment build hanya lewat GitHub Actions (tidak ada adb logcat
// / PC tools). Log dibaca & ditampilkan sebagai alert saat aplikasi
// dibuka kembali setelah crash, lalu file dihapus.
public static class CrashLogger
{
    private static string LogFilePath => Path.Combine(FileSystem.CacheDirectory, "aumo_crash_log.txt");

    public static void Install()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            WriteLog("AppDomain.UnhandledException", e.ExceptionObject as Exception, e.IsTerminating);
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            WriteLog("TaskScheduler.UnobservedTaskException", e.Exception, isTerminating: false);
            e.SetObserved();
        };
    }

    private static void WriteLog(string source, Exception? ex, bool isTerminating)
    {
        try
        {
            var text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Source: {source} | Terminating: {isTerminating}\n\n{ex}\n";
            // Tulis synchronous & langsung — proses bisa mati sesaat setelah ini.
            File.AppendAllText(LogFilePath, text + "\n----------------------------------------\n\n");
        }
        catch
        {
            // Jangan sampai logger sendiri melempar exception baru.
        }
    }

    // Dipanggil sekali di halaman pertama (LoginPage) saat app dibuka.
    // Mengembalikan isi log terakhir (jika ada) lalu menghapus filenya.
    public static string? ReadAndClearLastCrash()
    {
        try
        {
            if (!File.Exists(LogFilePath))
            {
                return null;
            }

            var content = File.ReadAllText(LogFilePath);
            File.Delete(LogFilePath);
            return string.IsNullOrWhiteSpace(content) ? null : content;
        }
        catch
        {
            return null;
        }
    }
}
