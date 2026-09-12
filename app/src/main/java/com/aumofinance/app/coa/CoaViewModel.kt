package com.aumofinance.app.coa

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import io.ktor.client.statement.HttpResponse
import io.ktor.http.isSuccess
import kotlinx.coroutines.launch

// State Compose (bukan LiveData) — mengikuti pola JournalEntryViewModel/PeriodsViewModel
// sejak halaman ini dipindah dari Activity/View ke Jetpack Compose.
class CoaViewModel : ViewModel() {
    private val api = CoaApi()

    var accounts: List<Account> by mutableStateOf(emptyList())
        private set

    var errorMessage: String? by mutableStateOf(null)
        private set

    fun load(
        search: String? = null,
        category: String? = null,
    ) {
        viewModelScope.launch {
            accounts =
                try {
                    api.list(search, category).body<AccountsResponse>().accounts
                } catch (t: Throwable) {
                    emptyList()
                }
        }
    }

    fun create(request: AccountRequest) {
        viewModelScope.launch { handleResult(api.create(request)) }
    }

    fun update(
        id: Int,
        request: UpdateAccountRequest,
    ) {
        viewModelScope.launch { handleResult(api.update(id, request)) }
    }

    // Backend MENOLAK delete kalau akun sudah punya baris jurnal (400, dengan
    // pesan yang menyuruh set Inactive lewat Update, bukan auto-nonaktifkan
    // sendiri) — pesan itu diteruskan apa adanya ke errorMessage.
    fun delete(id: Int) {
        viewModelScope.launch { handleResult(api.delete(id)) }
    }

    fun clearError() {
        errorMessage = null
    }

    private suspend fun handleResult(response: HttpResponse) {
        try {
            val body = response.body<SimpleApiResponse>()
            if (response.status.isSuccess() && body.success) {
                load()
            } else {
                errorMessage = body.message.ifBlank { "Gagal memproses permintaan (${response.status.value})" }
            }
        } catch (t: Throwable) {
            errorMessage = t.message ?: "Koneksi gagal"
        }
    }
}
