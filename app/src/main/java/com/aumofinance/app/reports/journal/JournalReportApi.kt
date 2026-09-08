package com.aumofinance.app.reports.journal

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.statement.HttpResponse

data class JournalReportLine(
    val id: Int,
    val accountId: Int,
    val accountName: String,
    val referenceNumber: Int,
    val lineDescription: String?,
    val debit: Double,
    val credit: Double,
    val lineOrder: Int
)

data class JournalReportEntry(
    val id: Int,
    val transactionNumber: String,
    val journalType: String,
    val entryDate: String,
    val createdAt: String,
    val updatedAt: String?,
    val lines: List<JournalReportLine>
)

data class JournalReportResponse(
    val success: Boolean,
    val hasPeriodSelected: Boolean?,
    val selectedPeriodName: String?,
    val isPeriodClosed: Boolean,
    val entries: List<JournalReportEntry>
)

class JournalReportApi(private val client: HttpClient = ApiClient.client) {
    // General Journal: seluruh entri tipe "General" di periode yang sedang
    // dipilih. Beda dari "journal-entry" (form input satu entri di
    // journal.JournalApi).
    suspend fun getGeneralJournal(): HttpResponse =
        client.get("/api/v1/reports/general-journal")

    // Adjusting Journal: sama seperti di atas tapi backend sudah memfilter
    // journalType == "Adjusting" saja.
    suspend fun getAdjustingJournal(): HttpResponse =
        client.get("/api/v1/reports/adjusting-journal")
}
