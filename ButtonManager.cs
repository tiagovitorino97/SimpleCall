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

namespace CallDealers
{
    public static class ButtonManager
    {
        #region Configuration

        public const bool IsDebugMode = false;
        private const float MaxWarpRadius = 150f;
        private const float ArrivalWaitTime = 15f;
        private const int MaxInitializationAttempts = 120;
        private const float InitializationRetryDelay = 0.5f;
        
        // UI Configuration
        private const string ButtonName = "CallDealerButton";
        private const string ButtonText = "Call Dealer";
        private const int ButtonWidth = 150;
        private const int ButtonHeight = 40;
        private const int ButtonFontSize = 24;
        
        // Vanilla paths
        private static readonly string[] VanillaPaths = 
        {
            "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/Content",
            "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/Content/Cash"
        };
        
        // Modded paths
        private static readonly string[] ModdedPaths = 
        {
            "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/ScrollingContent/Content",
            "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/ScrollingContent/Content/Cash"
        };
        
        // Button positions for different paths
        private static readonly Dictionary<bool, Vector3> ButtonPositions = new Dictionary<bool, Vector3>
        {
            { false, new Vector3(180, 60, 0) },    // Vanilla
            { true, new Vector3(190, 387, 0) }      // Modded
        };

        // Strategic locations for dealer warping
        private static readonly Vector3[] StrategicLocations =
        {
            new(165.71f, 10.04f, -77.76f),
            new(147.21f, 4.05f, -104.98f),
            new(145.43f, 4.14f, -101.05f),
            new(87.15f, 4.14f, -100.74f),
            new(-14.42f, 0.14f, -80.20f),
            new(-22.53f, 0.14f, -43.03f),
            new(-57.67f, -2.36f, -63.09f),
            new(11.81f, 0.14f, -32.88f),
            new(36.17f, 0.14f, 7.04f),
            new(87.55f, 0.14f, 7.06f),
            new(140.43f, 0.14f, 7.52f),
            new(178.73f, 0.04f, -10.85f),
            new(87.08f, 0.14f, 47.01f),
            new(127.57f, 0.14f, 55.56f),
            new(129.83f, 0.14f, 89.81f),
            new(35.69f, 0.14f, 55.51f),
            new(-14.81f, 0.14f, 55.08f),
            new(-54.45f, 0.14f, 62.60f),
            new(-104.30f, -3.86f, 72.61f),
            new(-133.67f, -3.86f, 32.15f),
            new(-149.34f, -3.86f, 88.37f),
            new(-166.26f, -3.66f, 113.98f),
            new(-148.01f, -3.86f, 126.18f),
            new(-75.39f, -3.86f, 119.47f),
            new(-60.99f, -3.96f, 145.99f),
            new(-33.89f, -3.86f, 134.41f),
            new(-22.74f, 0.14f, 86.44f),
            new(-63.92f, 0.14f, 82.83f),
            new(-5.18f, 0.16f, 100.53f),
            new(27.54f, 0.14f, 80.41f),
            new(80.28f, 0.14f, 91.45f),
            new(-71.91f, -2.45f, -49.06f)
        };
        #endregion

        #region State
        private static GameObject _targetParent;
        private static GameObject _gameObjectReference;
        private static bool _isUsingModdedPath;
        private static Vector3 _lastCalledPlayerPosition;
        #endregion

        #region Public Interface
        public static void Initialize()
        {
            MelonCoroutines.Start(InitializeCoroutine());
        }

        public static void Terminate()
        {
            try
            {
                RemoveButton();
                ResetState();
                
                if (IsDebugMode) MelonLogger.Msg("ButtonManager terminated successfully");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error during termination: {ex.Message}\n{ex.StackTrace}");
            }
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
            if (IsDebugMode) MelonLogger.Msg($"Player Vector3 position: {playerPosition}");
        }
        #endregion

        #region Initialization
        private static IEnumerator InitializeCoroutine()
        {
            if (IsDebugMode) MelonLogger.Msg("Starting initialization...");
            
            var attempts = 0;
            while (attempts < MaxInitializationAttempts)
            {
                if (TryFindUiElements(out _targetParent, out _gameObjectReference, out _isUsingModdedPath))
                {
                    CreateCallDealerButton();
                    yield break;
                }

                attempts++;
                yield return new WaitForSeconds(InitializationRetryDelay);
            }

            MelonLogger.Error($"Failed to find UI elements after {attempts} attempts");
        }

