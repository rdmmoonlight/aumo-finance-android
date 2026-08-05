using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services;
using AumoFinance.Models;

namespace AumoFinance.Pages;

public partial class RetainedEarningsPage : ContentPage
{
    private readonly AccountingService _accountingService;
    private readonly Guid _currentUserId;

    public RetainedEarningsPage(AccountingService accountingService, Guid currentUserId)
    {
        InitializeComponent();
        _accountingService = accountingService;
        _currentUserId = currentUserId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRetainedEarningsAsync();
    }

    private async Task LoadRetainedEarningsAsync()
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

            // Ambil Trial Balance dengan penyesuaian
            var rows = await _accountingService.GetTrialBalanceAsync(_currentUserId, period, includeAdjusting: true);

            // Hitung Income Statement untuk Net Income
            var incomeStatement = IncomeStatementPageViewModelHelper(rows, period);
            var reAccount = rows.FirstOrDefault(r => r.Type.Equals("RetainedEarnings", StringComparison.OrdinalIgnoreCase) || r.Role?.Equals("RetainedEarnings", StringComparison.OrdinalIgnoreCase) == true);

            decimal beginningBalance = reAccount?.NetBalance ?? 0;
            decimal netIncome = incomeStatement.NetIncome;
            decimal dividends = 0; // Dapat disesuaikan jika ada akun Dividen/Prive
            decimal endingBalance = beginningBalance + netIncome - dividends;

            var culture = new System.Globalization.CultureInfo("id-ID");

            // Update UI Bindings
            AccountNameTitleLabel.Text = reAccount?.AccountName ?? "Retained Earnings";
            BeginningLabel.Text = $"Retained earnings, {period.StartDate:MMMM d}";
            BeginningBalanceLabel.Text = beginningBalance.ToString("N0", culture);

            NetIncomeLabel.Text = netIncome.ToString("N0", culture);
            NetIncomeLabel.TextColor = netIncome >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

            if (dividends != 0)
            {
                DividendsLabel.Text = $"({dividends.ToString("N0", culture)})";
                DividendsRowContainer.IsVisible = true;
            }
            else
            {
                DividendsRowContainer.IsVisible = false;
            }

            EndingLabel.Text = $"Retained earnings, {period.EndDate:MMMM d}";
            EndingBalanceLabel.Text = endingBalance.ToString("N0", culture);
            EndingBalanceLabel.TextColor = endingBalance >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

            StatementContainer.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Gagal memuat Retained Earnings: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private IncomeStatementModelHelper IncomeStatementPageViewModelHelper(List<TrialBalanceRowViewModel> rows, Period period)
    {
        decimal totalRevenue = rows.Where(r => r.Type.Equals("OperatingIncome", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Revenue", StringComparison.OrdinalIgnoreCase)).Sum(r => r.NetBalance);
        decimal totalExpense = rows.Where(r => r.Type.Equals("OperatingExpenses", StringComparison.OrdinalIgnoreCase) || r.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase)).Sum(r => Math.Abs(r.NetBalance));
        decimal operatingIncome = totalRevenue - totalExpense;

        decimal otherInc = rows.Where(r => r.Type.Equals("OtherIncome", StringComparison.OrdinalIgnoreCase)).Sum(r => r.NetBalance);
        decimal otherExp = rows.Where(r => r.Type.Equals("OtherExpenses", StringComparison.OrdinalIgnoreCase)).Sum(r => Math.Abs(r.NetBalance));
        decimal netIncome = operatingIncome + otherInc - otherExp;

        return new IncomeStatementModelHelper { NetIncome = netIncome };
    }
}

public class IncomeStatementModelHelper
{
    public decimal NetIncome { get; set; }
}
