using System;
using Mirror;
using UnityEngine;

namespace ClearBattlefield
{
    /// <summary>Owns the server's <see cref="DropTracker"/> and removes tracked weapons through Mirror.</summary>
    internal static class BattlefieldClearer
    {
        public static readonly DropTracker Tracker = new DropTracker();

        public static double Now => Time.realtimeSinceStartupAsDouble;

        /// <summary>Host or single player, inside a level. Enemies only drop items in levels.</summary>
        public static bool Available =>
            NetworkServer.active && GameManager.Instance != null && GameManager.Instance.SceneType == GameSceneType.Level;

        public static void Reset(string reason)
        {
            var count = Tracker.Count;
            Tracker.Reset();
            if (count > 0)
                Plugin.Log.LogInfo($"{reason}; forgot {count} tracked drop(s)");
        }

        /// <summary>How many tracked weapons a clear would remove right now.</summary>
        public static int CountClearable() =>
            NetworkServer.active ? Tracker.SelectClearable(Now, MinAge, StateOf).Count : 0;

        public static int Clear()
        {
            if (!NetworkServer.active)
                return 0;

            var removed = 0;
            foreach (var netId in Tracker.SelectClearable(Now, MinAge, StateOf))
            {
                if (!NetworkServer.spawned.TryGetValue(netId, out var identity) || identity == null)
                    continue;
                try
                {
                    // Same call the game uses to remove picked-up and merged items; clients are told to destroy it too.
                    NetworkServer.Destroy(identity.gameObject);
                    removed++;
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Could not remove dropped weapon {netId}: {ex}");
                }
                Tracker.Untrack(netId);
            }

            Plugin.Log.LogInfo($"Cleared {removed} dropped weapon(s); {Tracker.Count} still tracked");
            Notify(ClearText.Result(removed));
            return removed;
        }

        private static double MinAge => Math.Max(0f, Plugin.MinDropAgeSeconds.Value);

        private static DropState StateOf(uint netId)
        {
            if (!NetworkServer.spawned.TryGetValue(netId, out var identity) || identity == null
                || !identity.TryGetComponent<PickableItem>(out var item) || item == null)
                return DropState.Gone;
            // A player is mid-pickup; the game destroys or releases it this frame.
            return item.NetworkSyncPlayerID != 0 ? DropState.Busy : DropState.OnGround;
        }

        /// <summary>The game's HUD notification. Only the host sees it; it counts down in game time, so after unpausing in solo.</summary>
        private static void Notify(string message)
        {
            try
            {
                NotificationHolder.Instance?.CreateNotification(new BasicNotification
                {
                    Title = "Clear Battlefield",
                    Description = message,
                    DisplayTime = 4f,
                });
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Could not show the clear notification: {ex.Message}");
            }
        }
    }
}
