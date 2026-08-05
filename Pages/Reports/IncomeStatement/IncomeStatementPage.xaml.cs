using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class IncomeStatementPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public IncomeStatementPage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadIncomeStatementAsync();
    }

    private async Task LoadIncomeStatementAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        StatementContainer.IsVisible = false;
        EmptyStateContainer.IsVisible = false;

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
            AsOfDateLabel.Text = $"Statement of Profit or Loss (IAS 1) — per {period.EndDate:dd MMMM yyyy}";

            // Ambil Trial Balance dengan penyesuaian (includeAdjusting: true)
            var trialBalanceRows = await _accountingService.GetTrialBalanceAsync(_currentUserId, period, includeAdjusting: true);

            if (!trialBalanceRows.Any())
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = $"Tidak ada data laporan pada periode {period.PeriodName}.";
                return;
            }

            // Mapping data ke baris laporan
            var revenues = trialBalanceRows.Where(r => r.Type.Equals("OperatingIncome", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Revenue", StringComparison.OrdinalIgnoreCase))
                .Select(r => new IncomeStatementLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = r.NetBalance }).ToList();

            var opExpenses = trialBalanceRows.Where(r => r.Type.Equals("OperatingExpenses", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase))
                .Select(r => new IncomeStatementLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = Math.Abs(r.NetBalance) }).ToList();

            var otherIncome = trialBalanceRows.Where(r => r.Type.Equals("OtherIncome", StringComparison.OrdinalIgnoreCase))
                .Select(r => new IncomeStatementLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = r.NetBalance }).ToList();

            var otherExpenses = trialBalanceRows.Where(r => r.Type.Equals("OtherExpenses", StringComparison.OrdinalIgnoreCase))
                .Select(r => new IncomeStatementLineModel { ReferenceNumber = r.ReferenceNumber, AccountName = r.AccountName, Amount = Math.Abs(r.NetBalance) }).ToList();

            decimal totalRevenue = revenues.Sum(r => r.Amount);
            decimal totalOpExpense = opExpenses.Sum(r => r.Amount);
            decimal operatingIncome = totalRevenue - totalOpExpense;

            decimal totalOtherInc = otherIncome.Sum(r => r.Amount);
            decimal totalOtherExp = otherExpenses.Sum(r => r.Amount);
            decimal netIncome = operatingIncome + totalOtherInc - totalOtherExp;

            var culture = new System.Globalization.CultureInfo("id-ID");

            // Update UI Bindings
            RevenueCollectionView.ItemsSource = revenues;
            TotalRevenueLabel.Text = totalRevenue.ToString("N0", culture);

            OpExpenseCollectionView.ItemsSource = opExpenses;
            TotalOpExpenseLabel.Text = $"({totalOpExpense.ToString("N0", culture)})";

            OperatingIncomeLabel.Text = operatingIncome.ToString("N0", culture);
            OperatingIncomeLabel.TextColor = operatingIncome >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

            // Other section
            if (otherIncome.Any() || otherExpenses.Any())
            {
                var combinedOther = otherIncome.Concat(otherExpenses).ToList();
                OtherCollectionView.ItemsSource = combinedOther;
                OtherSectionContainer.IsVisible = true;
            }
            else
            {
                OtherSectionContainer.IsVisible = false;
            }

            NetIncomeLabel.Text = netIncome.ToString("N0", culture);
            NetIncomeLabel.TextColor = netIncome >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

            StatementContainer.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Gagal memuat laporan laba rugi: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}

public class IncomeStatementLineModel
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    private static readonly System.Globalization.CultureInfo Idr = new("id-ID");

    public string FormattedAmount => Amount.ToString("N0", Idr);
    public string FormattedAmountBracket => $"({Amount.ToString("N0", Idr)})";
}
