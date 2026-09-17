# stonewards-clear-battlefield

A host-only BepInEx mod for Stonewards. It adds a **Clear Battlefield** button to the top-right of the pause menu that removes items dropped by enemies and, optionally, leftover scrap. Player-facing docs are in [thunderstore/README.md](thunderstore/README.md).

## How it works

- **Marking drops.** `ItemManager.ServerInstantiateEnemyDrop` is only called from `EnemyController.TrySpawnRandomDrop` (a player kills an enemy and the `DropItemChance` roll succeeds). A postfix records the spawned `PickableItem`'s netId and spawn time whatever the item is. Player drops, chests, barrels, props and digging spawn through other methods and are never marked.
- **Forgetting drops.** `PickableItem.OnDestroy` untracks picked-up, merged-away and fallen items (`NetworkServer.Destroy` keeps netId until then). The set is cleared on `MetaProgressManager.StartRun`/`EndRun` and `NetworkServer.Shutdown`, which resets netIds.
- **Merges.** Stackable items with the same ID merge after about a second (`PickableItem.Merge`). The surviving stack stays tracked only if both stacks were enemy drops.
- **Clearing.** Tracked IDs are resolved through `NetworkServer.spawned`. Missing ones are dropped, ones a player is picking up (`SyncPlayerID != 0`) are skipped, the rest are removed with `NetworkServer.Destroy`, the same call the game uses.
- **Scrap.** With `ClearScrap` on, every `ScrapPickableItem` in `NetworkServer.spawned` that isn't being picked up and isn't a tracked enemy drop is also destroyed. Scrap only spawns from digging (`DiggingManager`), and players collect the produced resource rather than the scrap item, so no player-dropped item is a `ScrapPickableItem`. The minimum age doesn't apply because scrap spawn times aren't tracked.
- **UI.** The button copies the Settings button's style and sits absolutely in the pause document's top-right. It shows only while the pause screen is interactive, on the server, in a level. Confirmation reuses the pause menu's `UIConfirmationPopup`; the result uses `NotificationHolder`.

The pure tracking logic is in `DropTracker.cs` and `ClearText.cs`, tested in `tests/`.

## Build

```sh
dotnet build src/ClearBattlefield          # against the installed game; deploys to the r2modman Default profile
dotnet build src/ClearBattlefield -p:UseRefs=true   # against lib/refs, as CI does
dotnet test tests/ClearBattlefield.Tests
scripts/package.sh                          # dist/darkharasho-ClearBattlefield-<version>.zip
scripts/update-refs.sh                      # regenerate lib/refs after a game update
```

Paths can be overridden in a gitignored `Local.props` or with `-p:GamePath=... -p:ProfilePath=...`.

## Release

Bump the version in `thunderstore/manifest.json`, `Plugin.cs` and the csproj, add a `CHANGELOG.md` entry, then push a `v<version>` tag. The workflow tests, builds against `lib/refs`, creates the GitHub release and publishes to Thunderstore with the `THUNDERSTORE_TOKEN` secret. A published version can't be replaced.
