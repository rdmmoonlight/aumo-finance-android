package com.aumofinance.app.reports.worksheet

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.statement.HttpResponse

data class WorksheetRow(
    val accountId: Int,
    val referenceNumber: Int,
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
    val bsCredit: Double,
)

data class WorksheetTotals(
    val tbDebit: Double,
    val tbCredit: Double,
    val adjDebit: Double,
    val adjCredit: Double,
    val adjTbDebit: Double,
    val adjTbCredit: Double,
    val isDebit: Double,
    val isCredit: Double,
    val bsDebit: Double,
    val bsCredit: Double,
    val netIncome: Double,
)

data class WorksheetReport(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val rows: List<WorksheetRow>,
    val totals: WorksheetTotals?,
)

class WorksheetApi(private val client: HttpClient = ApiClient.client) {
    suspend fun getWorksheet(): HttpResponse = client.get("/api/v1/reports/worksheet")
}
