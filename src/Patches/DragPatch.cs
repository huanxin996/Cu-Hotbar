using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 接入游戏原生拖拽：从快捷栏槽拖出物品（起点）、把拖拽物链接到槽（落点）、拖出丢弃后清除链接。
    /// </summary>
    [HarmonyPatch(typeof(PlayerCamera))]
    internal static class DragPatch
    {
        [HarmonyPatch("HandleStartDragging")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerCamera __instance)
        {
            if (!HotbarRuntime.Ready) return;

            bool onSlot = HotbarRuntime.View.TryGetSlotIndexAt(Input.mousePosition, out int index);
            if (__instance.dragItem == null && onSlot)
            {
                var item = HotbarRuntime.Model.GetItem(index);
                if (item != null) BeginDragFromSlot(__instance, item);
            }
        }

        private static void BeginDragFromSlot(PlayerCamera cam, Item item)
        {
            cam.dragItem = item;
            var dragImage = cam.dragImage;
            if (dragImage == null) return;
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;
            dragImage.enabled = true;
            dragImage.transform.localScale = Vector2.one * 1.75f;
            dragImage.sprite = sr.sprite;
            dragImage.rectTransform.sizeDelta = PlayerCamera.ImageSizeDelta(sr.sprite.texture, 5f, 128f);
        }

        [HarmonyPatch("HandleReleaseDragging")]
        [HarmonyPrefix]
        private static bool ReleasePrefix(PlayerCamera __instance)
        {
            if (!HotbarRuntime.Ready) return true;
            var item = __instance.dragItem;
            if (item == null) return true;

            if (HotbarRuntime.View.TryGetSlotIndexAt(Input.mousePosition, out int index))
            {
                HotbarRuntime.Model.Link(index, item);
                __instance.dragItem = null;
                if (__instance.dragImage != null) __instance.dragImage.enabled = false;
                return false;
            }
            return true;
        }
    }
}
