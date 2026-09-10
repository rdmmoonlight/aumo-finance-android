package com.aumofinance.app.coa

import com.aumofinance.app.network.ApiClient
import io.ktor.client.HttpClient
import io.ktor.client.request.delete
import io.ktor.client.request.get
import io.ktor.client.request.parameter
import io.ktor.client.request.post
import io.ktor.client.request.put
import io.ktor.client.request.setBody
import io.ktor.client.statement.HttpResponse
import io.ktor.http.ContentType
import io.ktor.http.contentType

// Type: salah satu dari "Assets", "Liabilities", "Equity", "OperatingIncome",
// "OperatingExpenses", "OtherIncome", "OtherExpenses" (lihat
// AccountClassification.cs di aumo-finance-web — nomor referensi harus masuk
// rentang yang sesuai: Assets 100-199, Liabilities 200-299, Equity 300-399,
// OperatingIncome 400-499, OperatingExpenses 500-599, OtherIncome 600-799,
// OtherExpenses 800-999).
// Role: peran khusus opsional, mis. "CashAndEquivalents" atau "RetainedEarnings"
// (dipakai backend untuk Dashboard, Cash Flow, Retained Earnings); default "Default".
data class Account(
    val id: Int,
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val role: String,
    val isActive: Boolean,
    val balance: Double,
)

data class AccountsResponse(val success: Boolean, val selectedPeriodName: String?, val accounts: List<Account>)

data class AccountRequest(
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val role: String = "Default",
)

data class UpdateAccountRequest(
    val referenceNumber: Int,
    val accountName: String,
    val type: String,
    val role: String = "Default",
    val isActive: Boolean,
)

data class SimpleApiResponse(val success: Boolean, val message: String)

class CoaApi(private val client: HttpClient = ApiClient.client) {
    suspend fun list(
        search: String? = null,
        category: String? = null,
    ): HttpResponse =
        client.get("/api/v1/chart-of-accounts") {
            if (search != null) parameter("search", search)
            if (category != null) parameter("category", category)
        }

    suspend fun create(request: AccountRequest): HttpResponse =
        client.post("/api/v1/chart-of-accounts") {
            contentType(ContentType.Application.Json)
            setBody(request)
        }

    suspend fun update(
        id: Int,
        request: UpdateAccountRequest,
    ): HttpResponse =
        client.put("/api/v1/chart-of-accounts/$id") {
            contentType(ContentType.Application.Json)
            setBody(request)
        }

    suspend fun delete(id: Int): HttpResponse = client.delete("/api/v1/chart-of-accounts/$id")
}
