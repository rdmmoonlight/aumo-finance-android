package com.aumofinance.app.coa

import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import com.aumofinance.app.ui.theme.AumoTheme

class CoaActivity : ComponentActivity() {
    private val viewModel: CoaViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContent {
            var searchQuery by remember { mutableStateOf("") }
            var accountBeingAdded by remember { mutableStateOf(false) }
            var accountBeingEdited by remember { mutableStateOf<Account?>(null) }

            LaunchedEffect(searchQuery) {
                viewModel.load(search = searchQuery.takeIf { it.isNotBlank() })
            }

            LaunchedEffect(viewModel.errorMessage) {
                viewModel.errorMessage?.let {
                    Toast.makeText(this@CoaActivity, it, Toast.LENGTH_LONG).show()
                    viewModel.clearError()
                }
            }

            AumoTheme {
                CoaScreen(
                    accounts = viewModel.accounts,
                    searchQuery = searchQuery,
                    onSearchChange = { searchQuery = it },
                    onAddClick = { accountBeingAdded = true },
                    onAccountClick = { account -> accountBeingEdited = account },
                )

                if (accountBeingAdded) {
                    AddAccountDialog(
                        onDismiss = { accountBeingAdded = false },
                        onSubmit = { request ->
                            viewModel.create(request)
                            accountBeingAdded = false
                        },
                    )
                }

                accountBeingEdited?.let { account ->
                    EditAccountDialog(
                        account = account,
                        onDismiss = { accountBeingEdited = null },
                        onSubmit = { request ->
                            viewModel.update(account.id, request)
                            accountBeingEdited = null
                        },
                        onDelete = {
                            viewModel.delete(account.id)
                            accountBeingEdited = null
                        },
                    )
                }
            }
        }
    }
}
