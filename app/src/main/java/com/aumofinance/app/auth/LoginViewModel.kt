package com.aumofinance.app.auth

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
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

// State Compose (bukan LiveData) — mengikuti pola JournalEntryViewModel/PeriodsViewModel
// sejak halaman ini dipindah dari Activity/View ke Jetpack Compose.
class LoginViewModel : ViewModel() {
    private val api = AuthApi()

    var state: LoginState by mutableStateOf(LoginState.Idle)
        private set

    fun login(
        email: String,
        password: String,
        keepSignedIn: Boolean,
        enableBiometric: Boolean,
    ) {
        state = LoginState.Loading
        // Biometrik cuma masuk akal kalau sesi memang disimpan — kalau
        // user centang biometrik tapi tidak centang "Ingat saya", anggap
        // keduanya diminta (tidak ada yang bisa dibuka biometrik kalau
        // tidak ada sesi tersimpan).
        val shouldKeepSignedIn = keepSignedIn || enableBiometric
        viewModelScope.launch {
            try {
                val response = api.login(LoginRequest(email, password, shouldKeepSignedIn))
                val body = response.body<LoginResponse>()
                if (response.status.isSuccess() && body.success && body.token != null && body.userId != null && body.fullName != null) {
                    SessionManager.token = body.token
                    SessionManager.userId = body.userId
                    SessionManager.fullName = body.fullName
                    SessionStore.save(body.token, body.userId, body.fullName, shouldKeepSignedIn, enableBiometric)
                    state = LoginState.Success(body.fullName)
                } else {
                    state = LoginState.Error(body.message.ifBlank { "Login gagal (${response.status.value})" })
                }
            } catch (t: Throwable) {
                state = LoginState.Error(t.message ?: "Koneksi gagal")
            }
        }
    }
}
