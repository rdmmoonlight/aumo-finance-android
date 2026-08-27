package com.aumofinance.app.auth

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.aumofinance.app.network.ApiClient
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

sealed class LoginState {
    object Idle : LoginState()
    object Loading : LoginState()
    data class Success(val token: String) : LoginState()
    data class Error(val message: String) : LoginState()
}

class LoginViewModel : ViewModel() {
    private val api = ApiClient.retrofit.create(AuthApi::class.java)

    private val _state = MutableLiveData<LoginState>(LoginState.Idle)
    val state: LiveData<LoginState> = _state

    fun login(username: String, password: String) {
        _state.value = LoginState.Loading
        api.login(LoginRequest(username, password)).enqueue(object : Callback<LoginResponse> {
            override fun onResponse(call: Call<LoginResponse>, response: Response<LoginResponse>) {
                val body = response.body()
                _state.value = if (response.isSuccessful && body != null) {
                    LoginState.Success(body.token)
                } else {
                    LoginState.Error("Login gagal (${response.code()})")
                }
            }

            override fun onFailure(call: Call<LoginResponse>, t: Throwable) {
                _state.value = LoginState.Error(t.message ?: "Koneksi gagal")
            }
        })
    }
}
