package com.aumofinance.app.network

import io.ktor.client.HttpClient
import io.ktor.client.engine.okhttp.OkHttp
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.cookies.HttpCookies
import io.ktor.client.plugins.defaultRequest
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logging
import io.ktor.client.request.url
import io.ktor.serialization.gson.gson

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

    // Diekspos (bukan private) supaya Splash/LoginActivity bisa memanggil
    // restoreFromDisk() sebelum request pertama setelah app di-restart, dan
    // LogoutActivity bisa memanggil clearAll() saat logout. Lihat
    // PersistentCookiesStorage untuk kenapa otentikasi sekarang berbasis
    // cookie ("AumoFinance.Session"), bukan header Authorization Bearer.
    val cookiesStorage = PersistentCookiesStorage()

    val client: HttpClient by lazy {
        HttpClient(OkHttp) {
            // Retrofit dulu selalu mengembalikan Response<T> sukses/gagal
            // apa adanya ke caller (isSuccessful dicek manual) — expectSuccess
            // = false menjaga perilaku yang sama di Ktor (tidak throw untuk
            // status 4xx/5xx, body error tetap bisa dibaca oleh caller).
            expectSuccess = false
            install(HttpCookies) {
                storage = cookiesStorage
            }
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
