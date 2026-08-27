package com.aumofinance.app.periods

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path

data class Period(
    val id: Int,
    val name: String,
    val startDate: String,
    val endDate: String,
    val isClosed: Boolean,
    val isSelected: Boolean
)

data class OpenPeriodRequest(
    val name: String,
    val startDate: String,
    val endDate: String,
    val openingCashBalance: Double?
)

interface PeriodsApi {
    @GET("api/periods")
    fun list(): Call<List<Period>>

    @POST("api/periods")
    fun open(@Body request: OpenPeriodRequest): Call<Period>

    @PUT("api/periods/{id}/close")
    fun close(@Path("id") id: Int): Call<Unit>
}
