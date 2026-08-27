using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace AumoFinance.Pages.Log;

public partial class CrashLogPage : ContentPage
{
    public CrashLogPage(string logContent)
    {
        InitializeComponent();
        LogEditor.Text = logContent;
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(LogEditor.Text))
        {
            await Clipboard.Default.SetTextAsync(LogEditor.Text);
            await DisplayAlertAsync("Sukses", "Seluruh teks log berhasil disalin ke clipboard!", "OK");
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
