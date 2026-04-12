using System.Text.Json;
using MyMauiApp.Models;

namespace MyMauiApp.Services;

public class FastingService
{
    private const string SessionsKey = "fasting_sessions";
    private const string ActiveSessionKey = "fasting_active";
    private List<FastingSession> _sessions = [];

    public static List<FastingPlan> AvailablePlans =>
    [
        new() { Name = "16:8", FastingHours = 16, EatingHours = 8, Description = "Most popular — 16h fast, 8h eating window" },
        new() { Name = "18:6", FastingHours = 18, EatingHours = 6, Description = "Extended — 18h fast, 6h eating window" },
        new() { Name = "20:4", FastingHours = 20, EatingHours = 4, Description = "Warrior — 20h fast, 4h eating window" },
        new() { Name = "OMAD", FastingHours = 23, EatingHours = 1, Description = "One Meal A Day — 23h fast" },
        new() { Name = "14:10", FastingHours = 14, EatingHours = 10, Description = "Beginner — 14h fast, 10h eating window" },
        new() { Name = "Custom", FastingHours = 0, EatingHours = 0, Description = "Set your own fasting and eating hours" },
    ];

    public FastingService()
    {
        Load();
    }

    public FastingSession? ActiveSession => _sessions.FirstOrDefault(s => !s.IsCompleted);

    public bool IsFasting => ActiveSession != null;

    public List<FastingSession> CompletedSessions => _sessions.Where(s => s.IsCompleted).OrderByDescending(s => s.StartTime).ToList();

    public int CurrentStreak
    {
        get
        {
            var completed = CompletedSessions;
            if (completed.Count == 0) return 0;
            int streak = 0;
            var date = DateTime.Today;
            foreach (var session in completed)
            {
                if (session.StartTime.Date == date || session.StartTime.Date == date.AddDays(-1))
                {
                    streak++;
                    date = session.StartTime.Date;
                }
                else break;
            }
            return Math.Max(streak, 1);
        }
    }

    public FastingSession StartFast(FastingPlan plan)
    {
        if (IsFasting) throw new InvalidOperationException("A fast is already active.");
        var session = new FastingSession
        {
            StartTime = DateTime.Now,
            TargetHours = plan.FastingHours,
            PlanName = plan.Name
        };
        _sessions.Add(session);
        Save();
        return session;
    }

    public FastingSession? StopFast()
    {
        var active = ActiveSession;
        if (active == null) return null;
        active.EndTime = DateTime.Now;
        Save();
        return active;
    }

    public List<FastingSession> GetHistory() => CompletedSessions;

    private void Save()
    {
        var json = JsonSerializer.Serialize(_sessions);
        Preferences.Set(SessionsKey, json);
    }

    private void Load()
    {
        var json = Preferences.Get(SessionsKey, "[]");
        _sessions = JsonSerializer.Deserialize<List<FastingSession>>(json) ?? [];
    }
}
