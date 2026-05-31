namespace CasualtiesUnknown.Hotbar
{
    /// <summary>Harmony patch 与运行时组件共享 cfg/model/view 的静态入口。</summary>
    internal static class HotbarRuntime
    {
        internal static HotbarConfig Config;
        internal static HotbarModel Model;
        internal static HotbarView View;

        internal static bool Ready => Config != null && Model != null && View != null;
    }
}
