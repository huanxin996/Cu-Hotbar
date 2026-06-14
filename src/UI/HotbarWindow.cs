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
        private float _drawScale = 1f;

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
            ImGuiImeRecovery.RequestClear();
        }

        internal bool ExpectsTextInput => Open && _content.IsCapturingHotkey;

        internal void Draw()
        {
            if (!Open) return;
            _drawScale = ComputeScale();
            _rect.width = Width;
            _rect.height = Height;
            float maxX = Mathf.Max(0f, Screen.width / _drawScale - Width);
            float maxY = Mathf.Max(0f, Screen.height / _drawScale - Height);
            _rect.x = Mathf.Clamp(_rect.x, 0f, maxX);
            _rect.y = Mathf.Clamp(_rect.y, 0f, maxY);

            BlackWhiteSkin.Push();
            Matrix4x4 prev = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.Scale(new Vector3(_drawScale, _drawScale, 1f));
                _rect = GUI.ModalWindow(WindowId, _rect, DrawContent, "");
            }
            finally
            {
                GUI.matrix = prev;
                BlackWhiteSkin.Pop();
            }
        }

        /// <summary>按屏幕尺寸算缩放比，使窗口不超出屏幕；屏幕足够大时不放大（上限 1）。</summary>
        private static float ComputeScale()
        {
            float fit = Mathf.Min(Screen.width / Width, Screen.height / Height) * 0.92f;
            return Mathf.Clamp(fit, 0.3f, 1f);
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
            _content.Draw();
            GUILayout.EndArea();

            GUI.DragWindow(new Rect(0f, 0f, Width - CloseBtnSize - 24f, TitleBarHeight));
        }
    }
}
