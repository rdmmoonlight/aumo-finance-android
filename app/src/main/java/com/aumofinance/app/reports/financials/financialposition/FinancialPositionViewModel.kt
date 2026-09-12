package com.aumofinance.app.reports.financials.financialposition

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.reports.financials.FinancialPositionReport
import com.aumofinance.app.reports.financials.FinancialsApi
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class FinancialPositionViewModel : ViewModel() {
    private val api = FinancialsApi()

    var report: FinancialPositionReport? by mutableStateOf(null)
        private set

    fun load(isPostClosing: Boolean = false) {
        viewModelScope.launch {
            report =
                try {
                    api.getFinancialPosition(isPostClosing).body<FinancialPositionReport>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
