package com.aumofinance.app.network

import android.content.Context
import android.content.SharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey

// Penyimpanan sesi terenkripsi (AES256-GCM via Android Keystore, lewat
// EncryptedSharedPreferences) — dipakai untuk fitur "Ingat saya" dan
// "Masuk dengan biometrik". SessionManager (in-memory) tetap sumber
// kebenaran SELAMA app berjalan; SessionStore hanya dibaca sekali di
// SplashActivity untuk memulihkan sesi setelah app di-restart.
//
// CATATAN JUJUR soal keamanan biometrik: implementasi ini memakai
// BiometricPrompt sebagai GERBANG masuk ke sesi yang sudah tersimpan
// (autentikasi biometrik harus sukses dulu sebelum SessionManager diisi
// dari sini), BUKAN mengikat token secara kriptografis ke sensor biometrik
// lewat Cipher/CryptoObject seperti pola paling ketat yang direkomendasikan
// Android. Ini cukup untuk mencegah orang lain yang pegang HP tak terkunci
// langsung masuk app tanpa sidik jari/wajah pemilik, tapi bukan proteksi
// kriptografis penuh terhadap ekstraksi token dari penyimpanan perangkat
// yang di-root. Peningkatan ke CryptoObject-based binding dicatat sebagai
// utang teknis di PHASES.md.
object SessionStore {
    private const val PREFS_NAME = "aumo_secure_session"
    private const val KEY_TOKEN = "token"
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

    fun save(token: String, userId: String, fullName: String, keepSignedIn: Boolean, biometricEnabled: Boolean) {
        if (!keepSignedIn) {
            clear()
            return
        }
        prefs.edit()
            .putString(KEY_TOKEN, token)
            .putString(KEY_USER_ID, userId)
            .putString(KEY_FULL_NAME, fullName)
            .putBoolean(KEY_KEEP_SIGNED_IN, true)
            .putBoolean(KEY_BIOMETRIC_ENABLED, biometricEnabled)
            .apply()
    }

    fun hasSavedSession(): Boolean =
        prefs.getBoolean(KEY_KEEP_SIGNED_IN, false) && !prefs.getString(KEY_TOKEN, null).isNullOrBlank()

    fun isBiometricEnabled(): Boolean = prefs.getBoolean(KEY_BIOMETRIC_ENABLED, false)

    fun restoreIntoSessionManager() {
        SessionManager.token = prefs.getString(KEY_TOKEN, null)
        SessionManager.userId = prefs.getString(KEY_USER_ID, null)
        SessionManager.fullName = prefs.getString(KEY_FULL_NAME, null)
    }

    fun clear() {
        prefs.edit().clear().apply()
    }
}
