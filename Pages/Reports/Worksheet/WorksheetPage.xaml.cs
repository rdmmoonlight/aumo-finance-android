using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;

namespace AumoFinance.Pages;

public partial class WorksheetPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public WorksheetPage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWorksheetAsync();
    }

    private async Task LoadWorksheetAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        WorksheetContainer.IsVisible = false;
        EmptyStateContainer.IsVisible = false;
        NetIncomeInfoCard.IsVisible = false;

        try
        {
            var period = await _accountingService.GetCurrentPeriodAsync(_currentUserId);
            if (period == null)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "Belum ada periode aktif yang dipilih.";
                return;
            }

            PeriodNameLabel.Text = period.PeriodName;

            // Ambil data Trial Balance unadjusted & adjusted via AccountingService
            var unadjustedRows = await _accountingService.GetTrialBalanceAsync(_currentUserId, period, includeAdjusting: false);
            var adjustedRows = await _accountingService.GetTrialBalanceAsync(_currentUserId, period, includeAdjusting: true);

            var accounts = await _accountingService.GetGeneralLedgerAsync(_currentUserId, period, isTemporary: false); // atau ambil dari Chart of Accounts
            
            // Generate rows worksheet 10 kolom
            var allAccountIds = unadjustedRows.Select(r => r.AccountId)
                .Union(adjustedRows.Select(r => r.AccountId))
                .Distinct()
                .ToList();

            if (!allAccountIds.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada data worksheet pada periode {period.PeriodName}.";
                return;
            }

            var culture = new System.Globalization.CultureInfo("id-ID");
            var worksheetRows = new System.Collections.Generic.List<WorksheetRowDisplayModel>();

            decimal totUnadjDr = 0, totUnadjCr = 0;
            decimal totAdjDr = 0, totAdjCr = 0;
            decimal totAdjTbDr = 0, totAdjTbCr = 0;
            decimal totIncDr = 0, totIncCr = 0;
            decimal totBsDr = 0, totBsCr = 0;

            foreach (var accId in allAccountIds)
            {
                var u = unadjustedRows.FirstOrDefault(r => r.AccountId == accId);
                var a = adjustedRows.FirstOrDefault(r => r.AccountId == accId);

                var refNum = u?.ReferenceNumber ?? a?.ReferenceNumber ?? "-";
                var name = u?.AccountName ?? a?.AccountName ?? "-";
                var type = u?.Type ?? a?.Type ?? "Asset";

                decimal uDr = u?.Debit ?? 0;
                decimal uCr = u?.Credit ?? 0;
                decimal aDr = a?.Debit ?? 0;
                decimal aCr = a?.Credit ?? 0;

                decimal adjNet = (aDr - aCr) - (uDr - uCr);
                decimal adjDr = adjNet > 0 ? adjNet : 0;
                decimal adjCr = adjNet < 0 ? -adjNet : 0;

                bool isTemporary = AccountClassification.IsTemporary(type);

                var row = new WorksheetRowDisplayModel
                {
                    ReferenceNumber = refNum,
                    AccountName = name,
                    UnadjustedDebit = uDr,
                    UnadjustedCredit = uCr,
                    AdjustmentDebit = adjDr,
                    AdjustmentCredit = adjCr,
                    AdjustedDebit = aDr,
                    AdjustedCredit = aCr,
                    IncomeStatementDebit = isTemporary ? aDr : 0,
                    IncomeStatementCredit = isTemporary ? aCr : 0,
                    FinancialPositionDebit = !isTemporary ? aDr : 0,
                    FinancialPositionCredit = !isTemporary ? aCr : 0
                };

                totUnadjDr += uDr;
                totUnadjCr += uCr;
                totAdjDr += adjDr;
                totAdjCr += adjCr;
                totAdjTbDr += aDr;
                totAdjTbCr += aCr;
                totIncDr += row.IncomeStatementDebit;
                totIncCr += row.IncomeStatementCredit;
                totBsDr += row.FinancialPositionDebit;
                totBsCr += row.FinancialPositionCredit;

                worksheetRows.Add(row);
            }

            // Hitung Net Income
            decimal netIncome = totIncCr - totIncDr;
            NetIncomeLabel.Text = $"Rp {netIncome.ToString("N0", culture)}";
            NetIncomeLabel.TextColor = netIncome >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

            // Update Total Footer Label
            TotUnadjDr.Text = totUnadjDr.ToString("N0", culture);
            TotUnadjCr.Text = totUnadjCr.ToString("N0", culture);
            TotAdjDr.Text = totAdjDr.ToString("N0", culture);
            TotAdjCr.Text = totAdjCr.ToString("N0", culture);
            TotAdjTbDr.Text = totAdjTbDr.ToString("N0", culture);
            TotAdjTbCr.Text = totAdjTbCr.ToString("N0", culture);
            TotIncDr.Text = totIncDr.ToString("N0", culture);
            TotIncCr.Text = totIncCr.ToString("N0", culture);
            TotBsDr.Text = totBsDr.ToString("N0", culture);
            TotBsCr.Text = totBsCr.ToString("N0", culture);

            WorksheetCollectionView.ItemsSource = worksheetRows;
            WorksheetContainer.IsVisible = true;
            NetIncomeInfoCard.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Gagal memuat worksheet: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}

public class WorksheetRowDisplayModel
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal UnadjustedDebit { get; set; }
    public decimal UnadjustedCredit { get; set; }
    public decimal AdjustmentDebit { get; set; }
    public decimal AdjustmentCredit { get; set; }
    public decimal AdjustedDebit { get; set; }
    public decimal AdjustedCredit { get; set; }
    public decimal IncomeStatementDebit { get; set; }
    public decimal IncomeStatementCredit { get; set; }
    public decimal FinancialPositionDebit { get; set; }
    public decimal FinancialPositionCredit { get; set; }

    private static readonly System.Globalization.CultureInfo Idr = new("id-ID");

    public string FormattedUnadjustedDebit => UnadjustedDebit > 0 ? UnadjustedDebit.ToString("N0", Idr) : "-";
    public string FormattedUnadjustedCredit => UnadjustedCredit > 0 ? UnadjustedCredit.ToString("N0", Idr) : "-";
    public string FormattedAdjustmentDebit => AdjustmentDebit > 0 ? AdjustmentDebit.ToString("N0", Idr) : "-";
    public string FormattedAdjustmentCredit => AdjustmentCredit > 0 ? AdjustmentCredit.ToString("N0", Idr) : "-";
    public string FormattedAdjustedDebit => AdjustedDebit > 0 ? AdjustedDebit.ToString("N0", Idr) : "-";
    public string FormattedAdjustedCredit => AdjustedCredit > 0 ? AdjustedCredit.ToString("N0", Idr) : "-";
    public string FormattedIncomeStatementDebit => IncomeStatementDebit > 0 ? IncomeStatementDebit.ToString("N0", Idr) : "-";
    public string FormattedIncomeStatementCredit => IncomeStatementCredit > 0 ? IncomeStatementCredit.ToString("N0", Idr) : "-";
    public string FormattedFinancialPositionDebit => FinancialPositionDebit > 0 ? FinancialPositionDebit.ToString("N0", Idr) : "-";
    public string FormattedFinancialPositionCredit => FinancialPositionCredit > 0 ? FinancialPositionCredit.ToString("N0", Idr) : "-";
}
