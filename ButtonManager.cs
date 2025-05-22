using System.Collections;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI.Phone.Messages;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SimpleCall;

public static class ButtonManager
{
    #region Configuration & Constants
    
    private const float MAX_WARP_RADIUS = 150f;
    private const int MAX_INITIALIZATION_ATTEMPTS = 120;
    private const float INITIALIZATION_RETRY_DELAY = 0.5f;
    private const int MAX_MOVEMENT_RETRIES = 2;
    private const float MIN_SUCCESS_DISTANCE = 10f;
    private const float CHECK_INTERVAL = 0.25f;
    private const float RETRY_DELAY = 0.25f;
    
    // UI Configuration
    private const string BUTTON_NAME = "CallDealerButton";
    private const string BUTTON_TEXT = "Call Dealer";
    private const int BUTTON_WIDTH = 150;
    private const int BUTTON_HEIGHT = 40;
    private const int BUTTON_FONT_SIZE = 24;

    private static readonly PathConfig[] UI_PATHS = {
        new("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/Content",
            "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/Content/Cash",
            new Vector3(180, 60, 0), false),
        new("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/ScrollingContent/Content",
            "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background/ScrollingContent/Content/Cash",
            new Vector3(190, 387, 0), true)
    };

    private static readonly Vector3[] STRATEGIC_LOCATIONS = {
        new(165.71f, 10.04f, -77.76f), new(147.21f, 4.05f, -104.98f), new(145.43f, 4.14f, -101.05f),
        new(87.15f, 4.14f, -100.74f), new(-14.42f, 0.14f, -80.20f), new(-22.53f, 0.14f, -43.03f),
        new(-57.67f, -2.36f, -63.09f), new(11.81f, 0.14f, -32.88f), new(36.17f, 0.14f, 7.04f),
        new(87.55f, 0.14f, 7.06f), new(140.43f, 0.14f, 7.52f), new(178.73f, 0.04f, -10.85f),
        new(87.08f, 0.14f, 47.01f), new(127.57f, 0.14f, 55.56f), new(129.83f, 0.14f, 89.81f),
        new(35.69f, 0.14f, 55.51f), new(-14.81f, 0.14f, 55.08f), new(-54.45f, 0.14f, 62.60f),
        new(-104.30f, -3.86f, 72.61f), new(-133.67f, -3.86f, 32.15f), new(-149.34f, -3.86f, 88.37f),
        new(-166.26f, -3.66f, 113.98f), new(-148.01f, -3.86f, 126.18f), new(-75.39f, -3.86f, 119.47f),
        new(-60.99f, -3.96f, 145.99f), new(-33.89f, -3.86f, 134.41f), new(-22.74f, 0.14f, 86.44f),
        new(-63.92f, 0.14f, 82.83f), new(-5.18f, 0.16f, 100.53f), new(27.54f, 0.14f, 80.41f),
        new(80.28f, 0.14f, 91.45f), new(-71.91f, -2.45f, -49.06f)
    };

    #endregion

    #region Data Structures

    private record PathConfig(string ParentPath, string ReferencePath, Vector3 ButtonPosition, bool IsModded);
    
    private class DealerMoveState
    {
        public Vector3 TargetPosition { get; }
        public int RetryCount { get; set; }
        public NPCScheduleManager ScheduleManager { get; }

        public DealerMoveState(Vector3 targetPosition, NPCScheduleManager scheduleManager)
        {
            TargetPosition = targetPosition;
            ScheduleManager = scheduleManager;
            RetryCount = 0;
        }
    }

    #endregion

    #region State

    private static GameObject _targetParent;
    private static GameObject _gameObjectReference;
    private static GameObject _callDealerButton;
    private static PathConfig _currentPathConfig;
    private static readonly Dictionary<int, DealerMoveState> _dealerMoveStates = new();

    #endregion

    #region Public Interface

    public static void Initialize() => MelonCoroutines.Start(InitializeCoroutine());

