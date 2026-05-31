using System;
using HarmonyLib;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 反射对接 SaveManager 的 ExternalTabRegistry，把快捷栏设置作为分页注册进其面板。
    /// SaveManager 版本不含该扩展点时静默降级。
    /// </summary>
    internal static class SaveManagerTabBridge
    {
        internal static bool Register(HotbarConfig cfg)
        {
            try
            {
                var type = AccessTools.TypeByName("CasualtiesUnknown.SaveManager.ExternalTabRegistry");
                if (type == null)
                {
                    ModLog.Warning("SaveManager 无 ExternalTabRegistry 扩展点，回退独立模式");
                    return false;
                }
                var register = AccessTools.Method(type, "Register", new[] { typeof(string), typeof(Action) });
                if (register == null)
                {
                    ModLog.Warning("ExternalTabRegistry.Register 未找到，回退独立模式");
                    return false;
                }
                var content = new HotbarSettingsContent(cfg);
                Action draw = content.Draw;
                register.Invoke(null, new object[] { HotbarI18n.T("tab.title"), draw });
                return true;
            }
            catch (Exception ex)
            {
                ModLog.Warning($"注册 SaveManager 分页失败：{ex.Message}");
                return false;
            }
        }
    }
}
