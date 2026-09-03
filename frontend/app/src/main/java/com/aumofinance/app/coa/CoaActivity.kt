package com.aumofinance.app.coa

import android.app.AlertDialog
import android.os.Bundle
import android.text.Editable
import android.text.TextWatcher
import android.widget.ArrayAdapter
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.Spinner
import android.widget.Switch
import android.widget.Toast
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.aumofinance.app.R

class CoaActivity : AppCompatActivity() {
    private val viewModel: CoaViewModel by viewModels()
    private lateinit var adapter: CoaAdapter

    // According to the reference number ranges in AccountClassification.cs in aumo-finance-web.
    private val accountTypes = listOf(
        "Assets", "Liabilities", "Equity",
        "OperatingIncome", "OperatingExpenses", "OtherIncome", "OtherExpenses"
    )

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_coa)

        adapter = CoaAdapter(emptyList()) { account -> showEditDialog(account) }
        findViewById<RecyclerView>(R.id.recyclerAccounts).apply {
            layoutManager = LinearLayoutManager(this@CoaActivity)
            adapter = this@CoaActivity.adapter
        }

        findViewById<EditText>(R.id.inputSearch).addTextChangedListener(object : TextWatcher {
            override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) = Unit
            override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) = Unit
            override fun afterTextChanged(s: Editable?) {
                viewModel.load(search = s?.toString()?.takeIf { it.isNotBlank() })
            }
        })

        findViewById<android.widget.Button>(R.id.buttonAddAccount).setOnClickListener {
            showCreateDialog()
        }

        viewModel.accounts.observe(this) { accounts -> adapter.submitList(accounts) }
        viewModel.errorMessage.observe(this) { message ->
            message?.let { Toast.makeText(this, it, Toast.LENGTH_LONG).show() }
        }
        viewModel.load()
    }

    private fun showCreateDialog() {
        val (container, refInput, nameInput, typeSpinner) = buildForm(null)

        AlertDialog.Builder(this)
            .setTitle("Add Account")
            .setView(container)
            .setPositiveButton("Save") { _, _ ->
                val refNumber = refInput.text.toString().toIntOrNull() ?: return@setPositiveButton
                viewModel.create(
                    AccountRequest(
                        referenceNumber = refNumber,
                        accountName = nameInput.text.toString(),
                        type = accountTypes[typeSpinner.selectedItemPosition]
                    )
                )
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private fun showEditDialog(account: Account) {
        val (container, refInput, nameInput, typeSpinner) = buildForm(account)
        val activeSwitch = Switch(this).apply {
            text = "Active"
            isChecked = account.isActive
        }
        container.addView(activeSwitch)

        AlertDialog.Builder(this)
            .setTitle("Edit Account")
            .setView(container)
            .setPositiveButton("Save") { _, _ ->
                val refNumber = refInput.text.toString().toIntOrNull() ?: return@setPositiveButton
                viewModel.update(
                    account.id,
                    UpdateAccountRequest(
                        referenceNumber = refNumber,
                        accountName = nameInput.text.toString(),
                        type = accountTypes[typeSpinner.selectedItemPosition],
                        isActive = activeSwitch.isChecked
                    )
                )
            }
            // Backend will reject (400) if the account already has journal entries —
            // the message (prompting to set to Inactive via the toggle above)
            // will automatically appear via viewModel.errorMessage.
            .setNeutralButton("Delete") { _, _ -> viewModel.delete(account.id) }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private data class FormViews(val container: LinearLayout, val refInput: EditText, val nameInput: EditText, val typeSpinner: Spinner)

    private fun buildForm(existing: Account?): FormViews {
        val container = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(48, 24, 48, 0)
        }
        val refInput = EditText(this).apply {
            hint = "Reference Number (e.g., 101)"
            inputType = android.text.InputType.TYPE_CLASS_NUMBER
            existing?.let { setText(it.referenceNumber.toString()) }
        }
        val nameInput = EditText(this).apply {
            hint = "Account Name"
            existing?.let { setText(it.accountName) }
        }
        val typeSpinner = Spinner(this).apply {
            adapter = ArrayAdapter(this@CoaActivity, android.R.layout.simple_spinner_dropdown_item, accountTypes)
            existing?.let { setSelection(accountTypes.indexOf(it.type).coerceAtLeast(0)) }
        }
        container.addView(refInput)
        container.addView(nameInput)
        container.addView(typeSpinner)
        return FormViews(container, refInput, nameInput, typeSpinner)
    }
}
