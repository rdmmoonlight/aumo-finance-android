package com.aumofinance.app.reports.journal

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

data class JournalReportLine(
    val accountName: String,
    val debit: Double,
    val credit: Double
)

data class JournalReportEntry(
    val transactionNo: String,
    val entryDate: String,
    val createdAt: String,
    val lines: List<JournalReportLine>
)

interface JournalReportApi {
    // type: "General" atau "Adjusting" — Closing tidak pernah tampil di sini (system-generated).
    @GET("api/generaljournal/report")
    fun getReport(@Query("periodId") periodId: Int, @Query("type") type: String): Call<List<JournalReportEntry>>
}
