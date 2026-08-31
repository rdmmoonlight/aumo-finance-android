package com.aumofinance.app.auth

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import androidx.appcompat.app.AppCompatActivity
import androidx.activity.viewModels
import com.aumofinance.app.home.HomeActivity
import com.aumofinance.app.R

class LoginActivity : AppCompatActivity() {

    private val viewModel: LoginViewModel by viewModels()
    private lateinit var inputEmail: EditText
    private lateinit var inputPassword: EditText
    private lateinit var buttonLogin: Button

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_login)

        inputEmail = findViewById(R.id.inputUsername)
        inputPassword = findViewById(R.id.inputPassword)
        buttonLogin = findViewById(R.id.buttonLogin)

        buttonLogin.setOnClickListener {
            viewModel.login(inputEmail.text.toString().trim(), inputPassword.text.toString())
        }

        viewModel.state.observe(this) { state ->
            when (state) {
                is LoginState.Success -> {
                    startActivity(Intent(this, HomeActivity::class.java))
                    finish()
                }
                is LoginState.Error -> {
                    // TODO: tampilkan pesan error di UI (Snackbar/TextView) — state.message
                }
                else -> Unit
            }
        }
    }
}
