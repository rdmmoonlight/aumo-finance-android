package com.aumofinance.app.coa

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.foundation.rememberScrollState
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.aumofinance.app.core.CurrencyFormatter
import com.aumofinance.app.ui.theme.AumoColors

// According to the reference number ranges in AccountClassification.cs in aumo-finance-web.
val CoaAccountTypes =
    listOf(
        "Assets",
        "Liabilities",
        "Equity",
        "OperatingIncome",
        "OperatingExpenses",
        "OtherIncome",
        "OtherExpenses",
    )

/** Padanan Compose dari activity_coa.xml + item_account.xml + dialog Add/Edit Account. */
@Composable
fun CoaScreen(
    accounts: List<Account>,
    searchQuery: String,
    onSearchChange: (String) -> Unit,
    onAddClick: () -> Unit,
    onAccountClick: (Account) -> Unit,
) {
    Scaffold(containerColor = AumoColors.Background) { innerPadding ->
        Column(modifier = Modifier.fillMaxSize().padding(innerPadding).padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                OutlinedTextField(
                    value = searchQuery,
                    onValueChange = onSearchChange,
                    placeholder = { Text("Cari nama/nomor akun") },
                    singleLine = true,
                    colors = coaFieldColors(),
                    modifier = Modifier.weight(1f),
                )
                Button(
                    onClick = onAddClick,
                    colors = ButtonDefaults.buttonColors(containerColor = AumoColors.Primary),
                    modifier = Modifier.padding(start = 8.dp),
                ) {
                    Text("Tambah")
                }
            }

            LazyColumn(modifier = Modifier.fillMaxSize().padding(top = 12.dp)) {
                items(accounts) { account ->
                    AccountRow(account = account, onClick = { onAccountClick(account) })
                }
            }
        }
    }
}

@Composable
private fun AccountRow(
    account: Account,
    onClick: () -> Unit,
) {
    Row(
        modifier =
            Modifier
                .fillMaxWidth()
                .padding(bottom = 8.dp)
                .background(AumoColors.Surface, RoundedCornerShape(8.dp))
                .clickable(onClick = onClick)
                .padding(14.dp),
        horizontalArrangement = Arrangement.SpaceBetween,
    ) {
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = "${account.referenceNumber} - ${account.accountName}",
                color = AumoColors.TextPrimary,
                fontWeight = FontWeight.Bold,
                fontSize = MaterialTheme.typography.bodyMedium.fontSize,
            )
            Text(
                text = account.type,
                color = AumoColors.TextMuted,
                fontSize = MaterialTheme.typography.labelSmall.fontSize,
                modifier = Modifier.padding(top = 2.dp),
            )
        }
        Column(horizontalAlignment = Alignment.End) {
            Text(
                text = CurrencyFormatter.format(account.balance),
                color = AumoColors.TextPrimary,
                fontSize = MaterialTheme.typography.bodyMedium.fontSize,
            )
            if (!account.isActive) {
                Text(
                    text = "Nonaktif",
                    color = AumoColors.Bad,
                    fontSize = MaterialTheme.typography.labelSmall.fontSize,
                    modifier = Modifier.padding(top = 2.dp),
                )
            }
        }
    }
}

/** Dialog Tambah Akun. */
@Composable
fun AddAccountDialog(
    onDismiss: () -> Unit,
    onSubmit: (AccountRequest) -> Unit,
) {
    AccountFormDialog(
        title = "Add Account",
        existing = null,
        onDismiss = onDismiss,
        onSave = { refNumber, name, type, _ ->
            onSubmit(AccountRequest(referenceNumber = refNumber, accountName = name, type = type))
        },
        onDelete = null,
    )
}

/** Dialog Edit Akun (dengan opsi Delete). */
@Composable
fun EditAccountDialog(
    account: Account,
    onDismiss: () -> Unit,
    onSubmit: (UpdateAccountRequest) -> Unit,
    onDelete: () -> Unit,
) {
    AccountFormDialog(
        title = "Edit Account",
        existing = account,
        onDismiss = onDismiss,
        onSave = { refNumber, name, type, isActive ->
            onSubmit(
                UpdateAccountRequest(
                    referenceNumber = refNumber,
                    accountName = name,
                    type = type,
                    isActive = isActive ?: true,
                ),
            )
        },
        onDelete = onDelete,
    )
}

