package com.aumofinance.app.reports.financials

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import io.ktor.client.statement.HttpResponse
import kotlinx.coroutines.launch

class FinancialsViewModel : ViewModel() {
    private val api = FinancialsApi()

    val incomeStatement = MutableLiveData<IncomeStatementReport?>()
    val retainedEarnings = MutableLiveData<RetainedEarningsReport?>()
    val financialPosition = MutableLiveData<FinancialPositionReport?>()
    val cashFlow = MutableLiveData<CashFlowReport?>()
    val closingJournal = MutableLiveData<ClosingJournalReport?>()

    fun loadIncomeStatement() {
        load(incomeStatement) { api.getIncomeStatement() }
    }

    fun loadRetainedEarnings() {
        load(retainedEarnings) { api.getRetainedEarnings() }
    }

    fun loadFinancialPosition(isPostClosing: Boolean = false) {
        load(financialPosition) { api.getFinancialPosition(isPostClosing) }
    }

    fun loadCashFlow() {
        load(cashFlow) { api.getCashFlow() }
    }

    fun loadClosingJournal() {
        load(closingJournal) { api.getClosingJournal() }
    }

    private inline fun <reified T> load(target: MutableLiveData<T?>, crossinline call: suspend () -> HttpResponse) {
        viewModelScope.launch {
            target.value = try {
                call().body<T>()
            } catch (t: Throwable) {
                null
            }
        }
    }
}
