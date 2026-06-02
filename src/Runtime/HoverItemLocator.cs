using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>定位鼠标当前指向的物品：先查 UI 背包格（InvButton），再查世界 Item 层。</summary>
    internal static class HoverItemLocator
    {
        private static readonly List<RaycastResult> _uiCasts = new List<RaycastResult>();

        internal static Item ItemUnderMouse()
        {
            var fromUi = ItemFromUI();
            if (fromUi != null) return fromUi;
            return ItemFromWorld();
        }

        private static Item ItemFromUI()
        {
            var es = EventSystem.current;
            if (es == null) return null;
            var ped = new PointerEventData(es) { position = Input.mousePosition };
            _uiCasts.Clear();
            es.RaycastAll(ped, _uiCasts);
            for (int i = 0; i < _uiCasts.Count; i++)
            {
                var go = _uiCasts[i].gameObject;
                if (go == null) continue;
                if (go.TryGetComponent<InvButton>(out var btn) && btn.TryGetItem(out var item))
                {
                    return item;
                }
            }
            return null;
        }

        private static Item ItemFromWorld()
        {
            var mainCam = Camera.main;
            if (mainCam == null) return null;
            Vector2 world = mainCam.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.OverlapPoint(world, LayerMask.GetMask("Item"));
            if (hit != null && hit.TryGetComponent<Item>(out var item)) return item;
            return null;
        }
    }
}
