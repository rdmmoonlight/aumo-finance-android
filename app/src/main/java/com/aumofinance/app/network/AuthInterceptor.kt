package com.aumofinance.app.network

import okhttp3.Interceptor
import okhttp3.Response

// aumo-finance-web mengharuskan header "Authorization: Bearer <token>" di
// setiap endpoint api/mobile/* (kecuali login). Dipasang otomatis di sini
// supaya setiap ApiService tidak perlu urus token satu-satu.
class AuthInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val original = chain.request()
        val token = SessionManager.token
        val request = if (!token.isNullOrBlank()) {
            original.newBuilder()
                .addHeader("Authorization", "Bearer $token")
                .build()
        } else {
            original
        }
        return chain.proceed(request)
    }
}