@Composable
private fun AccountFormDialog(
    title: String,
    existing: Account?,
    onDismiss: () -> Unit,
    onSave: (referenceNumber: Int, name: String, type: String, isActive: Boolean?) -> Unit,
    onDelete: (() -> Unit)?,
) {
    var refNumber by remember { mutableStateOf(existing?.referenceNumber?.toString() ?: "") }
    var name by remember { mutableStateOf(existing?.accountName ?: "") }
    var type by remember { mutableStateOf(existing?.type ?: CoaAccountTypes.first()) }
    var isActive by remember { mutableStateOf(existing?.isActive ?: true) }

    AlertDialog(
        onDismissRequest = onDismiss,
        containerColor = AumoColors.Surface,
        title = { Text(title, color = AumoColors.TextPrimary, fontWeight = FontWeight.Bold) },
        text = {
            Column(
                modifier = Modifier.fillMaxWidth().verticalScroll(rememberScrollState()),
                verticalArrangement = Arrangement.spacedBy(10.dp),
            ) {
                OutlinedTextField(
                    value = refNumber,
                    onValueChange = { refNumber = it.filter(Char::isDigit) },
                    label = { Text("Reference Number (e.g., 101)") },
                    singleLine = true,
                    colors = coaFieldColors(),
                    modifier = Modifier.fillMaxWidth(),
                )
                OutlinedTextField(
                    value = name,
                    onValueChange = { name = it },
                    label = { Text("Account Name") },
                    singleLine = true,
                    colors = coaFieldColors(),
                    modifier = Modifier.fillMaxWidth(),
                )
                AccountTypeDropdown(selected = type, onSelect = { type = it })
                if (existing != null) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Text("Active", color = AumoColors.TextPrimary, modifier = Modifier.weight(1f))
                        Switch(checked = isActive, onCheckedChange = { isActive = it })
                    }
                }
            }
        },
        confirmButton = {
            TextButton(
                onClick = {
                    val refNumberInt = refNumber.toIntOrNull() ?: return@TextButton
                    onSave(refNumberInt, name, type, if (existing != null) isActive else null)
                },
            ) {
                Text("Save", color = AumoColors.Primary)
            }
        },
        dismissButton = {
            Row {
                if (onDelete != null) {
                    // Backend will reject (400) if the account already has journal
                    // entries — the message (prompting to set to Inactive via the
                    // toggle above) will automatically appear via errorMessage.
                    TextButton(onClick = onDelete) {
                        Text("Delete", color = AumoColors.Bad)
                    }
                }
                TextButton(onClick = onDismiss) {
                    Text("Cancel", color = AumoColors.TextMuted)
                }
            }
        },
    )
}

@Composable
private fun AccountTypeDropdown(
    selected: String,
    onSelect: (String) -> Unit,
) {
    var expanded by remember { mutableStateOf(false) }
    Box {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.SpaceBetween,
            modifier =
                Modifier
                    .fillMaxWidth()
                    .background(AumoColors.Background, RoundedCornerShape(8.dp))
                    .clickable { expanded = true }
                    .padding(12.dp),
        ) {
            Text(selected, color = AumoColors.TextPrimary)
        }
        DropdownMenu(expanded = expanded, onDismissRequest = { expanded = false }) {
            CoaAccountTypes.forEach { option ->
                DropdownMenuItem(
                    text = { Text(option) },
                    onClick = {
                        onSelect(option)
                        expanded = false
                    },
                )
            }
        }
    }
}

@Composable
private fun coaFieldColors() =
    OutlinedTextFieldDefaults.colors(
        focusedContainerColor = AumoColors.Background,
        unfocusedContainerColor = AumoColors.Background,
        focusedTextColor = AumoColors.TextPrimary,
        unfocusedTextColor = AumoColors.TextPrimary,
        focusedBorderColor = AumoColors.Primary,
        unfocusedBorderColor = AumoColors.SurfaceElevated,
        unfocusedPlaceholderColor = AumoColors.TextMuted,
        focusedPlaceholderColor = AumoColors.TextMuted,
    )
