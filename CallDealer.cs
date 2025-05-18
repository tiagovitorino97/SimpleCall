using MelonLoader;
using UnityEngine;

namespace CallDealers;

public class CallDealer : MelonMod
{
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
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
    
    private static void ActivateMod()
    {
        MelonLogger.Msg("Mod has initialized");
        ButtonManager.Initialize();
    }


    public override void OnUpdate()
    {

        if (Input.GetKeyDown(KeyCode.N))
        {
            ButtonManager.LogPlayerPosition();
        }
        
    }

    private static void DeactivateMod()
    {
        MelonLogger.Msg("Mod has terminated");
        ButtonManager.Terminate();
    }
}