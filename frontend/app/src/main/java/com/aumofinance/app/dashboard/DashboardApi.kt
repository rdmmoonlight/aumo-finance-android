package com.aumofinance.app.dashboard

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

data class DashboardSummary(
    val totalAssets: Double,
    val totalLiabilities: Double,
    val totalEquity: Double,
    val netIncome: Double
)

interface DashboardApi {
    @GET("api/dashboard")
    fun getSummary(@Query("periodId") periodId: Int): Call<DashboardSummary>
}
