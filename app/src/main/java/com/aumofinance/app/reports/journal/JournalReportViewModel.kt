package com.aumofinance.app.reports.journal

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.journal.JournalApi
import com.aumofinance.app.journal.SimpleApiResponse
import io.ktor.client.call.body
import io.ktor.http.isSuccess
import kotlinx.coroutines.launch

// State Compose (bukan LiveData lagi) — mengikuti pola PeriodsViewModel/
// JournalEntryViewModel sejak halaman ini dipindah dari Activity/View ke
// Jetpack Compose (lihat JournalReportScreen.kt). Satu ViewModel dipakai
// baik oleh General maupun Adjusting Journal — loadGeneral()/loadAdjusting()
// beda endpoint, delete() otomatis reload endpoint yang terakhir dipakai.
class JournalReportViewModel : ViewModel() {
    private val reportApi = JournalReportApi()
    private val journalApi = JournalApi()

    var entries: List<JournalReportEntry> by mutableStateOf(emptyList())
        private set
    var selectedPeriodName: String? by mutableStateOf(null)
        private set
    var toastMessage: String? by mutableStateOf(null)
        private set

    private var isAdjusting = false

    fun loadGeneral() {
        isAdjusting = false
        fetch { reportApi.getGeneralJournal() }
    }

    fun loadAdjusting() {
        isAdjusting = true
        fetch { reportApi.getAdjustingJournal() }
    }

    private fun reload() {
        if (isAdjusting) loadAdjusting() else loadGeneral()
    }

    private fun fetch(call: suspend () -> io.ktor.client.statement.HttpResponse) {
        viewModelScope.launch {
            try {
                val body = call().body<JournalReportResponse>()
                entries = body.entries
                selectedPeriodName = body.selectedPeriodName
            } catch (t: Throwable) {
                entries = emptyList()
            }
        }
    }

    fun delete(entry: JournalReportEntry) {
        viewModelScope.launch {
            try {
                val response = journalApi.delete(entry.id)
                val body = response.body<SimpleApiResponse>()
                if (response.status.isSuccess() && body.success) {
                    reload()
                } else {
                    toastMessage = body.message.ifBlank { "Failed to delete entry." }
                }
            } catch (t: Throwable) {
                toastMessage = t.message ?: "Connection failed."
            }
        }
    }

    fun clearToast() {
        toastMessage = null
    }
}
