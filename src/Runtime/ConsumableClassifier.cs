using System;
using System.Collections.Generic;
using System.Reflection;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 判定物品是否为可直接食用/饮用的安全消耗品：扫描其 useAction 委托 IL 是否调用进食/饮用方法。
    /// 按物品 id 缓存，仅在按键判定时触发，不在每帧热路径。
    /// </summary>
    internal static class ConsumableClassifier
    {
        private static readonly Dictionary<string, bool> _cache = new Dictionary<string, bool>();

        internal static bool IsConsumable(Item item)
        {
            if (item == null || item.Stats == null) return false;
            string id = item.id ?? "";
            if (_cache.TryGetValue(id, out var cached)) return cached;

            bool result = false;
            try { result = AnalyzeUseAction(item.Stats.useAction); }
            catch (Exception ex) { ModLog.Warning($"消耗品判定失败 id={id}：{ex.Message}"); }

            _cache[id] = result;
            return result;
        }

        private static bool AnalyzeUseAction(Delegate useAction)
        {
            if (useAction == null) return false;
            var method = useAction.Method;
            var body = method?.GetMethodBody();
            if (body == null) return false;
            byte[] il = body.GetILAsByteArray();
            if (il == null) return false;

            var module = method.Module;
            for (int i = 0; i + 4 < il.Length; i++)
            {
                byte op = il[i];
                if (op != 0x28 && op != 0x6F) continue;
                int token = BitConverter.ToInt32(il, i + 1);
                MethodBase resolved = TryResolve(module, token);
                if (resolved != null && IsConsumeMethod(resolved)) return true;
            }
            return false;
        }

        private static bool IsConsumeMethod(MethodBase m)
        {
            if (m.Name != "Eat" && m.Name != "Drink") return false;
            var dt = m.DeclaringType;
            return dt == typeof(Body) || dt == typeof(WaterContainerItem);
        }

        private static MethodBase TryResolve(Module module, int token)
        {
            try { return module.ResolveMethod(token); }
            catch { return null; }
        }
    }
}
