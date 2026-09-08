package com.aumofinance.app.auth

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.client.statement.HttpResponse
import io.ktor.http.ContentType
import io.ktor.http.contentType

data class LoginRequest(val email: String, val password: String)
data class LoginResponse(
    val success: Boolean,
    val message: String,
    val token: String,
    val userId: String,
    val fullName: String
)

class AuthApi(private val client: HttpClient = ApiClient.client) {
    suspend fun login(request: LoginRequest): HttpResponse =
        client.post("/auth/login") {
            contentType(ContentType.Application.Json)
            setBody(request)
        }

    suspend fun me(): HttpResponse = client.get("/auth/me")

    suspend fun logout(): HttpResponse = client.post("/auth/logout")
}
