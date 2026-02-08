using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Phone.Messages;
using MelonLoader;
using SimpleCall.Models;
using SimpleCall.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SimpleCall.Controllers;

/// <summary>
/// Controller: Receives user input, validates context, and delegates to the model.
/// </summary>
public static class DealerCallController
{
    public static void CallDealerToPlayer()
    {
        var dealer = DealerManagementApp.Instance?.SelectedDealer;
        var player = Object.FindObjectOfType<Player>();

        if (ModSettings.EnableLogging.Value)
            MelonLogger.Msg("[SimpleCall] CallDealerToPlayer invoked");

        if (dealer == null)
        {
            if (ModSettings.EnableLogging.Value)
                MelonLogger.Warning("[SimpleCall] No dealer selected");
            return;
        }

        if (player == null)
        {
            if (ModSettings.EnableLogging.Value)
                MelonLogger.Warning("[SimpleCall] Player not found");
            return;
        }

        if (player.CurrentProperty != null)
        {
            if (ModSettings.ShowNoSignalNotification.Value)
                NotificationsManager.Instance?.SendNotification("SimpleCall", "No signal, go outside.", SpriteLoader.GetSignalSprite(), 4f, true);
            return;
        }

        if (DealerCallService.IsDealerInCall(dealer.GetInstanceID()))
            return;

        DealerCallService.ExecuteCall(dealer, player);
    }
}
