package com.aumofinance.app.network

import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object ApiClient {
    // Deployment aumo-finance-web (backend produksi yang sudah ada, dipindah
    // dari Railway ke Render) — BUKAN backend baru. App Kotlin ini murni
    // konsumen dari api/v1/* yang sudah lengkap di backend tersebut.
    // Prefix "api/v1" sengaja dimasukkan ke BASE_URL (bukan diulang di tiap
    // anotasi endpoint) supaya seragam dengan konsep base URL di sisi web.
    // Retrofit tetap mewajibkan trailing slash di sini — tanpa mengubah URL
    // yang benar-benar dipanggil (identik dengan tanpa slash).
    private const val BASE_URL = "https://aumonext-api.onrender.com/api/v1/"

    val retrofit: Retrofit by lazy {
        val logging = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BASIC
        }
        val client = OkHttpClient.Builder()
            .addInterceptor(AuthInterceptor())
            .addInterceptor(logging)
            .build()

        Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }
}
