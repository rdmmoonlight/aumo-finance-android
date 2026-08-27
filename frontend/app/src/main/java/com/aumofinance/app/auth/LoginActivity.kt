package com.aumofinance.app.auth

import android.content.Intent
import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import androidx.activity.viewModels
import com.aumofinance.app.core.MainActivity
import com.aumofinance.app.R

class LoginActivity : AppCompatActivity() {

    private val viewModel: LoginViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_login)

        viewModel.state.observe(this) { state ->
            when (state) {
                is LoginState.Success -> {
                    // TODO fase 3: simpan token (EncryptedSharedPreferences) sebelum lanjut
                    startActivity(Intent(this, MainActivity::class.java))
                    finish()
                }
                is LoginState.Error -> {
                    // TODO: tampilkan pesan error di UI (Snackbar/TextView)
                }
                else -> Unit
            }
        }

        // TODO: hubungkan tombol login di activity_login.xml ke viewModel.login(user, pass)
    }
}
