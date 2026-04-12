using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace MyMauiApp.Services;

public class NotificationService
{
    private const int FastCompleteId = 1000;
    private const int FastHalfwayId = 1001;
    private const int WaterReminderId = 2000;

    public bool NotificationsEnabled
    {
        get => Preferences.Get("notifications_enabled", true);
        set => Preferences.Set("notifications_enabled", value);
    }

    public bool WaterRemindersEnabled
    {
        get => Preferences.Get("water_reminders_enabled", true);
        set => Preferences.Set("water_reminders_enabled", value);
    }

    public async Task<bool> RequestPermissionAsync()
    {
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled())
            return true;

        return await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    public async Task ScheduleFastingNotifications(DateTime startTime, int targetHours, string planName)
    {
        if (!NotificationsEnabled)
            return;

        CancelFastingNotifications();

        var endTime = startTime.AddHours(targetHours);

        // Fast complete notification
        if (endTime > DateTime.Now)
        {
            var complete = new NotificationRequest
            {
                NotificationId = FastCompleteId,
                Title = "🎉 Fast Complete!",
                Description = $"Your {planName} fast ({targetHours}h) is done! Great job!",
                Schedule = { NotifyTime = endTime }
            };
            await LocalNotificationCenter.Current.Show(complete);
        }

        // Halfway notification
        var halfwayTime = startTime.AddHours(targetHours / 2.0);
        if (halfwayTime > DateTime.Now && targetHours >= 4)
        {
            var halfway = new NotificationRequest
            {
                NotificationId = FastHalfwayId,
                Title = "⏳ Halfway There!",
                Description = $"You're halfway through your {planName} fast. Keep it up!",
                Schedule = { NotifyTime = halfwayTime }
            };
            await LocalNotificationCenter.Current.Show(halfway);
        }
    }

    public void CancelFastingNotifications()
    {
        LocalNotificationCenter.Current.Cancel(FastCompleteId);
        LocalNotificationCenter.Current.Cancel(FastHalfwayId);
    }

    public async Task ScheduleWaterReminder(int minutesFromNow = 120)
    {
        if (!NotificationsEnabled || !WaterRemindersEnabled)
            return;

        CancelWaterReminder();

        var notifyAt = DateTime.Now.AddMinutes(minutesFromNow);

        // Only send between 8 AM and 10 PM
        if (notifyAt.Hour < 8 || notifyAt.Hour >= 22)
            return;

        var reminder = new NotificationRequest
        {
            NotificationId = WaterReminderId,
            Title = "💧 Stay Hydrated!",
            Description = "Don't forget to log your water intake.",
            Schedule = { NotifyTime = notifyAt }
        };
        await LocalNotificationCenter.Current.Show(reminder);
    }

    public void CancelWaterReminder()
    {
        LocalNotificationCenter.Current.Cancel(WaterReminderId);
    }

    public void CancelAll()
    {
        LocalNotificationCenter.Current.CancelAll();
    }
}
