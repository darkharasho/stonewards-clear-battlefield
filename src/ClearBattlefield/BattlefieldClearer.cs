using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ClearBattlefield
{
    /// <summary>Owns the server's <see cref="DropTracker"/> and removes tracked enemy drops, and optionally scrap, through Mirror.</summary>
    internal static class BattlefieldClearer
    {
        public static readonly DropTracker Tracker = new DropTracker();

        private static readonly List<uint> EmptyIds = new List<uint>();

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

        /// <summary>How many enemy drops and pieces of scrap a clear of the given scope would remove right now.</summary>
        public static (int drops, int scrap) CountClearable(bool includeDrops, bool includeScrap)
        {
            if (!NetworkServer.active)
                return (0, 0);
            return (includeDrops ? Tracker.SelectClearable(Now, MinAge, StateOf).Count : 0, ScrapOnGround(includeScrap).Count);
        }

        public static int Clear(bool includeDrops, bool includeScrap)
        {
            if (!NetworkServer.active)
                return 0;

            // Collect scrap before destroying anything, since destroying changes NetworkServer.spawned.
            var scrap = ScrapOnGround(includeScrap);
            var drops = 0;
            foreach (var netId in includeDrops ? Tracker.SelectClearable(Now, MinAge, StateOf) : EmptyIds)
            {
                if (Destroy(netId))
                    drops++;
                Tracker.Untrack(netId);
            }
            var scrapRemoved = 0;
            foreach (var netId in scrap)
            {
                if (Destroy(netId))
                    scrapRemoved++;
            }

            Plugin.Log.LogInfo($"Cleared {drops} enemy drop(s) and {scrapRemoved} scrap; {Tracker.Count} drop(s) still tracked");
            Notify(ClearText.Result(drops, scrapRemoved));
            return drops + scrapRemoved;
        }

        private static bool Destroy(uint netId)
        {
            if (!NetworkServer.spawned.TryGetValue(netId, out var identity) || identity == null)
                return false;
            try
            {
                // Same call the game uses to remove picked-up and merged items; clients are told to destroy it too.
                NetworkServer.Destroy(identity.gameObject);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Could not remove item {netId}: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Scrap on the ground, when the clear includes it. Scrap only spawns from digging (players take the
        /// produced resource, never the scrap itself), so any ScrapPickableItem not being picked up is leftover scrap.
        /// Enemy drops are left to the tracker so they aren't counted twice.
        /// </summary>
        private static List<uint> ScrapOnGround(bool include)
        {
            var result = new List<uint>();
            if (!include)
                return result;
            foreach (var identity in NetworkServer.spawned.Values)
            {
                if (identity == null || !identity.TryGetComponent<ScrapPickableItem>(out var scrap) || scrap == null)
                    continue;
                if (scrap.NetworkSyncPlayerID == 0 && !Tracker.IsTracked(identity.netId))
                    result.Add(identity.netId);
            }
            return result;
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
