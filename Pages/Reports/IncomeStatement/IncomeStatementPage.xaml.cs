using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services.Reports;

namespace AumoFinance.Pages;

public partial class IncomeStatementPage : ContentPage
{
    private readonly IncomeStatementService _incomeStatementService;

    public IncomeStatementPage(IncomeStatementService incomeStatementService)
    {
        InitializeComponent();
        _incomeStatementService = incomeStatementService;
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
            var (response, errorDetail) = await _incomeStatementService.GetIncomeStatementReportAsync();

            if (response == null || !response.Success)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = errorDetail ?? "Failed to load income statement report.";
                return;
            }

            if (!response.HasPeriodSelected)
            {
                EmptyStateContainer.IsVisible = true;
                EmptyStateLabel.Text = "No active period selected.";
                return;
            }

            PeriodNameLabel.Text = response.SelectedPeriodName;
            AsOfDateLabel.Text = $"Statement of Profit or Loss (IAS 1)";

            var revenues = response.RevenueAccounts?
                .Select(r => new IncomeStatementLineModel { ReferenceNumber = r.ReferenceNumber.ToString(), AccountName = r.AccountName, Amount = r.Amount }).ToList() ?? new();

            var opExpenses = response.ExpenseAccounts?
                .Select(r => new IncomeStatementLineModel { ReferenceNumber = r.ReferenceNumber.ToString(), AccountName = r.AccountName, Amount = r.Amount }).ToList() ?? new();

            decimal totalRevenue = response.TotalRevenue;
            decimal totalOpExpense = response.TotalExpenses;
            decimal netIncome = response.NetIncome;

            var culture = new System.Globalization.CultureInfo("id-ID");

            RevenueCollectionView.ItemsSource = revenues;
            TotalRevenueLabel.Text = totalRevenue.ToString("N0", culture);

            OpExpenseCollectionView.ItemsSource = opExpenses;
            TotalOpExpenseLabel.Text = $"({totalOpExpense.ToString("N0", culture)})";

            OperatingIncomeLabel.Text = netIncome.ToString("N0", culture);
            OperatingIncomeLabel.TextColor = netIncome >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

            OtherSectionContainer.IsVisible = false;

            NetIncomeLabel.Text = netIncome.ToString("N0", culture);
            NetIncomeLabel.TextColor = netIncome >= 0 ? Color.FromArgb("#4ADE80") : Color.FromArgb("#F87171");

            StatementContainer.IsVisible = true;
        }
        catch (Exception ex)
        {
            await this.DisplayAlert("Error", $"Failed to load income statement: {ex.Message}", "OK");
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
