package com.aumofinance.app.dashboard

import retrofit2.Call
import retrofit2.http.GET

data class CashAccountEntry(val accountId: Int, val referenceNumber: Int, val accountName: String, val balance: Double)

data class DashboardSummary(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val isPeriodClosed: Boolean,
    val totalAssets: Double,
    val totalLiabilities: Double,
    val totalEquity: Double,
    val totalRevenue: Double,
    val totalExpenses: Double,
    val netIncome: Double,
    val cashAccounts: List<CashAccountEntry>,
    val totalCashOnHand: Double,
    val bankAccounts: List<CashAccountEntry>,
    val totalBankBalance: Double
)

interface DashboardApi {
    // Tidak menerima periodId — otomatis mengikuti periode yang sedang
    // di-select user (lihat SelectedPeriodHelper di backend).
    @GET("api/mobile/dashboard")
    fun getSummary(): Call<DashboardSummary>
}
