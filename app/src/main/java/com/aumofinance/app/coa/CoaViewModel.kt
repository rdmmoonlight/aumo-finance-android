package com.aumofinance.app.coa

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.ktor.client.call.body
import io.ktor.client.statement.HttpResponse
import io.ktor.http.isSuccess
import kotlinx.coroutines.launch

class CoaViewModel : ViewModel() {
    private val api = CoaApi()

    private val _accounts = MutableLiveData<List<Account>>(emptyList())
    val accounts: LiveData<List<Account>> = _accounts

    private val _errorMessage = MutableLiveData<String?>()
    val errorMessage: LiveData<String?> = _errorMessage

    fun load(
        search: String? = null,
        category: String? = null,
    ) {
        viewModelScope.launch {
            _accounts.value =
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

    private suspend fun handleResult(response: HttpResponse) {
        try {
            val body = response.body<SimpleApiResponse>()
            if (response.status.isSuccess() && body.success) {
                load()
            } else {
                _errorMessage.value = body.message.ifBlank { "Gagal memproses permintaan (${response.status.value})" }
            }
        } catch (t: Throwable) {
            _errorMessage.value = t.message ?: "Koneksi gagal"
        }
    }
}
