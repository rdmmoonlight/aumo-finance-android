package com.aumofinance.app.auth

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.aumofinance.app.network.SessionManager
import com.aumofinance.app.network.SessionStore
import io.ktor.client.call.body
import io.ktor.http.isSuccess
import kotlinx.coroutines.launch

sealed class LoginState {
    object Idle : LoginState()
    object Loading : LoginState()
    data class Success(val fullName: String) : LoginState()
    data class Error(val message: String) : LoginState()
}

class LoginViewModel : ViewModel() {
    private val api = AuthApi()

    private val _state = MutableLiveData<LoginState>(LoginState.Idle)
    val state: LiveData<LoginState> = _state

    fun login(email: String, password: String, keepSignedIn: Boolean, enableBiometric: Boolean) {
        _state.value = LoginState.Loading
        viewModelScope.launch {
            try {
                val response = api.login(LoginRequest(email, password))
                val body = response.body<LoginResponse>()
                if (response.status.isSuccess() && body.success) {
                    SessionManager.token = body.token
                    SessionManager.userId = body.userId
                    SessionManager.fullName = body.fullName
                    // Biometrik cuma masuk akal kalau sesi memang disimpan —
                    // kalau user centang biometrik tapi tidak centang "Ingat
                    // saya", anggap keduanya diminta (tidak ada yang bisa
                    // dibuka biometrik kalau tidak ada sesi tersimpan).
                    val shouldKeepSignedIn = keepSignedIn || enableBiometric
                    SessionStore.save(body.token, body.userId, body.fullName, shouldKeepSignedIn, enableBiometric)
                    _state.value = LoginState.Success(body.fullName)
                } else {
                    _state.value = LoginState.Error(body.message.ifBlank { "Login gagal (${response.status.value})" })
                }
            } catch (t: Throwable) {
                _state.value = LoginState.Error(t.message ?: "Koneksi gagal")
            }
        }
    }
}
