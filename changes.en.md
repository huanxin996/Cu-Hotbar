# CuHotbar Changelog

> 中文更新日志：见 [changes.md](changes.md)

## v1.0.2

### Added

- **Horizontal / vertical layout**: choose the hotbar's orientation. Horizontal defaults to the bottom of the screen, vertical to the right side; switching orientation applies the matching default position automatically.
- **Multi-row / multi-column**: "Rows" means rows when horizontal and columns when vertical; slots are laid out in a grid.
- **Bind by pointing**: point the mouse at an inventory cell or a world item and press a slot's switch key to bind that item to the slot — no dragging needed.
- **Safe quick use**: optional toggle; when on, pressing a slot key or scrolling to an edible / drinkable consumable (detected by its actual use behavior — whether it eats / drinks, covering foods classified as "custom" like geofruit, including buffs with side effects) selects it without switching to the main hand, leaving the held item untouched; whether it's used is up to your "use key" (default Shift+Right-click). The use key branches by selected-slot type: for a consumable it uses the bound item (which may be in a backpack), for anything else it only uses the item in the main hand — explosives, magazines, syringes etc. are not recognized as consumables and won't be triggered by the use key.

### Changed

- When switching items, a held item with nowhere to go is no longer dropped immediately: it goes to an empty body slot first, then into a backpack container that can hold it; only when neither works is it dropped, with an on-screen notice (the notice can be turned off in settings).

## v1.0.1

### Added

- **Per-slot switch keys**: every slot is now individually bindable and supports key combos (e.g. `Ctrl+1`). The first 9 slots still default to Keypad 1-9; extra slots are unbound by default.
- **Key badge on slots**: each slot's top-left badge now shows its actual bound key and updates instantly when rebound; blank when unbound.
- **Custom use key with combos**: the old "right-click to use" toggle is now a configurable key with combo support, defaulting to Shift+Right-click; clear the binding to disable it.
- **Multi-row layout**: a new "Rows" setting lays slots out in a grid across multiple rows.
- **Unlimited slot count**: the 9-slot cap is removed; add as many as you need.

### Changed

- Reworked the "Switch keys" settings section: removed the keypad/main-row base toggle and the right-click-use toggle, replaced with per-slot and use-key binding rows; slot count and rows use -/+ steppers.

### Notes

- Switch keys and the use key accept mouse buttons and combos; click the button then press a new key to bind, or "Clear" to unbind.
- Slot count and rows can keep growing; very large values may overflow the screen — adjust rows and scale accordingly.
