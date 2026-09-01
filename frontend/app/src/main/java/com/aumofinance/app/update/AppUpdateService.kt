package com.aumofinance.app.update

import android.app.DownloadManager
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.net.Uri
import android.os.Environment
import android.provider.Settings
import android.util.Log
import androidx.core.content.FileProvider
import com.aumofinance.app.BuildConfig
import okhttp3.OkHttpClient
import okhttp3.Request
import org.json.JSONObject
import java.io.File
import java.util.concurrent.Executors

// Porting persis dari Services/UpdateService.cs (app MAUI lama) — sebelumnya
// TIDAK ADA SAMA SEKALI di app Kotlin ini, itu sebabnya auto-update tidak
// pernah terdeteksi sejak migrasi. Alur & keputusan desain dipertahankan
// sama seperti versi MAUI:
// - Cek GET api.github.com/repos/{user}/{repo}/releases/latest
// - Bandingkan tag_name (tanpa prefix "v") terhadap versionName APK ini
// - Kalau lebih baru, unduh asset .apk pertama lewat Android DownloadManager
// - Setelah unduhan selesai, langsung minta install lewat FileProvider +
//   Intent.ACTION_VIEW (kalau izin "install dari sumber tidak dikenal"
//   belum diberikan, arahkan ke halaman Settings yang relevan dulu)
object AppUpdateService {
    private const val TAG = "AppUpdateService"
    private const val GITHUB_USER = "rdmmoonlight"
    private const val GITHUB_REPO = "aumo-finance-android"

    // Nama & key preference DISENGAJA sama gaya dengan Preferences.Default
    // ("AutoUpdateEnabled") di App.xaml.cs versi MAUI lama, supaya konsisten
    // — walau storage-nya beda (SharedPreferences Android, bukan MAUI
    // Preferences), defaultnya sama: true.
    const val PREFS_NAME = "aumo_update_prefs"
    const val KEY_AUTO_UPDATE_ENABLED = "auto_update_enabled"

    private val httpClient = OkHttpClient.Builder().build()
    private val executor = Executors.newSingleThreadExecutor()

    // Dipanggil sekali setiap app start (lihat SplashActivity) — silent,
    // tidak menampilkan apapun ke user kecuali notifikasi unduhan bawaan
    // Android DownloadManager, sama seperti perilaku isSilent=true di
    // UpdateService.cs lama.
    fun checkForUpdateSilently(context: Context) {
        if (BuildConfig.DEBUG) {
            // Build debug punya versionNameSuffix "-debug" (mis. "26.9.1-debug")
            // yang tidak bisa dibandingkan apel-ke-apel dengan tag rilis GitHub
            // ("26.9.1") — auto-update hanya masuk akal untuk build release.
            Log.d(TAG, "Lewati cek update: build debug.")
            return
        }

        val prefs = context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)
        if (!prefs.getBoolean(KEY_AUTO_UPDATE_ENABLED, true)) {
            Log.d(TAG, "Lewati cek update: sakelar Auto-Update dimatikan user.")
            return
        }

