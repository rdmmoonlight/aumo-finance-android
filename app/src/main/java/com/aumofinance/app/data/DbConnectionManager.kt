package com.aumofinance.app.data

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

object DbConnectionManager {
    private val _isDbConnected = MutableStateFlow(false)
    val isDbConnected: StateFlow<Boolean> = _isDbConnected.asStateFlow()

    /**
     * Memastikan koneksi ke backend Render.com aktif.
     *
     * Jika server dalam posisi 'warm', fungsi ini akan selesai cepat (1-2 detik) dan indikator langsung hijau.
     * Jika server dalam posisi 'cold start', Ktor client di `onPingDb()` akan menggantung (suspending)
     * secara alami sampai server Render selesai booting up, menjaga indikator tetap kuning.
     */
    suspend fun ensureConnected(onPingDb: suspend () -> Unit) {
        // Jika sudah terhubung (hijau) dari layar sebelumnya, tidak perlu ping ulang
        if (_isDbConnected.value) return

        _isDbConnected.value = false // Default KUNING berkedip (proses connecting)

        try {
            // Tembak API via Ktor.
            // Baris ini akan menunggu respon HTTP pertama dari Render.
            onPingDb()

            // Hanya set HIJAU setelah respon resmi diterima dari server
            _isDbConnected.value = true
        } catch (e: Exception) {
            // Tetap KUNING berkedip jika request gagal/timeout
            _isDbConnected.value = false
        }
    }

    /**
     * Panggil ini jika terjadi network failure/timeout di halaman mana pun
     * untuk mengubah status indikator kembali ke KUNING.
     */
    fun setDisconnected() {
        _isDbConnected.value = false
    }
}
