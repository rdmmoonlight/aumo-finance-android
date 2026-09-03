package com.aumofinance.app.periods

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

// State Compose (bukan LiveData lagi) — mengikuti pola JournalEntryViewModel
// sejak halaman ini dipindah dari Activity/View ke Jetpack Compose.
class PeriodsViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(PeriodsApi::class.java)

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
        api.list().enqueue(object : Callback<PeriodsResponse> {
            override fun onResponse(call: Call<PeriodsResponse>, response: Response<PeriodsResponse>) {
                val body = response.body()
                periods = body?.periods ?: emptyList()
                selectedPeriodId = body?.selectedPeriodId
            }
            override fun onFailure(call: Call<PeriodsResponse>, t: Throwable) {
                periods = emptyList()
            }
        })
    }

    // Dipanggil sebelum menampilkan dialog Open New Period, supaya dialog
    // tahu harus menampilkan form "daftar akun baru" atau "lanjutkan akun
    // lama" — sesuai kondisi belum/sudah pernah ada periode yang ditutup.
    fun openNewPeriodDialog() {
        api.openInfo().enqueue(object : Callback<OpenPeriodInfoResponse> {
            override fun onResponse(call: Call<OpenPeriodInfoResponse>, response: Response<OpenPeriodInfoResponse>) {
                openPeriodInfo = response.body()
                if (response.body() == null) toastMessage = "Failed to load account info."
            }
            override fun onFailure(call: Call<OpenPeriodInfoResponse>, t: Throwable) {
                toastMessage = t.message ?: "Network error."
            }
        })
    }

    fun dismissOpenPeriodDialog() {
        openPeriodInfo = null
    }

    fun open(request: CreatePeriodRequest) {
        api.open(request).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) {
                val body = response.body() ?: SimpleApiResponse(
                    success = false,
                    message = "Failed to open period (HTTP ${response.code()})."
                )
                toastMessage = body.message
                if (body.success) {
                    openPeriodInfo = null
                    load()
                }
            }
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
                toastMessage = t.message ?: "Network error."
            }
        })
    }

    // Menandai periode ini sebagai yang sedang di-VIEW (ikon mata di halaman
    // Periods) — semua halaman lain (Dashboard, Journal, Laporan) mengikuti
    // periode mana yang IsSelected=true, bukan menerima periodId sebagai parameter.
    fun select(id: Int) {
        api.select(id).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) = load()
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) = Unit
        })
    }

    fun close(id: Int) {
        api.close(id).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) {
                toastMessage = response.body()?.message
                load()
            }
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
                toastMessage = t.message ?: "Network error."
            }
        })
    }

    fun clearToast() {
        toastMessage = null
    }
}
