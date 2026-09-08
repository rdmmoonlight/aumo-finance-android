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
    val isClosed: Boolean
)

data class PeriodsResponse(
    val success: Boolean,
    val selectedPeriodId: Int?,
    val periods: List<Period>
)

// Akun permanen (Assets/Liabilities/Equity) beserta saldo carry-forward-nya
// dari periode sebelumnya — ditampilkan apa adanya (read-only), tidak perlu
// dipilih manual lagi. balance sudah dalam representasi sisi normal akun
// itu (positif = sisi normal, mis. Debit utk Assets, Credit utk Equity).
data class CarryForwardAccount(
    val id: Int,
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val balance: Double
)

// Respons GET /api/v1/periods/open-info. hasExistingPermanentAccounts
// menentukan kondisi mana yang harus ditampilkan ke user:
// - false = belum ada periode sama sekali -> wajib daftar akun baru.
// - true  = sudah ada periode sebelumnya -> tampilkan carryForwardAccounts,
//           saldo & jurnal Opening Balance otomatis dari server.
data class OpenPeriodInfoResponse(
    val success: Boolean,
    val hasExistingPermanentAccounts: Boolean,
    val carryForwardAccounts: List<CarryForwardAccount>
)

data class CreatePeriodRequest(
    val month: Int,
    val year: Int,
    val setupMode: String,
    // --- Mode CreateNew (belum ada periode sama sekali) ---
    val cashAccountCode: String? = null,
    val cashAccountName: String? = null,
    val cashBalance: Double? = null,
    val bankAccountCode: String? = null,
    val bankAccountName: String? = null,
    val bankBalance: Double? = null,
    val retainedEarningsAccountCode: String? = null,
    val retainedEarningsAccountName: String? = null
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

    suspend fun select(id: Int): HttpResponse =
        client.post("/api/v1/periods/select/$id")

    suspend fun clearSelection(): HttpResponse =
        client.post("/api/v1/periods/clear-selection")

    suspend fun close(id: Int): HttpResponse =
        client.post("/api/v1/periods/close/$id")
}
