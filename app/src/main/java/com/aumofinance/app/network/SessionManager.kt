package com.aumofinance.app.network

// Status sesi di memori untuk kebutuhan tampilan (userId, fullName) dan
// flag kontrol — BUKAN penyimpan kredensial. Otentikasi sebenarnya dipegang
// oleh cookie "AumoFinance.Session" yang dikelola Ktor lewat
// PersistentCookiesStorage (lihat ApiClient.kt), sejak diketahui backend
// pakai ASP.NET Identity cookie-based auth, bukan JWT Bearer seperti
// desain awal migrasi Ktor.
//
// keepSignedIn dibaca oleh PersistentCookiesStorage.addCookie() untuk
// memutuskan apakah cookie yang baru diterima dari server harus ikut
// ditulis ke penyimpanan terenkripsi (SessionStore) atau cukup di memori
// saja selama proses app masih hidup.
object SessionManager {
    var isLoggedIn: Boolean = false
    var userId: String? = null
    var fullName: String? = null
    var keepSignedIn: Boolean = false

    fun clear() {
        isLoggedIn = false
        userId = null
        fullName = null
        keepSignedIn = false
    }
}
