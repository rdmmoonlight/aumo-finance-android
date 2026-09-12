package com.aumofinance.app.reports.ledger.permanent

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.reports.ledger.LedgerApi
import com.aumofinance.app.reports.ledger.LedgerResponse
import io.ktor.client.call.body
import kotlinx.coroutines.launch

// Akun Permanent (Neraca): Assets, Liabilities, Equity. isTemporary=false.
class GeneralLedgerPermanentViewModel : ViewModel() {
    private val api = LedgerApi()

    var report: LedgerResponse? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            report =
                try {
                    api.getLedger(isTemporary = false).body<LedgerResponse>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
