package com.aumofinance.app.reports.journal

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.delete
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
    val lineOrder: Int,
)

data class JournalReportEntry(
    val id: Int,
    val transactionNumber: String,
    val journalType: String,
    val entryDate: String,
    val createdAt: String,
    val updatedAt: String?,
    val lines: List<JournalReportLine>,
)

data class JournalReportResponse(
    val success: Boolean,
    val hasPeriodSelected: Boolean?,
    val selectedPeriodName: String?,
    val isPeriodClosed: Boolean,
    val entries: List<JournalReportEntry>,
)

class JournalReportApi(private val client: HttpClient = ApiClient.client) {
    // Backend mengelompokkan General/Adjusting/Closing di bawah satu route
    // "/api/v1/reports/journals" (bukan endpoint terpisah "general-journal"/
    // "adjusting-journal" seperti dugaan awal saya) — General Journal berisi
    // seluruh entri tipe "General" di periode yang sedang dipilih. Beda dari
    // "journal-entry" (form input satu entri di journal.JournalApi).
    suspend fun getGeneralJournal(): HttpResponse = client.get("/api/v1/reports/journals/general")

    // Adjusting Journal: sama seperti di atas tapi backend sudah memfilter
    // journalType == "Adjusting" saja.
    suspend fun getAdjustingJournal(): HttpResponse = client.get("/api/v1/reports/journals/adjusting")

    // Backend juga menyediakan DELETE khusus untuk entri Adjusting di bawah
    // route yang sama ("/adjusting/{id}", memfilter JournalType=="Adjusting"
    // sebagai proteksi tambahan) — dipakai JournalReportViewModel untuk
    // delete di halaman Adjusting Journal, sedangkan General Journal tetap
    // pakai journal-entry/delete/{id} yang generik (journal.JournalApi).
    suspend fun deleteAdjustingEntry(id: Int): HttpResponse = client.delete("/api/v1/reports/journals/adjusting/$id")
}
