package com.aumofinance.app.network

// Status sesi di memori. Otentikasi dipegang oleh token JWT (dikembalikan
// backend di response login, header "Authorization: Bearer <token>"
// disisipkan otomatis oleh AuthPlugin di ApiClient.kt untuk setiap request).
object SessionManager {
    var token: String? = null
    var userId: String? = null
    var fullName: String? = null

    fun isLoggedIn(): Boolean = !token.isNullOrBlank()

    fun clear() {
        token = null
        userId = null
        fullName = null
    }
}
