namespace CasualtiesUnknown.Hotbar
{
    /// <summary>无状态物品搬移：探测原位、放回原位、腾空主手、拿到主手。供 HotbarModel 激活流程调用。</summary>
    internal static class ItemMover
    {
        internal struct Origin
        {
            public int BodySlot;
            public Container Cont;
            public static Origin None => new Origin { BodySlot = -1, Cont = null };
        }

        /// <summary>item 当前所在身上槽下标；不在任何槽返回 -1。</summary>
        internal static int BodySlotOf(Body body, Item item)
        {
            if (body == null || item == null) return -1;
            for (int i = 0; i < body.slots.Length; i++)
            {
                if (body.GetItem(i) == item) return i;
            }
            return -1;
        }

        /// <summary>探测 item 当前所在位置作为原位：身上非主手槽优先，否则父容器。</summary>
        internal static Origin Probe(Body body, Item item)
        {
            var o = Origin.None;
            if (body == null || item == null) return o;
            int slot = BodySlotOf(body, item);
            if (slot >= 0 && slot != body.handSlot) { o.BodySlot = slot; return o; }
            if (slot < 0) o.Cont = item.ParentContainer();
            return o;
        }

        /// <summary>把 item 放回原位：原 body 槽 → 原容器 → 身上空槽 → 掉地。掉地返回 false。</summary>
        internal static bool Restore(Body body, Item item, Origin origin)
        {
            if (body == null || item == null) return false;
            int hand = body.handSlot;

            if (origin.BodySlot >= 0 && origin.BodySlot < body.slots.Length && origin.BodySlot != hand)
            {
                if (!body.HoldingItem(origin.BodySlot))
                {
                    body.SwapSlots(hand, origin.BodySlot);
                    if (body.GetItem(origin.BodySlot) == item) return true;
                }
            }
            if (origin.Cont != null && origin.Cont.CanHoldItem(item))
            {
                if (body.HoldingItem(item)) body.DropItem(item);
                origin.Cont.LoadItem(item);
                if (item.transform.parent == origin.Cont.transform) return true;
            }
            int? empty = body.FirstEmptySlot();
            if (empty.HasValue && empty.Value != hand)
            {
                body.SwapSlots(hand, empty.Value);
                if (body.GetItem(empty.Value) == item) return true;
            }
            if (body.HoldingItem(item)) body.DropItem(item);
            return false;
        }

        /// <summary>把占用主手的物品挪到身上空槽，无空槽则塞入可容纳的背包容器；都不行才掉地并警告。</summary>
        internal static void Stash(Body body, Item item)
        {
            if (body == null || item == null) return;
            int hand = body.handSlot;
            if (body.GetItem(hand) != item) return;
            int? empty = body.FirstEmptySlot();
            if (empty.HasValue && empty.Value != hand)
            {
                body.SwapSlots(hand, empty.Value);
                return;
            }
            var cont = FindStorableContainer(body, item);
            if (cont != null)
            {
                body.DropItem(item);
                cont.LoadItem(item);
                if (item.transform.parent == cont.transform) return;
            }
            body.DropItem(item);
            WarnDropped(item);
        }

        private static Container FindStorableContainer(Body body, Item item)
        {
            foreach (var it in body.GetAllItemsThorough())
            {
                if (it == null || it == item) continue;
                if (it.TryGetComponent<Container>(out var cont) && cont.CanHoldItem(item))
                {
                    return cont;
                }
            }
            return null;
        }

        private static void WarnDropped(Item item)
        {
            var cfg = HotbarRuntime.Config;
            if (cfg != null && !cfg.WarnOnDrop.Value) return;
            var cam = PlayerCamera.main;
            if (cam == null) return;
            string name = item.Stats != null ? item.fullName : item.id;
            cam.DoAlert(HotbarI18n.F("alert.dropped", name), true);
        }

        /// <summary>把 item 切到主手：身上槽间用 SwapSlots，容器/地面先脱离再拾取。要求主手已空。</summary>
        internal static bool GrabToHand(Body body, Item item)
        {
            if (body == null || item == null) return false;
            int hand = body.handSlot;
            if (body.GetItem(hand) == item) return true;

            int srcSlot = BodySlotOf(body, item);
            if (srcSlot >= 0)
            {
                body.SwapSlots(srcSlot, hand);
                return body.GetItem(hand) == item;
            }

            var cont = item.ParentContainer();
            if (cont != null) cont.UnloadItem(item);
            body.PickUpItem(item, hand, force: true);
            return body.GetItem(hand) == item;
        }
    }
}
