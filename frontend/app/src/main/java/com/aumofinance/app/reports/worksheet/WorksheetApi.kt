package com.aumofinance.app.reports.worksheet

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

data class WorksheetRow(
    val accountName: String,
    val tbDebit: Double,
    val tbCredit: Double,
    val adjDebit: Double,
    val adjCredit: Double,
    val adjTbDebit: Double,
    val adjTbCredit: Double,
    val isDebit: Double,
    val isCredit: Double,
    val bsDebit: Double,
    val bsCredit: Double
)

data class WorksheetTotals(
    val tbDebit: Double, val tbCredit: Double,
    val adjDebit: Double, val adjCredit: Double,
    val adjTbDebit: Double, val adjTbCredit: Double,
    val isDebit: Double, val isCredit: Double,
    val bsDebit: Double, val bsCredit: Double,
    val netIncome: Double
)

data class WorksheetReport(
    val rows: List<WorksheetRow>,
    val totals: WorksheetTotals
)

interface WorksheetApi {
    @GET("api/worksheet")
    fun getWorksheet(@Query("periodId") periodId: Int): Call<WorksheetReport>
}
