# ClearBattlefield

Long fights leave the ground covered in weapons that enemies dropped. Open the pause menu and press **Clear Battlefield** in the top-right corner to remove them all at once.

## What gets removed

Only weapons that dropped from an enemy you killed. The mod marks each one as it drops, so it never guesses from position or item type afterwards.

Everything else stays:

- weapons from the level: chests, rewards, missions, shops, barrels and props
- weapons a player dropped or threw
- resources, scrap, tomes and upgrades, and anything else that isn't a weapon
- an enemy-dropped weapon that merged with a stack a player dropped. The merged stack is kept.

Weapons someone is picking up at that moment are skipped. Marks are forgotten when the run ends.

## Using it

- The button shows in the pause menu for the host, in levels only.
- It asks for confirmation first and tells you how many weapons it will remove.
- Afterwards a notification says how many were cleared. In single player the game is paused, so it appears once you close the menu.
- You can also bind a key to it (none by default).

## Multiplayer

Only the host needs the mod. Items are removed on the host through the game's networking, so they disappear for every player. Clients don't need it installed and don't see the button.

## Configuration

Edit in r2modman under **Config editor** → `com.darkharasho.stonewards.clearbattlefield.cfg`, in `BepInEx/config`, or in game with ModSettings.

| Setting | Default | Description |
| --- | --- | --- |
| General → RequireConfirmation | `true` | Ask before removing. Applies to the button and the keybind. |
| General → MinDropAgeSeconds | `0` | Leave weapons that dropped less than this many seconds ago. |
| Controls → ClearKey | none | Clears the battlefield. With confirmation on, it opens the pause menu on the confirmation. |

## Issues

Report bugs at https://github.com/darkharasho/stonewards-clear-battlefield/issues. Please attach the host's `BepInEx/LogOutput.log`.
