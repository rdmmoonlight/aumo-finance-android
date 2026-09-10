package com.aumofinance.app.periods

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import com.aumofinance.app.ui.icons.TablerIcon
import com.aumofinance.app.ui.icons.TablerIcons
import com.aumofinance.app.ui.theme.AumoColors
import java.util.Calendar

/**
 * Halaman Periods, ditulis ulang dengan Jetpack Compose (sebelumnya
 * RecyclerView + AlertDialog berbasis View/XML biasa). Font mengikuti
 * MaterialTheme.typography global (Aptos Regular/Bold, lihat AumoTheme),
 * ikon memakai TablerIcon — sama seperti Journal Entry & Home.
 */
@Composable
fun PeriodsScreen(
    periods: List<Period>,
    selectedPeriodId: Int?,
    onSelect: (Period) -> Unit,
    onCloseRequest: (Period) -> Unit,
    onOpenNewPeriodClick: () -> Unit,
) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(modifier = Modifier.fillMaxSize().padding(innerPadding)) {
            Text(
                text = "Periods",
                color = AumoColors.TextPrimary,
                fontWeight = FontWeight.Bold,
                fontSize = MaterialTheme.typography.titleLarge.fontSize,
                modifier = Modifier.padding(16.dp, 14.dp, 16.dp, 0.dp),
            )

            Button(
                onClick = onOpenNewPeriodClick,
                colors = ButtonDefaults.buttonColors(containerColor = AumoColors.Primary),
                shape = RoundedCornerShape(8.dp),
                modifier = Modifier.padding(16.dp, 12.dp, 16.dp, 4.dp).fillMaxWidth(),
            ) {
                TablerIcon(TablerIcons.CirclePlus, tint = Color.White, size = 16.dp)
                Spacer(modifier = Modifier.width(8.dp))
                Text("Open New Period", color = Color.White, fontWeight = FontWeight.Bold)
            }

            if (periods.isEmpty()) {
                Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        TablerIcon(TablerIcons.CalendarOff, tint = AumoColors.TextMuted, size = 40.dp)
                        Spacer(modifier = Modifier.height(8.dp))
                        Text("No periods yet", color = AumoColors.TextMuted)
                    }
                }
            } else {
                LazyColumn(
                    modifier = Modifier.weight(1f).fillMaxWidth(),
                    contentPadding = PaddingValues(16.dp, 12.dp, 16.dp, 16.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp),
                ) {
                    items(periods) { period ->
                        PeriodCard(
                            period = period,
                            isSelected = period.id == selectedPeriodId,
                            onSelect = { onSelect(period) },
                            onCloseRequest = { onCloseRequest(period) },
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun PeriodCard(
    period: Period,
    isSelected: Boolean,
    onSelect: () -> Unit,
    onCloseRequest: () -> Unit,
) {
    Card(
        colors = CardDefaults.cardColors(containerColor = AumoColors.Surface),
        border = BorderStroke(1.dp, if (isSelected) AumoColors.Primary else AumoColors.SurfaceElevated),
        shape = RoundedCornerShape(12.dp),
        modifier = Modifier.fillMaxWidth(),
    ) {
        Column(modifier = Modifier.padding(14.dp)) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text(
                    text = period.periodName,
                    color = AumoColors.TextPrimary,
                    fontWeight = FontWeight.Bold,
                    fontSize = MaterialTheme.typography.titleMedium.fontSize,
                )
                Box(
                    modifier =
                        Modifier
                            .background(if (period.isClosed) AumoColors.Bad else AumoColors.Good, RoundedCornerShape(6.dp))
                            .padding(8.dp, 4.dp),
                ) {
                    Text(
                        text = if (period.isClosed) "Closed" else "Open",
                        color = Color.White,
                        fontWeight = FontWeight.Bold,
                        fontSize = MaterialTheme.typography.labelSmall.fontSize,
                    )
                }
            }

            Spacer(modifier = Modifier.height(4.dp))
            Text(
                text = "${period.startDate.take(10)} \u2013 ${period.endDate.take(10)}",
                color = AumoColors.TextSecondary,
                fontSize = MaterialTheme.typography.bodySmall.fontSize,
            )

            Spacer(modifier = Modifier.height(10.dp))
            Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                ActionChip(
                    icon = TablerIcons.Eye,
                    label = if (isSelected) "Viewing" else "View",
                    tint = if (isSelected) AumoColors.Primary else AumoColors.TextSecondary,
                    onClick = onSelect,
                )
                if (!period.isClosed) {
                    ActionChip(icon = TablerIcons.Lock, label = "Close", tint = AumoColors.Bad, onClick = onCloseRequest)
                }
            }
        }
    }
}

@Composable
private fun ActionChip(
    icon: String,
    label: String,
    tint: Color,
    onClick: () -> Unit,
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        modifier =
            Modifier
                .background(tint.copy(alpha = 0.15f), RoundedCornerShape(8.dp))
                .clickable(onClick = onClick)
                .padding(10.dp, 6.dp),
    ) {
        TablerIcon(icon, tint = tint, size = 14.dp)
        Spacer(modifier = Modifier.width(6.dp))
        Text(label, color = tint, fontWeight = FontWeight.Bold, fontSize = MaterialTheme.typography.labelSmall.fontSize)
    }
}

