## Project Context

A host-only BepInEx mod for Stonewards. It adds a "Clear Battlefield" button to the Esc/pause menu that removes only weapons dropped by enemies. It marks drops when they spawn by using a Harmony hook on the enemy drop path, which records weapon stack netIds in a server-side set. When the button is used, the mod destroys only the tracked stacks that still exist, through Mirror, so all clients stay in sync. Repo layout, build, packaging, tests and the Thunderstore release follow the user's existing mods: stonewards-upgrade-queue, stonewards-mod-settings and stonewards-persistent-multitome.

## Goals

- Research phase first: decompile Assembly-CSharp.dll with ilspycmd (DOTNET_ROOT set; older decompile may be in /tmp/sw), then report back before writing any code. The report covers: the exact method where enemies spawn drops (around EnemyController's DropItemChance roll, ItemDropPoolSO and RarityDropRateSO) and whether weapons use a different path; how weapons are detected (ItemDataSO or WeaponDataSO); how stacks are tracked and cleared, including merges and pickups; how the button is added to the pause menu (UIPauseMenu, BaseMenu); and any open questions
- Harmony-hook the enemy drop spawn path, possibly PickupItemStack.ServerInit, and record the spawned stack's netId in a server-side set, only when the item is a weapon
- Never guess from position or item type afterwards. Everything else on the ground must stay: weapons from the level or map (pedestals, rewards, missions, shops, chests), weapons dropped by players, resources, scrap, upgrade or tome pickups (PickupUpgradeItemDataSO) and storage (StorageItemStack)
- Remove IDs from the set when a stack is picked up or despawned, and clear the set when the level changes or the run ends
- Find out how the game handles an enemy-dropped stack merging with a player-dropped or placed stack, report it, and pick the safe option: keep the stack (untrack it)
- Add a pause-menu button that is visible only when NetworkServer.active (host or single player). It asks for confirmation, clears tracked stacks on the server with NetworkServer.Destroy or the game's own despawn path (e.g. ShatterStone PickupController.DespawnItem), logs how many were removed, and shows a short message such as 'Cleared 37 dropped weapons' if the game has an easy way to do that
- Config options: require confirmation (default on), keybind (default none), minimum drop age in seconds before a weapon can be cleared (default 0)
- Use the template repos' structure: csproj with lib/refs fallback, scripts/update-refs.sh, scripts/package.sh, a tag-triggered .github/workflows/release.yml with Thunderstore auto-publish, a thunderstore/ manifest, README, 256x256 icon and CHANGELOG
- Add xunit tests for the pure tracking and filtering logic
- Build against the real game and deploy to the r2modman Default profile. Also build against lib/refs, build the package, and set the THUNDERSTORE_TOKEN secret the same way as the other repos
- Do not commit, push or tag until the user asks, because a tag publishes to Thunderstore permanently

## Out of scope

- Picking up, auto-collecting or merging items
- Changing drop rates
- Clearing anything that isn't an enemy-dropped weapon
- Client-side install requirement (host-only mod)

## Suggested stack

- **C# / BepInEx plugin** — Matches the user's existing Stonewards mods
- **HarmonyLib** — Hooks the enemy drop spawn path so drops are marked when they spawn
- **Mirror networking (NetworkServer)** — The game's netcode. Clearing on the server keeps all clients in sync
- **xunit** — Unit tests for pure tracking and filtering logic, as in the template repos
- **GitHub Actions + Thunderstore** — Tag-triggered release with auto-publish, same as the other repos
- **ilspycmd** — Decompiles Assembly-CSharp.dll for the research phase
