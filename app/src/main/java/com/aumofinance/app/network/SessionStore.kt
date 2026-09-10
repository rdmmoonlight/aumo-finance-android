package com.aumofinance.app.network

import android.content.Context
import android.content.SharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey

// Penyimpanan sesi terenkripsi (AES256-GCM via Android Keystore, lewat
// EncryptedSharedPreferences) — dipakai untuk fitur "Ingat saya" dan
// "Masuk dengan biometrik". SessionManager (in-memori) tetap sumber
// kebenaran SELAMA app berjalan; SessionStore hanya dibaca sekali di
// SplashActivity/LoginActivity untuk memulihkan sesi setelah app di-restart.
//
// Yang disimpan di sini HANYA nilai cookie otentikasi mentah ("AumoFinance
// .Session", ditulis langsung oleh PersistentCookiesStorage.addCookie()
// tiap kali server mengirim Set-Cookie baru) plus info tampilan (userId,
// fullName) & flag — BUKAN token JWT seperti desain awal migrasi Ktor,
// sejak diketahui backend pakai cookie ASP.NET Identity.
//
// CATATAN JUJUR soal keamanan biometrik: implementasi ini memakai
// BiometricPrompt sebagai GERBANG masuk ke sesi yang sudah tersimpan
// (autentikasi biometrik harus sukses dulu sebelum SessionManager/cookie
// jar dipulihkan dari sini), BUKAN mengikat cookie secara kriptografis ke
// sensor biometrik lewat Cipher/CryptoObject seperti pola paling ketat yang
// direkomendasikan Android. Ini cukup untuk mencegah orang lain yang pegang
// HP tak terkunci langsung masuk app tanpa sidik jari/wajah pemilik, tapi
// bukan proteksi kriptografis penuh terhadap ekstraksi cookie dari
// penyimpanan perangkat yang di-root. Peningkatan ke CryptoObject-based
// binding dicatat sebagai utang teknis di PHASES.md.
object SessionStore {
    private const val PREFS_NAME = "aumo_secure_session"
    private const val KEY_COOKIE = "auth_cookie"
    private const val KEY_USER_ID = "user_id"
    private const val KEY_FULL_NAME = "full_name"
    private const val KEY_KEEP_SIGNED_IN = "keep_signed_in"
    private const val KEY_BIOMETRIC_ENABLED = "biometric_enabled"

    private lateinit var prefs: SharedPreferences

    fun init(context: Context) {
        val masterKey = MasterKey.Builder(context.applicationContext)
            .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
            .build()
        prefs = EncryptedSharedPreferences.create(
            context.applicationContext,
            PREFS_NAME,
            masterKey,
            EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
            EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
        )
    }

    // Dipanggil dari LoginViewModel setelah login sukses, menyimpan info
    // tampilan + flag. Nilai cookie itu sendiri TIDAK disimpan di sini —
    // itu ditulis langsung oleh PersistentCookiesStorage.addCookie() saat
    // Set-Cookie diterima, supaya selalu sinkron dengan cookie yang
    // benar-benar dipakai Ktor untuk request berikutnya.
    fun saveSessionInfo(userId: String, fullName: String, keepSignedIn: Boolean, biometricEnabled: Boolean) {
        if (!keepSignedIn) {
            clear()
            return
        }
        prefs.edit()
            .putString(KEY_USER_ID, userId)
            .putString(KEY_FULL_NAME, fullName)
            .putBoolean(KEY_KEEP_SIGNED_IN, true)
            .putBoolean(KEY_BIOMETRIC_ENABLED, biometricEnabled)
            .apply()
    }

    fun saveCookie(value: String) {
        prefs.edit().putString(KEY_COOKIE, value).apply()
    }

    fun loadCookie(): String? = prefs.getString(KEY_COOKIE, null)

    fun clearCookie() {
        prefs.edit().remove(KEY_COOKIE).apply()
    }

    fun hasSavedSession(): Boolean =
        prefs.getBoolean(KEY_KEEP_SIGNED_IN, false) && !prefs.getString(KEY_COOKIE, null).isNullOrBlank()

    fun isBiometricEnabled(): Boolean = prefs.getBoolean(KEY_BIOMETRIC_ENABLED, false)

    fun restoreIntoSessionManager() {
        SessionManager.userId = prefs.getString(KEY_USER_ID, null)
        SessionManager.fullName = prefs.getString(KEY_FULL_NAME, null)
        SessionManager.keepSignedIn = prefs.getBoolean(KEY_KEEP_SIGNED_IN, false)
        SessionManager.isLoggedIn = hasSavedSession()
    }

    fun clear() {
        prefs.edit().clear().apply()
    }
}
