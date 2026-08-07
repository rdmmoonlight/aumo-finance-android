using Android.App;
using Android.Content;
using AndroidX.Core.App;
using AumoFinance;
using AumoFinance.Services;
using Microsoft.Maui.Storage;

namespace AumoFinance.Platforms.Android;

/// <summary>
/// Fires the daily reminder notification when triggered by AlarmManager
/// (see NotificationService.ScheduleDailyReminderAsync), then reschedules
/// itself for the same time the next day.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false, Label = "AumoFinance Daily Reminder")]
public class ReminderBroadcastReceiver : BroadcastReceiver
{
    public const string ActionShowReminder = "com.bnrc.aumofinance.action.SHOW_DAILY_REMINDER";

    private const int NotificationId = 1001;
    private const string KeyReminderHour = "reminder_hour";
    private const string KeyReminderMinute = "reminder_minute";
    private const string KeyReminderEnabled = "reminder_enabled";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
        {
            return;
        }

        ShowNotification(context);

        // Only reschedule if the user still has the reminder switched on.
        if (Preferences.Default.Get(KeyReminderEnabled, true))
        {
            int hour = Preferences.Default.Get(KeyReminderHour, 20);
            int minute = Preferences.Default.Get(KeyReminderMinute, 0);

            var service = new NotificationService();
            _ = service.ScheduleDailyReminderAsync(hour, minute);
        }
    }

    private static void ShowNotification(Context context)
    {
        NotificationService.EnsureChannel();

        var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName!)
            ?? new Intent(context, typeof(ReminderBroadcastReceiver));
        launchIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);

        var contentIntent = PendingIntent.GetActivity(
            context,
            NotificationId,
            launchIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;

        var builder = new NotificationCompat.Builder(context, NotificationService.ChannelId)
            .SetContentTitle("Sudah Catat Keuangan Hari Ini? 💎")
            .SetContentText("Jangan lupa rapikan dan catat transaksi pengeluaran/pemasukan AumoFinance kamu ya!")
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetAutoCancel(true)
            .SetContentIntent(contentIntent)
            .SetPriority(NotificationCompat.PriorityDefault);

        var notificationManager = NotificationManagerCompat.From(context);

        try
        {
            notificationManager.Notify(NotificationId, builder.Build());
        }
        catch (Java.Lang.SecurityException)
        {
            // POST_NOTIFICATIONS not granted; nothing to do until the user enables it.
        }
    }
}
