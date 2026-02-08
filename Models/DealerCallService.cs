using System.Collections;
using System.Collections.Generic;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.PlayerScripts;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SimpleCall.Models;

/// <summary>
/// Model: Business logic for pathfinding and movement when calling a dealer to the player.
/// </summary>
public static class DealerCallService
{
    private static readonly HashSet<int> _dealersInCall = new();

    public static bool IsDealerInCall(int dealerInstanceId) => _dealersInCall.Contains(dealerInstanceId);

    private const float EXIT_BUILDING_DELAY = 0.5f;
    private const float ARRIVAL_DISTANCE = 3f;
    private const float CHECK_INTERVAL = 0.25f;
    private const float LOG_INTERVAL = 2f;
    private const int POSITION_HISTORY_SIZE = 10;
    private const int GOING_AWAY_SAMPLE_COUNT = 10;

    private static void Log(string message)
    {
        if (ModSettings.EnableLogging.Value)
            MelonLogger.Msg(message);
    }

    public static void ExecuteCall(NPC dealer, Player player)
    {
        var distance = Vector3.Distance(dealer.transform.position, player.PlayerBasePosition);
        var inBuilding = dealer.CurrentBuilding != null;
        Log($"[SimpleCall] Dealer: {dealer.gameObject.name} (ID:{dealer.GetInstanceID()}) | Distance: {distance:F1}m | In building: {inBuilding}");

        var movement = dealer.GetComponent<NPCMovement>();
        var scheduleManager = dealer.GetComponentInChildren<NPCScheduleManager>();

        if (movement == null)
        {
            MelonLogger.Warning("[SimpleCall] Dealer has no NPCMovement");
            return;
        }

        Log($"[SimpleCall] ScheduleManager: {(scheduleManager != null ? "found" : "null")} | ScheduleEnabled: {scheduleManager?.ScheduleEnabled ?? false}");

        _dealersInCall.Add(dealer.GetInstanceID());

        if (dealer.CurrentBuilding != null)
        {
            dealer.ExitBuilding(dealer.CurrentBuilding.GUID.ToString());
            MelonCoroutines.Start(WaitThenMove(dealer, player, movement, scheduleManager));
        }
        else
        {
            DoMove(dealer, player, movement, scheduleManager);
        }
    }

    private static IEnumerator WaitThenMove(NPC dealer, Player player, NPCMovement movement, NPCScheduleManager scheduleManager)
    {
        Log($"[SimpleCall] WaitThenMove: waiting {EXIT_BUILDING_DELAY}s");
        yield return new WaitForSeconds(EXIT_BUILDING_DELAY);
        DoMove(dealer, player, movement, scheduleManager);
    }

    private static void DoMove(NPC dealer, Player player, NPCMovement movement, NPCScheduleManager scheduleManager)
    {
        Log("[SimpleCall] DoMove: DisableSchedule, Stop, SetDestination");
        var dealerInitialPosition = dealer.transform.position;
        scheduleManager?.DisableSchedule();
        movement.Stop();

        var originalWalkSpeed = movement.WalkSpeed;
        if (ModSettings.DealerRunsToPlayer.Value)
            movement.WalkSpeed = movement.RunSpeed;

        movement.SetDestination(player.transform);

        if (ModSettings.DealerMessages.Value)
            dealer.SendTextMessage(DealerMessages.GetOnMyWay());

        Log($"[SimpleCall] DoMove: HasDestination={movement.HasDestination}");
        MelonCoroutines.Start(WaitUntilArrived(dealer, player, movement, scheduleManager, originalWalkSpeed, dealerInitialPosition));
    }

