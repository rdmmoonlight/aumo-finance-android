package com.aumofinance.app.reports.journal

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import com.aumofinance.app.journal.JournalApi
import com.aumofinance.app.journal.SimpleApiResponse
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

// State Compose (bukan LiveData lagi) — mengikuti pola PeriodsViewModel/
// JournalEntryViewModel sejak halaman ini dipindah dari Activity/View ke
// Jetpack Compose (lihat JournalReportScreen.kt). Satu ViewModel dipakai
// baik oleh General maupun Adjusting Journal — loadGeneral()/loadAdjusting()
// beda endpoint, delete() otomatis reload endpoint yang terakhir dipakai.
class JournalReportViewModel : ViewModel() {
    private val reportApi = ApiClient.retrofit.create(JournalReportApi::class.java)
    private val journalApi = ApiClient.retrofit.create(JournalApi::class.java)

    var entries: List<JournalReportEntry> by mutableStateOf(emptyList())
        private set
    var selectedPeriodName: String? by mutableStateOf(null)
        private set
    var toastMessage: String? by mutableStateOf(null)
        private set

    private var isAdjusting = false

    fun loadGeneral() {
        isAdjusting = false
        reportApi.getGeneralJournal().enqueue(handler())
    }

    fun loadAdjusting() {
        isAdjusting = true
        reportApi.getAdjustingJournal().enqueue(handler())
    }

    private fun reload() {
        if (isAdjusting) loadAdjusting() else loadGeneral()
    }

    private fun handler() = object : Callback<JournalReportResponse> {
        override fun onResponse(call: Call<JournalReportResponse>, response: Response<JournalReportResponse>) {
            val body = response.body()
            entries = body?.entries ?: emptyList()
            selectedPeriodName = body?.selectedPeriodName
        }
        override fun onFailure(call: Call<JournalReportResponse>, t: Throwable) {
            entries = emptyList()
        }
    }

    fun delete(entry: JournalReportEntry) {
        journalApi.delete(entry.id).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) {
                if (response.isSuccessful && response.body()?.success == true) {
                    reload()
                } else {
                    toastMessage = response.body()?.message ?: "Failed to delete entry."
                }
            }
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
                toastMessage = t.message ?: "Connection failed."
            }
        })
    }

    fun clearToast() {
        toastMessage = null
    }
}
