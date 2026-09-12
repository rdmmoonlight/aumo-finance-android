package com.aumofinance.app.reports.financials.retainedearnings

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.reports.financials.FinancialsApi
import com.aumofinance.app.reports.financials.RetainedEarningsReport
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class RetainedEarningsViewModel : ViewModel() {
    private val api = FinancialsApi()

    var report: RetainedEarningsReport? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            report =
                try {
                    api.getRetainedEarnings().body<RetainedEarningsReport>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
