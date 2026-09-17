package com.aumofinance.app.data

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

object DbConnectionManager {
    private val _isDbConnected = MutableStateFlow(false)
    val isDbConnected: StateFlow<Boolean> = _isDbConnected.asStateFlow()

    suspend fun ensureConnected(onPingDb: suspend () -> Unit) {
        // Jika dari halaman sebelumnya sudah terhubung (hijau), tidak perlu ping ulang
        if (_isDbConnected.value) return

        _isDbConnected.value = false // Default tetap KUNING berkedip saat belum siap

        try {
            // Eksekusi Ktor GET request.
            // Selama Render.com cold start, baris ini akan gantung (suspending)
            // menahan indikator tetap berkedip kuning hingga server me-return respon.
            onPingDb()

            // Begitu respon pertama dari Render diterima, server terbukti aktif & siap -> Ubah ke HIJAU
            _isDbConnected.value = true
        } catch (e: Exception) {
            _isDbConnected.value = false
        }
    }

    fun setDisconnected() {
        _isDbConnected.value = false
    }
}
