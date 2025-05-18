using System;
using System.Collections;
using System.Collections.Generic;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI.Phone.Messages;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CallDealers;

public class ButtonManager
{
    private static bool isDebugMode = false;
    private static GameObject targetParent;
    private static GameObject gameObjectReference;

    private static readonly string targetParentPath =
        "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/Content";

    private static readonly string gameObjectReferencePath =
        "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/Content/Cash";

    private static Vector3 lastCalledPlayerPosition;

    private static readonly Vector3[] strategicLocations =
    {
        new (165.71f, 10.04f, -77.76f),
        new (147.21f, 4.05f, -104.98f),
        new (145.43f, 4.14f, -101.05f),
        new (87.15f, 4.14f, -100.74f),
        new (-14.42f, 0.14f, -80.20f),
        new (-22.53f, 0.14f, -43.03f),
        new (-57.67f, -2.36f, -63.09f),
        new (11.81f, 0.14f, -32.88f),
        new (36.17f, 0.14f, 7.04f),
        new (87.55f, 0.14f, 7.06f),
        new (140.43f, 0.14f, 7.52f),
        new (178.73f, 0.04f, -10.85f),
        new (87.08f, 0.14f, 47.01f),
        new (127.57f, 0.14f, 55.56f),
        new (129.83f, 0.14f, 89.81f),
        new (35.69f, 0.14f, 55.51f),
        new (-14.81f, 0.14f, 55.08f),
        new (-54.45f, 0.14f, 62.60f),
        new (-112.77f, -3.86f, 75.57f),
        new (-133.67f, -3.86f, 32.15f),
        new (-149.34f, -3.86f, 88.37f),
        new (-166.26f, -3.66f, 113.98f),
        new (-148.01f, -3.86f, 126.18f),
        new (-75.39f, -3.86f, 119.47f),
        new (-60.99f, -3.96f, 145.99f),
        new (-33.89f, -3.86f, 134.41f),
        new (-22.74f, 0.14f, 86.44f),
        new (-63.92f, 0.14f, 82.83f),
        new (-5.18f, 0.16f, 100.53f),
        new (27.54f, 0.14f, 80.41f),
        new (80.28f, 0.14f, 91.45f),
        new (-71.91f, -2.45f, -49.06f)
    };
    
    private static readonly float maxWarpRadius = 150f;

    public static void Initialize()
    {
        MelonCoroutines.Start(WaitForTargetObject());
    }

    public static void Terminate()
    {
        // Cleanup logic if needed
    }

