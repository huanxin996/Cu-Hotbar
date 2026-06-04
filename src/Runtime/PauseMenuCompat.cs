using System.Reflection;
using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    internal static class PauseMenuCompat
    {
        private static readonly System.Type PauseHandlerType = typeof(PlayerCamera).Assembly.GetType("PauseHandler");
        private static readonly PropertyInfo PauseHandlerPausedProperty =
            PauseHandlerType?.GetProperty("paused", BindingFlags.Public | BindingFlags.Static);
        private static readonly FieldInfo PauseHandlerMainField =
            PauseHandlerType?.GetField("main", BindingFlags.Public | BindingFlags.Static);
        private static readonly FieldInfo PauseHandlerContainerField =
            PauseHandlerType?.GetField("pauseContainer", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo BrightnessPanelField =
            typeof(PlayerCamera).GetField("brightnessPanel", BindingFlags.Public | BindingFlags.Instance);

        internal static bool IsPauseMenuOpen()
        {
            if (PlayerCamera.main == null) return false;

            if (TryGetPauseMenuVisible(out bool visible)) return visible;
            if (BrightnessPanelField == null) return false;

            try
            {
                var panel = BrightnessPanelField.GetValue(PlayerCamera.main) as GameObject;
                return panel != null && panel.activeSelf;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetPauseMenuVisible(out bool visible)
        {
            visible = false;
            if (PauseHandlerType == null) return false;

            try
            {
                bool paused = PauseHandlerPausedProperty?.GetValue(null, null) is bool pausedValue && pausedValue;
                var main = PauseHandlerMainField?.GetValue(null);
                var pauseContainer = main != null ? PauseHandlerContainerField?.GetValue(main) as GameObject : null;
                visible = paused || (pauseContainer != null && pauseContainer.activeSelf);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}