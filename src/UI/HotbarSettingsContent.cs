using System;
using BepInEx.Configuration;
using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 快捷栏设置的可复用 GUILayout 内容，供独立窗口与 SaveManager 合并分页共用。
    /// 只画控件不画窗口框；样式继承宿主当前 GUISkin。
    /// </summary>
    internal sealed class HotbarSettingsContent
    {
        private const float LabelW = 220f;
        private const float RowH = 40f;

        private readonly HotbarConfig _cfg;
        private bool _capturingPanelKey;

        internal HotbarSettingsContent(HotbarConfig cfg)
        {
            _cfg = cfg;
        }

        internal void CancelKeyCapture() => _capturingPanelKey = false;

        internal void Draw()
        {
            BlackWhiteSkin.EnsureStyles();
            GUILayout.Label(HotbarI18n.T("hint.usage"));
            GUILayout.Space(8f);

            GUILayout.Label(HotbarI18n.T("sec.display"), BlackWhiteSkin.HeaderStyle);
            GUILayout.BeginVertical(BlackWhiteSkin.CardStyle);
            bool vis = DrawSwitch(HotbarI18n.T("sw.visible"), _cfg.Visible.Value);
            if (vis != _cfg.Visible.Value) _cfg.Visible.Value = vis;
            DrawIntSlider(HotbarI18n.F("fmt.slot_count", _cfg.SlotCount.Value), _cfg.SlotCount, 1, 9);
            DrawFloatSlider(HotbarI18n.F("fmt.scale", _cfg.Scale.Value), _cfg.Scale, 0.5f, 2f);
            DrawFloatSlider(HotbarI18n.F("fmt.bg_alpha", _cfg.BackgroundAlpha.Value), _cfg.BackgroundAlpha, 0f, 1f);
            GUILayout.EndVertical();

            GUILayout.Space(12f);
            GUILayout.Label(HotbarI18n.T("sec.keys"), BlackWhiteSkin.HeaderStyle);
            GUILayout.BeginVertical(BlackWhiteSkin.CardStyle);
            DrawKeyBase();
            bool rc = DrawSwitch(HotbarI18n.T("sw.right_click_use"), _cfg.RightClickUse.Value);
            if (rc != _cfg.RightClickUse.Value) _cfg.RightClickUse.Value = rc;
            bool sc = DrawSwitch(HotbarI18n.T("sw.enable_scroll"), _cfg.EnableScroll.Value);
            if (sc != _cfg.EnableScroll.Value) _cfg.EnableScroll.Value = sc;
            bool ar = DrawSwitch(HotbarI18n.T("sw.auto_refill"), _cfg.AutoRefill.Value);
            if (ar != _cfg.AutoRefill.Value) _cfg.AutoRefill.Value = ar;
            GUILayout.EndVertical();

            GUILayout.Space(12f);
            GUILayout.Label(HotbarI18n.T("sec.position"), BlackWhiteSkin.HeaderStyle);
            GUILayout.BeginVertical(BlackWhiteSkin.CardStyle);
            DrawFloatSlider(HotbarI18n.F("fmt.anchor_x", _cfg.AnchorX.Value), _cfg.AnchorX, 0f, 1f);
            DrawFloatSlider(HotbarI18n.F("fmt.anchor_y", _cfg.AnchorY.Value), _cfg.AnchorY, 0f, 1f);
            DrawFloatSlider(HotbarI18n.F("fmt.offset_x", _cfg.OffsetX.Value), _cfg.OffsetX, -960f, 960f);
            DrawFloatSlider(HotbarI18n.F("fmt.offset_y", _cfg.OffsetY.Value), _cfg.OffsetY, 0f, 540f);
            if (GUILayout.Button(HotbarI18n.T("btn.reset_pos"), GUILayout.MaxWidth(260f), GUILayout.MinHeight(RowH)))
            {
                _cfg.AnchorX.Value = 0.5f;
                _cfg.AnchorY.Value = 0f;
                _cfg.OffsetX.Value = 0f;
                _cfg.OffsetY.Value = 90f;
            }
            GUILayout.EndVertical();

            GUILayout.Space(12f);
            GUILayout.Label(HotbarI18n.T("sec.hotkeys"), BlackWhiteSkin.HeaderStyle);
            GUILayout.BeginVertical(BlackWhiteSkin.CardStyle);
            DrawHotkeyRow(HotbarI18n.T("lbl.hotkey_panel"), _cfg.ToggleSettingsHotkey, ref _capturingPanelKey);
            CaptureKeyDownIfNeeded();
            GUILayout.EndVertical();

            GUILayout.Space(12f);
            GUILayout.Label(HotbarI18n.T("sec.misc"), BlackWhiteSkin.HeaderStyle);
            GUILayout.BeginVertical(BlackWhiteSkin.CardStyle);
            bool log = DrawSwitch(HotbarI18n.T("sw.show_log_in_console"), _cfg.ShowLogInConsole.Value);
            if (log != _cfg.ShowLogInConsole.Value)
            {
                _cfg.ShowLogInConsole.Value = log;
                ModLog.ShowInConsole = log;
            }
            bool upd = DrawSwitch(HotbarI18n.T("sw.accept_update_notice"), _cfg.AcceptUpdateNotice.Value);
            if (upd != _cfg.AcceptUpdateNotice.Value)
            {
                _cfg.AcceptUpdateNotice.Value = upd;
                UpdateChecker.Enabled = upd;
            }
            GUILayout.EndVertical();
        }

        private void DrawKeyBase()
        {
            bool keypad = _cfg.UseKeypad.Value;
            GUILayout.BeginHorizontal();
            GUILayout.Label(HotbarI18n.T("lbl.key_base"), BlackWhiteSkin.RowLabelStyle,
                GUILayout.MinWidth(LabelW), GUILayout.ExpandWidth(false), GUILayout.MinHeight(RowH));
            if (GUILayout.Button(HotbarI18n.T("opt.keypad"),
                keypad ? BlackWhiteSkin.TabActiveStyle : BlackWhiteSkin.TabStyle,
                GUILayout.MinWidth(160f), GUILayout.MinHeight(RowH)))
            {
                _cfg.UseKeypad.Value = true;
            }
            GUILayout.Space(8f);
            if (GUILayout.Button(HotbarI18n.T("opt.alpha"),
                keypad ? BlackWhiteSkin.TabStyle : BlackWhiteSkin.TabActiveStyle,
                GUILayout.MinWidth(160f), GUILayout.MinHeight(RowH)))
            {
                _cfg.UseKeypad.Value = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (!keypad) GUILayout.Label(HotbarI18n.T("hint.alpha_conflict"));
        }

        private void DrawHotkeyRow(string label, ConfigEntry<KeyboardShortcut> entry, ref bool capturing)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, BlackWhiteSkin.RowLabelStyle, GUILayout.MinWidth(LabelW), GUILayout.ExpandWidth(false), GUILayout.MinHeight(RowH));
            string shown = capturing
                ? HotbarI18n.T("btn.press_a_key")
                : (entry.Value.MainKey == KeyCode.None ? HotbarI18n.T("btn.unbound") : entry.Value.MainKey.ToString());
            if (GUILayout.Button(shown, GUILayout.MinWidth(200f), GUILayout.MinHeight(RowH)))
            {
                capturing = !capturing;
            }
            GUILayout.Space(8f);
            if (GUILayout.Button(HotbarI18n.T("btn.clear"), GUILayout.MinWidth(100f), GUILayout.MinHeight(RowH)))
            {
                entry.Value = new KeyboardShortcut(KeyCode.None);
                capturing = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void CaptureKeyDownIfNeeded()
        {
            if (!_capturingPanelKey) return;
            var e = Event.current;
            if (e != null && e.isKey && e.keyCode != KeyCode.None)
            {
                _cfg.ToggleSettingsHotkey.Value = new KeyboardShortcut(e.keyCode);
                _capturingPanelKey = false;
                e.Use();
            }
        }

        private static bool DrawSwitch(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, BlackWhiteSkin.RowLabelStyle, GUILayout.MinWidth(LabelW), GUILayout.ExpandWidth(false), GUILayout.MinHeight(RowH));
            GUILayout.Space(20f);
            bool result = value;
            if (GUILayout.Button(HotbarI18n.T("sw.on"),
                value ? BlackWhiteSkin.TabActiveStyle : BlackWhiteSkin.TabStyle,
                GUILayout.MinWidth(110f), GUILayout.MinHeight(RowH)))
            {
                result = true;
            }
            GUILayout.Space(8f);
            if (GUILayout.Button(HotbarI18n.T("sw.off"),
                value ? BlackWhiteSkin.TabStyle : BlackWhiteSkin.TabActiveStyle,
                GUILayout.MinWidth(110f), GUILayout.MinHeight(RowH)))
            {
                result = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return result;
        }

        private static void DrawIntSlider(string label, ConfigEntry<int> entry, int min, int max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, BlackWhiteSkin.RowLabelStyle, GUILayout.MinWidth(LabelW), GUILayout.ExpandWidth(false), GUILayout.MinHeight(RowH));
            float v = GUILayout.HorizontalSlider(entry.Value, min, max,
                GUILayout.MinWidth(280f), GUILayout.ExpandWidth(true));
            int iv = Mathf.RoundToInt(v);
            if (iv != entry.Value) entry.Value = iv;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void DrawFloatSlider(string label, ConfigEntry<float> entry, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, BlackWhiteSkin.RowLabelStyle, GUILayout.MinWidth(LabelW), GUILayout.ExpandWidth(false), GUILayout.MinHeight(RowH));
            float v = GUILayout.HorizontalSlider(entry.Value, min, max,
                GUILayout.MinWidth(280f), GUILayout.ExpandWidth(true));
            if (Math.Abs(v - entry.Value) > 0.0001f) entry.Value = v;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
