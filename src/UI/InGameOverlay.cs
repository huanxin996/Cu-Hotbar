using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>独立模式下 ESC 暂停面板打开时的游戏内浮动唤起按钮。</summary>
    internal sealed class InGameOverlay
    {
        private const float ButtonWidth = 320f;
        private const float ButtonHeight = 84f;
        private const float MarginRight = 24f;
        private const float MarginBottom = 224f;

        internal static bool ShouldShow()
        {
            if (PlayerCamera.main == null) return false;
            var panel = PlayerCamera.main.brightnessPanel;
            return panel != null && panel.activeSelf;
        }

        internal void Draw(System.Action onClick)
        {
            if (!ShouldShow()) return;

            float x = Screen.width - ButtonWidth - MarginRight;
            float y = Screen.height - ButtonHeight - MarginBottom;
            var rect = new Rect(x, y, ButtonWidth, ButtonHeight);

            BlackWhiteSkin.Push();
            try
            {
                if (GUI.Button(rect, HotbarI18n.T("app.menu_button"))) onClick?.Invoke();
                BlackWhiteSkin.DrawBorder(rect, 5f);
            }
            finally
            {
                BlackWhiteSkin.Pop();
            }
        }
    }
}
