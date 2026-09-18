package com.aumofinance.app.data

import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

object DbConnectionManager {
    private val _isDbConnected = MutableStateFlow(false)
    val isDbConnected: StateFlow<Boolean> = _isDbConnected.asStateFlow()

    // Jeda antar percobaan ulang saat server masih cold start.
    private const val RETRY_DELAY_MS = 3_000L

    /**
     * Memastikan koneksi ke backend Render.com aktif.
     *
     * Jika server dalam posisi 'warm', fungsi ini akan selesai cepat (1-2 detik) dan indikator langsung hijau.
     * Jika server dalam posisi 'cold start', Ktor client di `onPingDb()` bisa saja timeout duluan sebelum
     * Render selesai booting. Sebelumnya kegagalan pertama langsung membuat indikator KUNING permanen
     * (tidak ada retry) — sekarang fungsi ini akan MENCOBA ULANG setiap RETRY_DELAY_MS sampai berhasil,
     * supaya kuning berkedip benar-benar berarti "masih menyambung", bukan "sudah menyerah".
     */
    suspend fun ensureConnected(onPingDb: suspend () -> Unit) {
        // Jika sudah terhubung (hijau) dari layar sebelumnya, tidak perlu ping ulang
        if (_isDbConnected.value) return

        while (true) {
            _isDbConnected.value = false // KUNING berkedip (proses connecting)

            try {
                // Tembak API via Ktor. Baris ini akan menunggu respon HTTP pertama dari Render,
                // dibatasi timeout yang lega (lihat ApiClient) supaya cold start sempat selesai.
                onPingDb()

                // Hanya set HIJAU setelah respon resmi diterima dari server
                _isDbConnected.value = true
                return
            } catch (e: Exception) {
                // Jangan menyerah: server kemungkinan masih cold start.
                // Tunggu sebentar lalu coba lagi, tetap KUNING berkedip di layar.
                if (_isDbConnected.value) return // sempat ditandai connected dari sumber lain, berhenti
                delay(RETRY_DELAY_MS)
            }
        }
    }

    /**
     * Panggil ini dari halaman mana pun (mis. Periods) begitu sebuah request ke backend
     * TERBUKTI berhasil. Ini langsung menandai koneksi hidup tanpa menunggu ping Home selesai,
     * jadi kalau Periods duluan yang berhasil membangunkan server, indikator di Home ikut
     * langsung hijau saat halaman itu dibuka lagi.
     */
    fun markConnected() {
        _isDbConnected.value = true
    }

    /**
     * Panggil ini jika terjadi network failure/timeout di halaman mana pun
     * untuk mengubah status indikator kembali ke KUNING.
     */
    fun setDisconnected() {
        _isDbConnected.value = false
    }
}
