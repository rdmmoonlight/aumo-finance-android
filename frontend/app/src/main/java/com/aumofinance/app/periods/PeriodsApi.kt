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

data class CreatePeriodRequest(
    val periodName: String,
    val startDate: String,
    val endDate: String
)

data class SimpleApiResponse(val success: Boolean, val message: String)

interface PeriodsApi {
    @GET("api/mobile/periods")
    fun list(): Call<PeriodsResponse>

    @POST("api/mobile/periods/create")
    fun open(@Body request: CreatePeriodRequest): Call<SimpleApiResponse>

    @POST("api/mobile/periods/select/{id}")
    fun select(@Path("id") id: Int): Call<SimpleApiResponse>

    @POST("api/mobile/periods/clear-selection")
    fun clearSelection(): Call<SimpleApiResponse>

    @POST("api/mobile/periods/close/{id}")
    fun close(@Path("id") id: Int): Call<SimpleApiResponse>
}
