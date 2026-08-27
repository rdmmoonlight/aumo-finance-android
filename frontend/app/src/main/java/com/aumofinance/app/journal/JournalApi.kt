package com.aumofinance.app.journal

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path

data class JournalLine(
    val accountId: Int,
    val accountName: String,
    val debit: Double,
    val credit: Double
)

data class JournalEntry(
    val id: Int,
    val transactionNo: String,
    val entryDate: String,   // tanggal manual dari date picker
    val createdAt: String,   // waktu lokal perangkat saat input
    val type: String,        // General / Adjusting
    val lines: List<JournalLine>,
    val isBalanced: Boolean
)

data class JournalEntryRequest(
    val entryDate: String,
    val createdAt: String,
    val type: String,
    val lines: List<JournalLine>
)

interface JournalApi {
    @GET("api/generaljournal")
    fun list(): Call<List<JournalEntry>>

    @POST("api/generaljournal")
    fun create(@Body request: JournalEntryRequest): Call<JournalEntry>

    @PUT("api/generaljournal/{id}")
    fun update(@Path("id") id: Int, @Body request: JournalEntryRequest): Call<JournalEntry>

    @DELETE("api/generaljournal/{id}")
    fun delete(@Path("id") id: Int): Call<Unit>
}
