package com.aumofinance.app.reports.financials.cashflow

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.reports.financials.CashFlowReport
import com.aumofinance.app.reports.financials.FinancialsApi
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class CashFlowViewModel : ViewModel() {
    private val api = FinancialsApi()

    var report: CashFlowReport? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            report =
                try {
                    api.getCashFlow().body<CashFlowReport>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
