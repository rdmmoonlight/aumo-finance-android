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

// Akun permanen (Assets/Liabilities/Equity) beserta saldo carry-forward-nya
// dari periode sebelumnya — ditampilkan apa adanya (read-only), tidak perlu
// dipilih manual lagi. balance sudah dalam representasi sisi normal akun
// itu (positif = sisi normal, mis. Debit utk Assets, Credit utk Equity).
data class CarryForwardAccount(
    val id: Int,
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val balance: Double
)

// Respons GET /api/mobile/periods/open-info. hasExistingPermanentAccounts
// menentukan kondisi mana yang harus ditampilkan ke user:
// - false = belum ada periode sama sekali -> wajib daftar akun baru.
// - true  = sudah ada periode sebelumnya -> tampilkan carryForwardAccounts,
//           saldo & jurnal Opening Balance otomatis dari server.
data class OpenPeriodInfoResponse(
    val success: Boolean,
    val hasExistingPermanentAccounts: Boolean,
    val carryForwardAccounts: List<CarryForwardAccount>
)

data class CreatePeriodRequest(
    val month: Int,
    val year: Int,
    val setupMode: String,
    // --- Mode CreateNew (belum ada periode sama sekali) ---
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
