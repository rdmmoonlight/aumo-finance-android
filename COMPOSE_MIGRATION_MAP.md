# Compose Migration Map — aumo-finance-android

Status per page as of this audit. "Compose" = hosts a `@Composable` screen via
`setContent { ... }`. "XML" = still uses `setContentView(R.layout...)`.

## Sudah Compose (7)
| Screen | Activity host |
|---|---|
| HomeScreen | HomeActivity |
| JournalEntryScreen | JournalEntryActivity |
| PeriodsScreen | PeriodsActivity |
| JournalReportScreen | GeneralJournalReportActivity, AdjustingJournalReportActivity |
| ReportsMenuScreen | ReportsMenuActivity |

## Belum Compose — masih XML (17)
| Activity | Layout XML |
|---|---|
| LoginActivity | activity_login.xml |
| CoaActivity | activity_coa.xml |
| CrashLogActivity | activity_crash_log.xml |
| DashboardActivity | activity_dashboard.xml |
| CashFlowActivity | activity_cash_flow.xml |
| ClosingJournalActivity | activity_closing_journal.xml |
| FinancialPositionActivity | activity_financial_position.xml |
| IncomeStatementActivity | activity_income_statement.xml |
| RetainedEarningsActivity | activity_retained_earnings.xml |
| GeneralLedgerPermanentActivity | activity_general_ledger.xml |
| GeneralLedgerTemporaryActivity | activity_general_ledger.xml |
| AdjustedTrialBalanceActivity | activity_trial_balance.xml |
| PostClosingTrialBalanceActivity | activity_trial_balance.xml |
| TrialBalanceActivity | activity_trial_balance.xml |
| WorksheetActivity | activity_worksheet.xml |
| LogoutActivity | activity_logout.xml |
| SettingsActivity | activity_settings.xml |
| SplashActivity | activity_splash.xml |

Supporting XML item layouts still in use by the above (RecyclerView adapters):
`item_account.xml`, `item_cash_account.xml`, `item_ledger_account.xml`,
`item_ledger_line.xml`, `item_menu.xml`, `item_trial_balance_row.xml`.

## Ringkasan
- Total halaman: 24
- Compose: 7 (29%)
- Masih XML/View: 17 (71%)

Prioritas migrasi yang disarankan (paling sering dipakai / paling sederhana dulu):
1. Dashboard, Login, Settings, Logout, Splash — halaman inti, struktur sederhana.
2. Laporan keuangan (Financial Position, Income Statement, Cash Flow, Retained
   Earnings, Closing Journal) — pola mirip satu sama lain, bisa dikerjakan
   sekaligus sebagai satu batch.
3. General Ledger, Trial Balance (3 varian), Worksheet — lebih kompleks
   (tabel + RecyclerView adapter), kerjakan terakhir.