    private static IEnumerator WaitUntilArrived(NPC dealer, Player player, NPCMovement movement, NPCScheduleManager scheduleManager, float originalWalkSpeed, Vector3 dealerInitialPosition)
    {
        var elapsed = 0f;
        var lastRepath = 0f;
        var positionHistory = new List<Vector3>(POSITION_HISTORY_SIZE);
        var dealerPlayerHistory = new List<(Vector3 dealer, Vector3 player)>(GOING_AWAY_SAMPLE_COUNT + 1);
        var fallbackIndex = 0;
        var playerWasOutside = true;
        var sentAtDoorMessage = false;
        var waitingAtDoorSince = -1f;
        GameObject targetMarker = null;
        var dealerId = dealer != null ? dealer.GetInstanceID() : 0;

        Log($"[SimpleCall] WaitUntilArrived: started, target <= {ARRIVAL_DISTANCE}m");

        try
        {
            while (true)
            {
                yield return new WaitForSeconds(CHECK_INTERVAL);
                elapsed += CHECK_INTERVAL;

                if (player == null || dealer == null)
                {
                    Log("[SimpleCall] WaitUntilArrived: player or dealer became null");
                    yield break;
                }

                if (scheduleManager?.ScheduleEnabled == true)
                {
                    Log("[SimpleCall] WaitUntilArrived: schedule re-enabled during transit, disabling");
                    scheduleManager.DisableSchedule();
                }

                var playerOutside = player.CurrentProperty == null;
                var distanceToPlayer = Vector3.Distance(dealer.transform.position, player.PlayerBasePosition);
                var dealerPos = dealer.transform.position;
                var playerPos = player.PlayerBasePosition;
                var hasDest = movement.HasDestination;

                dealerPlayerHistory.Add((dealerPos, playerPos));
                if (dealerPlayerHistory.Count > GOING_AWAY_SAMPLE_COUNT + 1)
                    dealerPlayerHistory.RemoveAt(0);

                var reachedPlayer = distanceToPlayer <= ARRIVAL_DISTANCE;

                if (reachedPlayer)
                {
                    Log($"[SimpleCall] Arrived at t={elapsed:F1}s, dist={distanceToPlayer:F1}m");
                    break;
                }

                var atWaitingPoint = false;
                if (!playerOutside && positionHistory.Count >= 2)
                {
                    var target = GetFallbackPosition(positionHistory, fallbackIndex);
                    if (target.HasValue && Vector3.Distance(dealerPos, target.Value) <= ARRIVAL_DISTANCE)
                        atWaitingPoint = true;
                }

                if (!atWaitingPoint && IsDealerGoingAway(dealerPlayerHistory))
                {
                    if (playerOutside)
                    {
                        GiveUp(dealer, movement, scheduleManager, originalWalkSpeed, "Dealer gave up: moving away from player");
                        yield break;
                    }
                    fallbackIndex++;
                    dealerPlayerHistory.Clear();
                    lastRepath = 0f;
                    if (fallbackIndex >= ModSettings.GetMaxFallbackAttempts())
                    {
                        GiveUp(dealer, movement, scheduleManager, originalWalkSpeed, "Dealer gave up: moving away at all fallback positions");
                        yield break;
                    }
                }

                if (playerOutside)
                {
                    positionHistory.Add(player.PlayerBasePosition);
                    if (positionHistory.Count > POSITION_HISTORY_SIZE)
                        positionHistory.RemoveAt(0);

                    var shouldRepath = hasDest == false || elapsed - lastRepath >= ModSettings.GetRepathInterval();
                    if (shouldRepath)
                    {
                        movement.SetDestination(player.transform);
                        lastRepath = elapsed;
                    }
                    playerWasOutside = true;
                }
                else
                {
                    if (playerWasOutside)
                    {
                        fallbackIndex = 0;
                        playerWasOutside = false;
                    }

                    var targetPosition = GetFallbackPosition(positionHistory, fallbackIndex);
                    if (targetPosition.HasValue)
                    {
                        var distanceToTarget = Vector3.Distance(dealerPos, targetPosition.Value);
                        var reachedWaitingPoint = distanceToTarget <= ARRIVAL_DISTANCE;

                        if (reachedWaitingPoint)
                        {
                            movement.Stop();
                            dealerPlayerHistory.Clear();
                            if (ModSettings.DealerMessages.Value && !sentAtDoorMessage)
                            {
                                dealer.SendTextMessage(DealerMessages.GetAtDoor());
                                sentAtDoorMessage = true;
                            }
                            if (waitingAtDoorSince < 0f)
                                waitingAtDoorSince = elapsed;
                            if (elapsed - waitingAtDoorSince >= ModSettings.GetMaxWaitAtDoor())
                            {
                                GiveUp(dealer, movement, scheduleManager, originalWalkSpeed, "Dealer gave up: waited too long at door");
                                yield break;
                            }
                        }
                        else
                        {
                            waitingAtDoorSince = -1f;
                            var shouldRepath = hasDest == false || elapsed - lastRepath >= ModSettings.GetRepathInterval();
                            if (shouldRepath)
                            {
                                if (targetMarker == null)
                                    targetMarker = new GameObject("[SimpleCall] TargetMarker");
                                targetMarker.transform.position = targetPosition.Value;
                                movement.SetDestination(targetMarker.transform);
                                lastRepath = elapsed;

                                if (!movement.HasDestination)
                                {
                                    fallbackIndex++;
                                    lastRepath = 0f;
                                    if (fallbackIndex >= ModSettings.GetMaxFallbackAttempts())
                                    {
                                        GiveUp(dealer, movement, scheduleManager, originalWalkSpeed, "Dealer gave up: no path to any fallback position");
                                        yield break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        GiveUp(dealer, movement, scheduleManager, originalWalkSpeed, "Dealer gave up: not enough position history");
                        yield break;
                    }
                }

                var shouldLog = elapsed <= LOG_INTERVAL
                    ? elapsed >= CHECK_INTERVAL - 0.01f
                    : (int)(elapsed / LOG_INTERVAL) > (int)((elapsed - CHECK_INTERVAL) / LOG_INTERVAL);
                if (shouldLog)
                {
                    Log($"[SimpleCall] t={elapsed:F1}s | dist={distanceToPlayer:F1}m | dealer=({dealerPos.x:F0},{dealerPos.y:F0},{dealerPos.z:F0}) | player=({playerPos.x:F0},{playerPos.y:F0},{playerPos.z:F0}) | HasDest={hasDest} | ScheduleOn={scheduleManager?.ScheduleEnabled ?? false}");
                }
            }

            Log("[SimpleCall] Arrival: Stop, restore WalkSpeed");
            if (movement != null)
            {
                movement.Stop();
                if (originalWalkSpeed > 0f)
                    movement.WalkSpeed = originalWalkSpeed;
            }

            var meetDelay = ModSettings.GetMeetDelay();
            var meetElapsed = 0f;
            var sentLeavingMessage = false;

            while (meetElapsed < meetDelay)
            {
                yield return new WaitForSeconds(CHECK_INTERVAL);
                meetElapsed += CHECK_INTERVAL;

                if (player == null || dealer == null) yield break;

                var distToPlayer = Vector3.Distance(dealer.transform.position, player.PlayerBasePosition);
                if (distToPlayer > ARRIVAL_DISTANCE)
                {
                    if (ModSettings.DealerMessages.Value && !sentLeavingMessage)
                    {
                        dealer.SendTextMessage(DealerMessages.GetLeaving());
                        sentLeavingMessage = true;
                    }
                    yield break;
                }
            }

            var returnMarker = new GameObject("[SimpleCall] ReturnMarker");
            returnMarker.transform.position = dealerInitialPosition;
            movement?.SetDestination(returnMarker.transform);

            if (ModSettings.DealerMessages.Value && !sentLeavingMessage)
                dealer.SendTextMessage(DealerMessages.GetLeaving());

            while (dealer != null && movement != null && Vector3.Distance(dealer.transform.position, dealerInitialPosition) > ARRIVAL_DISTANCE)
            {
                yield return new WaitForSeconds(CHECK_INTERVAL);
                if (scheduleManager?.ScheduleEnabled == true)
                    scheduleManager.DisableSchedule();
            }

            Object.Destroy(returnMarker);
            Log("[SimpleCall] Arrival: EnableSchedule");
            scheduleManager?.EnableSchedule();
        }
        finally
        {
            _dealersInCall.Remove(dealerId);
            if (targetMarker != null)
            {
                Object.Destroy(targetMarker);
            }
        }
    }

    private static Vector3? GetFallbackPosition(List<Vector3> history, int fallbackIndex)
    {
        var nthLast = fallbackIndex + 2;
        if (history.Count < nthLast)
            return null;
        return history[history.Count - nthLast];
    }

    private static bool IsDealerGoingAway(List<(Vector3 dealer, Vector3 player)> history)
    {
        if (history.Count < GOING_AWAY_SAMPLE_COUNT + 1)
            return false;
        var awayCount = 0;
        for (var i = 1; i < history.Count; i++)
        {
            var prev = history[i - 1];
            var curr = history[i];
            var dealerVelocity = curr.dealer - prev.dealer;
            if (dealerVelocity.sqrMagnitude < 0.0001f)
                continue;
            var directionToPlayer = (curr.player - curr.dealer).normalized;
            if (Vector3.Dot(dealerVelocity.normalized, directionToPlayer) > 0f)
                return false;
            awayCount++;
        }
        return awayCount >= 5;
    }

    private static void GiveUp(NPC dealer, NPCMovement movement, NPCScheduleManager scheduleManager, float originalWalkSpeed, string message)
    {
        MelonLogger.Error($"[SimpleCall] {message}");
        movement?.Stop();
        if (movement != null && originalWalkSpeed > 0f)
            movement.WalkSpeed = originalWalkSpeed;
        scheduleManager?.EnableSchedule();
    }
}