    public static void Terminate()
    {
        try
        {
            RemoveButton();
            ResetState();
            LogDebug("ButtonManager terminated successfully");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error during termination: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public static void LogPlayerPosition()
    {
        var player = GetPlayer();
        if (player == null)
        {
            MelonLogger.Error("Player is null, cannot get Vector3 position.");
            return;
        }
        
        MelonLogger.Msg($"Player Vector3 position: {player.PlayerBasePosition}");
    }

    #endregion

    #region Initialization

    private static IEnumerator InitializeCoroutine()
    {
        LogDebug("Starting initialization...");

        for (int attempts = 0; attempts < MAX_INITIALIZATION_ATTEMPTS; attempts++)
        {
            if (TryFindUiElements())
            {
                CreateCallDealerButton();
                yield break;
            }

            yield return new WaitForSeconds(INITIALIZATION_RETRY_DELAY);
        }

        MelonLogger.Error($"Failed to find UI elements after {MAX_INITIALIZATION_ATTEMPTS} attempts");
    }

    private static bool TryFindUiElements()
    {
        foreach (var pathConfig in UI_PATHS)
        {
            var parent = GameObject.Find(pathConfig.ParentPath);
            var reference = GameObject.Find(pathConfig.ReferencePath);

            if (parent != null && reference != null)
            {
                _targetParent = parent;
                _gameObjectReference = reference;
                _currentPathConfig = pathConfig;
                LogDebug($"Found UI elements ({(pathConfig.IsModded ? "modded" : "vanilla")} path)");
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Button Management

    private static void CreateCallDealerButton()
    {
        LogDebug("Creating Call Dealer button...");

        try
        {
            if (_targetParent == null || _gameObjectReference == null)
            {
                MelonLogger.Error("Required UI elements are null");
                return;
            }
            
            RemoveButton();

            _callDealerButton = new GameObject(BUTTON_NAME) { layer = _targetParent.layer };
            _callDealerButton.transform.SetParent(_targetParent.transform, false);

            SetupButtonComponents(_callDealerButton);
            LogDebug("Button created successfully");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error creating button: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void SetupButtonComponents(GameObject buttonGameObject)
    {
        // RectTransform
        var rectTransform = buttonGameObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = _currentPathConfig.ButtonPosition;

        // Visual
        buttonGameObject.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);

        // Text
        SetupButtonText(buttonGameObject);

        // Click handler
        buttonGameObject.AddComponent<Button>().onClick.AddListener((Action)OnCallDealerButtonClicked);
    }

    private static void SetupButtonText(GameObject buttonGameObject)
    {
        var textObject = new GameObject("ButtonText");
        textObject.transform.SetParent(buttonGameObject.transform, false);

        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;

        var textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = BUTTON_TEXT;
        textComponent.color = Color.white;
        textComponent.fontSize = BUTTON_FONT_SIZE;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = false;
        textComponent.overflowMode = TextOverflowModes.Overflow;
    }

    private static void RemoveButton()
    {
        if (_callDealerButton != null)
        {
            LogDebug("Destroying Call Dealer button");
            Object.Destroy(_callDealerButton);
            _callDealerButton = null;
        }
    }

    private static void ResetState()
    {
        _targetParent = null;
        _gameObjectReference = null;
        _currentPathConfig = null;
        _callDealerButton = null; // Add this line
        _dealerMoveStates.Clear();
    }

    #endregion

    #region Dealer Management

    private static void OnCallDealerButtonClicked()
    {
        LogDebug("Call Dealer button clicked");

        try
        {
            var player = GetPlayer();
            var dealer = GetSelectedDealer();
            
            if (player == null || dealer == null) return;

            var playerPosition = player.PlayerBasePosition;
            var scheduleManager = dealer.GetComponentInChildren<NPCScheduleManager>();
            
            if (!TryExitBuilding(dealer)) return;

            if (scheduleManager != null)
            {
                scheduleManager.DisableSchedule();
                LogDebug("Disabled dealer's schedule for transit.");
            }

            var dealerId = dealer.GetInstanceID();
            _dealerMoveStates[dealerId] = new DealerMoveState(playerPosition, scheduleManager);

            if (ModSettings.ActivateDealerMessages.Value) 
                dealer.SendTextMessage(DealerMessages.GetRandomCalledMessage());
                
            MoveDealerToPlayer(dealer);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error in button click handler: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static Player GetPlayer()
    {
        var player = Object.FindObjectOfType<Player>();
        if (player == null) MelonLogger.Error("Failed to find Player object");
        return player;
    }

    private static NPC GetSelectedDealer()
    {
        if (DealerManagementApp.Instance?.SelectedDealer == null)
        {
            MelonLogger.Error("Selected dealer is null");
            return null;
        }
        return DealerManagementApp.Instance.SelectedDealer;
    }

    private static bool TryExitBuilding(NPC dealer)
    {
        try
        {
            var currentBuilding = dealer.CurrentBuilding;
            if (currentBuilding != null)
            {
                LogDebug($"Making dealer exit building: {currentBuilding.GUID}");
                dealer.ExitBuilding(currentBuilding.GUID.ToString());
            }
            return true;
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Exception when trying to exit building: {e.Message}");
            return false;
        }
    }

    #endregion

    #region Movement Logic

    private static void MoveDealerToPlayer(NPC dealer)
    {
        var dealerId = dealer.GetInstanceID();
        if (!_dealerMoveStates.TryGetValue(dealerId, out var moveState))
        {
            MelonLogger.Error("Move state not found for dealer - can't continue");
            return;
        }

        var movement = dealer.GetComponent<NPCMovement>();
        if (movement == null)
        {
            MelonLogger.Error("Could not find NPCMovement component");
            return;
        }

        var dealerPosition = dealer.transform.position;
        var distanceToPlayer = Vector3.Distance(dealerPosition, moveState.TargetPosition);
        var callback = CreateMovementCallback(dealer);

        if (distanceToPlayer <= MAX_WARP_RADIUS)
        {
            LogDebug($"Dealer is within {MAX_WARP_RADIUS} units of target. Walking directly.");
            ExecuteMovement(movement, moveState.TargetPosition, callback);
        }
        else
        {
            var warpPosition = FindOptimalWarpPosition(moveState.TargetPosition);
            LogDebug($"Warping dealer to position: {warpPosition}");
            movement.Warp(warpPosition);
            ExecuteMovement(movement, moveState.TargetPosition, callback);
        }
    }

    private static void ExecuteMovement(NPCMovement movement, Vector3 targetPosition, 
        Il2CppSystem.Action<NPCMovement.WalkResult> callback)
    {
        if (movement.CanGetTo(targetPosition))
        {
            movement.SetDestination(targetPosition, callback, 1f);
        }
        else
        {
            var closestWalkable = FindClosestWalkablePoint(movement, targetPosition);
            if (closestWalkable.HasValue)
            {
                LogDebug($"Walking to closest strategic location at {closestWalkable.Value}");
                movement.SetDestination(closestWalkable.Value, callback, 1f);
            }
            else
            {
                MelonLogger.Warning("Cannot move to any location. Invoking completion directly.");
                callback?.Invoke(NPCMovement.WalkResult.Success);
            }
        }
    }

    private static Il2CppSystem.Action<NPCMovement.WalkResult> CreateMovementCallback(NPC dealer)
    {
        return (Il2CppSystem.Action<NPCMovement.WalkResult>)((result) =>
        {
            var dealerId = dealer.GetInstanceID();
            if (!_dealerMoveStates.TryGetValue(dealerId, out var moveState)) return;

            var currentPos = dealer.transform.position;
            var distanceToTarget = Vector3.Distance(currentPos, moveState.TargetPosition);
            var player = GetPlayer();
            var distanceToPlayer = player != null ? Vector3.Distance(currentPos, player.PlayerBasePosition) : float.MaxValue;

            LogDebug($"Walk result: {result}. Distance to target: {distanceToTarget}, distance to player: {distanceToPlayer}");

            // Success conditions
            if (IsCloseEnough(distanceToTarget, distanceToPlayer))
            {
                LogDebug("Dealer is close enough. Activating schedule.");
                if (moveState.ScheduleManager != null)
                    MelonCoroutines.Start(HandleDealerArrival(moveState.ScheduleManager, dealer));
                return;
            }

            // Handle retry logic
            var (shouldRetry, shouldActivate) = ShouldRetryMovement(result, moveState, distanceToTarget, distanceToPlayer);
            
            if (shouldRetry && moveState.RetryCount < MAX_MOVEMENT_RETRIES)
            {
                moveState.RetryCount++;
                LogDebug($"Retrying movement (attempt {moveState.RetryCount}/{MAX_MOVEMENT_RETRIES})");
                MelonCoroutines.Start(RetryMovementAfterDelay(dealer));
            }
            else if (shouldActivate && moveState.ScheduleManager != null)
            {
                MelonCoroutines.Start(HandleDealerArrival(moveState.ScheduleManager, dealer));
            }
        });
    }

    private static bool IsCloseEnough(float distanceToTarget, float distanceToPlayer) =>
        distanceToTarget <= MIN_SUCCESS_DISTANCE || distanceToPlayer <= ModSettings.DealerInteractionDistance.Value;

    private static (bool shouldRetry, bool shouldActivate) ShouldRetryMovement(
        NPCMovement.WalkResult result, DealerMoveState moveState, float distanceToTarget, float distanceToPlayer)
    {
        return result switch
        {
            NPCMovement.WalkResult.Failed => (moveState.RetryCount < MAX_MOVEMENT_RETRIES, true),
            NPCMovement.WalkResult.Success => (moveState.RetryCount < MAX_MOVEMENT_RETRIES, true),
            NPCMovement.WalkResult.Interrupted => (
                moveState.RetryCount < MAX_MOVEMENT_RETRIES && 
                distanceToTarget > MIN_SUCCESS_DISTANCE && 
                distanceToPlayer > ModSettings.DealerInteractionDistance.Value, 
                true),
            _ => (false, true)
        };
    }

    private static IEnumerator RetryMovementAfterDelay(NPC dealer)
    {
        yield return new WaitForSeconds(RETRY_DELAY);
        MoveDealerToPlayer(dealer);
    }

    private static IEnumerator HandleDealerArrival(NPCScheduleManager scheduleManager, NPC dealerNPC)
    {
        if (scheduleManager == null) yield break;

        LogDebug("Dealer has arrived. Waiting and keeping schedule disabled...");
        if (ModSettings.ActivateDealerMessages.Value) 
            dealerNPC.SendTextMessage(DealerMessages.GetRandomArrivedMessage());

        var elapsedTime = 0f;
        var player = GetPlayer();

        while (elapsedTime < ModSettings.DealerAwaitTime.Value)
        {
            if (scheduleManager.ScheduleEnabled)
            {
                LogDebug("Schedule was enabled during wait - disabling it");
                scheduleManager.DisableSchedule();
            }

            // Reset timer if player is close
            if (player != null && IsPlayerCloseToDealer(player, dealerNPC))
            {
                LogDebug("Player is close to dealer. Resetting wait timer.");
                elapsedTime = 0f;
            }

            yield return new WaitForSeconds(CHECK_INTERVAL);
            elapsedTime += CHECK_INTERVAL;
        }

        LogDebug("Finished waiting. Enabling schedule.");
        scheduleManager.EnableSchedule();
        if (ModSettings.ActivateDealerMessages.Value) 
            dealerNPC.SendTextMessage(DealerMessages.GetRandomLeavingMessage());
    }

    private static bool IsPlayerCloseToDealer(Player player, NPC dealer)
    {
        var distance = Vector3.Distance(player.PlayerBasePosition, dealer.transform.position);
        return distance <= ModSettings.DealerInteractionDistance.Value;
    }

    #endregion

    #region Position Utilities

    private static Vector3 FindOptimalWarpPosition(Vector3 targetPosition)
    {
        var pointsInRange = GetStrategicPointsInRange(targetPosition, MAX_WARP_RADIUS * 0.5f, MAX_WARP_RADIUS * 1.2f);
        
        if (pointsInRange.Count > 0)
        {
            var selectedPoint = pointsInRange[0];
            LogDebug($"Found strategic point: {selectedPoint} at distance {Vector3.Distance(targetPosition, selectedPoint)}");
            return selectedPoint;
        }

        return CalculateFallbackPosition(targetPosition);
    }

    private static List<Vector3> GetStrategicPointsInRange(Vector3 targetPosition, float minDistance, float maxDistance)
    {
        var pointsInRange = new List<(float distance, Vector3 point)>();

        foreach (var point in STRATEGIC_LOCATIONS)
        {
            var distance = Vector3.Distance(targetPosition, point);
            if (distance >= minDistance && distance <= maxDistance)
                pointsInRange.Add((distance, point));
        }

        pointsInRange.Sort((a, b) => a.distance.CompareTo(b.distance));
        return pointsInRange.Select(p => p.point).ToList();
    }

    private static Vector3 CalculateFallbackPosition(Vector3 targetPosition)
    {
        var closestPoint = FindClosestStrategicPoint(targetPosition);
        var distanceToClosest = Vector3.Distance(targetPosition, closestPoint);

        if (distanceToClosest != MAX_WARP_RADIUS)
        {
            var direction = (closestPoint - targetPosition).normalized;
            var fallbackPoint = targetPosition + direction * (MAX_WARP_RADIUS * 0.8f);
            LogDebug($"Generated fallback warp point at {fallbackPoint}");
            return fallbackPoint;
        }

        LogDebug($"Using closest strategic point: {closestPoint}");
        return closestPoint;
    }

    private static Vector3 FindClosestStrategicPoint(Vector3 position)
    {
        if (STRATEGIC_LOCATIONS.Length == 0) return position;

        return STRATEGIC_LOCATIONS.OrderBy(point => Vector3.Distance(position, point)).First();
    }

    private static Vector3? FindClosestWalkablePoint(NPCMovement movement, Vector3 targetPosition)
    {
        var maxWalkDistance = MAX_WARP_RADIUS * 1.5f;
        
        return STRATEGIC_LOCATIONS
            .Where(point => Vector3.Distance(targetPosition, point) < maxWalkDistance && movement.CanGetTo(point))
            .OrderBy(point => Vector3.Distance(targetPosition, point))
            .FirstOrDefault();
    }

    #endregion

    #region Utilities

    private static void LogDebug(string message)
    {
        if (ModSettings.ActivateDebug.Value) 
            MelonLogger.Msg(message);
    }

    #endregion
}