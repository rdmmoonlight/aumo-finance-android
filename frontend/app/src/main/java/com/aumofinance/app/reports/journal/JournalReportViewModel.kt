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

    private val _selectedPeriodName = MutableLiveData<String?>(null)
    val selectedPeriodName: LiveData<String?> = _selectedPeriodName

    fun loadGeneral() {
        api.getGeneralJournal().enqueue(handler())
    }

    fun loadAdjusting() {
        api.getAdjustingJournal().enqueue(handler())
    }

    private fun handler() = object : Callback<JournalReportResponse> {
        override fun onResponse(call: Call<JournalReportResponse>, response: Response<JournalReportResponse>) {
            val body = response.body()
            _entries.value = body?.entries ?: emptyList()
            _selectedPeriodName.value = body?.selectedPeriodName
        }
        override fun onFailure(call: Call<JournalReportResponse>, t: Throwable) {
            _entries.value = emptyList()
        }
    }
}
