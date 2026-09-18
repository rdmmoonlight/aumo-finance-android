package com.aumofinance.app.data

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/**
 * Satu-satunya sumber kebenaran status koneksi ke backend Render. Tidak terikat
 * lifecycle Activity/Screen mana pun — object ini hidup selama proses aplikasi
 * hidup, termasuk heartbeat-nya, supaya indikator selalu mencerminkan kondisi
 * server yang sebenarnya walau user sedang di halaman lain.
 */
object DbConnectionManager {
    private val _isDbConnected = MutableStateFlow(false)
    val isDbConnected: StateFlow<Boolean> = _isDbConnected.asStateFlow()

    // Jeda antar percobaan ulang saat server masih cold start / heartbeat gagal.
    private const val RETRY_DELAY_MS = 3_000L

    // Render men-tidurkan server free-tier setelah ~15 menit tanpa traffic.
    // Heartbeat jalan tiap 10 menit (beri margin 5 menit) supaya server tidak
    // pernah sempat idle sampai batas itu, selama app masih berjalan.
    private const val HEARTBEAT_INTERVAL_MS = 10 * 60 * 1000L

    // Scope sendiri (bukan lifecycleScope milik Activity) supaya heartbeat tetap
    // berjalan di background walau user berpindah-pindah halaman/Activity.
    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.IO)
    private var heartbeatJob: Job? = null

    /**
     * Memastikan koneksi ke backend Render.com aktif saat pertama kali dipanggil.
     *
     * Jika server dalam posisi 'warm', fungsi ini akan selesai cepat (1-2 detik) dan indikator langsung hijau.
     * Jika server dalam posisi 'cold start', ping bisa saja timeout duluan sebelum Render selesai booting —
     * fungsi ini akan MENCOBA ULANG setiap RETRY_DELAY_MS sampai berhasil, supaya kuning berkedip benar-benar
     * berarti "masih menyambung", bukan "sudah menyerah".
     */
    suspend fun ensureConnected(onPingDb: suspend () -> Unit) {
        // Jika sudah terhubung (hijau), tidak perlu ping ulang
        if (_isDbConnected.value) return
        connectWithRetry(onPingDb)
    }

    /**
     * Jalankan heartbeat berkala di background (sekali per proses aplikasi) yang menjaga
     * server tetap bangun sebelum sempat idle 15 menit dan menjaga status koneksi tetap akurat
     * meski user tidak sedang membuka halaman mana pun yang memanggil API.
     *
     * Aman dipanggil berkali-kali (mis. tiap kali HomeActivity dibuat ulang) — heartbeat
     * yang sudah berjalan tidak akan diduplikasi.
     */
    fun startHeartbeat(onPingDb: suspend () -> Unit) {
        if (heartbeatJob?.isActive == true) return

        heartbeatJob =
            scope.launch {
                while (true) {
                    delay(HEARTBEAT_INTERVAL_MS)
                    connectWithRetry(onPingDb)
                }
            }
    }

    private suspend fun connectWithRetry(onPingDb: suspend () -> Unit) {
        while (true) {
            _isDbConnected.value = false // KUNING berkedip (proses connecting)

            try {
                onPingDb()
                _isDbConnected.value = true // HIJAU hanya setelah respon resmi diterima
                return
            } catch (e: Exception) {
                // Jangan menyerah: tunggu sebentar lalu coba lagi, tetap KUNING berkedip.
                delay(RETRY_DELAY_MS)
            }
        }
    }

    /**
     * Panggil ini dari halaman mana pun begitu sebuah request ke backend TERBUKTI berhasil.
     * Ini langsung menandai koneksi hidup tanpa menunggu ping/heartbeat lain selesai.
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
