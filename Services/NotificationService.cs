using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AumoFinance.Platforms.Android;
using Microsoft.Maui.ApplicationModel;

namespace AumoFinance.Services;

/// <summary>
/// Runtime permission for posting notifications (required from Android 13 / API 33+).
/// </summary>
public class PostNotificationsPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(33)
            ? new (string androidPermission, bool isRuntime)[]
              {
                  (Android.Manifest.Permission.PostNotifications, true)
              }
            : Array.Empty<(string androidPermission, bool isRuntime)>();
}

/// <summary>
/// Native Android implementation of the daily reminder notification.
/// Replaces the previous Plugin.LocalNotification-based implementation, which
/// could not be reliably resolved for this project's build configuration.
/// Uses AlarmManager + a BroadcastReceiver (see
/// Platforms/Android/ReminderBroadcastReceiver.cs) so the reminder still fires
/// even if the app itself isn't running.
/// Minimum supported OS: Android 9 (API 28).
/// </summary>
public class NotificationService
{
    public const string ChannelId = "aumo_daily_reminder";
    internal const int ReminderRequestCode = 2001;

    public async Task<bool> RequestPermissionAsync()
    {
        // POST_NOTIFICATIONS only exists/is enforced from Android 13 (API 33) onward.
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            return true;
        }

        var status = await Permissions.CheckStatusAsync<PostNotificationsPermission>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<PostNotificationsPermission>();
        }

        return status == PermissionStatus.Granted;
    }

    public Task ScheduleDailyReminderAsync(int hour = 20, int minute = 0)
    {
        EnsureChannel();

        var context = Android.App.Application.Context;

        var now = Java.Util.Calendar.Instance!;
        var trigger = Java.Util.Calendar.Instance!;
        trigger.Set(Java.Util.CalendarField.HourOfDay, hour);
        trigger.Set(Java.Util.CalendarField.Minute, minute);
        trigger.Set(Java.Util.CalendarField.Second, 0);
        trigger.Set(Java.Util.CalendarField.Millisecond, 0);

        if (trigger.TimeInMillis <= now.TimeInMillis)
        {
            trigger.Add(Java.Util.CalendarField.DayOfMonth, 1);
        }

        var pendingIntent = BuildReminderPendingIntent(context);
        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService)!;

        if (OperatingSystem.IsAndroidVersionAtLeast(31) && !alarmManager.CanScheduleExactAlarms())
        {
            // User hasn't granted exact-alarm access (Settings > Alarms & reminders).
            // Fall back to an inexact alarm so the reminder still fires close to the chosen time.
            alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, trigger.TimeInMillis, pendingIntent);
        }
        else
        {
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, trigger.TimeInMillis, pendingIntent);
        }

        return Task.CompletedTask;
    }

    public void CancelDailyReminder()
    {
        var context = Android.App.Application.Context;
        var pendingIntent = BuildReminderPendingIntent(context);

        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService)!;
        alarmManager.Cancel(pendingIntent);
        pendingIntent.Cancel();
    }

    internal static PendingIntent BuildReminderPendingIntent(Context context)
    {
        var intent = new Intent(context, typeof(ReminderBroadcastReceiver));
        intent.SetAction(ReminderBroadcastReceiver.ActionShowReminder);

        return PendingIntent.GetBroadcast(
            context,
            ReminderRequestCode,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    internal static void EnsureChannel()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            return;
        }

        var context = Android.App.Application.Context;
        var channel = new NotificationChannel(
            ChannelId,
            "Pengingat Harian",
            NotificationImportance.Default)
        {
            Description = "Pengingat harian untuk mencatat transaksi keuangan AumoFinance"
        };

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        manager.CreateNotificationChannel(channel);
    }
}
