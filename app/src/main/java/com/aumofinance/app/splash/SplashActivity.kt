package com.aumofinance.app.splash

import android.content.Intent
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.aumofinance.app.auth.BiometricHelper
import com.aumofinance.app.auth.LoginActivity
import com.aumofinance.app.home.HomeActivity
import com.aumofinance.app.network.SessionStore
import com.aumofinance.app.update.AppUpdateService

// Splash screen custom (bukan API splash minimalis Android 12+) karena
// desainnya penuh — logo + teks atribusi "by rdmmoonlight" — bukan cuma
// ikon kecil di kotak seperti yang dipaksakan API splash bawaan. Ini
// LAUNCHER activity (menggantikan LoginActivity langsung).
//
// Setelah durasi splash, cek sesi tersimpan (SessionStore):
// - Ada sesi + biometrik AKTIF -> minta biometrik dulu, sukses baru ke Home.
//   Batal/gagal -> tetap ke Login (bukan dipaksa keluar app), sesi tersimpan
//   TIDAK dihapus supaya bisa dicoba lagi dari tombol "Masuk dengan
//   Biometrik" di layar Login.
// - Ada sesi + "Ingat saya" saja (tanpa biometrik) -> langsung ke Home.
// - Tidak ada sesi tersimpan -> ke Login seperti biasa.
class SplashActivity : ComponentActivity() {
    companion object {
        private const val SPLASH_DURATION_MS = 1200L
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent { SplashScreen() }

        // Cek update di background, silent, setiap app dibuka — porting
        // persis perilaku App.xaml.cs di versi MAUI lama (dulu TIDAK ADA
        // SAMA SEKALI di app Kotlin ini, itu sebabnya auto-update tidak
        // pernah terdeteksi sejak migrasi).
        AppUpdateService.checkForUpdateSilently(applicationContext)

        Handler(Looper.getMainLooper()).postDelayed({ proceedAfterSplash() }, SPLASH_DURATION_MS)
    }

    private fun proceedAfterSplash() {
        if (!SessionStore.hasSavedSession()) {
            goToLogin()
            return
        }

        if (SessionStore.isBiometricEnabled() && BiometricHelper.isAvailable(this)) {
            BiometricHelper.authenticate(
                activity = this,
                title = "Masuk ke AumoFinance",
                subtitle = "Gunakan sidik jari atau wajah Anda",
                onSuccess = {
                    restoreSessionAndGoHome()
                },
                onFailure = { goToLogin() },
            )
        } else {
            restoreSessionAndGoHome()
        }
    }

    // Token JWT sudah dimuat balik ke SessionManager oleh
    // restoreIntoSessionManager() di atas — cukup itu saja, tidak perlu
    // langkah tambahan sebelum request pertama ke Home (beda dari saat
    // sempat pakai cookie, yang perlu dipulihkan terpisah ke jar Ktor).
    private fun restoreSessionAndGoHome() {
        SessionStore.restoreIntoSessionManager()
        goToHome()
    }

    private fun goToHome() {
        startActivity(Intent(this, HomeActivity::class.java))
        finish()
    }

    private fun goToLogin() {
        startActivity(Intent(this, LoginActivity::class.java))
        finish()
    }
}
