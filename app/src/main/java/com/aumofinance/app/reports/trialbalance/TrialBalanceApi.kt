package com.aumofinance.app.reports.trialbalance

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.request.parameter
import io.ktor.client.statement.HttpResponse

data class TrialBalanceRow(
    val accountId: Int,
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val role: String,
    val normalBalanceIsDebit: Boolean,
    val netBalance: Double,
    val debit: Double,
    val credit: Double,
)

data class TrialBalanceReport(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val reportTitle: String,
    val type: String,
    val totalDebit: Double,
    val totalCredit: Double,
    val isBalanced: Boolean,
    val rows: List<TrialBalanceRow>,
)

class TrialBalanceApi(private val client: HttpClient = ApiClient.client) {
    // type: "unadjusted" (hanya General), "adjusted" (General+Adjusting),
    // atau "post-closing" (Retained Earnings sudah termasuk efek Closing,
    // walau baris Closing itu sendiri tidak pernah tersimpan sebagai entri).
    suspend fun getTrialBalance(type: String): HttpResponse =
        client.get("/api/v1/reports/trial-balance") {
            parameter("type", type)
        }
}