        private static bool TryFindUiElements(out GameObject parent, out GameObject reference, out bool isModdedPath)
        {
            parent = GameObject.Find(VanillaPaths[0]);
            reference = GameObject.Find(VanillaPaths[1]);
            
            if (parent != null && reference != null)
            {
                isModdedPath = false;
                if (IsDebugMode) MelonLogger.Msg("Found UI elements (vanilla path)");
                return true;
            }
            
            parent = GameObject.Find(ModdedPaths[0]);
            reference = GameObject.Find(ModdedPaths[1]);
            
            if (parent != null && reference != null)
            {
                isModdedPath = true;
                if (IsDebugMode) MelonLogger.Msg("Found UI elements (modded path)");
                return true;
            }

            isModdedPath = false;
            return false;
        }
        #endregion

        #region Button Management
        private static void CreateCallDealerButton()
        {
            if (IsDebugMode) MelonLogger.Msg("Creating Call Dealer button...");
            
            try
            {
                if (_targetParent == null || _gameObjectReference == null)
                {
                    MelonLogger.Error("Required UI elements are null");
                    return;
                }

                var referenceImage = _gameObjectReference.GetComponent<Image>();
                if (referenceImage == null)
                {
                    MelonLogger.Error("Reference image component not found");
                    return;
                }

                var buttonGameObject = new GameObject(ButtonName)
                {
                    layer = _targetParent.layer
                };
                
                buttonGameObject.transform.SetParent(_targetParent.transform, false);
                
                SetupButtonRectTransform(buttonGameObject);
                SetupButtonVisuals(buttonGameObject);
                SetupButtonText(buttonGameObject);
                SetupButtonClickHandler(buttonGameObject);

                if (IsDebugMode) MelonLogger.Msg("Button created successfully");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error creating button: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void SetupButtonRectTransform(GameObject buttonGameObject)
        {
            var rectTransform = buttonGameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = ButtonPositions[_isUsingModdedPath];
        }

        private static void SetupButtonVisuals(GameObject buttonGameObject)
        {
            var buttonImage = buttonGameObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f);
        }

        private static void SetupButtonText(GameObject buttonGameObject)
        {
            var textObject = new GameObject("ButtonText");
            textObject.transform.SetParent(buttonGameObject.transform, false);
            
            var textRectTransform = textObject.AddComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = Vector2.zero;
            textRectTransform.offsetMax = Vector2.zero;
            
            var textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = ButtonText;
            textComponent.color = Color.white;
            textComponent.fontSize = ButtonFontSize;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.enableWordWrapping = false;
            textComponent.overflowMode = TextOverflowModes.Overflow;
        }

        private static void SetupButtonClickHandler(GameObject buttonGameObject)
        {
            var button = buttonGameObject.AddComponent<Button>();
            button.onClick.AddListener((Action)OnCallDealerButtonClicked);
        }

        private static void RemoveButton()
        {
            var button = GameObject.Find(ButtonName);
            if (button != null)
            {
                if (IsDebugMode) MelonLogger.Msg("Destroying Call Dealer button");
                Object.Destroy(button);
            }
        }

        private static void ResetState()
        {
            _targetParent = null;
            _gameObjectReference = null;
            _isUsingModdedPath = false;
            _lastCalledPlayerPosition = Vector3.zero;
        }
        #endregion

        #region Dealer Calling Logic
        private static void OnCallDealerButtonClicked()
        {
            if (IsDebugMode) MelonLogger.Msg("Call Dealer button clicked");
            
            try
            {
                var player = GetPlayer();
                if (player == null) return;

                var dealerNPC = GetSelectedDealer();
                if (dealerNPC == null) return;

                var playerPosition = player.PlayerBasePosition;
                _lastCalledPlayerPosition = playerPosition;
                
                TryExitBuilding(dealerNPC);
                
                var scheduleManager = dealerNPC.GetComponentInChildren<NPCScheduleManager>();
                if (scheduleManager != null)
                {
                    scheduleManager.DisableSchedule();
                    if (IsDebugMode) MelonLogger.Msg("Disabled dealer's schedule for transit.");
                }

                MoveDealerToPlayer(dealerNPC, playerPosition, scheduleManager);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in button click handler: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static Player GetPlayer()
        {
            var player = Object.FindObjectOfType<Player>();
            if (player == null)
            {
                MelonLogger.Error("Failed to find Player object");
                return null;
            }
            return player;
        }

        private static NPC GetSelectedDealer()
        {
            if (DealerManagementApp.Instance == null)
            {
                MelonLogger.Error("DealerManagementApp instance is null");
                return null;
            }

            var dealerNPC = DealerManagementApp.Instance.SelectedDealer;
            if (dealerNPC == null)
            {
                MelonLogger.Error("Selected dealer is null");
                return null;
            }

            return dealerNPC;
        }

        private static void TryExitBuilding(NPC dealer)
        {
            try
            {
                var currentBuilding = dealer.CurrentBuilding;
                if (currentBuilding != null)
                {
                    if (IsDebugMode) MelonLogger.Msg($"Making dealer exit building: {currentBuilding.GUID}");
                    dealer.ExitBuilding(currentBuilding.GUID.ToString());
                }
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"Exception when trying to exit building: {e.Message}");
            }
        }

        private static void MoveDealerToPlayer(NPC dealer, Vector3 playerPosition, NPCScheduleManager scheduleManager)
        {
            var dealerMovement = dealer.GetComponent<NPCMovement>();
            if (dealerMovement == null)
            {
                MelonLogger.Error("Could not find NPCMovement component");
                return;
            }

            var dealerPosition = dealer.transform.position;
            var distanceToPlayer = Vector3.Distance(dealerPosition, playerPosition);
            
            var arrivalCallback = CreateArrivalCallback(scheduleManager);
            
            if (distanceToPlayer <= MaxWarpRadius)
            {
                MoveDealerWithinRange(dealerMovement, playerPosition, arrivalCallback);
                return;
            }
            
            WarpDealerFromDistance(dealerMovement, playerPosition, arrivalCallback);
        }

        private static Il2CppSystem.Action<NPCMovement.WalkResult> CreateArrivalCallback(NPCScheduleManager scheduleManager)
        {
            return (Il2CppSystem.Action<NPCMovement.WalkResult>)((NPCMovement.WalkResult result) =>
            {
                if (IsDebugMode) MelonLogger.Msg($"Final position adjustment result: {result}");
                if (scheduleManager != null)
                {
                    MelonCoroutines.Start(HandleDealerArrivalSequence(scheduleManager));
                }
            });
        }

        private static void MoveDealerWithinRange(NPCMovement movement, Vector3 playerPosition, 
            Il2CppSystem.Action<NPCMovement.WalkResult> callback)
        {
            if (IsDebugMode) MelonLogger.Msg($"Dealer is within {MaxWarpRadius} units of player. Walking directly.");
            
            if (movement.CanGetTo(playerPosition))
            {
                movement.SetDestination(playerPosition, callback, 1f);
            }
            else
            {
                MoveToClosestStrategicLocation(movement, playerPosition, callback);
            }
        }

        private static void WarpDealerFromDistance(NPCMovement movement, Vector3 playerPosition, 
            Il2CppSystem.Action<NPCMovement.WalkResult> callback)
        {
            var warpPosition = FindAppropriateDealerWarpPosition(playerPosition);
            if (IsDebugMode) MelonLogger.Msg($"Warping dealer to position: {warpPosition}");

            movement.Warp(warpPosition);
            
            if (movement.CanGetTo(playerPosition))
            {
                if (IsDebugMode) MelonLogger.Msg("Setting final position adjustments after warp");
                movement.SetDestination(playerPosition, callback, 1f);
            }
            else
            {
                MoveToClosestStrategicLocation(movement, playerPosition, callback);
            }
        }

        private static void MoveToClosestStrategicLocation(NPCMovement movement, Vector3 playerPosition, 
            Il2CppSystem.Action<NPCMovement.WalkResult> callback)
        {
            var closestPoint = FindClosestWalkablePoint(movement, playerPosition);
            
            if (closestPoint.HasValue)
            {
                if (IsDebugMode) MelonLogger.Msg($"Walking to closest strategic location at {closestPoint.Value}");
                movement.SetDestination(closestPoint.Value, callback, 1f);
            }
            else if (callback != null)
            {
                if (IsDebugMode) MelonLogger.Msg("Cannot move to any location. Starting arrival sequence directly.");
                callback.Invoke(NPCMovement.WalkResult.Success);
            }
        }

        private static Vector3? FindClosestWalkablePoint(NPCMovement movement, Vector3 playerPosition)
        {
            float maxWalkDistance = MaxWarpRadius * 1.5f;
            Vector3? closestPoint = null;
            float closestDistance = float.MaxValue;

            foreach (var point in StrategicLocations)
            {
                var distance = Vector3.Distance(playerPosition, point);
                if (distance < maxWalkDistance && distance < closestDistance && movement.CanGetTo(point))
                {
                    closestPoint = point;
                    closestDistance = distance;
                }
            }

            return closestPoint;
        }
        #endregion

        #region Dealer Arrival Handling
        private static IEnumerator HandleDealerArrivalSequence(NPCScheduleManager scheduleManager)
        {
            if (scheduleManager == null)
            {
                MelonLogger.Error("Schedule manager is null. Cannot proceed.");
                yield break;
            }

            if (IsDebugMode) MelonLogger.Msg("Dealer has arrived. Waiting...");
            yield return new WaitForSeconds(ArrivalWaitTime);

            if (IsDebugMode) MelonLogger.Msg("Finished waiting. Checking schedule status.");
            var isScheduleEnabled = scheduleManager.ScheduleEnabled;
            if (IsDebugMode) MelonLogger.Msg($"Schedule Enabled: {isScheduleEnabled}");

            if (!isScheduleEnabled)
            {
                if (IsDebugMode) MelonLogger.Msg("Enabling schedule.");
                scheduleManager.EnableSchedule();
            }
            else if (IsDebugMode)
            {
                MelonLogger.Msg("Schedule is already enabled.");
            }
        }
        #endregion

        #region Position Calculation
        private static Vector3 FindAppropriateDealerWarpPosition(Vector3 playerPosition)
        {
            if (IsDebugMode) MelonLogger.Msg($"Finding warp position near player at {playerPosition}");
            
            var pointsWithinRange = GetStrategicPointsInRange(playerPosition);
            
            if (pointsWithinRange.Count > 0)
            {
                var selectedPoint = pointsWithinRange[0].Value;
                if (IsDebugMode) MelonLogger.Msg($"Found strategic point: {selectedPoint} at distance {pointsWithinRange[0].Key}");
                return selectedPoint;
            }
            
            return CalculateFallbackWarpPosition(playerPosition);
        }

        private static List<KeyValuePair<float, Vector3>> GetStrategicPointsInRange(Vector3 playerPosition)
        {
            var pointsWithinRange = new List<KeyValuePair<float, Vector3>>();
            var minDistance = MaxWarpRadius * 0.5f;
            var maxDistance = MaxWarpRadius * 1.2f;

            foreach (var point in StrategicLocations)
            {
                var distance = Vector3.Distance(playerPosition, point);
                if (distance <= maxDistance && distance >= minDistance)
                {
                    pointsWithinRange.Add(new KeyValuePair<float, Vector3>(distance, point));
                }
            }
            
            pointsWithinRange.Sort((a, b) => a.Key.CompareTo(b.Key));
            return pointsWithinRange;
        }

        private static Vector3 CalculateFallbackWarpPosition(Vector3 playerPosition)
        {
            var closestPoint = FindClosestPoint(playerPosition);
            var distanceToClosest = Vector3.Distance(playerPosition, closestPoint);

            if (distanceToClosest < MaxWarpRadius)
            {
                return CreatePositionInDirection(playerPosition, closestPoint, MaxWarpRadius * 0.8f);
            }
            
            if (distanceToClosest > MaxWarpRadius)
            {
                return CreatePositionInDirection(playerPosition, closestPoint, MaxWarpRadius * 0.8f);
            }

            if (IsDebugMode) MelonLogger.Msg($"Using closest strategic point: {closestPoint}");
            return closestPoint;
        }

        private static Vector3 CreatePositionInDirection(Vector3 playerPosition, Vector3 referencePoint, float distance)
        {
            var direction = (referencePoint - playerPosition).normalized;
            var targetPoint = playerPosition + (direction * distance);
            
            if (IsDebugMode) MelonLogger.Msg($"Generated new warp point at {targetPoint}");
            return targetPoint;
        }

        private static Vector3 FindClosestPoint(Vector3 position)
        {
            if (StrategicLocations.Length == 0)
            {
                MelonLogger.Warning("No strategic locations available. Returning original position.");
                return position;
            }

            var closest = StrategicLocations[0];
            var minDistance = Vector3.Distance(position, closest);

            for (var i = 1; i < StrategicLocations.Length; i++)
            {
                var distance = Vector3.Distance(position, StrategicLocations[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = StrategicLocations[i];
                }
            }

            return closest;
        }
        #endregion
    }
}