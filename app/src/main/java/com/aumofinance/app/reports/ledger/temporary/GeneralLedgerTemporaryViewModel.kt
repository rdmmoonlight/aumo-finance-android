package com.aumofinance.app.reports.ledger.temporary

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.reports.ledger.LedgerApi
import com.aumofinance.app.reports.ledger.LedgerResponse
import io.ktor.client.call.body
import kotlinx.coroutines.launch

// Akun Temporary (Laba Rugi): OperatingIncome, OperatingExpenses, OtherIncome,
// OtherExpenses. isTemporary=true.
class GeneralLedgerTemporaryViewModel : ViewModel() {
    private val api = LedgerApi()

    var report: LedgerResponse? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            report =
                try {
                    api.getLedger(isTemporary = true).body<LedgerResponse>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
