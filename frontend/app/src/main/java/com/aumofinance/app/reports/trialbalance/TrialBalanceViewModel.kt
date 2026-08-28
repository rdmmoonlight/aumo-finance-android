package com.aumofinance.app.reports.trialbalance

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class TrialBalanceViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(TrialBalanceApi::class.java)

    private val _report = MutableLiveData<TrialBalanceReport?>()
    val report: LiveData<TrialBalanceReport?> = _report

    fun load(periodId: Int, adjusted: Boolean) {
        api.getTrialBalance(periodId, adjusted).enqueue(object : Callback<TrialBalanceReport> {
            override fun onResponse(call: Call<TrialBalanceReport>, response: Response<TrialBalanceReport>) {
                _report.value = response.body()
            }
            override fun onFailure(call: Call<TrialBalanceReport>, t: Throwable) {
                _report.value = null
            }
        })
    }
}
