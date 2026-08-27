package com.aumofinance.app.reports.ledger

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class LedgerViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(LedgerApi::class.java)

    private val _accounts = MutableLiveData<List<LedgerAccount>>(emptyList())
    val accounts: LiveData<List<LedgerAccount>> = _accounts

    fun load(periodId: Int, accountType: String) {
        api.getLedger(periodId, accountType).enqueue(object : Callback<List<LedgerAccount>> {
            override fun onResponse(call: Call<List<LedgerAccount>>, response: Response<List<LedgerAccount>>) {
                _accounts.value = response.body() ?: emptyList()
            }
            override fun onFailure(call: Call<List<LedgerAccount>>, t: Throwable) {
                _accounts.value = emptyList()
            }
        })
    }
}
