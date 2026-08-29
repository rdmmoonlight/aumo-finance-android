package com.aumofinance.app.coa

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import com.google.gson.Gson
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class CoaViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(CoaApi::class.java)

    private val _accounts = MutableLiveData<List<Account>>(emptyList())
    val accounts: LiveData<List<Account>> = _accounts

    private val _errorMessage = MutableLiveData<String?>()
    val errorMessage: LiveData<String?> = _errorMessage

    fun load(search: String? = null, category: String? = null) {
        api.list(search, category).enqueue(object : Callback<AccountsResponse> {
            override fun onResponse(call: Call<AccountsResponse>, response: Response<AccountsResponse>) {
                _accounts.value = response.body()?.accounts ?: emptyList()
            }
            override fun onFailure(call: Call<AccountsResponse>, t: Throwable) {
                _accounts.value = emptyList()
            }
        })
    }

    fun create(request: AccountRequest) {
        api.create(request).enqueue(resultHandler { load() })
    }

    fun update(id: Int, request: UpdateAccountRequest) {
        api.update(id, request).enqueue(resultHandler { load() })
    }

    // Backend MENOLAK delete kalau akun sudah punya baris jurnal (400, dengan
    // pesan yang menyuruh set Inactive lewat Update, bukan auto-nonaktifkan
    // sendiri) — pesan itu diteruskan apa adanya ke errorMessage.
    fun delete(id: Int) {
        api.delete(id).enqueue(resultHandler { load() })
    }

    private fun resultHandler(onSuccess: () -> Unit) = object : Callback<SimpleApiResponse> {
        override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) {
            if (response.isSuccessful && response.body()?.success == true) {
                onSuccess()
            } else {
                val message = parseErrorMessage(response) ?: response.body()?.message ?: "Gagal memproses permintaan (${response.code()})"
                _errorMessage.value = message
            }
        }
        override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) {
            _errorMessage.value = t.message ?: "Koneksi gagal"
        }
    }

    private fun parseErrorMessage(response: Response<SimpleApiResponse>): String? {
        return try {
            val errorJson = response.errorBody()?.string() ?: return null
            Gson().fromJson(errorJson, SimpleApiResponse::class.java)?.message
        } catch (e: Exception) {
            null
        }
    }
}
