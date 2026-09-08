package com.aumofinance.app.periods

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import io.ktor.http.isSuccess
import kotlinx.coroutines.launch

// State Compose (bukan LiveData lagi) — mengikuti pola JournalEntryViewModel
// sejak halaman ini dipindah dari Activity/View ke Jetpack Compose.
class PeriodsViewModel : ViewModel() {
    private val api = PeriodsApi()

    var periods: List<Period> by mutableStateOf(emptyList())
        private set
    var selectedPeriodId: Int? by mutableStateOf(null)
        private set

    // Non-null berarti dialog "Open New Period" sedang ditampilkan; isinya
    // memberi tahu kondisi mana yang berlaku (belum/sudah pernah ada periode
    // yang ditutup) dan daftar akun yang tersedia untuk kondisi kedua.
    var openPeriodInfo: OpenPeriodInfoResponse? by mutableStateOf(null)
        private set

    // Pesan sukses/gagal terakhir dari backend, ditampilkan sebagai Toast lalu
    // dibersihkan lewat clearToast() — supaya kegagalan (mis. "period already
    // exists") tidak lagi diam saja seperti sebelumnya.
    var toastMessage: String? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            try {
                val body = api.list().body<PeriodsResponse>()
                periods = body.periods
                selectedPeriodId = body.selectedPeriodId
            } catch (t: Throwable) {
                periods = emptyList()
            }
        }
    }

    // Dipanggil sebelum menampilkan dialog Open New Period, supaya dialog
    // tahu harus menampilkan form "daftar akun baru" atau "lanjutkan akun
    // lama" — sesuai kondisi belum/sudah pernah ada periode yang ditutup.
    fun openNewPeriodDialog() {
        viewModelScope.launch {
            try {
                openPeriodInfo = api.openInfo().body<OpenPeriodInfoResponse>()
            } catch (t: Throwable) {
                toastMessage = t.message ?: "Network error."
            }
        }
    }

    fun dismissOpenPeriodDialog() {
        openPeriodInfo = null
    }

    fun open(request: CreatePeriodRequest) {
        viewModelScope.launch {
            try {
                val response = api.open(request)
                val body = try {
                    response.body<SimpleApiResponse>()
                } catch (parseError: Throwable) {
                    SimpleApiResponse(success = false, message = "Failed to open period (HTTP ${response.status.value}).")
                }
                toastMessage = body.message
                if (body.success) {
                    openPeriodInfo = null
                    load()
                }
            } catch (t: Throwable) {
                toastMessage = t.message ?: "Network error."
            }
        }
    }

    // Menandai periode ini sebagai yang sedang di-VIEW (ikon mata di halaman
    // Periods) — semua halaman lain (Dashboard, Journal, Laporan) mengikuti
    // periode mana yang IsSelected=true, bukan menerima periodId sebagai parameter.
    fun select(id: Int) {
        viewModelScope.launch {
            try {
                val response = api.select(id)
                val body = response.body<SimpleApiResponse>()
                if (response.status.isSuccess() && body.success) {
                    load()
                } else {
                    toastMessage = body.message.ifBlank { "Failed to switch period (HTTP ${response.status.value})." }
                }
            } catch (t: Throwable) {
                toastMessage = t.message ?: "Network error."
            }
        }
    }

    fun close(id: Int) {
        viewModelScope.launch {
            try {
                val response = api.close(id)
                toastMessage = response.body<SimpleApiResponse>().message
                load()
            } catch (t: Throwable) {
                toastMessage = t.message ?: "Network error."
            }
        }
    }

    fun clearToast() {
        toastMessage = null
    }
}
