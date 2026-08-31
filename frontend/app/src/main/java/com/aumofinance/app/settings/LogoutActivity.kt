package com.aumofinance.app.settings

import android.content.Intent
import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.aumofinance.app.auth.LoginActivity
import com.aumofinance.app.network.ApiClient
import com.aumofinance.app.network.SessionManager
import com.aumofinance.app.R
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class LogoutActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_logout)

        // JWT bersifat stateless di sisi server (lihat AuthController.Logout di
        // aumo-finance-web) — panggilan ini hanya formalitas, sesi sebenarnya
        // berakhir begitu token dihapus dari SessionManager di bawah.
        val api = ApiClient.retrofit.create(com.aumofinance.app.auth.AuthApi::class.java)
        api.logout().enqueue(object : Callback<Map<String, Any?>> {
            override fun onResponse(call: Call<Map<String, Any?>>, response: Response<Map<String, Any?>>) = Unit
            override fun onFailure(call: Call<Map<String, Any?>>, t: Throwable) = Unit
        })

        SessionManager.clear()

        val intent = Intent(this, LoginActivity::class.java)
        intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
        startActivity(intent)
        finish()
    }
}
