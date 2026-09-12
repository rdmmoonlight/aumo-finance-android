package com.aumofinance.app.crashlog

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.aumofinance.app.ui.theme.AumoColors

/** Padanan Compose dari activity_crash_log.xml. */
@Composable
fun CrashLogScreen(
    content: String,
    onCopyClick: () -> Unit,
) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(modifier = Modifier.fillMaxSize().padding(innerPadding)) {
            Button(
                onClick = onCopyClick,
                colors = ButtonDefaults.buttonColors(containerColor = AumoColors.Primary),
                modifier = Modifier.fillMaxWidth().padding(16.dp),
            ) {
                Text("Copy Crash Log", color = AumoColors.TextPrimary)
            }

            Text(
                text = content,
                color = AumoColors.TextPrimary,
                fontFamily = FontFamily.Monospace,
                fontSize = 12.sp,
                modifier =
                    Modifier
                        .fillMaxSize()
                        .verticalScroll(rememberScrollState())
                        .padding(16.dp),
            )
        }
    }
}
