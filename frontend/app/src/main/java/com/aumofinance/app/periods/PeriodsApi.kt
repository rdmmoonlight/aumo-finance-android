package com.aumofinance.app.periods

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path

data class Period(
    val id: Int,
    val periodName: String,
    val startDate: String,
    val endDate: String,
    val isClosed: Boolean
)

data class PeriodsResponse(
    val success: Boolean,
    val selectedPeriodId: Int?,
    val periods: List<Period>
)

// Akun ringkas untuk dropdown pemilihan Cash/Bank/Retained Earnings saat
// melanjutkan akun permanen dari periode sebelumnya.
data class AccountOption(
    val id: Int,
    val referenceNumber: Int,
    val accountName: String,
    val displayLabel: String
)

// Respons GET /api/mobile/periods/open-info. hasExistingPermanentAccounts
// menentukan kondisi mana yang harus ditampilkan ke user:
// - false = belum ada periode yang pernah ditutup -> wajib daftar akun baru.
// - true  = sudah ada periode yang pernah ditutup -> tinggal lanjutkan akun lama.
data class OpenPeriodInfoResponse(
    val success: Boolean,
    val hasExistingPermanentAccounts: Boolean,
    val availableCashAndBankAccounts: List<AccountOption>,
    val availableRetainedEarningsAccounts: List<AccountOption>
)

data class CreatePeriodRequest(
    val month: Int,
    val year: Int,
    val setupMode: String,
    // --- Mode LoadExisting (sudah ada periode yang pernah ditutup) ---
    val cashAccountId: Int? = null,
    val bankAccountId: Int? = null,
    val retainedEarningsAccountId: Int? = null,
    // --- Mode CreateNew (belum ada periode yang pernah ditutup) ---
    val cashAccountCode: String? = null,
    val cashAccountName: String? = null,
    val cashBalance: Double? = null,
    val bankAccountCode: String? = null,
    val bankAccountName: String? = null,
    val bankBalance: Double? = null,
    val retainedEarningsAccountCode: String? = null,
    val retainedEarningsAccountName: String? = null
) {
    companion object {
        const val MODE_LOAD_EXISTING = "LoadExisting"
        const val MODE_CREATE_NEW = "CreateNew"
    }
}

data class SimpleApiResponse(val success: Boolean, val message: String)

interface PeriodsApi {
    @GET("api/mobile/periods")
    fun list(): Call<PeriodsResponse>

    @GET("api/mobile/periods/open-info")
    fun openInfo(): Call<OpenPeriodInfoResponse>

    @POST("api/mobile/periods/create")
    fun open(@Body request: CreatePeriodRequest): Call<SimpleApiResponse>

    @POST("api/mobile/periods/select/{id}")
    fun select(@Path("id") id: Int): Call<SimpleApiResponse>

    @POST("api/mobile/periods/clear-selection")
    fun clearSelection(): Call<SimpleApiResponse>

    @POST("api/mobile/periods/close/{id}")
    fun close(@Path("id") id: Int): Call<SimpleApiResponse>
}
