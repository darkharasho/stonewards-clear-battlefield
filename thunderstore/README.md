# ClearBattlefield

Long fights leave the ground covered in whatever enemies dropped. Open the pause menu and press **Clear Battlefield** in the top-right corner to remove it all at once.

## What gets removed

Everything that dropped from an enemy you killed, whatever the item is. The mod marks each drop as it spawns, so it never guesses from position or item type afterwards.

With **Also clear scrap** turned on, scrap still lying on the ground (such as scrap wood from digging) is removed too. That includes scrap from any player's digging.

Everything else stays:

- items from the level: chests, rewards, missions, shops, barrels and props
- items a player dropped or threw
- an enemy drop that merged with a stack a player dropped. The merged stack is kept.

Items someone is picking up at that moment are skipped. Marks are forgotten when the run ends.

## Using it

- The button shows in the pause menu for the host, in levels only.
- It asks for confirmation first and tells you how much it will remove.
- Afterwards a notification says how much was cleared. In single player the game is paused, so it appears once you close the menu.
- You can also bind keys (none by default): one for the same clear as the button, one for enemy drops only and one for scrap only. The scrap key works whether or not **Also clear scrap** is on, so you can leave that setting off and clear wood on demand.

## Multiplayer

Only the host needs the mod. Items are removed on the host through the game's networking, so they disappear for every player. Clients don't need it installed and don't see the button.

## Configuration

Edit in r2modman under **Config editor** → `com.darkharasho.stonewards.clearbattlefield.cfg`, in `BepInEx/config`, or in game with ModSettings.

| Setting | Default | Description |
| --- | --- | --- |
| General → RequireConfirmation | `true` | Ask before removing. Applies to the button and the keybind. |
| General → MinDropAgeSeconds | `0` | Leave enemy drops newer than this many seconds. `0` clears them all. Doesn't apply to scrap. |
| General → ClearScrap | `false` | Also remove scrap left on the ground. |
| Controls → ClearKey | none | Clears the battlefield: enemy drops, plus scrap if ClearScrap is on. With confirmation on, it opens the pause menu on the confirmation. |
| Controls → ClearDropsKey | none | Clears enemy drops only, never scrap. |
| Controls → ClearScrapKey | none | Clears scrap only. Works whether or not ClearScrap is on. |

## Issues

Report bugs at https://github.com/darkharasho/stonewards-clear-battlefield/issues. Please attach the host's `BepInEx/LogOutput.log`.
