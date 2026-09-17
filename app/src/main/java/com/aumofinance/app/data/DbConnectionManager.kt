package com.aumofinance.app.data

import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

object DbConnectionManager {
    private val _isDbConnected = MutableStateFlow(false)
    val isDbConnected: StateFlow<Boolean> = _isDbConnected.asStateFlow()

    // Render free tier butuh waktu ~30-40 detik untuk murni ready dari mode sleep
    private const val COLD_START_DELAY_MS = 40_000L

    /**
     * Menahan indikator tetap KUNING (berkedip) selama jeda waktu cold start Render.
     * Setelah jeda waktu terlampaui dan ping berhasil, indikator baru HIJAU.
     */
    suspend fun ensureConnected(onPingDb: suspend () -> Unit) {
        // Jika sudah hijau dari layar sebelumnya, tidak perlu menunggu ulang
        if (_isDbConnected.value) return

        _isDbConnected.value = false // Tetap kuning berkedip

        try {
            // 1. Tahan di kuning berkedip selama durasi akun free tidur
            delay(COLD_START_DELAY_MS)

            // 2. Lakukan ping ke DB / API Render untuk konfirmasi akhir
            onPingDb()

            // 3. Hanya set HIJAU jika server terbukti sudah ready
            _isDbConnected.value = true
        } catch (e: Exception) {
            _isDbConnected.value = false // Tetap kuning/gagal jika server belum merespon
        }
    }

    fun setDisconnected() {
        _isDbConnected.value = false
    }
}
