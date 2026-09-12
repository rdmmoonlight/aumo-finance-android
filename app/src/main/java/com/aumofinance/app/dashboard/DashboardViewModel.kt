package com.aumofinance.app.dashboard

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import kotlinx.coroutines.launch

// State Compose (bukan LiveData) — mengikuti pola JournalEntryViewModel/PeriodsViewModel
// sejak halaman ini dipindah dari Activity/View ke Jetpack Compose.
class DashboardViewModel : ViewModel() {
    private val api = DashboardApi()

    var summary: DashboardSummary? by mutableStateOf(null)
        private set

    fun load() {
        viewModelScope.launch {
            summary =
                try {
                    api.getSummary().body<DashboardSummary>()
                } catch (t: Throwable) {
                    null
                }
        }
    }
}
