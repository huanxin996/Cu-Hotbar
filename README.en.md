# CuHotbar V1.0.1 for Casualties: Unknown

> A Minecraft-style hotbar mod for Casualties: Unknown
> Compatible with: SaveManager (settings merge into its panel when present; its absence does not break features)

中文说明：见 [README.md](README.md)

## Overview

This mod adds a customizable hotbar in-game: drag items from the vanilla inventory onto slots to bind them, switch an item to your main hand with each slot's own key or the scroll wheel, and use the held item with a configurable key (Shift+Right-click by default). The slot count is unlimited and can be split across multiple rows; every slot's switch key is individually bindable (combos supported), and each slot shows its bound key at the top-left. The hotbar sits at the bottom-center of the screen by default; position, scale, and opacity are all adjustable. Bindings are cached per character and persist across sessions. When SaveManager is present, settings are merged into one of its panel tabs; otherwise the mod injects its own button and panel.

## Features

- **Drag to bind**: drag an item from your body/backpack onto a slot to bind it; an item occupies only one slot, and re-binding removes it from the old slot.
- **Per-slot keys**: every slot's switch key is individually bindable and supports combos (e.g. Ctrl+1); the first 9 slots default to Keypad 1-9, extra slots are unbound by default and can be set manually.
- **Key badge**: each slot shows its actual bound key at the top-left, updated instantly when rebound; blank when unbound.
- **Quick switch**: each slot's bound key or the scroll wheel switches the active slot and moves its item to the main hand; the selected slot has a bold white border.
- **Custom use key**: use the held item with a configurable key, combos supported, defaulting to Shift+Right-click (rebind or clear it to disable in settings).
- **Multi-row layout**: the slot count is unlimited, and rows can be set to lay them out in a grid.
- **Return to origin**: when switching/clearing, the previous item is returned in order of "original spot → empty body slot → dropped".
- **Auto-refill**: after the held item is thrown or used up, if you still carry the same kind it is auto-refilled to the main hand (can be disabled in settings).
- **Status display**: a slot shows the count of the same item on you, plus a bottom bar for battery charge / magazine rounds / condition.
- **Adjustable look**: slot count, rows, overall scale, background opacity (item icons stay opaque), screen anchor and offset are all configurable.
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
| Switch and hold | Each slot's bound key (first 9 default to Keypad 1-9) / scroll wheel |
| Use held item | Shift+Right-click (rebindable) |
| Take out of slot | Drag out of the slot (using on a limb, returning to backpack, etc. follow vanilla logic) |

### Settings panel

| Section | Purpose |
|---------|---------|
| Display | Visibility toggle, slot count, rows, overall scale, background opacity |
| Switch keys | Per-slot switch key, use-held-item key, scroll switch, auto-refill |
| Position | Screen anchor X/Y, pixel offset X/Y, reset position |
| Hotkeys | Open / close the settings panel |
| Misc | Show mod logs in game console, accept update notifications |

> Switch keys and the use key both support combos. Click the matching button, then press a new key (mouse buttons included) to bind, or "Clear" to unbind. Slot count and rows can keep increasing; set too large and slots may overflow the screen.

## Soft dependencies

- **SaveManager**: when present, this mod registers its settings as a tab via SaveManager's extension point for a unified style; when absent or on a version without the extension point, it falls back to a standalone main-menu button + in-game launcher button + its own settings window.

## Update check

On startup it compares against the latest GitHub release and shows a red notice at the top-left when a new version exists; click it to open the release page. It can be turned off under Settings → Misc.

## Related

- [CasualtiesUnknown-SkinEditor](https://github.com/huanxin996/CasualtiesUnknown-SkinEditor): live preview and animation preview.
- Repository: [huanxin996/Cu-Hotbar](https://github.com/huanxin996/Cu-Hotbar)
