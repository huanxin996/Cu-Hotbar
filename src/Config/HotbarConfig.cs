using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 全局（非按角色）配置：槽位数、显示位置、切换按键基准、面板快捷键。
    /// </summary>
    internal sealed class HotbarConfig
    {
        private static readonly KeyboardShortcut Unbound = new KeyboardShortcut(KeyCode.None);

        internal ConfigEntry<int> SlotCount { get; }
        internal ConfigEntry<bool> Visible { get; }
        internal ConfigEntry<bool> UseKeypad { get; }
        internal ConfigEntry<float> AnchorX { get; }
        internal ConfigEntry<float> AnchorY { get; }
        internal ConfigEntry<float> OffsetX { get; }
        internal ConfigEntry<float> OffsetY { get; }
        internal ConfigEntry<float> Scale { get; }
        internal ConfigEntry<float> BackgroundAlpha { get; }

        internal ConfigEntry<KeyboardShortcut> ToggleSettingsHotkey { get; }
        internal ConfigEntry<bool> ShowLogInConsole { get; }
        internal ConfigEntry<bool> AcceptUpdateNotice { get; }
        internal ConfigEntry<bool> RightClickUse { get; }
        internal ConfigEntry<bool> EnableScroll { get; }
        internal ConfigEntry<bool> AutoRefill { get; }

        internal HotbarConfig(ConfigFile config)
        {
            SlotCount = config.Bind("Hotbar", "SlotCount", 9,
                new ConfigDescription("快捷栏槽位数量。", new AcceptableValueRange<int>(1, 9)));
            Visible = config.Bind("Hotbar", "Visible", true, "是否显示快捷栏。");
            UseKeypad = config.Bind("Hotbar", "UseKeypad", true,
                "切换按键基准：true=小键盘 Keypad1-9；false=主键盘 Alpha1-9（与游戏倍速键冲突）。");
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

            ToggleSettingsHotkey = config.Bind("Hotkeys", "ToggleSettingsHotkey", Unbound,
                "打开 / 关闭快捷栏设置面板。默认未绑定。");
            ShowLogInConsole = config.Bind("Misc", "ShowInConsole", false,
                "是否把模组日志同步打印到游戏内控制台（` 键打开）。");
            AcceptUpdateNotice = config.Bind("Misc", "AcceptUpdateNotice", true,
                "是否在启动时检查 GitHub 新版本并在游戏内提示。关闭则不检测不提示。");
            RightClickUse = config.Bind("Hotbar", "RightClickUse", true,
                "切到主手的快捷栏物品，直接按右键即可使用（无需对准物品）。");
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

        /// <summary>第 index（0 基）个槽位的切换键。</summary>
        internal KeyCode SlotKey(int index)
        {
            if (index < 0 || index > 8) return KeyCode.None;
            return UseKeypad.Value
                ? KeyCode.Keypad1 + index
                : KeyCode.Alpha1 + index;
        }
    }
}
