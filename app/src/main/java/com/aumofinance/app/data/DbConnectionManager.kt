package com.aumofinance.app.data

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

object DbConnectionManager {
    private val _isConnected = MutableStateFlow(false)
    val isConnected: StateFlow<Boolean> = _isConnected.asStateFlow()

    suspend fun checkOrInitConnection() {
        if (_isConnected.value) return // Kalau sudah terkoneksi, tidak perlu re-connect

        _isConnected.value = false // Default kuning berkedip saat connecting
        try {
            // Panggil endpoint health check/ping API Render.com di sini
            // contoh: apiService.pingDatabase()
            _isConnected.value = true // Hijau saat sukses
        } catch (e: Exception) {
            _isConnected.value = false // Tetap/kembali berkedip jika gagal/retrying
        }
    }
}
