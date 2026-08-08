using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using AumoFinance.Services;

namespace AumoFinance.Pages.Settings;

public partial class SettingsPage : ContentPage
{
    private const string KeyReminderEnabled = "reminder_enabled";
    private const string KeyReminderHour = "reminder_hour";
    private const string KeyReminderMinute = "reminder_minute";

    private readonly NotificationService _notificationService;

    public SettingsPage(NotificationService notificationService)
    {
        InitializeComponent();
        _notificationService = notificationService;
        LoadSavedSettings();
    }

    private void LoadSavedSettings()
    {
        bool isEnabled = Preferences.Default.Get(KeyReminderEnabled, true);
        int hour = Preferences.Default.Get(KeyReminderHour, 20); // Default 8:00 PM
        int minute = Preferences.Default.Get(KeyReminderMinute, 0);

        ReminderSwitch.IsToggled = isEnabled;
        ReminderTimePicker.Time = new TimeSpan(hour, minute, 0);
        TimePickerContainer.IsVisible = isEnabled;
    }

    private void OnReminderToggled(object? sender, ToggledEventArgs e)
    {
        TimePickerContainer.IsVisible = e.Value;
    }

    private void OnTimeSelected(object? sender, TimeChangedEventArgs e)
    {
        // Handled upon clicking Save
    }

    private async void OnSaveSettingsClicked(object? sender, EventArgs e)
    {
        bool isEnabled = ReminderSwitch.IsToggled;
        
        // ReminderTimePicker.Time mengembalikan TimeSpan (non-nullable)
        TimeSpan selectedTime = ReminderTimePicker.Time;

        Preferences.Default.Set(KeyReminderEnabled, isEnabled);
        Preferences.Default.Set(KeyReminderHour, selectedTime.Hours);
        Preferences.Default.Set(KeyReminderMinute, selectedTime.Minutes);

        if (isEnabled)
        {
            await _notificationService.ScheduleDailyReminderAsync(selectedTime.Hours, selectedTime.Minutes);
            await DisplayAlert("Settings Saved", $"Daily reminder set for {DateTime.Today.Add(selectedTime):hh:mm tt}.", "OK");
        }
        else
        {
            _notificationService.CancelDailyReminder();
            await DisplayAlert("Settings Saved", "Daily reminder has been disabled.", "OK");
        }
    }
}
