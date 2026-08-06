using System.Text.Json;

namespace Aumo.Services; // Sesuaikan jika namespace root proyek Anda berbeda (misal: AumoApp.Services)

public class UpdateService
{
    private readonly HttpClient _httpClient;

    public UpdateService()
    {
        _httpClient = new HttpClient();
        // GitHub API WAJIB menyertakan User-Agent
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Aumo-AutoUpdater");
    }

    /// <summary>
    /// Memeriksa ke GitHub Releases dan memicu update jika ada versi baru.
    /// </summary>
    /// <param name="githubUser">Username atau Nama Organisasi GitHub pemilik repositori Aumo</param>
    /// <param name="githubRepo">Nama Repositori (misal: "Aumo" atau "aumo-app")</param>
    public async Task CheckAndInstallUpdateAsync(string githubUser, string githubRepo)
    {
        try
        {
            // 1. Panggil API GitHub Release Terbaru
            string apiUrl = $"https://api.github.com/repos/{githubUser}/{githubRepo}/releases/latest";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode) return;

            string json = await response.ContentReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 2. Ambil tag_name dari YAML (misal: "v26.8.105" -> diubah jadi "26.8.105")
            string rawTag = root.GetProperty("tag_name").GetString() ?? "";
            string latestVersionStr = rawTag.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                ? rawTag.Substring(1)
                : rawTag;

            string currentVersionStr = AppInfo.Current.VersionString;

            // 3. Bandingkan Versi CalVer (26.8.BUILD)
            if (Version.TryParse(latestVersionStr, out var latestVersion) &&
                Version.TryParse(currentVersionStr, out var currentVersion))
            {
                if (latestVersion > currentVersion)
                {
                    // 4. Cari URL download file *-Signed.apk dari daftar assets GitHub Release
                    string apkDownloadUrl = string.Empty;

                    if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var asset in assetsElement.EnumerateArray())
                        {
                            string fileName = asset.GetProperty("name").GetString() ?? "";

                            // Mencari file APK yang dihasilkan dari dotnet publish
                            if (fileName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                            {
                                apkDownloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(apkDownloadUrl))
                    {
                        // 5. Tampilkan Dialog Konfirmasi Update
                        bool userChoice = await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            return await Shell.Current.DisplayAlert(
                                "Pembaruan Aumo",
                                $"Versi baru (v{latestVersionStr}) telah tersedia. Apakah Anda ingin memperbarui sekarang?",
                                "Ya, Unduh",
                                "Nanti");
                        });

                        if (userChoice)
                        {
                            await DownloadAndInstallApkAsync(apkDownloadUrl);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Aumo AutoUpdate Error] {ex.Message}");
        }
    }

    private async Task DownloadAndInstallApkAsync(string apkUrl)
    {
#if ANDROID
        try
        {
            string fileName = "aumo_update.apk";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Unduh file APK ke folder cache lokal
            var apkBytes = await _httpClient.GetByteArrayAsync(apkUrl);
            await File.WriteAllBytesAsync(filePath, apkBytes);

            // Jalankan instalasi via platform Android
            InstallApkOnAndroid(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Aumo Download Error] {ex.Message}");
        }
#else
        await Task.CompletedTask;
#endif
    }

#if ANDROID
    private void InstallApkOnAndroid(string filePath)
    {
        var context = Android.App.Application.Context;

        // Cek Izin Install dari Sumber Tidak Dikenal pada Android 8.0+ (API 26+)
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

        // Buka Installer Sistem Android menggunakan FileProvider
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
