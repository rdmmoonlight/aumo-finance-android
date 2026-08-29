package com.aumofinance.app.reports.worksheet

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class WorksheetViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(WorksheetApi::class.java)

    private val _report = MutableLiveData<WorksheetReport?>()
    val report: LiveData<WorksheetReport?> = _report

    fun load() {
        api.getWorksheet().enqueue(object : Callback<WorksheetReport> {
            override fun onResponse(call: Call<WorksheetReport>, response: Response<WorksheetReport>) {
                _report.value = response.body()
            }
            override fun onFailure(call: Call<WorksheetReport>, t: Throwable) {
                _report.value = null
            }
        })
    }
}
