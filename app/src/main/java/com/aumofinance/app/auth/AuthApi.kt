package com.aumofinance.app.auth

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.client.statement.HttpResponse
import io.ktor.http.ContentType
import io.ktor.http.contentType

// rememberMe menentukan apakah cookie sesi dari backend bersifat persisten
// (lihat SignInManager.PasswordSignInAsync di AuthController) — DIPAKAI
// LANGSUNG oleh backend untuk keputusan itu, terpisah dari
// SessionManager.keepSignedIn di sisi Android (yang mengatur apakah cookie
// itu ikut disalin ke penyimpanan lokal terenkripsi kita sendiri untuk
// bertahan lewat restart app). userAgent/operatingSystem opsional, dipakai
// backend untuk mencatat riwayat sesi (fitur Guardian) — tidak dikirim di
// sini, backend jatuh balik ke header User-Agent HTTP kalau kosong.
data class LoginRequest(val email: String, val password: String, val rememberMe: Boolean = false)

// userId/fullName HANYA ada saat success == true — backend mengembalikan
// success:false + message saja untuk kredensial salah/akun terkunci dsb.
data class LoginResponse(
    val success: Boolean,
    val message: String,
    val userId: String? = null,
    val fullName: String? = null,
)

class AuthApi(private val client: HttpClient = ApiClient.client) {
    suspend fun login(request: LoginRequest): HttpResponse =
        client.post("/api/v1/auth/login") {
            contentType(ContentType.Application.Json)
            setBody(request)
        }

    suspend fun me(): HttpResponse = client.get("/api/v1/auth/me")

    suspend fun logout(): HttpResponse = client.post("/api/v1/auth/logout")
}
