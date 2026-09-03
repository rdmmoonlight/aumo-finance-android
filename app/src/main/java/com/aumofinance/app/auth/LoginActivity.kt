package com.aumofinance.app.auth

import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.CheckBox
import android.widget.EditText
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.activity.viewModels
import com.aumofinance.app.home.HomeActivity
import com.aumofinance.app.network.SessionStore
import com.aumofinance.app.R

class LoginActivity : AppCompatActivity() {

    private val viewModel: LoginViewModel by viewModels()
    private lateinit var inputEmail: EditText
    private lateinit var inputPassword: EditText
    private lateinit var buttonLogin: Button
    private lateinit var checkboxKeepSignedIn: CheckBox
    private lateinit var checkboxBiometric: CheckBox
    private lateinit var buttonBiometricLogin: Button
    private lateinit var textLoginError: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_login)

        inputEmail = findViewById(R.id.inputUsername)
        inputPassword = findViewById(R.id.inputPassword)
        buttonLogin = findViewById(R.id.buttonLogin)
        checkboxKeepSignedIn = findViewById(R.id.checkboxKeepSignedIn)
        checkboxBiometric = findViewById(R.id.checkboxBiometric)
        buttonBiometricLogin = findViewById(R.id.buttonBiometricLogin)
        textLoginError = findViewById(R.id.textLoginError)

        setupBiometricVisibility()

        // "Aktifkan biometrik" cuma masuk akal kalau sesi juga disimpan —
        // centang otomatis "Ingat saya" dan kunci (tidak bisa dicentang lepas
        // tanpa "Ingat saya" ikut aktif).
        checkboxBiometric.setOnCheckedChangeListener { _, isChecked ->
            if (isChecked) checkboxKeepSignedIn.isChecked = true
        }

        buttonLogin.setOnClickListener {
            textLoginError.visibility = View.GONE
            viewModel.login(
                inputEmail.text.toString().trim(),
                inputPassword.text.toString(),
                checkboxKeepSignedIn.isChecked,
                checkboxBiometric.isChecked
            )
        }

        buttonBiometricLogin.setOnClickListener { attemptBiometricLogin() }

        viewModel.state.observe(this) { state ->
            when (state) {
                is LoginState.Success -> goToHome()
                is LoginState.Error -> {
                    textLoginError.text = state.message
                    textLoginError.visibility = View.VISIBLE
                }
                else -> Unit
            }
        }
    }

    // Kalau biometrik sudah pernah diaktifkan dan ada sesi tersimpan,
    // tawarkan tombol "Masuk dengan Biometrik" alih-alih checkbox aktivasi
    // (checkbox aktivasi cuma relevan saat SETUP pertama kali).
    private fun setupBiometricVisibility() {
        val biometricAvailable = BiometricHelper.isAvailable(this)
        val alreadyEnabled = SessionStore.isBiometricEnabled() && SessionStore.hasSavedSession()

        checkboxBiometric.visibility = if (biometricAvailable && !alreadyEnabled) View.VISIBLE else View.GONE
        buttonBiometricLogin.visibility = if (biometricAvailable && alreadyEnabled) View.VISIBLE else View.GONE
    }

    private fun attemptBiometricLogin() {
        BiometricHelper.authenticate(
            activity = this,
            title = "Masuk ke AumoFinance",
            subtitle = "Gunakan sidik jari atau wajah Anda",
            onSuccess = {
                SessionStore.restoreIntoSessionManager()
                goToHome()
            },
            onFailure = { message ->
                textLoginError.text = message
                textLoginError.visibility = View.VISIBLE
            }
        )
    }

    private fun goToHome() {
        startActivity(Intent(this, HomeActivity::class.java))
        finish()
    }
}
