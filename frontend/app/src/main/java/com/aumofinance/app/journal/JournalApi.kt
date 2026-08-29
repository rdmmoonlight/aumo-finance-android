package com.aumofinance.app.journal

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.Query

data class JournalLine(
    val accountId: Int,
    val lineDescription: String?,
    val debit: Double,
    val credit: Double,
    val lineOrder: Int = 0
)

// JournalType di backend hanya "General" atau "Adjusting" (string bebas,
// bukan enum) — Closing TIDAK PERNAH ada di sini, itu murni dihitung on-the-fly
// oleh ClosingJournalApi dari Trial Balance, tidak pernah disimpan sebagai entry.
data class CreateJournalEntryRequest(
    val journalType: String,
    val entryDate: String,
    val createdAt: String, // waktu lokal perangkat saat input, wajib Kind Unspecified
    val lines: List<JournalLine>
)

data class UpdateJournalEntryRequest(
    val journalType: String,
    val entryDate: String,
    val updatedAt: String, // waktu lokal perangkat saat edit disimpan
    val lines: List<JournalLine>
)

data class JournalEntryDetail(
    val id: Int,
    val transactionNumber: String,
    val journalType: String,
    val entryDate: String,
    val createdAt: String,
    val updatedAt: String?,
    val isLocked: Boolean,
    val lines: List<JournalEntryDetailLine>
)

data class JournalEntryDetailLine(
    val id: Int,
    val accountId: Int,
    val lineDescription: String?,
    val debit: Double,
    val credit: Double,
    val lineOrder: Int
)

data class JournalEntryDetailResponse(val success: Boolean, val entry: JournalEntryDetail?)
data class CreateJournalEntryResponse(val success: Boolean, val message: String, val entryId: Int?, val transactionNumber: String?)
data class SimpleApiResponse(val success: Boolean, val message: String)
data class NextTransactionNumberResponse(val success: Boolean, val transactionNumber: String)

interface JournalApi {
    // Dipakai oleh halaman Journal Entry (form input/edit satu entri).
    @GET("api/mobile/journal-entry/{id}")
    fun getById(@Path("id") id: Int): Call<JournalEntryDetailResponse>

    @POST("api/mobile/journal-entry/create")
    fun create(@Body request: CreateJournalEntryRequest): Call<CreateJournalEntryResponse>

    @PUT("api/mobile/journal-entry/edit/{id}")
    fun update(@Path("id") id: Int, @Body request: UpdateJournalEntryRequest): Call<SimpleApiResponse>

    @DELETE("api/mobile/journal-entry/delete/{id}")
    fun delete(@Path("id") id: Int): Call<SimpleApiResponse>

    @GET("api/mobile/journal-entry/search-descriptions")
    fun searchDescriptions(@Query("q") query: String): Call<List<String>>

    @GET("api/mobile/journal-entry/next-transaction-number")
    fun nextTransactionNumber(@Query("journalType") journalType: String, @Query("entryDate") entryDate: String? = null): Call<NextTransactionNumberResponse>
}
