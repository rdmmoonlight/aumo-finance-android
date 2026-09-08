package com.aumofinance.app.reports.trialbalance

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class TrialBalanceViewModel : ViewModel() {
    private val api = TrialBalanceApi()

    private val _report = MutableLiveData<TrialBalanceReport?>()
    val report: LiveData<TrialBalanceReport?> = _report

    fun load(type: String) {
        viewModelScope.launch {
            _report.value = try {
                api.getTrialBalance(type).body<TrialBalanceReport>()
            } catch (t: Throwable) {
                null
            }
        }
    }
}
