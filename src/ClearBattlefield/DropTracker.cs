using System;
using System.Collections.Generic;

namespace ClearBattlefield
{
    /// <summary>What the game world says about a tracked netId when the battlefield is cleared.</summary>
    public enum DropState
    {
        /// <summary>No longer spawned: picked up, merged away, fell out of the world or the scene unloaded.</summary>
        Gone,
        /// <summary>Still spawned but not safe to remove right now, e.g. a player is picking it up.</summary>
        Busy,
        /// <summary>Lying on the ground.</summary>
        OnGround,
    }

    /// <summary>
    /// Server-side record of weapons spawned by enemy drops, keyed by Mirror netId with the time each one spawned.
    /// Knows nothing about Unity so it can be unit tested; the patches feed it.
    /// </summary>
    public sealed class DropTracker
    {
        private readonly Dictionary<uint, double> _spawnTimes = new Dictionary<uint, double>();

        public int Count => _spawnTimes.Count;

        public bool IsTracked(uint netId) => _spawnTimes.ContainsKey(netId);

        public void Track(uint netId, double spawnTime)
        {
            if (netId != 0)
                _spawnTimes[netId] = spawnTime;
        }

        public bool Untrack(uint netId) => _spawnTimes.Remove(netId);

        /// <summary>
        /// A merge moves <paramref name="absorbed"/> into <paramref name="survivor"/> and destroys it. The survivor only
        /// stays clearable if both stacks were enemy drops; otherwise both are untracked so nothing a player dropped or
        /// the level placed is ever removed. When both were drops, both stay tracked so clearing mid-merge removes the
        /// pair instead of leaving the absorbed stack frozen.
        /// </summary>
        /// <returns>True if the survivor is still tracked.</returns>
        public bool OnMerge(uint survivor, uint absorbed)
        {
            if (IsTracked(survivor) && IsTracked(absorbed))
                return true;
            Untrack(survivor);
            Untrack(absorbed);
            return false;
        }

        public void Reset() => _spawnTimes.Clear();

        /// <summary>
        /// Picks the tracked drops to remove now: on the ground and at least <paramref name="minAgeSeconds"/> old.
        /// Drops that are gone are forgotten; busy or too-young drops stay tracked for next time.
        /// </summary>
        public List<uint> SelectClearable(double now, double minAgeSeconds, Func<uint, DropState> stateOf)
        {
            var clearable = new List<uint>();
            var gone = new List<uint>();
            foreach (var entry in _spawnTimes)
            {
                switch (stateOf(entry.Key))
                {
                    case DropState.Gone:
                        gone.Add(entry.Key);
                        break;
                    case DropState.OnGround when now - entry.Value >= minAgeSeconds:
                        clearable.Add(entry.Key);
                        break;
                }
            }
            foreach (var netId in gone)
                _spawnTimes.Remove(netId);
            clearable.Sort();
            return clearable;
        }
    }
}