    private static void CreateCallDealerButton()
    {
        if (isDebugMode) MelonLogger.Msg("Creating Call Dealer button...");
        try
        {
            if (targetParent == null || gameObjectReference == null)
            {
                MelonLogger.Error("Target parent or reference object is null in CreateCallDealerButton");
                return;
            }

            var referenceImage = gameObjectReference.GetComponent<Image>();
            if (referenceImage == null)
            {
                MelonLogger.Error("Reference image component not found");
                return;
            }

            var callDealerButtonGameObject = new GameObject("CallDealerButton");
            callDealerButtonGameObject.layer = targetParent.layer;
            callDealerButtonGameObject.transform.SetParent(targetParent.transform, false);
            var rectTransform = callDealerButtonGameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(150, 40);
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = new Vector3(180, 60, 0);
            var buttonImage = callDealerButtonGameObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f);
            var textObject = new GameObject("ButtonText");
            textObject.transform.SetParent(callDealerButtonGameObject.transform, false);
            var textRectTransform = textObject.AddComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = Vector2.zero;
            textRectTransform.offsetMax = Vector2.zero;
            var textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = "Call Dealer";
            textComponent.color = Color.white;
            textComponent.fontSize = 24;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.enableWordWrapping = false;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            var button = callDealerButtonGameObject.AddComponent<Button>();
            button.onClick.AddListener((Action)(() => { OnCallDealerButtonClicked(); }));
            if (isDebugMode) MelonLogger.Msg("Call Dealer button created successfully");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error creating button: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static IEnumerator HandleDealerArrivalSequence(NPCScheduleManager scheduleManagerInstance)
    {
        if (scheduleManagerInstance == null)
        {
            MelonLogger.Error("HandleDealerArrivalSequence: NPCScheduleManager instance is null. Cannot proceed.");
            yield break;
        }

        if (isDebugMode) MelonLogger.Msg("Dealer has arrived. Waiting for 15 seconds.");
        yield return new WaitForSeconds(15f);

        if (isDebugMode) MelonLogger.Msg("Finished waiting. Checking schedule status.");
        var isScheduleEnabled = scheduleManagerInstance.ScheduleEnabled;
        if (isDebugMode) MelonLogger.Msg($"Schedule Enabled: {isScheduleEnabled}");

        if (!isScheduleEnabled)
        {
            if (isDebugMode) MelonLogger.Msg("Schedule is not enabled. Enabling schedule.");
            scheduleManagerInstance.EnableSchedule();
        }
        else
        {
            if (isDebugMode) MelonLogger.Msg("Schedule is already enabled.");
        }
    }

    private static void OnCallDealerButtonClicked()
    {
        if (isDebugMode) MelonLogger.Msg("Call Dealer button clicked");
        try
        {
            var player = Object.FindObjectOfType<Player>();
            if (player == null)
            {
                MelonLogger.Error("Failed to find Player object");
                return;
            }

            var playerPosition = player.PlayerBasePosition;
            if (isDebugMode) MelonLogger.Msg($"Player position: {playerPosition}");

            if (DealerManagementApp.Instance == null)
            {
                MelonLogger.Error("DealerManagementApp instance is null");
                return;
            }

            var dealerNPC = DealerManagementApp.Instance.SelectedDealer;
            if (dealerNPC == null)
            {
                MelonLogger.Error("Selected dealer is null");
                return;
            }

            try
            {
                var currentBuilding = dealerNPC.CurrentBuilding;
                if (currentBuilding != null)
                {
                    if (isDebugMode) MelonLogger.Msg($"Making dealer exit building: {currentBuilding.GUID}");
                    dealerNPC.ExitBuilding(currentBuilding.GUID.ToString());
                }
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"Exception when trying to exit building: {e.Message}");
            }

            var scheduleManager = dealerNPC.GetComponentInChildren<NPCScheduleManager>();
            if (scheduleManager == null)
            {
                MelonLogger.Warning(
                    "Could not find NPCScheduleManager component. Arrival logic and schedule disabling might not work.");
            }
            else
            {
                scheduleManager.DisableSchedule();
                if (isDebugMode) MelonLogger.Msg("Disabled dealer's schedule for transit.");
            }

            var dealerMovement = dealerNPC.GetComponent<NPCMovement>();
            if (dealerMovement == null)
            {
                MelonLogger.Error("Could not find NPCMovement component");
                return;
            }

            lastCalledPlayerPosition = playerPosition;
            
            // Get the dealer's current position
            Vector3 dealerPosition = dealerNPC.transform.position;
            float distanceToPlayer = Vector3.Distance(dealerPosition, playerPosition);
            
            // Define a callback for when the dealer reaches the player
            var finalArrivalCallback = (Il2CppSystem.Action<NPCMovement.WalkResult>)((NPCMovement.WalkResult result) =>
            {
                if (isDebugMode) MelonLogger.Msg($"Final position adjustment result: {result}");
                MelonCoroutines.Start(HandleDealerArrivalSequence(scheduleManager));
            });
            
            // Check if dealer is already within the maxWarpRadius of the player
            if (distanceToPlayer <= maxWarpRadius)
            {
                if (isDebugMode) MelonLogger.Msg($"Dealer is already within {maxWarpRadius} units of player. Walking directly to player.");
                if (dealerMovement.CanGetTo(playerPosition))
                {
                    dealerMovement.SetDestination(playerPosition, finalArrivalCallback, 1f);
                }
                else
                {
                    // If can't walk to player, try to find the closest strategic location near player that's walkable
                    Vector3 closestWalkablePoint = playerPosition;
                    float closestDistance = float.MaxValue;
                    bool foundWalkablePoint = false;
                    float maxWalkDistance = maxWarpRadius * 1.5f;

                    foreach (Vector3 point in strategicLocations)
                    {
                        float distance = Vector3.Distance(playerPosition, point);
                        if (distance < maxWalkDistance && distance < closestDistance && dealerMovement.CanGetTo(point))
                        {
                            closestWalkablePoint = point;
                            closestDistance = distance;
                            foundWalkablePoint = true;
                        }
                    }

                    if (foundWalkablePoint)
                    {
                        if (isDebugMode) MelonLogger.Msg($"Cannot walk to player directly. Walking to closest strategic location at {closestWalkablePoint}");
                        dealerMovement.SetDestination(closestWalkablePoint, finalArrivalCallback, 1f);
                    }
                    else
                    {
                        // Start the post-arrival sequence immediately if can't move to any strategic location
                        if (isDebugMode) MelonLogger.Msg("Cannot make final position adjustment to any strategic location. Starting arrival sequence directly.");
                        MelonCoroutines.Start(HandleDealerArrivalSequence(scheduleManager));
                    }
                }
                return;
            }
            
            // Find appropriate warp location near player if dealer is far away
            Vector3 warpPosition = FindAppropriateDealerWarpPosition(playerPosition);
            if (isDebugMode) MelonLogger.Msg($"Warping dealer from {dealerPosition} to position: {warpPosition}");

            // Warp the dealer to the position
            dealerMovement.Warp(warpPosition);
            
            // After warping, we might want to set a destination for fine-tuning position
            if (dealerMovement.CanGetTo(playerPosition))
            {
                if (isDebugMode) MelonLogger.Msg("Setting final position adjustments after warp");
                dealerMovement.SetDestination(playerPosition, finalArrivalCallback, 1f);
            }
            else
            {
                // If can't walk to player, try to find the closest strategic location near player that's walkable
                Vector3 closestWalkablePoint = playerPosition;
                float closestDistance = float.MaxValue;
                bool foundWalkablePoint = false;

                foreach (Vector3 point in strategicLocations)
                {
                    float distance = Vector3.Distance(playerPosition, point);
                    if (distance < closestDistance && dealerMovement.CanGetTo(point))
                    {
                        closestWalkablePoint = point;
                        closestDistance = distance;
                        foundWalkablePoint = true;
                    }
                }

                if (foundWalkablePoint)
                {
                    if (isDebugMode) MelonLogger.Msg($"Cannot walk to player directly. Walking to closest strategic location at {closestWalkablePoint}");
                    dealerMovement.SetDestination(closestWalkablePoint, finalArrivalCallback, 1f);
                }
                else
                {
                    // Start the post-arrival sequence immediately if can't move to any strategic location
                    if (isDebugMode) MelonLogger.Msg("Cannot make final position adjustment to any strategic location. Starting arrival sequence directly.");
                    MelonCoroutines.Start(HandleDealerArrivalSequence(scheduleManager));
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error in OnCallDealerButtonClicked: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static Vector3 FindAppropriateDealerWarpPosition(Vector3 playerPosition)
    {
        if (isDebugMode) MelonLogger.Msg($"Finding appropriate warp position near player at {playerPosition}");
        
        // Find strategic points within range of player
        List<KeyValuePair<float, Vector3>> pointsWithinRange = new List<KeyValuePair<float, Vector3>>();
        
        foreach (Vector3 point in strategicLocations)
        {
            float distance = Vector3.Distance(playerPosition, point);
            // Only consider points that are within reasonable distance but NOT too close
            if (distance <= maxWarpRadius * 1.2f && distance >= maxWarpRadius * 0.5f)
            {
                pointsWithinRange.Add(new KeyValuePair<float, Vector3>(distance, point));
            }
        }
        
        // Sort by distance (closest first but still within our defined range)
        pointsWithinRange.Sort((a, b) => a.Key.CompareTo(b.Key));
        
        if (pointsWithinRange.Count > 0)
        {
            // Use the closest strategic point that's in our desired range
            Vector3 selectedPoint = pointsWithinRange[0].Value;
            if (isDebugMode) MelonLogger.Msg($"Found strategic point within desired range: {selectedPoint} at distance {pointsWithinRange[0].Key}");
            return selectedPoint;
        }
        
        // If no points in desired range, find closest strategic point
        Vector3 closestPoint = FindClosestPoint(playerPosition);
        float distanceToClosest = Vector3.Distance(playerPosition, closestPoint);
        
        // If closest point is too close to player, create a new point at the target radius
        if (distanceToClosest < maxWarpRadius)
        {
            // Find a second closest point to determine a good direction
            Vector3 secondClosestPoint = FindSecondClosestPoint(playerPosition, closestPoint);
            
            // Create a direction vector either away from player or using second closest point
            Vector3 direction;
            if (secondClosestPoint != closestPoint)
            {
                // Direction away from player but influenced by the line between the two closest points
                direction = ((closestPoint - playerPosition) + (secondClosestPoint - closestPoint)).normalized;
            }
            else
            {
                // Just use direction away from player if no good second point
                direction = (closestPoint - playerPosition).normalized;
            }
            
            // Place at target radius
            Vector3 targetPoint = playerPosition + (direction * maxWarpRadius * 0.8f);
            if (isDebugMode) MelonLogger.Msg($"Generated new warp point at {targetPoint} based on direction from closest points");
            return targetPoint;
        }
        
        // If closest point is too far, scale it to be at the target radius
        if (distanceToClosest > maxWarpRadius)
        {
            Vector3 direction = (closestPoint - playerPosition).normalized;
            Vector3 targetPoint = playerPosition + (direction * maxWarpRadius * 0.8f);
            if (isDebugMode) MelonLogger.Msg($"Generated new warp point at {targetPoint} by scaling down from distant point");
            return targetPoint;
        }
        
        // Default fallback just to be safe
        if (isDebugMode) MelonLogger.Msg($"Using closest strategic point: {closestPoint}");
        return closestPoint;
    }
    
    private static Vector3 FindClosestPoint(Vector3 position)
    {
        if (strategicLocations.Length == 0)
        {
            MelonLogger.Warning("FindClosestPoint: No strategic locations available. Returning original position.");
            return position;
        }

        var closest = strategicLocations[0];
        var minDistance = Vector3.Distance(position, closest);

        for (var i = 1; i < strategicLocations.Length; i++)
        {
            var distance = Vector3.Distance(position, strategicLocations[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = strategicLocations[i];
            }
        }

        return closest;
    }
    
    private static Vector3 FindSecondClosestPoint(Vector3 position, Vector3 closestPoint)
    {
        if (strategicLocations.Length <= 1)
        {
            return closestPoint; // Return closest if there's only one
        }

        var secondClosest = closestPoint;
        var minDistance = float.MaxValue;

        for (var i = 0; i < strategicLocations.Length; i++)
        {
            var point = strategicLocations[i];
            
            // Skip if this is the closest point
            if (Vector3.Distance(point, closestPoint) < 0.1f)
                continue;
                
            var distance = Vector3.Distance(position, point);
            if (distance < minDistance)
            {
                minDistance = distance;
                secondClosest = point;
            }
        }

        return secondClosest;
    }

    private static IEnumerator WaitForTargetObject()
    {
        var attempts = 0;
        var maxAttempts = 120;
        if (isDebugMode) MelonLogger.Msg("Starting to look for target objects...");
        while (attempts < maxAttempts)
        {
            try
            {
                targetParent = GameObject.Find(targetParentPath);
                gameObjectReference = GameObject.Find(gameObjectReferencePath);
                if (targetParent != null && gameObjectReference != null)
                {
                    if (isDebugMode) MelonLogger.Msg($"Found target objects after {attempts + 1} attempts");
                    CreateCallDealerButton();
                    yield break;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error finding objects: {ex.Message}");
            }

            attempts++;
            yield return new WaitForSeconds(0.5f);
        }

        MelonLogger.Error(
            $"Failed to find target objects. Parent found: {targetParent != null}, Reference found: {gameObjectReference != null}");
    }

    public static void LogPlayerPosition()
    {
        var player = Object.FindObjectOfType<Player>();
        if (player == null)
        {
            MelonLogger.Error("Failed to find Player object");
            return;
        }

        var playerPosition = player.PlayerBasePosition;
        if (isDebugMode) MelonLogger.Msg($"Player Vector3 position: {playerPosition}");
    }
}