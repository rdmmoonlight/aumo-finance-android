package com.aumofinance.app.dashboard

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.statement.HttpResponse

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

class DashboardApi(private val client: HttpClient = ApiClient.client) {
    // Tidak menerima periodId — otomatis mengikuti periode yang sedang
    // di-select user (lihat SelectedPeriodHelper di backend).
    suspend fun getSummary(): HttpResponse = client.get("/api/v1/dashboard")
}
