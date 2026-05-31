using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>挂在每个快捷栏槽的 UGUI 物体上，供拖拽 patch 通过射线命中识别槽位下标。</summary>
    internal sealed class HotbarSlotMarker : MonoBehaviour
    {
        internal int Index;
    }
}
