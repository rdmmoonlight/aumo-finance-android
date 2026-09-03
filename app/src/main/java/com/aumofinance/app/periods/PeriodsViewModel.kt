package com.aumofinance.app.periods

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class PeriodsViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(PeriodsApi::class.java)

    private val _periods = MutableLiveData<List<Period>>(emptyList())
    val periods: LiveData<List<Period>> = _periods

    private val _selectedPeriodId = MutableLiveData<Int?>(null)
    val selectedPeriodId: LiveData<Int?> = _selectedPeriodId

    // Info untuk dialog "Open New Period": kondisi mana yang harus
    // ditampilkan (belum ada periode yang pernah ditutup, vs sudah ada) dan
    // daftar akun Cash/Bank/Retained Earnings yang tersedia jika kondisinya
    // "sudah ada periode yang pernah ditutup".
    private val _openPeriodInfo = MutableLiveData<OpenPeriodInfoResponse?>(null)
    val openPeriodInfo: LiveData<OpenPeriodInfoResponse?> = _openPeriodInfo

    // Hasil terakhir dari operasi open()/close() — dipakai Activity untuk
    // menampilkan pesan sukses/gagal dari backend (mis. "period already
    // exists", "reference codes already in use") alih-alih diam saja.
    private val _actionResult = MutableLiveData<SimpleApiResponse?>(null)
    val actionResult: LiveData<SimpleApiResponse?> = _actionResult

    fun load() {
        api.list().enqueue(object : Callback<PeriodsResponse> {
            override fun onResponse(call: Call<PeriodsResponse>, response: Response<PeriodsResponse>) {
                val body = response.body()
                _periods.value = body?.periods ?: emptyList()
                _selectedPeriodId.value = body?.selectedPeriodId
            }
            override fun onFailure(call: Call<PeriodsResponse>, t: Throwable) {
                _periods.value = emptyList()
            }
        })
    }

    // Dipanggil sebelum menampilkan dialog Open New Period, supaya dialog
    // tahu harus menampilkan form "daftar akun baru" atau "lanjutkan akun
    // lama" — sesuai kondisi belum/sudah pernah ada periode yang ditutup.
    fun loadOpenPeriodInfo() {
        api.openInfo().enqueue(object : Callback<OpenPeriodInfoResponse> {
            override fun onResponse(call: Call<OpenPeriodInfoResponse>, response: Response<OpenPeriodInfoResponse>) {
                _openPeriodInfo.value = response.body()
            }
            override fun onFailure(call: Call<OpenPeriodInfoResponse>, t: Throwable) {
                _openPeriodInfo.value = null
            }
        })
    }

    fun open(request: CreatePeriodRequest) {
        api.open(request).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) {
                val body = response.body() ?: SimpleApiResponse(
                    success = false,
                    message = "Failed to open period (HTTP ${response.code()})."
                )
                _actionResult.value = body
                if (body.success) load()
            }
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
                _actionResult.value = SimpleApiResponse(success = false, message = t.message ?: "Network error.")
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
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) = load()
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) = Unit
        })
    }
}
