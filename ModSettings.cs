using MelonLoader;

namespace SimpleCall;

public static class ModSettings
{
    public static MelonPreferences_Category BasicCategory { get; private set; }
    public static MelonPreferences_Category AdvancedCategory { get; private set; }
    public static MelonPreferences_Category DebugCategory { get; private set; }

    public static MelonPreferences_Entry<int> MeetDelay { get; private set; }
    public static MelonPreferences_Entry<bool> DealerRunsToPlayer { get; private set; }
    public static MelonPreferences_Entry<bool> ShowNoSignalNotification { get; private set; }
    public static MelonPreferences_Entry<bool> DealerMessages { get; private set; }

    public static MelonPreferences_Entry<int> MaxFallbackAttempts { get; private set; }
    public static MelonPreferences_Entry<float> RepathInterval { get; private set; }
    public static MelonPreferences_Entry<int> MaxWaitAtDoor { get; private set; }
    public static MelonPreferences_Entry<int> GiveUpRadius { get; private set; }

    public static MelonPreferences_Entry<bool> EnableLogging { get; private set; }

    private const int MEET_DELAY_MIN = 1;
    private const int MEET_DELAY_MAX = 120;
    private const int MAX_FALLBACK_ATTEMPTS_MIN = 1;
    private const int MAX_FALLBACK_ATTEMPTS_MAX = 20;
    private const float REPATH_INTERVAL_MIN = 0.25f;
    private const float REPATH_INTERVAL_MAX = 10f;
    private const int MAX_WAIT_AT_DOOR_MIN = 30;
    private const int MAX_WAIT_AT_DOOR_MAX = 600;
    private const int GIVE_UP_RADIUS_MIN = 20;
    private const int GIVE_UP_RADIUS_MAX = 200;

    public static void Initialize()
    {
        CreateBasicSettings();
        CreateAdvancedSettings();
        CreateDebugSettings();
    }

    private static void CreateBasicSettings()
    {
        BasicCategory = MelonPreferences.CreateCategory("SimpleCall_01_Basic", "Basic");

        MeetDelay = BasicCategory.CreateEntry("MeetDelay", 10, "Meet Delay (1-120s)");
        MeetDelay.OnEntryValueChanged.Subscribe(OnMeetDelayChanged);
        DealerRunsToPlayer = BasicCategory.CreateEntry("DealerRunsToPlayer", true, "Allow Dealer Run");
        ShowNoSignalNotification = BasicCategory.CreateEntry("ShowNoSignalNotification", true, "No Signal Notification");
        DealerMessages = BasicCategory.CreateEntry("DealerMessages", true, "Dealer Messages");
    }

    private static void CreateAdvancedSettings()
    {
        AdvancedCategory = MelonPreferences.CreateCategory("SimpleCall_02_Advanced", "Advanced");

        MaxFallbackAttempts = AdvancedCategory.CreateEntry("MaxFallbackAttempts", 8, "Max Fallback Attempts (1-20)");
        MaxFallbackAttempts.OnEntryValueChanged.Subscribe(OnMaxFallbackAttemptsChanged);
        RepathInterval = AdvancedCategory.CreateEntry("RepathInterval", 2.5f, "Repath Interval (0.25-10s)");
        RepathInterval.OnEntryValueChanged.Subscribe(OnRepathIntervalChanged);
        MaxWaitAtDoor = AdvancedCategory.CreateEntry("MaxWaitAtDoor", 120, "Max Wait At Door (30-600s)");
        MaxWaitAtDoor.OnEntryValueChanged.Subscribe(OnMaxWaitAtDoorChanged);
        GiveUpRadius = AdvancedCategory.CreateEntry("GiveUpRadius", 60, "Give Up Radius (20-200)");
        GiveUpRadius.OnEntryValueChanged.Subscribe(OnGiveUpRadiusChanged);
    }

    private static void CreateDebugSettings()
    {
        DebugCategory = MelonPreferences.CreateCategory("SimpleCall_03_Debug", "Debug");

        EnableLogging = DebugCategory.CreateEntry("EnableLogging", false, "Debug Logging");
    }

    public static float GetMeetDelay()
    {
        var v = MeetDelay.Value;
        if (v < MEET_DELAY_MIN) return MEET_DELAY_MIN;
        if (v > MEET_DELAY_MAX) return MEET_DELAY_MAX;
        return (float)v;
    }

    public static int GetMaxFallbackAttempts()
    {
        var v = MaxFallbackAttempts.Value;
        if (v < MAX_FALLBACK_ATTEMPTS_MIN) return MAX_FALLBACK_ATTEMPTS_MIN;
        if (v > MAX_FALLBACK_ATTEMPTS_MAX) return MAX_FALLBACK_ATTEMPTS_MAX;
        return v;
    }

    public static float GetRepathInterval()
    {
        var v = RepathInterval.Value;
        if (v < REPATH_INTERVAL_MIN) return REPATH_INTERVAL_MIN;
        if (v > REPATH_INTERVAL_MAX) return REPATH_INTERVAL_MAX;
        return v;
    }

    public static float GetMaxWaitAtDoor()
    {
        var v = MaxWaitAtDoor.Value;
        if (v < MAX_WAIT_AT_DOOR_MIN) return MAX_WAIT_AT_DOOR_MIN;
        if (v > MAX_WAIT_AT_DOOR_MAX) return MAX_WAIT_AT_DOOR_MAX;
        return (float)v;
    }

    public static float GetGiveUpRadius()
    {
        var v = GiveUpRadius.Value;
        if (v < GIVE_UP_RADIUS_MIN) return GIVE_UP_RADIUS_MIN;
        if (v > GIVE_UP_RADIUS_MAX) return GIVE_UP_RADIUS_MAX;
        return (float)v;
    }

    private static void OnMeetDelayChanged(int _, int newValue) =>
        ClampAndUpdate(MeetDelay, newValue, MEET_DELAY_MIN, MEET_DELAY_MAX);

    private static void OnMaxFallbackAttemptsChanged(int _, int newValue) =>
        ClampAndUpdate(MaxFallbackAttempts, newValue, MAX_FALLBACK_ATTEMPTS_MIN, MAX_FALLBACK_ATTEMPTS_MAX);

    private static void OnRepathIntervalChanged(float _, float newValue) =>
        ClampAndUpdate(RepathInterval, newValue, REPATH_INTERVAL_MIN, REPATH_INTERVAL_MAX);

    private static void OnMaxWaitAtDoorChanged(int _, int newValue) =>
        ClampAndUpdate(MaxWaitAtDoor, newValue, MAX_WAIT_AT_DOOR_MIN, MAX_WAIT_AT_DOOR_MAX);

    private static void OnGiveUpRadiusChanged(int _, int newValue) =>
        ClampAndUpdate(GiveUpRadius, newValue, GIVE_UP_RADIUS_MIN, GIVE_UP_RADIUS_MAX);

    private static void ClampAndUpdate(MelonPreferences_Entry<int> entry, int newValue, int min, int max)
    {
        if (newValue >= min && newValue <= max) return;
        entry.Value = newValue < min ? min : max;
    }

    private static void ClampAndUpdate(MelonPreferences_Entry<float> entry, float newValue, float min, float max)
    {
        if (newValue >= min && newValue <= max) return;
        entry.Value = newValue < min ? min : max;
    }
}
