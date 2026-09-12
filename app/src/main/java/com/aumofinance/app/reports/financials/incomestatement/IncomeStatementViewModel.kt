package com.aumofinance.app.reports.financials.incomestatement

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.reports.financials.FinancialsApi
import com.aumofinance.app.reports.financials.IncomeStatementReport
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class IncomeStatementViewModel : ViewModel() {
    private val api = FinancialsApi()

    var report: IncomeStatementReport? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            report =
                try {
                    api.getIncomeStatement().body<IncomeStatementReport>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
