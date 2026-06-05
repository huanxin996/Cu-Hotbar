using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 释放 IMGUI / IME 残留焦点，避免快捷栏按键在输入法使用后失效。
    /// 独立窗口无 TextField；合并到 SaveManager 时仍可能受全局 IME 状态影响。
    /// </summary>
    internal static class ImGuiImeRecovery
    {
        private static bool _pendingClear;

        internal static void RequestClear() => _pendingClear = true;

        internal static void TickUpdate(bool textInputExpected)
        {
            if (textInputExpected) _pendingClear = false;
        }

        internal static void TickOnGui(bool textInputExpected)
        {
            if (textInputExpected) return;
            if (!_pendingClear) return;

            var ev = Event.current;
            if (ev == null || ev.type != EventType.Layout) return;

            GUI.FocusControl(null);
            _pendingClear = false;
        }
    }
}
