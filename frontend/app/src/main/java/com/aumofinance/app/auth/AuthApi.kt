package com.aumofinance.app.auth

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.POST

data class LoginRequest(val username: String, val password: String)
data class LoginResponse(val token: String, val expiresAt: String)

interface AuthApi {
    @POST("api/auth/login")
    fun login(@Body request: LoginRequest): Call<LoginResponse>
}
