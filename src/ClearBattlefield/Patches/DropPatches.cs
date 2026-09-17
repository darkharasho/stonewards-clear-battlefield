using HarmonyLib;
using Mirror;

namespace ClearBattlefield.Patches
{
    /// <summary>
    /// ItemManager.ServerInstantiateEnemyDrop is only called from EnemyController.TrySpawnRandomDrop, when a player
    /// kills an enemy and the drop chance roll succeeds. Player drops, chests, barrels, props and digging all spawn
    /// through other methods, so anything tagged here is an enemy drop. The item is already spawned, so netId is set.
    /// </summary>
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.ServerInstantiateEnemyDrop))]
    internal static class TrackEnemyDropPatch
    {
        private static void Postfix(PickableItem __result)
        {
            if (__result == null || !NetworkServer.active)
                return;
            BattlefieldClearer.Tracker.Track(__result.netId, BattlefieldClearer.Now);
        }
    }

    /// <summary>
    /// Picked-up, merged-away and fallen items all go through NetworkServer.Destroy, which keeps netId intact until
    /// Unity destroys the object at the end of the frame.
    /// </summary>
    [HarmonyPatch(typeof(PickableItem), "OnDestroy")]
    internal static class UntrackDestroyedPatch
    {
        private static void Postfix(PickableItem __instance)
        {
            if (__instance.netId != 0)
                BattlefieldClearer.Tracker.Untrack(__instance.netId);
        }
    }

    /// <summary>
    /// Stackable items with the same ID merge about a second after spawning. Merge(other) starts moving other into
    /// this stack; the survivor stays clearable only if both were enemy drops.
    /// </summary>
    [HarmonyPatch(typeof(PickableItem), "Merge")]
    internal static class MergePatch
    {
        private static readonly AccessTools.FieldRef<PickableItem, PickableItem> MergeTarget =
            AccessTools.FieldRefAccess<PickableItem, PickableItem>("mergeTarget");

        private static void Postfix(PickableItem __instance, PickableItem _Other)
        {
            // Merge returns early without merging if either stack is already busy.
            if (_Other == null || MergeTarget(_Other) != __instance)
                return;
            BattlefieldClearer.Tracker.OnMerge(__instance.netId, _Other.netId);
        }
    }

    /// <summary>
    /// MetaProgressManager brackets a run (StartRun when a level loads, EndRun on returning to the hub or disconnecting).
    /// Shutting the server down also resets Mirror's netId counter, so old IDs could point at new objects.
    /// </summary>
    internal static class ResetPatches
    {
        [HarmonyPatch(typeof(MetaProgressManager), nameof(MetaProgressManager.StartRun))]
        private static class StartRunPatch
        {
            private static void Prefix() => BattlefieldClearer.Reset("Run started");
        }

        [HarmonyPatch(typeof(MetaProgressManager), nameof(MetaProgressManager.EndRun))]
        private static class EndRunPatch
        {
            private static void Postfix() => BattlefieldClearer.Reset("Run ended");
        }

        [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.Shutdown))]
        private static class ServerShutdownPatch
        {
            private static void Prefix() => BattlefieldClearer.Reset("Server stopped");
        }
    }
}
