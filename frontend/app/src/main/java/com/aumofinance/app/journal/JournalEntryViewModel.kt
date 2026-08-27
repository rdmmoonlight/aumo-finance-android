package com.aumofinance.app.journal

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class JournalEntryViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(JournalApi::class.java)

    private val _entries = MutableLiveData<List<JournalEntry>>(emptyList())
    val entries: LiveData<List<JournalEntry>> = _entries

    fun load() {
        api.list().enqueue(object : Callback<List<JournalEntry>> {
            override fun onResponse(call: Call<List<JournalEntry>>, response: Response<List<JournalEntry>>) {
                _entries.value = response.body() ?: emptyList()
            }
            override fun onFailure(call: Call<List<JournalEntry>>, t: Throwable) {
                _entries.value = emptyList()
            }
        })
    }

    fun save(id: Int?, request: JournalEntryRequest) {
        val call = if (id == null) api.create(request) else api.update(id, request)
        call.enqueue(object : Callback<JournalEntry> {
            override fun onResponse(call: Call<JournalEntry>, response: Response<JournalEntry>) = load()
            override fun onFailure(call: Call<JournalEntry>, t: Throwable) = Unit
        })
    }

    fun delete(id: Int) {
        api.delete(id).enqueue(object : Callback<Unit> {
            override fun onResponse(call: Call<Unit>, response: Response<Unit>) = load()
            override fun onFailure(call: Call<Unit>, t: Throwable) = Unit
        })
    }
}
