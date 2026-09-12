package com.aumofinance.app.reports.trialbalance.unadjusted

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.reports.trialbalance.TrialBalanceApi
import com.aumofinance.app.reports.trialbalance.TrialBalanceReport
import io.ktor.client.call.body
import kotlinx.coroutines.launch

// Neraca Saldo (belum disesuaikan): type="unadjusted", hanya jurnal General.
class TrialBalanceViewModel : ViewModel() {
    private val api = TrialBalanceApi()

    var report: TrialBalanceReport? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            report =
                try {
                    api.getTrialBalance("unadjusted").body<TrialBalanceReport>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
