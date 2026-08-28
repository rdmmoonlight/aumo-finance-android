package com.aumofinance.app.reports.financials

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

data class AccountAmount(val accountName: String, val amount: Double)

data class IncomeStatementReport(
    val revenueAccounts: List<AccountAmount>,
    val totalRevenue: Double,
    val expenseAccounts: List<AccountAmount>,
    val totalExpenses: Double,
    val operatingIncome: Double,
    val otherIncomeAccounts: List<AccountAmount>,
    val otherExpenseAccounts: List<AccountAmount>,
    val netIncome: Double
)

data class RetainedEarningsReport(
    val beginningBalance: Double,
    val netIncome: Double,
    val drawings: Double,
    val endingBalance: Double
)

data class FinancialPositionReport(
    val assetAccounts: List<AccountAmount>,
    val totalAssets: Double,
    val liabilityAccounts: List<AccountAmount>,
    val totalLiabilities: Double,
    val equityAccounts: List<AccountAmount>,
    val totalEquity: Double
)

data class CashFlowReport(
    val operatingActivities: List<AccountAmount>,
    val netOperating: Double,
    val investingActivities: List<AccountAmount>,
    val netInvesting: Double,
    val financingActivities: List<AccountAmount>,
    val netFinancing: Double,
    val netChangeInCash: Double,
    val endingCashBalance: Double
)

data class ClosingJournalLine(val accountName: String, val debit: Double, val credit: Double)
data class ClosingJournalReport(val entries: List<ClosingJournalLine>)

interface FinancialsApi {
    @GET("api/incomestatement")
    fun getIncomeStatement(@Query("periodId") periodId: Int): Call<IncomeStatementReport>

    @GET("api/retainedearnings")
    fun getRetainedEarnings(@Query("periodId") periodId: Int): Call<RetainedEarningsReport>

    @GET("api/financialposition")
    fun getFinancialPosition(@Query("periodId") periodId: Int): Call<FinancialPositionReport>

    @GET("api/cashflow")
    fun getCashFlow(@Query("periodId") periodId: Int): Call<CashFlowReport>

    @GET("api/closingjournal")
    fun getClosingJournal(@Query("periodId") periodId: Int): Call<ClosingJournalReport>
}
