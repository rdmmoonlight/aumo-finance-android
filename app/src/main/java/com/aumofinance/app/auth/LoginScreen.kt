package com.aumofinance.app.auth

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.foundation.rememberScrollState
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Checkbox
import androidx.compose.material3.CheckboxDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import com.aumofinance.app.R
import com.aumofinance.app.ui.theme.AumoColors

/** Padanan Compose dari activity_login.xml. */
@Composable
fun LoginScreen(
    email: String,
    onEmailChange: (String) -> Unit,
    password: String,
    onPasswordChange: (String) -> Unit,
    keepSignedIn: Boolean,
    onKeepSignedInChange: (Boolean) -> Unit,
    showBiometricSetupCheckbox: Boolean,
    biometricSetupChecked: Boolean,
    onBiometricSetupChange: (Boolean) -> Unit,
    showBiometricLoginButton: Boolean,
    onBiometricLoginClick: () -> Unit,
    errorMessage: String?,
    isLoading: Boolean,
    onLoginClick: () -> Unit,
) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(
            modifier =
                Modifier
                    .fillMaxSize()
                    .padding(innerPadding)
                    .verticalScroll(rememberScrollState())
                    .padding(horizontal = 28.dp)
                    .padding(top = 72.dp, bottom = 32.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            Image(
                painter = painterResource(id = R.drawable.app_logo),
                contentDescription = null,
                modifier = Modifier.size(88.dp),
            )
            Text(
                text = "AumoFinance",
                color = AumoColors.TextPrimary,
                fontWeight = FontWeight.Bold,
                fontSize = MaterialTheme.typography.headlineSmall.fontSize,
                modifier = Modifier.padding(top = 16.dp),
            )
            Text(
                text = "Kelola pembukuan usaha Anda",
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
                modifier = Modifier.padding(top = 4.dp),
            )

            Column(
                modifier =
                    Modifier
                        .fillMaxWidth()
                        .padding(top = 40.dp)
                        .background(AumoColors.Surface, RoundedCornerShape(16.dp))
                        .padding(20.dp),
            ) {
                Text("Email", color = AumoColors.TextMuted, fontSize = MaterialTheme.typography.labelSmall.fontSize)
                OutlinedTextField(
                    value = email,
                    onValueChange = onEmailChange,
                    placeholder = { Text("nama@email.com") },
                    singleLine = true,
                    colors = loginFieldColors(),
                    modifier = Modifier.fillMaxWidth().padding(top = 6.dp),
                )

                Text(
                    "Password",
                    color = AumoColors.TextMuted,
                    fontSize = MaterialTheme.typography.labelSmall.fontSize,
                    modifier = Modifier.padding(top = 16.dp),
                )
                OutlinedTextField(
                    value = password,
                    onValueChange = onPasswordChange,
                    placeholder = { Text("••••••••") },
                    singleLine = true,
                    visualTransformation = PasswordVisualTransformation(),
                    colors = loginFieldColors(),
                    modifier = Modifier.fillMaxWidth().padding(top = 6.dp),
                )

                LoginCheckboxRow(
                    label = "Ingat saya di perangkat ini",
                    checked = keepSignedIn,
                    onCheckedChange = onKeepSignedInChange,
                    modifier = Modifier.padding(top = 18.dp),
                )

                if (showBiometricSetupCheckbox) {
                    LoginCheckboxRow(
                        label = "Masuk dengan sidik jari/wajah lain kali",
                        checked = biometricSetupChecked,
                        onCheckedChange = onBiometricSetupChange,
                        modifier = Modifier.padding(top = 4.dp),
                    )
                }

                if (errorMessage != null) {
                    Text(
                        text = errorMessage,
                        color = AumoColors.Bad,
                        fontSize = MaterialTheme.typography.labelSmall.fontSize,
                        modifier = Modifier.fillMaxWidth().padding(top = 10.dp),
                    )
                }

                Button(
                    onClick = onLoginClick,
                    enabled = !isLoading,
                    colors = ButtonDefaults.buttonColors(containerColor = AumoColors.Primary),
                    modifier = Modifier.fillMaxWidth().padding(top = 20.dp),
                ) {
                    Text(if (isLoading) "Memproses..." else "Masuk", fontSize = MaterialTheme.typography.bodyMedium.fontSize)
                }

                if (showBiometricLoginButton) {
                    Button(
                        onClick = onBiometricLoginClick,
                        colors = ButtonDefaults.buttonColors(containerColor = AumoColors.Background),
                        modifier = Modifier.fillMaxWidth().padding(top = 10.dp),
                    ) {
                        Text("\uD83D\uDD12  Masuk dengan Biometrik", color = AumoColors.TextPrimary, fontSize = MaterialTheme.typography.bodyMedium.fontSize)
                    }
                }
            }
        }
    }
}

@Composable
private fun LoginCheckboxRow(
    label: String,
    checked: Boolean,
    onCheckedChange: (Boolean) -> Unit,
    modifier: Modifier = Modifier,
) {
    androidx.compose.foundation.layout.Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        modifier = modifier,
    ) {
        Checkbox(
            checked = checked,
            onCheckedChange = onCheckedChange,
            colors = CheckboxDefaults.colors(checkedColor = AumoColors.Primary),
        )
        Text(label, color = AumoColors.TextPrimary, fontSize = MaterialTheme.typography.bodySmall.fontSize)
    }
}

@Composable
private fun loginFieldColors() =
    OutlinedTextFieldDefaults.colors(
        focusedTextColor = AumoColors.TextPrimary,
        unfocusedTextColor = AumoColors.TextPrimary,
        focusedContainerColor = AumoColors.Background,
        unfocusedContainerColor = AumoColors.Background,
        focusedBorderColor = AumoColors.Border,
        unfocusedBorderColor = AumoColors.Border,
        unfocusedPlaceholderColor = AumoColors.TextMuted,
        focusedPlaceholderColor = AumoColors.TextMuted,
    )
