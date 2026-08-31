package com.aumofinance.app.reports.financials

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

data class AccountAmount(val referenceNumber: Int, val accountName: String, val amount: Double)

data class IncomeStatementReport(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val asOfDate: String?,
    val revenueAccounts: List<AccountAmount>,
    val totalRevenue: Double,
    val expenseAccounts: List<AccountAmount>,
    val totalExpenses: Double,
    val operatingIncome: Double,
    val otherIncomeAccounts: List<AccountAmount>,
    val otherExpenseAccounts: List<AccountAmount>,
    val totalOtherIncome: Double,
    val totalOtherExpenses: Double,
    val netIncome: Double
)

data class RetainedEarningsReport(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val beginningRetainedEarnings: Double,
    val netIncome: Double,
    val dividendsOrDraws: Double,
    val endingRetainedEarnings: Double
)

// accountId di sini SELALU 0 (backend belum mengirim id akun sungguhan untuk
// baris Neraca, hanya referenceNumber+accountName+amount) — jangan dipakai
// sebagai kunci navigasi/edit.
data class FinancialPositionLine(val accountId: Int, val referenceNumber: Int, val accountName: String, val amount: Double)

data class FinancialPositionReport(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val assetAccounts: List<FinancialPositionLine>,
    val totalAssets: Double,
    val liabilityAccounts: List<FinancialPositionLine>,
    val totalLiabilities: Double,
    val equityAccounts: List<FinancialPositionLine>, // sudah termasuk baris "Retained Earnings" di akhir
    val totalEquity: Double,
    val totalLiabilitiesAndEquity: Double,
    val isBalanced: Boolean
)

data class CashFlowLine(val description: String, val amount: Double)

data class CashFlowReport(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val operatingActivities: List<CashFlowLine>,
    val netCashFromOperating: Double,
    val investingActivities: List<CashFlowLine>,
    val netCashFromInvesting: Double,
    val financingActivities: List<CashFlowLine>,
    val netCashFromFinancing: Double,
    val netChangeInCash: Double,
    val beginningCash: Double,
    val endingCash: Double
)

data class ClosingJournalLine(val referenceNumber: Int, val accountName: String, val debit: Double, val credit: Double)
data class ClosingJournalGroup(val description: String, val lines: List<ClosingJournalLine>, val totalDebit: Double, val totalCredit: Double)
data class ClosingJournalData(val netIncome: Double, val retainedEarningsAccountName: String, val groups: List<ClosingJournalGroup>)
data class ClosingJournalReport(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val closingJournal: ClosingJournalData?
)

interface FinancialsApi {
    @GET("api/mobile/reports/income-statement")
    fun getIncomeStatement(): Call<IncomeStatementReport>

    @GET("api/mobile/reports/retained-earnings")
    fun getRetainedEarnings(): Call<RetainedEarningsReport>

    // isPostClosing: laporan Neraca versi post-closing (akun Temporary sudah
    // ditutup) — dipisahkan sebagai query, bukan endpoint terpisah.
    @GET("api/mobile/reports/statement-of-financial-position")
    fun getFinancialPosition(@Query("isPostClosing") isPostClosing: Boolean = false): Call<FinancialPositionReport>

    @GET("api/mobile/reports/cash-flow")
    fun getCashFlow(): Call<CashFlowReport>

    // Read-only, murni dihitung dari Trial Balance — TIDAK ADA entri Closing
    // yang benar-benar tersimpan di database.
    @GET("api/mobile/reports/closing-journal")
    fun getClosingJournal(): Call<ClosingJournalReport>
}
