package com.aumofinance.app.auth

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import com.aumofinance.app.network.SessionManager
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

sealed class LoginState {
    object Idle : LoginState()
    object Loading : LoginState()
    data class Success(val fullName: String) : LoginState()
    data class Error(val message: String) : LoginState()
}

class LoginViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(AuthApi::class.java)

    private val _state = MutableLiveData<LoginState>(LoginState.Idle)
    val state: LiveData<LoginState> = _state

    fun login(email: String, password: String) {
        _state.value = LoginState.Loading
        api.login(LoginRequest(email, password)).enqueue(object : Callback<LoginResponse> {
            override fun onResponse(call: Call<LoginResponse>, response: Response<LoginResponse>) {
                val body = response.body()
                if (response.isSuccessful && body?.success == true) {
                    SessionManager.token = body.token
                    SessionManager.userId = body.userId
                    SessionManager.fullName = body.fullName
                    _state.value = LoginState.Success(body.fullName)
                } else {
                    _state.value = LoginState.Error(body?.message ?: "Login gagal (${response.code()})")
                }
            }

            override fun onFailure(call: Call<LoginResponse>, t: Throwable) {
                _state.value = LoginState.Error(t.message ?: "Koneksi gagal")
            }
        })
    }
}
