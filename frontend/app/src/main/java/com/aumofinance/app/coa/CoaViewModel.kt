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

    fun load() {
        api.list().enqueue(object : Callback<List<Account>> {
            override fun onResponse(call: Call<List<Account>>, response: Response<List<Account>>) {
                _accounts.value = response.body() ?: emptyList()
            }
            override fun onFailure(call: Call<List<Account>>, t: Throwable) {
                _accounts.value = emptyList()
            }
        })
    }

    fun save(id: Int?, request: AccountRequest) {
        val call = if (id == null) api.create(request) else api.update(id, request)
        call.enqueue(object : Callback<Account> {
            override fun onResponse(call: Call<Account>, response: Response<Account>) = load()
            override fun onFailure(call: Call<Account>, t: Throwable) = Unit
        })
    }

    fun delete(id: Int) {
        api.delete(id).enqueue(object : Callback<Unit> {
            override fun onResponse(call: Call<Unit>, response: Response<Unit>) = load()
            override fun onFailure(call: Call<Unit>, t: Throwable) = Unit
        })
    }
}
