using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace AumoFinance.Services;

public class UpdateService
{
    private readonly HttpClient _httpClient;

    public UpdateService()
    {
        _httpClient = new HttpClient();
        // Header User-Agent wajib diisi untuk request ke GitHub REST API
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AumoFinance-AutoUpdater");
    }

    /// <summary>
    /// Memeriksa ke GitHub Releases dan memicu instalasi jika ada versi baru.
    /// </summary>
    /// <param name="githubUser">Username atau Organisasi GitHub</param>
    /// <param name="githubRepo">Nama Repositori GitHub</param>
    /// <param name="isSilent">Jika true, langsung mengunduh & menginstal tanpa dialog pertanyaan</param>
    public async Task CheckAndInstallUpdateAsync(string githubUser, string githubRepo, bool isSilent = true)
    {
        try
        {
            string apiUrl = $"https://api.github.com/repos/{githubUser}/{githubRepo}/releases/latest";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode) return;

            string json = await response.ContentReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 1. Ambil tag_name dari GitHub Release (misal: "v26.8.105" -> ubah jadi "26.8.105")
            string rawTag = root.GetProperty("tag_name").GetString() ?? "";
            string latestVersionStr = rawTag.StartsWith("v", StringComparison.OrdinalIgnoreCase) 
                ? rawTag.Substring(1) 
                : rawTag;

            string currentVersionStr = AppInfo.Current.VersionString;

            // 2. Bandingkan versi saat ini dengan versi di GitHub
            if (Version.TryParse(latestVersionStr, out var latestVersion) && 
                Version.TryParse(currentVersionStr, out var currentVersion))
            {
                if (latestVersion > currentVersion)
                {
                    // 3. Cari URL download file APK dari daftar assets
                    string apkDownloadUrl = string.Empty;
                    
                    if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var asset in assetsElement.EnumerateArray())
                        {
                            string fileName = asset.GetProperty("name").GetString() ?? "";
                            
                            // Mencari file APK yang dihasilkan dari dotnet publish pipeline YAML
                            if (fileName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                            {
                                apkDownloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(apkDownloadUrl))
                    {
                        if (isSilent)
                        {
                            // AUTO UPDATE ON: Langsung unduh & panggil installer tanpa tanya
                            await DownloadAndInstallApkAsync(apkDownloadUrl);
                        }
                        else
                        {
                            // CHECK MANUAL: Tampilkan dialog konfirmasi ke pengguna
                            bool userChoice = await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                var currentPage = Application.Current?.Windows[0]?.Page;
                                if (currentPage != null)
                                {
                                    return await currentPage.DisplayAlert(
                                        "Pembaruan AumoFinance",
                                        $"Versi baru (v{latestVersionStr}) telah tersedia. Apakah Anda ingin memperbarui sekarang?",
                                        "Ya, Unduh",
                                        "Nanti");
                                }
                                return false;
                            });

                            if (userChoice)
                            {
                                await DownloadAndInstallApkAsync(apkDownloadUrl);
                            }
                        }
                    }
                }
                else if (!isSilent)
                {
                    // Jika pengecekan manual dan aplikasi sudah versi terbaru
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        var currentPage = Application.Current?.Windows[0]?.Page;
                        if (currentPage != null)
                        {
                            await currentPage.DisplayAlert("AumoFinance", "Aplikasi Anda sudah menggunakan versi terbaru.", "OK");
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AumoFinance AutoUpdate Error] {ex.Message}");
        }
    }

    private async Task DownloadAndInstallApkAsync(string apkUrl)
    {
#if ANDROID
        try
        {
            string fileName = "aumo_update.apk";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Unduh file APK ke cache lokal
            var apkBytes = await _httpClient.GetByteArrayAsync(apkUrl);
            await File.WriteAllBytesAsync(filePath, apkBytes);

            // Eksekusi installer Android
            InstallApkOnAndroid(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AumoFinance Download Error] {ex.Message}");
        }
#else
        await Task.CompletedTask;
#endif
    }

#if ANDROID
    private void InstallApkOnAndroid(string filePath)
    {
        var context = Android.App.Application.Context;

        // Cek Izin Install Unknown Apps untuk Android 8.0+ (API 26+)
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            if (!context.PackageManager!.CanRequestPackageInstalls())
            {
                var settingsIntent = new Android.Content.Intent(Android.Provider.Settings.ActionManageUnknownAppSources)
                    .SetData(Android.Net.Uri.Parse($"package:{context.PackageName}"))
                    .AddFlags(Android.Content.ActivityFlags.NewTask);
                
                context.StartActivity(settingsIntent);
                return;
            }
        }

        // Buka Installer bawaan Android menggunakan FileProvider
        var apkFile = new Java.IO.File(filePath);
        var apkUri = androidx.core.content.FileProvider.GetUriForFile(
            context,
            $"{context.PackageName}.fileprovider",
            apkFile);

        var installIntent = new Android.Content.Intent(Android.Content.Intent.ActionView);
        installIntent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
        installIntent.AddFlags(Android.Content.ActivityFlags.NewTask);
        installIntent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);

        context.StartActivity(installIntent);
    }
#endif
}
