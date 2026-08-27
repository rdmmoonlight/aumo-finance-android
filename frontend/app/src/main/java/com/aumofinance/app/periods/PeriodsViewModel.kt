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

    fun load() {
        api.list().enqueue(object : Callback<List<Period>> {
            override fun onResponse(call: Call<List<Period>>, response: Response<List<Period>>) {
                _periods.value = response.body() ?: emptyList()
            }
            override fun onFailure(call: Call<List<Period>>, t: Throwable) {
                _periods.value = emptyList()
            }
        })
    }

    fun open(request: OpenPeriodRequest) {
        api.open(request).enqueue(object : Callback<Period> {
            override fun onResponse(call: Call<Period>, response: Response<Period>) = load()
            override fun onFailure(call: Call<Period>, t: Throwable) = Unit
        })
    }

    fun close(id: Int) {
        api.close(id).enqueue(object : Callback<Unit> {
            override fun onResponse(call: Call<Unit>, response: Response<Unit>) = load()
            override fun onFailure(call: Call<Unit>, t: Throwable) = Unit
        })
    }
}
