package com.aumofinance.app.reports.trialbalance

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

data class TrialBalanceRow(
    val accountId: Int,
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val role: String,
    val normalBalanceIsDebit: Boolean,
    val netBalance: Double,
    val debit: Double,
    val credit: Double
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
    val rows: List<TrialBalanceRow>
)

interface TrialBalanceApi {
    // type: "unadjusted" (hanya General), "adjusted" (General+Adjusting),
    // atau "post-closing" (Retained Earnings sudah termasuk efek Closing,
    // walau baris Closing itu sendiri tidak pernah tersimpan sebagai entri).
    @GET("api/mobile/reports/trial-balance")
    fun getTrialBalance(@Query("type") type: String): Call<TrialBalanceReport>
}
