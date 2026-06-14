using System;
using HarmonyLib;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>反射对接 SaveManager.ExternalTabRegistry 把快捷栏设置嵌入侧栏分页。</summary>
    internal static class SaveManagerTabBridge
    {
        internal static bool Register(HotbarConfig cfg)
        {
            try
            {
                var type = AccessTools.TypeByName("CasualtiesUnknown.SaveManager.ExternalTabRegistry");
                if (type == null) { ModLog.Warning("SaveManager 无 ExternalTabRegistry 扩展点，跳过注册"); return false; }
                var content = new HotbarSettingsContent(cfg);
                Action draw = content.Draw;
                Func<string> title = () => HotbarI18n.T("tab.title");
                Func<string> status = () => "由 CuHotbar 提供";
                var fn3 = AccessTools.Method(type, "Register", new[] { typeof(Func<string>), typeof(Action), typeof(Func<string>) });
                if (fn3 != null) { fn3.Invoke(null, new object[] { title, draw, status }); return true; }
                var fn = AccessTools.Method(type, "Register", new[] { typeof(Func<string>), typeof(Action) });
                if (fn == null) return false;
                fn.Invoke(null, new object[] { title, draw });
                return true;
            }
            catch (Exception ex) { ModLog.Warning($"注册 SaveManager 侧栏失败：{ex.Message}"); return false; }
        }
    }
}
