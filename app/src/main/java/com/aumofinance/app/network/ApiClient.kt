package com.aumofinance.app.network

import io.ktor.client.HttpClient
import io.ktor.client.engine.okhttp.OkHttp
import io.ktor.client.plugins.api.createClientPlugin
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.defaultRequest
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logging
import io.ktor.client.request.url
import io.ktor.http.HttpHeaders
import io.ktor.serialization.gson.gson

// Menggantikan AuthInterceptor versi Retrofit/OkHttp — dipasang sebagai Ktor
// client plugin custom, membaca SessionManager.token di setiap request
// (bukan fixed di saat client dibuat), supaya tetap berlaku setelah
// login/logout tanpa perlu rebuild client.
private val AuthPlugin = createClientPlugin("AuthPlugin") {
    onRequest { request, _ ->
        val token = SessionManager.token
        if (!token.isNullOrBlank()) {
            request.headers.append(HttpHeaders.Authorization, "Bearer $token")
        }
    }
}

object ApiClient {
    private const val BASE_URL = "https://aumonext-api.onrender.com/api/v1"

    val client: HttpClient by lazy {
        HttpClient(OkHttp) {
            // Retrofit dulu selalu mengembalikan Response<T> sukses/gagal
            // apa adanya ke caller (isSuccessful dicek manual) — expectSuccess
            // = false menjaga perilaku yang sama di Ktor (tidak throw untuk
            // status 4xx/5xx, body error tetap bisa dibaca oleh caller).
            expectSuccess = false
            install(AuthPlugin)
            install(ContentNegotiation) {
                gson()
            }
            install(Logging) {
                level = LogLevel.INFO
            }
            defaultRequest {
                url(BASE_URL)
            }
        }
    }
}
