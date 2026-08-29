package com.aumofinance.app.reports.financials

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class FinancialsViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(FinancialsApi::class.java)

    val incomeStatement = MutableLiveData<IncomeStatementReport?>()
    val retainedEarnings = MutableLiveData<RetainedEarningsReport?>()
    val financialPosition = MutableLiveData<FinancialPositionReport?>()
    val cashFlow = MutableLiveData<CashFlowReport?>()
    val closingJournal = MutableLiveData<ClosingJournalReport?>()

    fun loadIncomeStatement() {
        api.getIncomeStatement().enqueue(simple(incomeStatement))
    }

    fun loadRetainedEarnings() {
        api.getRetainedEarnings().enqueue(simple(retainedEarnings))
    }

    fun loadFinancialPosition(isPostClosing: Boolean = false) {
        api.getFinancialPosition(isPostClosing).enqueue(simple(financialPosition))
    }

    fun loadCashFlow() {
        api.getCashFlow().enqueue(simple(cashFlow))
    }

    fun loadClosingJournal() {
        api.getClosingJournal().enqueue(simple(closingJournal))
    }

    private fun <T> simple(target: MutableLiveData<T?>) = object : Callback<T> {
        override fun onResponse(call: Call<T>, response: Response<T>) {
            target.value = response.body()
        }
        override fun onFailure(call: Call<T>, t: Throwable) {
            target.value = null
        }
    }
}
