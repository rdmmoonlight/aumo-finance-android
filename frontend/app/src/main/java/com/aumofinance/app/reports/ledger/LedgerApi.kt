package com.aumofinance.app.reports.ledger

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

data class LedgerLine(
    val date: String,
    val description: String,
    val debit: Double,
    val credit: Double,
    val balance: Double
)

data class LedgerAccount(
    val accountId: Int,
    val accountName: String,
    val lines: List<LedgerLine>,
    val endingBalance: Double
)

interface LedgerApi {
    // accountType: "Permanent" atau "Temporary".
    // Backend memfilter transaksi ketat pada rentang periode yang dipilih saja
    // (tidak ada carry-over lintas periode di sini, itu berlaku hanya untuk saldo Neraca).
    @GET("api/generalledger")
    fun getLedger(@Query("periodId") periodId: Int, @Query("accountType") accountType: String): Call<List<LedgerAccount>>
}
