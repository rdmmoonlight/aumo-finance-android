package com.aumofinance.app.network

// Menyimpan JWT hasil login di memori untuk dipasang ke header Authorization
// pada setiap request (lihat AuthInterceptor). TODO: pindahkan persistensi ke
// EncryptedSharedPreferences supaya sesi bertahan setelah app di-restart —
// saat ini token hilang begitu proses aplikasi mati.
object SessionManager {
    var token: String? = null
    var userId: String? = null
    var fullName: String? = null

    fun isLoggedIn(): Boolean = !token.isNullOrBlank()

    fun clear() {
        token = null
        userId = null
        fullName = null
    }
}
