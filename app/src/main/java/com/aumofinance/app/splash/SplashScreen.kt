package com.aumofinance.app.splash

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import com.aumofinance.app.R

/**
 * Splash penuh (logo + atribusi) — bukan API splash minimalis Android 12+,
 * lihat catatan di SplashActivity. Padanan Compose dari activity_splash.xml
 * (FrameLayout putih + ImageView fitCenter).
 */
@Composable
fun SplashScreen() {
    Surface(modifier = Modifier.fillMaxSize().background(Color.White)) {
        Image(
            painter = painterResource(id = R.drawable.splash_screen),
            contentDescription = null,
            contentScale = ContentScale.Fit,
            modifier = Modifier.fillMaxSize(),
        )
    }
}
