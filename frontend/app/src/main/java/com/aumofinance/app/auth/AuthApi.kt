package com.aumofinance.app.auth

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST

data class LoginRequest(val email: String, val password: String)
data class LoginResponse(
    val success: Boolean,
    val message: String,
    val token: String,
    val userId: String,
    val fullName: String
)

interface AuthApi {
    @POST("api/mobile/auth/login")
    fun login(@Body request: LoginRequest): Call<LoginResponse>

    @GET("api/mobile/auth/me")
    fun me(): Call<Map<String, Any?>>

    @POST("api/mobile/auth/logout")
    fun logout(): Call<Map<String, Any?>>
}
