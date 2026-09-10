package com.aumofinance.app.network

import io.ktor.client.plugins.cookies.CookiesStorage
import io.ktor.http.Cookie
import io.ktor.http.Url
import io.ktor.util.date.GMTDate
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock

// CookiesStorage kustom pengganti AcceptAllCookiesStorage bawaan Ktor.
// Backend memakai cookie ASP.NET Identity "AumoFinance.Session" untuk
// otentikasi (lihat Program.cs ConfigureApplicationCookie & semua
// [Authorize(AuthenticationSchemes = "Identity.Application")] di
// Controllers) — BUKAN JWT Bearer seperti dugaan awal saya saat migrasi
// Retrofit->Ktor. JWT Bearer memang ada dikonfigurasi di backend tapi
// tidak dipakai satupun endpoint yang dikonsumsi app ini.
//
// Cookie perlu bertahan lewat restart app untuk fitur "Ingat saya"/
// biometrik (sebelumnya ini peran SessionManager.token yang sudah
// dihapus). Setiap kali server mengirim Set-Cookie baru untuk cookie ini,
// nilainya ditulis ke SessionStore (EncryptedSharedPreferences) SELAMA
// SessionManager.keepSignedIn == true; kalau tidak, cookie hanya hidup di
// memori (hilang saat proses app mati) — setara sesi browser biasa.
class PersistentCookiesStorage : CookiesStorage {
    companion object {
        const val AUTH_COOKIE_NAME = "AumoFinance.Session"
    }

    private val mutex = Mutex()
    private val container = mutableListOf<Cookie>()

    override suspend fun get(requestUrl: Url): List<Cookie> = mutex.withLock {
        val now = GMTDate()
        container.filter { it.expires == null || it.expires!! > now }
    }

    override suspend fun addCookie(requestUrl: Url, cookie: Cookie) {
        mutex.withLock {
            container.removeAll { it.name == cookie.name }
            if (cookie.value.isNotEmpty()) {
                container.add(cookie)
            }
        }
        if (cookie.name == AUTH_COOKIE_NAME) {
            if (cookie.value.isEmpty()) {
                // Server mengirim cookie kosong (mis. saat logout) -> ikut
                // hapus salinan yang tersimpan supaya tidak dipakai lagi.
                SessionStore.clearCookie()
            } else if (SessionManager.keepSignedIn) {
                SessionStore.saveCookie(cookie.value)
            }
        }
    }

    // Dipanggil sekali di Splash/biometric-unlock SEBELUM request pertama,
    // supaya cookie yang tersimpan ikut dipakai (dimasukkan langsung ke jar
    // lokal, bukan menunggu header Set-Cookie dari server).
    suspend fun restoreFromDisk() {
        val value = SessionStore.loadCookie() ?: return
        mutex.withLock {
            container.removeAll { it.name == AUTH_COOKIE_NAME }
            container.add(Cookie(name = AUTH_COOKIE_NAME, value = value))
        }
    }

    suspend fun clearAll() {
        mutex.withLock { container.clear() }
    }

    override fun close() {}
}