        executor.execute {
            try {
                val request = Request.Builder()
                    .url("https://api.github.com/repos/$GITHUB_USER/$GITHUB_REPO/releases/latest")
                    .header("User-Agent", "AumoFinance-AutoUpdater")
                    .build()

                httpClient.newCall(request).execute().use { response ->
                    if (!response.isSuccessful) return@use
                    val body = response.body?.string() ?: return@use
                    val json = JSONObject(body)

                    val rawTag = json.optString("tag_name", "")
                    val latestVersion = if (rawTag.startsWith("v", ignoreCase = true)) rawTag.substring(1) else rawTag
                    val currentVersion = BuildConfig.VERSION_NAME

                    if (compareVersions(latestVersion, currentVersion) > 0) {
                        val assets = json.optJSONArray("assets") ?: return@use
                        var apkUrl: String? = null
                        for (i in 0 until assets.length()) {
                            val asset = assets.getJSONObject(i)
                            val fileName = asset.optString("name", "")
                            if (fileName.endsWith(".apk", ignoreCase = true)) {
                                apkUrl = asset.optString("browser_download_url", "")
                                break
                            }
                        }
                        if (!apkUrl.isNullOrBlank()) {
                            downloadAndInstallApk(context, apkUrl, latestVersion)
                        }
                    }
                }
            } catch (e: Exception) {
                Log.e(TAG, "Cek update gagal: ${e.message}")
            }
        }
    }

    // Perbandingan versi dotted-numeric sederhana (mis. "26.9.1" vs "26.8.212")
    // — setara System.Version.CompareTo() yang dipakai versi MAUI lama.
    // Segmen yang tidak ada dianggap 0 (mis. "26.9" vs "26.9.1" -> "26.9" < "26.9.1").
    private fun compareVersions(a: String, b: String): Int {
        val partsA = a.split(".").mapNotNull { it.toIntOrNull() }
        val partsB = b.split(".").mapNotNull { it.toIntOrNull() }
        val maxLen = maxOf(partsA.size, partsB.size)
        for (i in 0 until maxLen) {
            val partA = partsA.getOrElse(i) { 0 }
            val partB = partsB.getOrElse(i) { 0 }
            if (partA != partB) return partA.compareTo(partB)
        }
        return 0
    }

    private fun downloadAndInstallApk(context: Context, apkUrl: String, version: String) {
        try {
            val fileName = "AumoFinance_v$version.apk"
            val request = DownloadManager.Request(Uri.parse(apkUrl)).apply {
                setTitle("Memperbarui AumoFinance")
                setDescription("Mengunduh versi v$version...")
                setNotificationVisibility(DownloadManager.Request.VISIBILITY_VISIBLE_NOTIFY_COMPLETED)
                setDestinationInExternalFilesDir(context, Environment.DIRECTORY_DOWNLOADS, fileName)
                setMimeType("application/vnd.android.package-archive")
            }

            val downloadManager = context.getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
            val downloadId = downloadManager.enqueue(request)

            val receiver = object : BroadcastReceiver() {
                override fun onReceive(ctx: Context?, intent: Intent?) {
                    val id = intent?.getLongExtra(DownloadManager.EXTRA_DOWNLOAD_ID, -1) ?: -1
                    if (id == downloadId) {
                        triggerInstall(context, fileName)
                        try {
                            context.unregisterReceiver(this)
                        } catch (_: Exception) {
                        }
                    }
                }
            }
            // App dikunci minimum Android 9 (API 28) — flag RECEIVER_EXPORTED
            // (wajib eksplisit sejak API 33+ lewat ContextCompat) tidak relevan
            // di sini karena minSdk sudah jauh di bawah itu, tapi tetap aman
            // dipakai di semua versi >= 28.
            context.registerReceiver(receiver, IntentFilter(DownloadManager.ACTION_DOWNLOAD_COMPLETE))
        } catch (e: Exception) {
            Log.e(TAG, "Download/install gagal: ${e.message}")
        }
    }

    private fun triggerInstall(context: Context, fileName: String) {
        if (!context.packageManager.canRequestPackageInstalls()) {
            val settingsIntent = Intent(Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES)
                .setData(Uri.parse("package:${context.packageName}"))
                .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            context.startActivity(settingsIntent)
            return
        }

        val file = File(context.getExternalFilesDir(Environment.DIRECTORY_DOWNLOADS), fileName)
        if (!file.exists()) return

        val apkUri = FileProvider.getUriForFile(context, "${context.packageName}.fileprovider", file)

        val installIntent = Intent(Intent.ACTION_VIEW).apply {
            setDataAndType(apkUri, "application/vnd.android.package-archive")
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
            addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP)
        }
        context.startActivity(installIntent)
    }
}
