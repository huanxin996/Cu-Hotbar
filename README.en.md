# CuHotbar V1.0.0 for Casualties: Unknown

> A Minecraft-style hotbar mod for Casualties: Unknown
> Compatible with: SaveManager (settings merge into its panel when present; its absence does not break features)

中文说明：见 [README.md](README.md)

## Overview

This mod adds a customizable hotbar in-game: drag items from the vanilla inventory onto slots to bind them, switch an item to your main hand with number keys or the scroll wheel, and use the held item with a right-click. The hotbar sits at the bottom-center of the screen by default; position, scale, and opacity are all adjustable. Bindings are cached per character and persist across sessions. When SaveManager is present, settings are merged into one of its panel tabs; otherwise the mod injects its own button and panel.

## Features

- **Drag to bind**: drag an item from your body/backpack onto a slot to bind it; an item occupies only one slot, and re-binding removes it from the old slot.
- **Quick switch**: number keys (Keypad 1-9 by default) or the scroll wheel switch the active slot and move its item to the main hand; the selected slot has a bold white border.
- **Right-click to use**: while holding a hotbar item, right-click uses it directly without aiming (can be disabled in settings).
- **Return to origin**: when switching/clearing, the previous item is returned in order of "original spot → empty body slot → dropped".
- **Auto-refill**: after the held item is thrown or used up, if you still carry the same kind it is auto-refilled to the main hand (can be disabled in settings).
- **Status display**: a slot shows the count of the same item on you, plus a bottom bar for battery charge / magazine rounds / condition.
- **Adjustable look**: slot count, overall scale, background opacity (item icons stay opaque), screen anchor and offset are all configurable.
- **Per-character cache**: hotbar bindings are saved per character and restored automatically on re-entry.

## How to use

### Install

Place the `CuHotbar` folder under `BepInEx/plugins/`, containing:

- `CuHotbar.dll`

After launch, a "CuHotbar" button is injected on the main menu; in-game, opening the pause panel with ESC also shows a launcher button at the bottom-right. When SaveManager is present, settings move into its "CuHotbar" tab instead.

### Basic controls

| Action | Default key / method |
|--------|----------------------|
| Bind item to slot | Drag an item from the inventory onto a hotbar slot |
| Switch and hold | Keypad 1-9 / scroll wheel |
| Use held item | Right-click |
| Take out of slot | Drag out of the slot (using on a limb, returning to backpack, etc. follow vanilla logic) |

### Settings panel

| Section | Purpose |
|---------|---------|
| Display | Visibility toggle, slot count, overall scale, background opacity |
| Switch keys | Number-key base (keypad / main row), right-click use, scroll switch, auto-refill |
| Position | Screen anchor X/Y, pixel offset X/Y, reset position |
| Hotkeys | Open / close the settings panel |
| Misc | Show mod logs in game console, accept update notifications |

> Main-row keys 1-3 conflict with the game's time-scale keys; keeping the keypad base is recommended.

## Soft dependencies

- **SaveManager**: when present, this mod registers its settings as a tab via SaveManager's extension point for a unified style; when absent or on a version without the extension point, it falls back to a standalone main-menu button + in-game launcher button + its own settings window.

## Update check

On startup it compares against the latest GitHub release and shows a red notice at the top-left when a new version exists; click it to open the release page. It can be turned off under Settings → Misc.

## Related

- [CasualtiesUnknown-SkinEditor](https://github.com/huanxin996/CasualtiesUnknown-SkinEditor): live preview and animation preview.
- Repository: [huanxin996/Cu-Hotbar](https://github.com/huanxin996/Cu-Hotbar)
