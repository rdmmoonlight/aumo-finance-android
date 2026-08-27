package com.aumofinance.app.reports.journal

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class JournalReportViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(JournalReportApi::class.java)

    private val _entries = MutableLiveData<List<JournalReportEntry>>(emptyList())
    val entries: LiveData<List<JournalReportEntry>> = _entries

    fun load(periodId: Int, type: String) {
        api.getReport(periodId, type).enqueue(object : Callback<List<JournalReportEntry>> {
            override fun onResponse(call: Call<List<JournalReportEntry>>, response: Response<List<JournalReportEntry>>) {
                _entries.value = response.body() ?: emptyList()
            }
            override fun onFailure(call: Call<List<JournalReportEntry>>, t: Throwable) {
                _entries.value = emptyList()
            }
        })
    }
}
