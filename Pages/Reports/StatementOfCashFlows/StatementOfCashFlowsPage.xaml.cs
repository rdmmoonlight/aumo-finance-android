using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using AumoFinance.Services.Reports;

namespace AumoFinance.Pages.Reports.StatementOfCashFlows;

public partial class StatementOfCashFlowsPage : ContentPage
{
    private readonly StatementOfCashFlowsService _cashFlowsService;
    private readonly CultureInfo _culture = new("id-ID");

    public StatementOfCashFlowsPage(StatementOfCashFlowsService cashFlowsService)
    {
        InitializeComponent();
        _cashFlowsService = cashFlowsService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReportAsync();
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadReportAsync();
    }

    private async void OnRefreshViewRefreshing(object? sender, EventArgs e)
    {
        await LoadReportAsync();
        CashFlowRefreshView.IsRefreshing = false;
    }

    private async Task LoadReportAsync()
    {
        SetLoadingState(true);

        var (data, errorDetail) = await _cashFlowsService.GetStatementOfCashFlowsReportAsync();

        SetLoadingState(false);

        if (errorDetail != null)
        {
            await this.DisplayAlertAsync("Error Loading Report", errorDetail, "OK");
            ShowEmptyState(true);
            return;
        }

        if (data == null || !data.Success)
        {
            SelectedPeriodHeaderLabel.Text = data?.SelectedPeriodName ?? "No Period Selected";
            ShowEmptyState(true);
            return;
        }

        SelectedPeriodHeaderLabel.Text = data.SelectedPeriodName ?? "Active Period";
        ShowEmptyState(false);

        // Render Activity Categories
        RenderActivitySection(OperatingItemsLayout, data.OperatingActivities, NetOperatingLabel, data.NetCashFromOperating);
        RenderActivitySection(InvestingItemsLayout, data.InvestingActivities, NetInvestingLabel, data.NetCashFromInvesting);
        RenderActivitySection(FinancingItemsLayout, data.FinancingActivities, NetFinancingLabel, data.NetCashFromFinancing);

        // Render Summary Reconciliation
        NetChangeCashLabel.Text = FormatAmount(data.NetChangeInCash);
        NetChangeCashLabel.TextColor = data.NetChangeInCash >= 0 ? Color.FromArgb("#166534") : Color.FromArgb("#991B1B");

        BeginningCashLabel.Text = data.BeginningCash.ToString("N0", _culture);
        EndingCashLabel.Text = data.EndingCash.ToString("N0", _culture);
    }

    private void RenderActivitySection(
        VerticalStackLayout container,
        List<CashFlowItemDto> items,
        Label netLabel,
        decimal netAmount)
    {
        container.Children.Clear();

        if (items.Count == 0)
        {
            container.Children.Add(new Label
            {
                Text = "No activities recorded",
                FontSize = 11,
                TextColor = Color.FromArgb("#94A3B8"),
                FontAttributes = FontAttributes.Italic
            });
        }
        else
        {
            foreach (var item in items)
            {
                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    }
                };

                grid.Children.Add(new Label
                {
                    Text = item.Description,
                    FontSize = 11,
                    LineBreakMode = LineBreakMode.TailTruncation
                });

                var amountLabel = new Label
                {
                    Text = FormatAmount(item.Amount),
                    FontSize = 11,
                    HorizontalTextAlignment = TextAlignment.End
                };
                amountLabel.TextColor = item.Amount >= 0 ? Color.FromArgb("#166534") : Color.FromArgb("#991B1B");
                Grid.SetColumn(amountLabel, 1);
                grid.Children.Add(amountLabel);

                container.Children.Add(grid);
            }
        }

        netLabel.Text = FormatAmount(netAmount);
        netLabel.TextColor = netAmount >= 0 ? Color.FromArgb("#166534") : Color.FromArgb("#991B1B");
    }

    private string FormatAmount(decimal amount)
    {
        if (amount == 0) return "-";
        return amount < 0 ? $"({Math.Abs(amount).ToString("N0", _culture)})" : amount.ToString("N0", _culture);
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;

        if (isLoading)
        {
            EmptyStateView.IsVisible = false;
            MainContentLayout.IsVisible = false;
        }
    }

    private void ShowEmptyState(bool show)
    {
        EmptyStateView.IsVisible = show;
        MainContentLayout.IsVisible = !show;
    }
}
