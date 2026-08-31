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

    private val _report = MutableLiveData<LedgerResponse?>()
    val report: LiveData<LedgerResponse?> = _report

    fun load(isTemporary: Boolean) {
        api.getLedger(isTemporary).enqueue(object : Callback<LedgerResponse> {
            override fun onResponse(call: Call<LedgerResponse>, response: Response<LedgerResponse>) {
                _report.value = response.body()
            }
            override fun onFailure(call: Call<LedgerResponse>, t: Throwable) {
                _report.value = null
            }
        })
    }
}
