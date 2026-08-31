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

    fun open(request: CreatePeriodRequest) {
        api.open(request).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) = load()
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) = Unit
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
