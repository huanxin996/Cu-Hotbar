using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>独立设置窗口（SaveManager 不在场时使用），黑白线条风格。内容委托 HotbarSettingsContent。</summary>
    internal sealed class HotbarWindow
    {
        private const int WindowId = 0x48425730;
        private const float Width = 900f;
        private const float Height = 700f;
        private const float TitleBarHeight = 64f;
        private const float CloseBtnSize = 52f;

        private readonly HotbarSettingsContent _content;
        private Rect _rect = new Rect(160f, 110f, Width, Height);
        private Vector2 _scroll;

        internal bool Open { get; private set; }

        internal HotbarWindow(HotbarConfig cfg)
        {
            _content = new HotbarSettingsContent(cfg);
        }

        internal void Toggle()
        {
            if (Open) Close();
            else Open = true;
        }

        internal void OpenIfClosed()
        {
            Open = true;
        }

        internal void Close()
        {
            Open = false;
            _content.CancelKeyCapture();
        }

        internal void Draw()
        {
            if (!Open) return;
            BlackWhiteSkin.Push();
            try
            {
                _rect = GUI.ModalWindow(WindowId, _rect, DrawContent, "");
            }
            finally
            {
                BlackWhiteSkin.Pop();
            }
        }

        private void DrawContent(int id)
        {
            BlackWhiteSkin.DrawBorder(new Rect(0f, 0f, Width, Height), 6f);

            GUI.Label(new Rect(28f, 14f, Width - CloseBtnSize - 56f, 40f),
                HotbarI18n.T("app.name"), BlackWhiteSkin.HeaderStyle);

            var closeRect = new Rect(Width - CloseBtnSize - 12f, 8f, CloseBtnSize, CloseBtnSize);
            if (GUI.Button(closeRect, GUIContent.none)) Close();
            BlackWhiteSkin.DrawBorder(closeRect, 4f);
            BlackWhiteSkin.DrawCloseX(new Rect(closeRect.x + 13f, closeRect.y + 13f,
                closeRect.width - 26f, closeRect.height - 26f), 6f);

            BlackWhiteSkin.DrawHLine(new Rect(0f, TitleBarHeight, Width, 4f));

            var bodyRect = new Rect(24f, TitleBarHeight + 16f, Width - 48f, Height - TitleBarHeight - 40f);
            GUILayout.BeginArea(bodyRect);
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _content.Draw();
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUI.DragWindow(new Rect(0f, 0f, Width - CloseBtnSize - 24f, TitleBarHeight));
        }
    }
}
