using System;
using System.Linq;
using System.Reflection;
using MelonLoader;

namespace SimpleCall;

public class ModSettings
{
    private static MethodInfo _requestUIRefreshMethod;
    private static bool _isModManagerIntegrated;

    public static MelonPreferences_Category GeneralCategory { get; private set; }
    public static MelonPreferences_Category DebugCategory { get; private set; }
    
    public static MelonPreferences_Entry<bool> ActivateMod { get; private set; }
    public static MelonPreferences_Entry<bool> ActivateDealerMessages { get; private set; }
    public static MelonPreferences_Entry<int> DealerAwaitTime { get; private set; }
    public static MelonPreferences_Entry<int> DealerInteractionDistance { get; private set; }
    
    public static MelonPreferences_Entry<bool> ActivateDebug { get; private set; }
    public static MelonPreferences_Entry<bool> ActivatePositionLogging { get; private set; }
    
    public static void Initialize()
    {
        CreateGeneralSettings();
        CreateDebugSettings();
    }

    public static void InitializeModIntegration()
    {
        if (_isModManagerIntegrated) return;
        
        try
        {
            var modManagerAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetType("ModManagerPhoneApp.ModSettingsEvents") != null);

            if (modManagerAssembly == null) return;
            
            var eventsType = modManagerAssembly.GetType("ModManagerPhoneApp.ModSettingsEvents");
            _requestUIRefreshMethod = eventsType?.GetMethod("RequestUIRefresh");
            _isModManagerIntegrated = _requestUIRefreshMethod != null;
            
            if (_isModManagerIntegrated)
                MelonLogger.Msg("Mod Manager integration initialized successfully");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to initialize Mod Manager integration: {ex.Message}");
        }
    }

    private static void CreateGeneralSettings()
    {
        GeneralCategory = MelonPreferences.CreateCategory("SimpleCall_01_General", "General");
        
        ActivateMod = GeneralCategory.CreateEntry("ActivateMod", true, "Activate Mod");
        ActivateMod.OnEntryValueChanged.Subscribe(OnActivateModChanged);
        
        ActivateDealerMessages = GeneralCategory.CreateEntry("ActivateDealerMessages", true, "Activate Dealer Messages");
        DealerAwaitTime = GeneralCategory.CreateEntry("DealerAwaitTime", 15, "Dealer Await Time (Default 15)");
        DealerInteractionDistance = GeneralCategory.CreateEntry("DealerInteractionDistance", 3, "Dealer Interaction Distance (Default 3)");
        
        // Subscribe to simple value change events (no special handling needed)
        ActivateDealerMessages.OnEntryValueChanged.Subscribe((_, _) => { });
        DealerAwaitTime.OnEntryValueChanged.Subscribe((_, _) => { });
        DealerInteractionDistance.OnEntryValueChanged.Subscribe((_, _) => { });
    }
    
    private static void CreateDebugSettings()
    {
        DebugCategory = MelonPreferences.CreateCategory("SimpleCall_02_Debug", "Debug");
        
        ActivateDebug = DebugCategory.CreateEntry("ActivateDebug", false, "Debug");
        ActivatePositionLogging = DebugCategory.CreateEntry("ActivatePositionLogging", false, "Position Logging (N key)");
        
        // Subscribe to simple value change events (no special handling needed)
        ActivateDebug.OnEntryValueChanged.Subscribe((_, _) => { });
        ActivatePositionLogging.OnEntryValueChanged.Subscribe((_, _) => { });
    }

    private static void OnActivateModChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            MelonLogger.Msg("Mod has been activated");
            ButtonManager.Initialize(); 
        }

        else
        {
            MelonLogger.Msg("Mod has been deactivated");
            ButtonManager.Terminate();
        }
    }

    public static void RequestUIRefresh() //Maybe useful in the future
    {
        if (!_isModManagerIntegrated) return;
        
        try
        {
            _requestUIRefreshMethod?.Invoke(null, null);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to request UI refresh: {ex.Message}");
        }
    }
    
    public static void Terminate()
    {
        // Add any cleanup logic here if needed in the future
    }
}