package com.aumofinance.app.reports.worksheet

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class WorksheetViewModel : ViewModel() {
    private val api = WorksheetApi()

    private val _report = MutableLiveData<WorksheetReport?>()
    val report: LiveData<WorksheetReport?> = _report

    fun load() {
        viewModelScope.launch {
            _report.value = try {
                api.getWorksheet().body<WorksheetReport>()
            } catch (t: Throwable) {
                null
            }
        }
    }
}
