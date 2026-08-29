package com.aumofinance.app.network

import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object ApiClient {
    // Deployment aumo-finance-web (backend produksi yang sudah ada, dipindah
    // dari Railway ke Render) — BUKAN backend baru. App Kotlin ini murni
    // konsumen dari api/mobile/* yang sudah lengkap di backend tersebut.
    private const val BASE_URL = "https://aumo.onrender.com/"

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
