package com.aumofinance.app.reports.ledger

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

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

interface LedgerApi {
    // Satu endpoint, dibedakan lewat query isTemporary — BUKAN dua endpoint
    // terpisah seperti dugaan awal saya.
    @GET("api/mobile/reports/general-ledger")
    fun getLedger(@Query("isTemporary") isTemporary: Boolean): Call<LedgerResponse>
}
