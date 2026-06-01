using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 全局（非按角色）配置：槽位数、显示位置、每槽切换键、使用键、面板快捷键。
    /// </summary>
    internal sealed class HotbarConfig
    {
        internal const int DefaultKeyedSlots = 9;

        private static readonly KeyboardShortcut Unbound = new KeyboardShortcut(KeyCode.None);

        private readonly ConfigFile _config;
        private readonly Dictionary<int, ConfigEntry<KeyboardShortcut>> _slotHotkeys
            = new Dictionary<int, ConfigEntry<KeyboardShortcut>>();

        internal ConfigEntry<int> SlotCount { get; }
        internal ConfigEntry<int> RowCount { get; }
        internal ConfigEntry<bool> Visible { get; }
        internal ConfigEntry<float> AnchorX { get; }
        internal ConfigEntry<float> AnchorY { get; }
        internal ConfigEntry<float> OffsetX { get; }
        internal ConfigEntry<float> OffsetY { get; }
        internal ConfigEntry<float> Scale { get; }
        internal ConfigEntry<float> BackgroundAlpha { get; }

        internal ConfigEntry<KeyboardShortcut> UseItemHotkey { get; }
        internal ConfigEntry<KeyboardShortcut> ToggleSettingsHotkey { get; }
        internal ConfigEntry<bool> ShowLogInConsole { get; }
        internal ConfigEntry<bool> AcceptUpdateNotice { get; }
        internal ConfigEntry<bool> EnableScroll { get; }
        internal ConfigEntry<bool> AutoRefill { get; }

        internal HotbarConfig(ConfigFile config)
        {
            _config = config;
            SlotCount = config.Bind("Hotbar", "SlotCount", 9,
                new ConfigDescription("快捷栏槽位数量。", new AcceptableValueRange<int>(1, int.MaxValue)));
            RowCount = config.Bind("Hotbar", "RowCount", 1,
                new ConfigDescription("快捷栏排数，槽位按网格分排显示。", new AcceptableValueRange<int>(1, int.MaxValue)));
            Visible = config.Bind("Hotbar", "Visible", true, "是否显示快捷栏。");
            Scale = config.Bind("Hotbar", "Scale", 1.0f,
                new ConfigDescription("快捷栏整体缩放。", new AcceptableValueRange<float>(0.5f, 2.0f)));
            BackgroundAlpha = config.Bind("Hotbar", "BackgroundAlpha", 0.55f,
                new ConfigDescription("槽位背景与边框透明度（不影响物品图标）。", new AcceptableValueRange<float>(0f, 1f)));

            AnchorX = config.Bind("Position", "AnchorX", 0.5f,
                new ConfigDescription("屏幕锚点 X（0 左 1 右）。", new AcceptableValueRange<float>(0f, 1f)));
            AnchorY = config.Bind("Position", "AnchorY", 0f,
                new ConfigDescription("屏幕锚点 Y（0 下 1 上）。", new AcceptableValueRange<float>(0f, 1f)));
            OffsetX = config.Bind("Position", "OffsetX", 0f, "相对锚点的像素偏移 X。");
            OffsetY = config.Bind("Position", "OffsetY", 90f, "相对锚点的像素偏移 Y。");

            UseItemHotkey = config.Bind("Hotkeys", "UseItemHotkey",
                new KeyboardShortcut(KeyCode.Mouse1, KeyCode.LeftShift),
                "使用主手物品的按键，支持组合键（如 Shift+鼠标右键）。未绑定则关闭该功能。");
            ToggleSettingsHotkey = config.Bind("Hotkeys", "ToggleSettingsHotkey", Unbound,
                "打开 / 关闭快捷栏设置面板。默认未绑定。");

            ShowLogInConsole = config.Bind("Misc", "ShowInConsole", false,
                "是否把模组日志同步打印到游戏内控制台（` 键打开）。");
            AcceptUpdateNotice = config.Bind("Misc", "AcceptUpdateNotice", true,
                "是否在启动时检查 GitHub 新版本并在游戏内提示。关闭则不检测不提示。");
            EnableScroll = config.Bind("Hotbar", "EnableScroll", true,
                "鼠标滚轮切换当前选中槽并切到主手。");
            AutoRefill = config.Bind("Hotbar", "AutoRefill", true,
                "槽内物品丢出或用尽消失后，若身上仍有同种物品则自动补上，不取消绑定。");
        }

        internal static bool TriggeredThisFrame(ConfigEntry<KeyboardShortcut> entry)
        {
            var sc = entry.Value;
            if (sc.MainKey == KeyCode.None) return false;
            return sc.IsDown();
        }

        /// <summary>第 index（0 基）槽位的切换键，按需 Bind；前 9 槽默认小键盘 1-9，其余默认未绑。</summary>
        internal ConfigEntry<KeyboardShortcut> SlotHotkey(int index)
        {
            if (_slotHotkeys.TryGetValue(index, out var entry)) return entry;
            var def = index < DefaultKeyedSlots
                ? new KeyboardShortcut(KeyCode.Keypad1 + index)
                : Unbound;
            entry = _config.Bind("Hotkeys", "SlotHotkey" + (index + 1), def,
                $"切换到第 {index + 1} 个槽位的按键，支持组合键。");
            _slotHotkeys[index] = entry;
            return entry;
        }
    }
}
