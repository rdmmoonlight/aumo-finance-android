package com.aumofinance.app.reports.ledger

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class LedgerViewModel : ViewModel() {
    private val api = LedgerApi()

    private val _report = MutableLiveData<LedgerResponse?>()
    val report: LiveData<LedgerResponse?> = _report

    fun load(isTemporary: Boolean) {
        viewModelScope.launch {
            _report.value = try {
                api.getLedger(isTemporary).body<LedgerResponse>()
            } catch (t: Throwable) {
                null
            }
        }
    }
}
