using System.Collections.Generic;
using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>极简 i18n：按 Locale.currentLangName 选择中英文，找不到 key 时回落 key 本身。</summary>
    internal static class HotbarI18n
    {
        private static readonly string[] ChineseKeywords =
        {
            "中文", "汉化", "简中", "简体", "繁中", "繁体", "繁體", "chinese"
        };

        private static readonly Dictionary<string, string> _zh = new Dictionary<string, string>
        {
            ["app.name"] = "CuHotbar",
            ["app.menu_button"] = "CuHotbar",
            ["tab.title"] = "快捷栏设置",
            ["sec.display"] = "显示",
            ["sw.visible"] = "显示快捷栏",
            ["fmt.slot_count"] = "槽位数量：{0}",
            ["fmt.row_count"] = "排数：{0}",
            ["fmt.col_count"] = "列数：{0}",
            ["lbl.direction"] = "排列方向：",
            ["opt.horizontal"] = "横排",
            ["opt.vertical"] = "竖排",
            ["fmt.scale"] = "缩放：{0:0.00}",
            ["fmt.bg_alpha"] = "背景透明度：{0:0.00}",
            ["sec.keys"] = "切换按键",
            ["fmt.slot_hotkey"] = "切换到槽位 {0}：",
            ["lbl.use_item"] = "使用主手物品：",
            ["sw.enable_scroll"] = "滚轮切换选中槽",
            ["sw.auto_refill"] = "同种物品用尽前自动补充",
            ["sw.warn_on_drop"] = "物品因背包满掉落时提示",
            ["sw.safe_quick_use"] = "快捷安全使用（消耗品选中不切主手）",
            ["sec.position"] = "位置",
            ["fmt.anchor_x"] = "锚点 X：{0:0.00}",
            ["fmt.anchor_y"] = "锚点 Y：{0:0.00}",
            ["fmt.offset_x"] = "偏移 X：{0:0}",
            ["fmt.offset_y"] = "偏移 Y：{0:0}",
            ["btn.reset_pos"] = "恢复默认位置",
            ["sec.hotkeys"] = "快捷键",
            ["lbl.hotkey_panel"] = "打开 / 关闭设置面板：",
            ["btn.press_a_key"] = "请按下新按键…",
            ["btn.clear"] = "清除",
            ["btn.unbound"] = "未绑定",
            ["sec.misc"] = "其他",
            ["sw.show_log_in_console"] = "在游戏控制台显示模组日志",
            ["sw.accept_update_notice"] = "接受新版本更新提示",
            ["update.available"] = "CuHotbar 有新版本：{0}（点击打开 release 页）",
            ["alert.dropped"] = "背包已满，{0} 已掉落",
            ["sw.on"] = "开",
            ["sw.off"] = "关",
            ["hint.usage"] = "拖物品到槽位或指向物品按对应键绑定；按切换键切到主手；右键（默认 Shift+右键）使用。",
        };

        private static readonly Dictionary<string, string> _en = new Dictionary<string, string>
        {
            ["app.name"] = "CuHotbar",
            ["app.menu_button"] = "CuHotbar",
            ["tab.title"] = "Hotbar Settings",
            ["sec.display"] = "Display",
            ["sw.visible"] = "Show hotbar",
            ["fmt.slot_count"] = "Slot count: {0}",
            ["fmt.row_count"] = "Rows: {0}",
            ["fmt.col_count"] = "Columns: {0}",
            ["lbl.direction"] = "Layout direction:",
            ["opt.horizontal"] = "Horizontal",
            ["opt.vertical"] = "Vertical",
            ["fmt.scale"] = "Scale: {0:0.00}",
            ["fmt.bg_alpha"] = "Background alpha: {0:0.00}",
            ["sec.keys"] = "Switch keys",
            ["fmt.slot_hotkey"] = "Switch to slot {0}:",
            ["lbl.use_item"] = "Use held item:",
            ["sw.enable_scroll"] = "Scroll wheel switches slot",
            ["sw.auto_refill"] = "Auto-refill slot while stock remains",
            ["sw.warn_on_drop"] = "Warn when an item is dropped (inventory full)",
            ["sw.safe_quick_use"] = "Safe quick use (consumables select without switching)",
            ["sec.position"] = "Position",
            ["fmt.anchor_x"] = "Anchor X: {0:0.00}",
            ["fmt.anchor_y"] = "Anchor Y: {0:0.00}",
            ["fmt.offset_x"] = "Offset X: {0:0}",
            ["fmt.offset_y"] = "Offset Y: {0:0}",
            ["btn.reset_pos"] = "Reset position",
            ["sec.hotkeys"] = "Hotkeys",
            ["lbl.hotkey_panel"] = "Open / close settings panel:",
            ["btn.press_a_key"] = "Press a new key...",
            ["btn.clear"] = "Clear",
            ["btn.unbound"] = "Unbound",
            ["sec.misc"] = "Misc",
            ["sw.show_log_in_console"] = "Show mod logs in game console",
            ["sw.accept_update_notice"] = "Accept update notifications",
            ["update.available"] = "CuHotbar update available: {0} (click to open release page)",
            ["alert.dropped"] = "Inventory full, {0} was dropped",
            ["sw.on"] = "ON",
            ["sw.off"] = "OFF",
            ["hint.usage"] = "Drag an item onto a slot, or point at an item and press its key to bind; press the key to hold; use with the use-key (default Shift+RightClick).",
        };

        private static Dictionary<string, string> Current
        {
            get
            {
                return UseChinese() ? _zh : _en;
            }
        }

        private static bool UseChinese()
        {
            switch (NormalizeLanguageMode(HotbarRuntime.Config?.PreferredLanguage?.Value))
            {
                case "zh":
                    return true;
                case "en":
                    return false;
                default:
                    return IsChineseLocaleName(ReadCurrentLanguageName());
            }
        }

        private static string ReadCurrentLanguageName()
        {
            string name = null;
            try { name = Locale.currentLangName; } catch { }
            if (string.IsNullOrEmpty(name))
            {
                try { name = PlayerPrefs.GetString("locale"); } catch { }
            }
            return name;
        }

        private static string NormalizeLanguageMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode)) return "auto";
            mode = mode.Trim().ToLowerInvariant();
            return mode == "zh" || mode == "en" ? mode : "auto";
        }

        private static bool IsChineseLocaleName(string name)
        {
            string normalized = StripRichText(name).Trim();
            if (string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            if (normalized.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("WC", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            for (int i = 0; i < ChineseKeywords.Length; i++)
            {
                if (normalized.IndexOf(ChineseKeywords[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string StripRichText(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf('<') < 0)
            {
                return value ?? string.Empty;
            }

            var sb = new System.Text.StringBuilder(value.Length);
            bool inTag = false;
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (ch == '<')
                {
                    inTag = true;
                    continue;
                }
                if (ch == '>')
                {
                    inTag = false;
                    continue;
                }
                if (!inTag)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        internal static string T(string key)
        {
            return Current.TryGetValue(key, out var v) ? v : key;
        }

        internal static string F(string key, params object[] args)
        {
            string fmt = T(key);
            try { return string.Format(fmt, args); }
            catch { return fmt; }
        }
    }
}
