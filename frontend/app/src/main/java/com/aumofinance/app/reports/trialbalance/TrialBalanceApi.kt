package com.aumofinance.app.reports.trialbalance

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

data class TrialBalanceRow(
    val accountId: Int,
    val accountName: String,
    val debit: Double,
    val credit: Double
)

data class TrialBalanceReport(
    val rows: List<TrialBalanceRow>,
    val totalDebit: Double,
    val totalCredit: Double
)

interface TrialBalanceApi {
    // adjusted=false -> hanya jurnal General.
    // adjusted=true  -> jurnal General + Adjusting.
    // Closing TIDAK PERNAH dihitung di kedua varian ini.
    @GET("api/trialbalance")
    fun getTrialBalance(@Query("periodId") periodId: Int, @Query("adjusted") adjusted: Boolean): Call<TrialBalanceReport>
}