/**
 * Dialog "Open New Period" dengan dua kondisi:
 * - hasExistingPermanentAccounts == false: belum pernah ada akun Cash/Bank &
 *   Retained Earnings sama sekali -> form daftar akun baru + saldo awal
 *   (mode CreateNew).
 * - hasExistingPermanentAccounts == true: sudah ada -> user MEMILIH akun
 *   Cash, Bank, dan Retained Earnings mana yang dilanjutkan dari
 *   availableCashAndBankAccounts / availableRetainedEarningsAccounts (mode
 *   LoadExisting). Backend tidak lagi mengirim saldo carry-forward di sini —
 *   saldo berjalan otomatis mengikuti saldo ledger akun yang dipilih.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun OpenPeriodDialog(
    info: OpenPeriodInfoResponse,
    onDismiss: () -> Unit,
    onSubmit: (CreatePeriodRequest) -> Unit,
) {
    val now = remember { Calendar.getInstance() }
    var month by remember { mutableStateOf((now.get(Calendar.MONTH) + 1).toString()) }
    var year by remember { mutableStateOf(now.get(Calendar.YEAR).toString()) }

    val hasExisting = info.hasExistingPermanentAccounts

    var selectedCashAccount by remember { mutableStateOf<AvailableAccount?>(null) }
    var selectedBankAccount by remember { mutableStateOf<AvailableAccount?>(null) }
    var selectedRetainedAccount by remember { mutableStateOf<AvailableAccount?>(null) }

    var cashCode by remember { mutableStateOf("") }
    var cashName by remember { mutableStateOf("") }
    var cashBalance by remember { mutableStateOf("") }
    var bankCode by remember { mutableStateOf("") }
    var bankName by remember { mutableStateOf("") }
    var bankBalance by remember { mutableStateOf("") }
    var retainedCode by remember { mutableStateOf("") }
    var retainedName by remember { mutableStateOf("") }

    AlertDialog(
        onDismissRequest = onDismiss,
        containerColor = AumoColors.Surface,
        title = { Text("Open New Period", color = AumoColors.TextPrimary, fontWeight = FontWeight.Bold) },
        text = {
            Column(
                modifier =
                    Modifier
                        .fillMaxWidth()
                        .heightIn(max = 460.dp)
                        .verticalScroll(rememberScrollState()),
                verticalArrangement = Arrangement.spacedBy(10.dp),
            ) {
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(
                        value = month,
                        onValueChange = { month = it.filter(Char::isDigit) },
                        label = { Text("Month (1-12)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        colors = dialogFieldColors(),
                        shape = RoundedCornerShape(8.dp),
                        singleLine = true,
                        modifier = Modifier.weight(1f),
                    )
                    OutlinedTextField(
                        value = year,
                        onValueChange = { year = it.filter(Char::isDigit) },
                        label = { Text("Year") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        colors = dialogFieldColors(),
                        shape = RoundedCornerShape(8.dp),
                        singleLine = true,
                        modifier = Modifier.weight(1f),
                    )
                }

                if (hasExisting) {
                    Text(
                        text = "Select which existing accounts to carry forward into this period:",
                        color = AumoColors.TextSecondary,
                        fontSize = MaterialTheme.typography.bodySmall.fontSize,
                    )
                    AccountPickerField(
                        label = "Cash Account",
                        options = info.availableCashAndBankAccounts,
                        selected = selectedCashAccount,
                        onSelect = { selectedCashAccount = it },
                    )
                    AccountPickerField(
                        label = "Bank Account",
                        options = info.availableCashAndBankAccounts,
                        selected = selectedBankAccount,
                        onSelect = { selectedBankAccount = it },
                    )
                    AccountPickerField(
                        label = "Retained Earnings Account",
                        options = info.availableRetainedEarningsAccounts,
                        selected = selectedRetainedAccount,
                        onSelect = { selectedRetainedAccount = it },
                    )
                } else {
                    Text(
                        text = "No period has been closed yet. Register your permanent accounts and opening balances:",
                        color = AumoColors.TextSecondary,
                        fontSize = MaterialTheme.typography.bodySmall.fontSize,
                    )
                    NewAccountFields(
                        "Cash Account",
                        cashCode,
                        { cashCode = it.filter(Char::isDigit) },
                        cashName,
                        { cashName = it },
                        cashBalance,
                    ) {
                        cashBalance =
                            it.filter {
                                    c ->
                                c.isDigit() || c == '.'
                            }
                    }
                    NewAccountFields(
                        "Bank Account",
                        bankCode,
                        { bankCode = it.filter(Char::isDigit) },
                        bankName,
                        { bankName = it },
                        bankBalance,
                    ) {
                        bankBalance =
                            it.filter {
                                    c ->
                                c.isDigit() || c == '.'
                            }
                    }
                    NewAccountFields("Retained Earnings Account", retainedCode, {
                        retainedCode = it.filter(Char::isDigit)
                    }, retainedName, { retainedName = it }, null, null)
                }
            }
        },
        confirmButton = {
            TextButton(onClick = {
                val monthInt = month.toIntOrNull() ?: return@TextButton
                val yearInt = year.toIntOrNull() ?: return@TextButton

                val request =
                    if (hasExisting) {
                        val cashId = selectedCashAccount?.id ?: return@TextButton
                        val bankId = selectedBankAccount?.id ?: return@TextButton
                        val retainedId = selectedRetainedAccount?.id ?: return@TextButton
                        CreatePeriodRequest(
                            month = monthInt,
                            year = yearInt,
                            setupMode = CreatePeriodRequest.MODE_LOAD_EXISTING,
                            cashAccountId = cashId,
                            bankAccountId = bankId,
                            retainedEarningsAccountId = retainedId,
                        )
                    } else {
                        CreatePeriodRequest(
                            month = monthInt,
                            year = yearInt,
                            setupMode = CreatePeriodRequest.MODE_CREATE_NEW,
                            cashAccountCode = cashCode,
                            cashAccountName = cashName,
                            cashBalance = cashBalance.toDoubleOrNull(),
                            bankAccountCode = bankCode,
                            bankAccountName = bankName,
                            bankBalance = bankBalance.toDoubleOrNull(),
                            retainedEarningsAccountCode = retainedCode,
                            retainedEarningsAccountName = retainedName,
                        )
                    }
                onSubmit(request)
            }) {
                Text("Open", color = AumoColors.Good, fontWeight = FontWeight.Bold)
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel", color = AumoColors.TextSecondary)
            }
        },
    )
}

// Dropdown sederhana untuk memilih salah satu akun yang tersedia dari
// backend (availableCashAndBankAccounts / availableRetainedEarningsAccounts)
// saat membuka periode dalam mode LoadExisting.
@Composable
private fun AccountPickerField(
    label: String,
    options: List<AvailableAccount>,
    selected: AvailableAccount?,
    onSelect: (AvailableAccount) -> Unit,
) {
    var expanded by remember { mutableStateOf(false) }

    Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
        Text(label, color = AumoColors.TextSecondary, fontWeight = FontWeight.Bold, fontSize = MaterialTheme.typography.labelSmall.fontSize)
        Box {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween,
                modifier =
                    Modifier
                        .fillMaxWidth()
                        .background(AumoColors.Background, RoundedCornerShape(8.dp))
                        .clickable { expanded = true }
                        .padding(12.dp, 12.dp),
            ) {
                Text(
                    text = selected?.displayLabel ?: "Select an account",
                    color = if (selected != null) AumoColors.TextPrimary else AumoColors.TextSecondary,
                    fontSize = MaterialTheme.typography.bodyMedium.fontSize,
                )
                TablerIcon(TablerIcons.Eye, tint = AumoColors.TextSecondary, size = 14.dp)
            }
            androidx.compose.material3.DropdownMenu(
                expanded = expanded,
                onDismissRequest = { expanded = false },
            ) {
                if (options.isEmpty()) {
                    androidx.compose.material3.DropdownMenuItem(
                        text = { Text("No accounts available") },
                        onClick = { expanded = false },
                    )
                }
                options.forEach { account ->
                    androidx.compose.material3.DropdownMenuItem(
                        text = { Text(account.displayLabel) },
                        onClick = {
                            onSelect(account)
                            expanded = false
                        },
                    )
                }
            }
        }
    }
}

@Composable
private fun NewAccountFields(
    title: String,
    code: String,
    onCodeChange: (String) -> Unit,
    name: String,
    onNameChange: (String) -> Unit,
    balance: String?,
    onBalanceChange: ((String) -> Unit)?,
) {
    Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
        Text(title, color = AumoColors.TextSecondary, fontWeight = FontWeight.Bold, fontSize = MaterialTheme.typography.labelSmall.fontSize)
        OutlinedTextField(
            value = code,
            onValueChange = onCodeChange,
            placeholder = { Text("Reference Code", color = AumoColors.TextSecondary) },
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
            colors = dialogFieldColors(),
            shape = RoundedCornerShape(8.dp),
            singleLine = true,
            modifier = Modifier.fillMaxWidth(),
        )
        OutlinedTextField(
            value = name,
            onValueChange = onNameChange,
            placeholder = { Text("Account Name", color = AumoColors.TextSecondary) },
            colors = dialogFieldColors(),
            shape = RoundedCornerShape(8.dp),
            singleLine = true,
            modifier = Modifier.fillMaxWidth(),
        )
        if (balance != null && onBalanceChange != null) {
            OutlinedTextField(
                value = balance,
                onValueChange = onBalanceChange,
                placeholder = { Text("Opening Balance", color = AumoColors.TextSecondary) },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                colors = dialogFieldColors(),
                shape = RoundedCornerShape(8.dp),
                singleLine = true,
                modifier = Modifier.fillMaxWidth(),
            )
        }
    }
}

@Composable
private fun dialogFieldColors() =
    OutlinedTextFieldDefaults.colors(
        focusedContainerColor = AumoColors.Background,
        unfocusedContainerColor = AumoColors.Background,
        disabledContainerColor = AumoColors.Background,
        focusedTextColor = AumoColors.TextPrimary,
        unfocusedTextColor = AumoColors.TextPrimary,
        disabledTextColor = AumoColors.TextSecondary,
        focusedBorderColor = AumoColors.Primary,
        unfocusedBorderColor = AumoColors.SurfaceElevated,
        disabledBorderColor = AumoColors.SurfaceElevated,
    )
