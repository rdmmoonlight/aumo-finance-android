package com.aumofinance.app.dashboard

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import kotlinx.coroutines.launch

class DashboardViewModel : ViewModel() {
    private val api = DashboardApi()

    private val _summary = MutableLiveData<DashboardSummary?>()
    val summary: LiveData<DashboardSummary?> = _summary

    fun load() {
        viewModelScope.launch {
            _summary.value =
                try {
                    api.getSummary().body<DashboardSummary>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
