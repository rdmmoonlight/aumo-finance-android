package com.aumofinance.app.coa

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path

data class Account(
    val id: Int,
    val code: String,
    val name: String,
    val type: String, // Permanent / Temporary
    val category: String, // Asset / Liability / Equity / Revenue / Expense
    val isActive: Boolean,
    val balance: Double
)

data class AccountRequest(
    val code: String,
    val name: String,
    val type: String,
    val category: String
)

interface CoaApi {
    @GET("api/chartofaccounts")
    fun list(): Call<List<Account>>

    @POST("api/chartofaccounts")
    fun create(@Body request: AccountRequest): Call<Account>

    @PUT("api/chartofaccounts/{id}")
    fun update(@Path("id") id: Int, @Body request: AccountRequest): Call<Account>

    @DELETE("api/chartofaccounts/{id}")
    fun delete(@Path("id") id: Int): Call<Unit>
}
