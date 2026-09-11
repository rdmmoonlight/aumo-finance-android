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

// Sempat diganti cookie ASP.NET Identity (lihat riwayat PersistentCookiesStorage
// yang sudah dihapus), lalu diminta dikembalikan ke JWT Bearer. Backend
// sekarang menerbitkan token JWT di response login (field "token") DAN tetap
// menerima cookie untuk web/nextjs — dua mekanisme berjalan berdampingan di
// [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")], jadi
// Android cukup pakai Bearer saja tanpa perlu apa pun dari sisi cookie.
private val AuthPlugin =
    createClientPlugin("AuthPlugin") {
        onRequest { request, _ ->
            val token = SessionManager.token
            if (!token.isNullOrBlank()) {
                request.headers.append(HttpHeaders.Authorization, "Bearer $token")
            }
        }
    }

object ApiClient {
    // HANYA scheme+host, TANPA path — endpoint di setiap ApiX.kt WAJIB
    // ditulis absolut mulai dari "/" (mis. "/api/v1/auth/login"). Ktor
    // memperlakukan path yang diawali "/" sebagai path ABSOLUT yang
    // MENGGANTIKAN seluruh path bawaan di sini (bukan digabung/append) —
    // jadi kalau BASE_URL ikut menyertakan "/api/v1", bagian itu justru
    // dibuang begitu request memberi path absolut, dan "/api/v1" harus
    // ditulis ulang di endpoint itu sendiri. Simpan "/api/v1" di endpoint,
    // bukan di sini, supaya konsisten dan tidak diam-diam hilang.
    private const val BASE_URL = "https://aumonext-api.onrender.com"

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
