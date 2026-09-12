package com.aumofinance.app.reports.financials.closingjournal

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.reports.financials.ClosingJournalReport
import com.aumofinance.app.reports.financials.FinancialsApi
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class ClosingJournalViewModel : ViewModel() {
    private val api = FinancialsApi()

    var report: ClosingJournalReport? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            report =
                try {
                    api.getClosingJournal().body<ClosingJournalReport>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
