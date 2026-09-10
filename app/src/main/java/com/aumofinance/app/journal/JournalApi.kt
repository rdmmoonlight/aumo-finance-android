package com.aumofinance.app.journal

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.delete
import io.ktor.client.request.get
import io.ktor.client.request.parameter
import io.ktor.client.request.post
import io.ktor.client.request.put
import io.ktor.client.request.setBody
import io.ktor.client.statement.HttpResponse
import io.ktor.http.ContentType
import io.ktor.http.contentType

data class JournalLine(
    val accountId: Int,
    val lineDescription: String?,
    val debit: Double,
    val credit: Double,
    val lineOrder: Int = 0,
)

// JournalType di backend hanya "General" atau "Adjusting" (string bebas,
// bukan enum) — Closing TIDAK PERNAH ada di sini, itu murni dihitung on-the-fly
// oleh ClosingJournalApi dari Trial Balance, tidak pernah disimpan sebagai entry.
data class CreateJournalEntryRequest(
    val journalType: String,
    val entryDate: String,
    val createdAt: String, // waktu lokal perangkat saat input, wajib Kind Unspecified
    val lines: List<JournalLine>,
)

data class UpdateJournalEntryRequest(
    val journalType: String,
    val entryDate: String,
    val updatedAt: String, // waktu lokal perangkat saat edit disimpan
    val lines: List<JournalLine>,
)

data class JournalEntryDetail(
    val id: Int,
    val transactionNumber: String,
    val journalType: String,
    val entryDate: String,
    val createdAt: String,
    val updatedAt: String?,
    val isLocked: Boolean,
    val lines: List<JournalEntryDetailLine>,
)

data class JournalEntryDetailLine(
    val id: Int,
    val accountId: Int,
    val lineDescription: String?,
    val debit: Double,
    val credit: Double,
    val lineOrder: Int,
)

data class JournalEntryDetailResponse(val success: Boolean, val entry: JournalEntryDetail?)

data class CreateJournalEntryResponse(val success: Boolean, val message: String, val entryId: Int?, val transactionNumber: String?)

data class SimpleApiResponse(val success: Boolean, val message: String)

data class NextTransactionNumberResponse(val success: Boolean, val transactionNumber: String)

class JournalApi(private val client: HttpClient = ApiClient.client) {
    // Dipakai oleh halaman Journal Entry (form input/edit satu entri).
    suspend fun getById(id: Int): HttpResponse = client.get("/api/v1/journal-entry/$id")

    suspend fun create(request: CreateJournalEntryRequest): HttpResponse =
        client.post("/api/v1/journal-entry/create") {
            contentType(ContentType.Application.Json)
            setBody(request)
        }

    suspend fun update(
        id: Int,
        request: UpdateJournalEntryRequest,
    ): HttpResponse =
        client.put("/api/v1/journal-entry/edit/$id") {
            contentType(ContentType.Application.Json)
            setBody(request)
        }

    suspend fun delete(id: Int): HttpResponse = client.delete("/api/v1/journal-entry/delete/$id")

    suspend fun searchDescriptions(query: String): HttpResponse =
        client.get("/api/v1/journal-entry/search-descriptions") {
            parameter("q", query)
        }

    suspend fun nextTransactionNumber(
        journalType: String,
        entryDate: String? = null,
    ): HttpResponse =
        client.get("/api/v1/journal-entry/next-transaction-number") {
            parameter("journalType", journalType)
            if (entryDate != null) parameter("entryDate", entryDate)
        }
}
