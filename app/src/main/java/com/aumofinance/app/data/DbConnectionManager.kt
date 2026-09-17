package com.aumofinance.app.data

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

/**
 * Singleton penampung status koneksi DB tunggal (Render.com).
 * Menjamin status koneksi tidak ter-reset saat pengguna berpindah halaman.
 */
object DbConnectionManager {
    private val _isDbConnected = MutableStateFlow(false)
    val isDbConnected: StateFlow<Boolean> = _isDbConnected.asStateFlow()

    /**
     * Memanggil ping/koneksi ke DB Render.com.
     * Jika sudah pernah terhubung, tidak akan melakukan reconnect/reset state.
     */
    suspend fun ensureConnected(onPingDb: suspend () -> Unit) {
        if (_isDbConnected.value) return // Sudah terhubung, jangan reset ke kuning lagi

        try {
            // Eksekusi ping/query riil ke backend Render.com
            onPingDb()
            _isDbConnected.value = true
        } catch (e: Exception) {
            _isDbConnected.value = false
        }
    }

    /**
     * Panggil ini jika API call di halaman manapun mengalami network error/timeout
     * agar indikator kembali ke kuning saat mencoba memulihkan koneksi.
     */
    fun setDisconnected() {
        _isDbConnected.value = false
    }
}
