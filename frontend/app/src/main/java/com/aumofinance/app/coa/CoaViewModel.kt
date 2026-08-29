package com.aumofinance.app.coa

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class CoaViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(CoaApi::class.java)

    private val _accounts = MutableLiveData<List<Account>>(emptyList())
    val accounts: LiveData<List<Account>> = _accounts

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
        api.create(request).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) = load()
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) = Unit
        })
    }

    fun update(id: Int, request: UpdateAccountRequest) {
        api.update(id, request).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) = load()
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) = Unit
        })
    }

    fun delete(id: Int) {
        api.delete(id).enqueue(object : Callback<SimpleApiResponse> {
            override fun onResponse(call: Call<SimpleApiResponse>, response: Response<SimpleApiResponse>) = load()
            override fun onFailure(call: Call<SimpleApiResponse>, t: Throwable) = Unit
        })
    }
}
