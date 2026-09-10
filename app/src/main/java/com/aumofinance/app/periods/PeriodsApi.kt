package com.aumofinance.app.periods

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.client.statement.HttpResponse
import io.ktor.http.ContentType
import io.ktor.http.contentType

data class Period(
    val id: Int,
    val periodName: String,
    val startDate: String,
    val endDate: String,
    val isClosed: Boolean,
)

data class PeriodsResponse(
    val success: Boolean,
    val selectedPeriodId: Int?,
    val periods: List<Period>,
)

// Akun Cash/Bank (Role == "CashAndEquivalents") atau Retained Earnings
// (Role == "RetainedEarnings") yang tersedia untuk dipilih saat membuka
// periode baru dalam mode LoadExisting (lanjutan dari periode sebelumnya) —
// backend TIDAK mengirim saldo carry-forward di sini; saldo berjalan
// otomatis mengikuti saldo ledger akun tsb, tidak perlu dipilih manual.
data class AvailableAccount(
    val id: Int,
    val referenceNumber: Int,
    val accountName: String,
    val displayLabel: String,
)

data class PermanentAccountInfo(
    val id: Int,
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val displayLabel: String,
)

// Respons GET /api/v1/periods/open-info. hasExistingPermanentAccounts
// menentukan kondisi mana yang harus ditampilkan ke user:
// - false = belum ada akun Cash/Bank & Retained Earnings sama sekali ->
//   wajib daftar akun baru (mode CreateNew).
// - true  = sudah ada -> user memilih akun Cash, Bank, dan Retained Earnings
//   yang mana yang dilanjutkan (mode LoadExisting) dari
//   availableCashAndBankAccounts / availableRetainedEarningsAccounts.
data class OpenPeriodInfoResponse(
    val success: Boolean,
    val hasExistingPermanentAccounts: Boolean,
    val availableCashAndBankAccounts: List<AvailableAccount>,
    val availableRetainedEarningsAccounts: List<AvailableAccount>,
    val permanentAccounts: List<PermanentAccountInfo>,
)

data class CreatePeriodRequest(
    val month: Int,
    val year: Int,
    val setupMode: String,
    // --- Mode LoadExisting (melanjutkan akun permanen yang sudah ada) ---
    val cashAccountId: Int? = null,
    val bankAccountId: Int? = null,
    val retainedEarningsAccountId: Int? = null,
    // --- Mode CreateNew (belum ada periode sama sekali) ---
    val cashAccountCode: String? = null,
    val cashAccountName: String? = null,
    val cashBalance: Double? = null,
    val bankAccountCode: String? = null,
    val bankAccountName: String? = null,
    val bankBalance: Double? = null,
    val retainedEarningsAccountCode: String? = null,
    val retainedEarningsAccountName: String? = null,
) {
    companion object {
        const val MODE_LOAD_EXISTING = "LoadExisting"
        const val MODE_CREATE_NEW = "CreateNew"
    }
}

data class SimpleApiResponse(val success: Boolean, val message: String)

class PeriodsApi(private val client: HttpClient = ApiClient.client) {
    suspend fun list(): HttpResponse = client.get("/api/v1/periods")

    suspend fun openInfo(): HttpResponse = client.get("/api/v1/periods/open-info")

    suspend fun open(request: CreatePeriodRequest): HttpResponse =
        client.post("/api/v1/periods") {
            contentType(ContentType.Application.Json)
            setBody(request)
        }

    suspend fun select(id: Int): HttpResponse = client.post("/api/v1/periods/select/$id")

    suspend fun clearSelection(): HttpResponse = client.post("/api/v1/periods/clear-selection")

    suspend fun close(id: Int): HttpResponse = client.post("/api/v1/periods/close/$id")
}
