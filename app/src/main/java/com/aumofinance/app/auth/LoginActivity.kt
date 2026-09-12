package com.aumofinance.app.auth

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import com.aumofinance.app.home.HomeActivity
import com.aumofinance.app.network.SessionStore
import com.aumofinance.app.ui.theme.AumoTheme

class LoginActivity : ComponentActivity() {
    private val viewModel: LoginViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        // Kalau biometrik sudah pernah diaktifkan dan ada sesi tersimpan,
        // tawarkan tombol "Masuk dengan Biometrik" alih-alih checkbox aktivasi
        // (checkbox aktivasi cuma relevan saat SETUP pertama kali).
        val biometricAvailable = BiometricHelper.isAvailable(this)
        val alreadyEnabled = SessionStore.isBiometricEnabled() && SessionStore.hasSavedSession()
        val showBiometricSetupCheckbox = biometricAvailable && !alreadyEnabled
        val showBiometricLoginButton = biometricAvailable && alreadyEnabled

        setContent {
            var email by remember { mutableStateOf("") }
            var password by remember { mutableStateOf("") }
            var keepSignedIn by remember { mutableStateOf(false) }
            var biometricSetupChecked by remember { mutableStateOf(false) }
            var biometricError by remember { mutableStateOf<String?>(null) }

            val state = viewModel.state
            LaunchedEffect(state) {
                if (state is LoginState.Success) goToHome()
            }

            val errorMessage =
                biometricError ?: (state as? LoginState.Error)?.message

            AumoTheme {
                LoginScreen(
                    email = email,
                    onEmailChange = { email = it },
                    password = password,
                    onPasswordChange = { password = it },
                    keepSignedIn = keepSignedIn,
                    onKeepSignedInChange = { keepSignedIn = it },
                    showBiometricSetupCheckbox = showBiometricSetupCheckbox,
                    biometricSetupChecked = biometricSetupChecked,
                    onBiometricSetupChange = { checked ->
                        biometricSetupChecked = checked
                        // "Aktifkan biometrik" cuma masuk akal kalau sesi juga
                        // disimpan — centang otomatis "Ingat saya" (tidak bisa
                        // dicentang lepas tanpa "Ingat saya" ikut aktif).
                        if (checked) keepSignedIn = true
                    },
                    showBiometricLoginButton = showBiometricLoginButton,
                    onBiometricLoginClick = {
                        biometricError = null
                        BiometricHelper.authenticate(
                            activity = this@LoginActivity,
                            title = "Masuk ke AumoFinance",
                            subtitle = "Gunakan sidik jari atau wajah Anda",
                            onSuccess = {
                                // Token JWT ikut dipulihkan oleh
                                // restoreIntoSessionManager() di bawah — cukup
                                // itu saja untuk request pertama di Home.
                                SessionStore.restoreIntoSessionManager()
                                goToHome()
                            },
                            onFailure = { message -> biometricError = message },
                        )
                    },
                    errorMessage = errorMessage,
                    isLoading = state is LoginState.Loading,
                    onLoginClick = {
                        biometricError = null
                        viewModel.login(email.trim(), password, keepSignedIn, biometricSetupChecked)
                    },
                )
            }
        }
    }

    private fun goToHome() {
        startActivity(Intent(this, HomeActivity::class.java))
        finish()
    }
}
