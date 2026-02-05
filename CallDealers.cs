using System.Linq;
using MelonLoader;
using UnityEngine;

namespace SimpleCall;

public class CallDealers : MelonMod
{
    private static bool _isModActive;
    private static bool _hasModManagerIntegration;

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("Initializing SimpleCall mod settings");
        ModSettings.Initialize();

        // Check for Mod Manager integration once during initialization
        _hasModManagerIntegration = RegisteredMelons.Any(mod => mod.Info.Name == "Mod Manager & Phone App");
        
        if (_hasModManagerIntegration)
            ModSettings.InitializeModIntegration();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (!ModSettings.ActivateMod.Value) return;

        switch (sceneName)
        {
            case "Main":
                ActivateMod();
                break;
            case "Menu":
                DeactivateMod();
                break;
        }
    }

    public override void OnUpdate()
    {
        if (_isModActive && Input.GetKeyDown(KeyCode.N) && ModSettings.ActivatePositionLogging.Value)
        {
            ButtonManager.LogPlayerPosition();
        }
    }

    private static void ActivateMod()
    {
        if (_isModActive) return;

        MelonLogger.Msg("Mod has activated");
        ButtonManager.Initialize();
        _isModActive = true;
    }

    private static void DeactivateMod()
    {
        if (!_isModActive) return;

        MelonLogger.Msg("Mod has deactivated");
        ButtonManager.Terminate();
        _isModActive = false;
    }
}