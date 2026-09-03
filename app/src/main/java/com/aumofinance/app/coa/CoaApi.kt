package com.aumofinance.app.coa

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.Query

// Type: salah satu dari "Assets", "Liabilities", "Equity", "OperatingIncome",
// "OperatingExpenses", "OtherIncome", "OtherExpenses" (lihat
// AccountClassification.cs di aumo-finance-web — nomor referensi harus masuk
// rentang yang sesuai: Assets 100-199, Liabilities 200-299, Equity 300-399,
// OperatingIncome 400-499, OperatingExpenses 500-599, OtherIncome 600-799,
// OtherExpenses 800-999).
// Role: peran khusus opsional, mis. "CashAndEquivalents" atau "RetainedEarnings"
// (dipakai backend untuk Dashboard, Cash Flow, Retained Earnings); default "Default".
data class Account(
    val id: Int,
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val role: String,
    val isActive: Boolean,
    val balance: Double
)

data class AccountsResponse(val success: Boolean, val selectedPeriodName: String?, val accounts: List<Account>)

data class AccountRequest(
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val role: String = "Default"
)

data class UpdateAccountRequest(
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val role: String = "Default",
    val isActive: Boolean
)

data class SimpleApiResponse(val success: Boolean, val message: String)

interface CoaApi {
    @GET("api/mobile/chart-of-accounts")
    fun list(@Query("search") search: String? = null, @Query("category") category: String? = null): Call<AccountsResponse>

    @POST("api/mobile/chart-of-accounts/create")
    fun create(@Body request: AccountRequest): Call<SimpleApiResponse>

    @PUT("api/mobile/chart-of-accounts/update/{id}")
    fun update(@Path("id") id: Int, @Body request: UpdateAccountRequest): Call<SimpleApiResponse>

    @DELETE("api/mobile/chart-of-accounts/delete/{id}")
    fun delete(@Path("id") id: Int): Call<SimpleApiResponse>
}
