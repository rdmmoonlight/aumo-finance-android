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
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AumoFinance-AutoUpdater");
    }

    public async Task CheckAndInstallUpdateAsync(string githubUser, string githubRepo, bool isSilent = true)
    {
        try
        {
            string apiUrl = $"https://api.github.com/repos/{githubUser}/{githubRepo}/releases/latest";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode) return;

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string rawTag = root.GetProperty("tag_name").GetString() ?? "";
            string latestVersionStr = rawTag.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                ? rawTag.Substring(1)
                : rawTag;

            string currentVersionStr = AppInfo.Current.VersionString;

            if (Version.TryParse(latestVersionStr, out var latestVersion) &&
                Version.TryParse(currentVersionStr, out var currentVersion))
            {
                if (latestVersion > currentVersion)
                {
                    string apkDownloadUrl = string.Empty;

                    if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var asset in assetsElement.EnumerateArray())
                        {
                            string fileName = asset.GetProperty("name").GetString() ?? "";

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
                            DownloadAndInstallApk(apkDownloadUrl, latestVersionStr);
                        }
                        else
                        {
                            bool userChoice = await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                var currentPage = Application.Current?.Windows[0]?.Page;
                                if (currentPage != null)
                                {
                                    return await currentPage.DisplayAlertAsync(
                                        "Pembaruan AumoFinance",
                                        $"Versi baru (v{latestVersionStr}) tersedia. Unduh sekarang?",
                                        "Ya, Unduh",
                                        "Nanti");
                                }
                                return false;
                            });

                            if (userChoice)
                            {
                                DownloadAndInstallApk(apkDownloadUrl, latestVersionStr);
                            }
                        }
                    }
                }
                else if (!isSilent)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        var currentPage = Application.Current?.Windows[0]?.Page;
                        if (currentPage != null)
                        {
                            await currentPage.DisplayAlertAsync("AumoFinance", "Aplikasi Anda sudah menggunakan versi terbaru.", "OK");
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

    private void DownloadAndInstallApk(string apkUrl, string version)
    {
#if ANDROID
        try
        {
            // Menggunakan Android DownloadManager agar ada notifikasi progress bawaan sistem
            var context = Android.App.Application.Context;
            var request = new Android.App.DownloadManager.Request(Android.Net.Uri.Parse(apkUrl));

            string fileName = $"AumoFinance_v{version}.apk";

            request.SetTitle("Memperbarui AumoFinance");
            request.SetDescription($"Mengunduh versi v{version}...");
            request.SetNotificationVisibility(Android.App.DownloadVisibility.VisibleNotifyCompleted);
            request.SetDestinationInExternalFilesDir(context, Android.OS.Environment.DirectoryDownloads, fileName);
            request.SetMimeType("application/vnd.android.package-archive");

            var downloadManager = (Android.App.DownloadManager?)context.GetSystemService(Android.Content.Context.DownloadService);
            long downloadId = downloadManager?.Enqueue(request) ?? -1;

            if (downloadId != -1)
            {
                // Register BroadcastReceiver untuk menangkap event saat download selesai lalu eksekusi install
                var onCompleteReceiver = new DownloadCompleteReceiver(downloadId, fileName);
                context.RegisterReceiver(
                    onCompleteReceiver, 
                    new Android.Content.IntentFilter(Android.App.DownloadManager.ActionDownloadComplete), 
                    Android.Content.ReceiverFlags.Exported);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DownloadManager Error] {ex.Message}");
        }
#endif
    }
}

#if ANDROID
// Receiver untuk menangkap status saat proses download via Notifikasi selesai
public class DownloadCompleteReceiver : Android.Content.BroadcastReceiver
{
    private readonly long _downloadId;
    private readonly string _fileName;

    public DownloadCompleteReceiver(long downloadId, string fileName)
    {
        _downloadId = downloadId;
        _fileName = fileName;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("android26.0")]
    public override void OnReceive(Android.Content.Context? context, Android.Content.Intent? intent)
    {
        if (context == null || intent == null) return;

        long id = intent.GetLongExtra(Android.App.DownloadManager.ExtraDownloadId, -1);
        if (id == _downloadId)
        {
            TriggerInstall(context, _fileName);
            
            try
            {
                context.UnregisterReceiver(this);
            }
            catch { }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("android26.0")]
    private void TriggerInstall(Android.Content.Context context, string fileName)
    {
        // 1. Cek & minta izin "Install Unknown Apps" pada Android 8.0+
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

        // 2. Dapatkan file APK dari folder unduhan
        var file = new Java.IO.File(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads), fileName);
        
        if (!file.Exists()) return;

        // 3. Panggil FileProvider untuk membuka installer bawaan OS
        var apkUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
            context,
            $"{context.PackageName}.fileprovider",
            file);

        var installIntent = new Android.Content.Intent(Android.Content.Intent.ActionView);
        installIntent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
        installIntent.AddFlags(Android.Content.ActivityFlags.NewTask);
        installIntent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
        installIntent.AddFlags(Android.Content.ActivityFlags.ClearTop);

        context.StartActivity(installIntent);
    }
}
#endif
