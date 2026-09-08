package com.aumofinance.app.reports.ledger

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.request.parameter
import io.ktor.client.statement.HttpResponse

data class LedgerLine(
    val journalEntryId: Int,
    val entryDate: String,
    val description: String?,
    val debit: Double,
    val credit: Double,
    val runningBalance: Double
)

data class LedgerAccount(
    val accountId: Int,
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val normalBalanceIsDebit: Boolean,
    val endingBalance: Double,
    val lines: List<LedgerLine>
)

data class LedgerResponse(
    val success: Boolean,
    val hasPeriodSelected: Boolean,
    val selectedPeriodName: String?,
    val isTemporary: Boolean,
    val netIncomeBeforeClosing: Double,
    val ledgers: List<LedgerAccount>
)

class LedgerApi(private val client: HttpClient = ApiClient.client) {
    // Satu endpoint, dibedakan lewat query isTemporary — BUKAN dua endpoint
    // terpisah seperti dugaan awal saya.
    suspend fun getLedger(isTemporary: Boolean): HttpResponse =
        client.get("/api/v1/reports/general-ledger") {
            parameter("isTemporary", isTemporary)
        }
}
