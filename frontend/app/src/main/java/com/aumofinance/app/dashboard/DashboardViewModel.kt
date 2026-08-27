package com.aumofinance.app.dashboard

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class DashboardViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(DashboardApi::class.java)

    private val _summary = MutableLiveData<DashboardSummary?>()
    val summary: LiveData<DashboardSummary?> = _summary

    fun load(periodId: Int) {
        api.getSummary(periodId).enqueue(object : Callback<DashboardSummary> {
            override fun onResponse(call: Call<DashboardSummary>, response: Response<DashboardSummary>) {
                _summary.value = response.body()
            }
            override fun onFailure(call: Call<DashboardSummary>, t: Throwable) {
                _summary.value = null
            }
        })
    }
}
