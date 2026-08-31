package com.aumofinance.app.reports.journal

import retrofit2.Call
import retrofit2.http.GET

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
    val hasPeriodSelected: Boolean? ,
    val selectedPeriodName: String?,
    val isPeriodClosed: Boolean,
    val entries: List<JournalReportEntry>
)

interface JournalReportApi {
    // General Journal: seluruh entri (General+Adjusting) di periode yang
    // sedang dipilih. Route JAMAK ("journal-entries"), beda dari
    // "journal-entry" (form input satu entri di journal.JournalApi).
    @GET("api/mobile/journal-entries")
    fun getGeneralJournal(): Call<JournalReportResponse>

    // Adjusting Journal: sama seperti di atas tapi backend sudah memfilter
    // journalType == "Adjusting" saja.
    @GET("api/mobile/reports/adjusting-journal")
    fun getAdjustingJournal(): Call<JournalReportResponse>
}
