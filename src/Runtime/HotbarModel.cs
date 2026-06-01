using System.Collections.Generic;
using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 快捷栏运行时模型：维护槽位指向（运行时 Item 引用 + 持久化物品 id），
    /// 负责按角色加载/保存、引用失效后按 id 重绑定、激活槽位切到主手并归还上一物品。
    /// </summary>
    internal sealed class HotbarModel
    {
        private readonly HotbarConfig _cfg;
        private readonly HotbarStore _store;

        private Item[] _items;
        private string[] _ids;
        private int _runId;

        private int _activeIndex = -1;
        private Item _activeItem;
        private ItemMover.Origin _activeOrigin = ItemMover.Origin.None;

        internal HotbarModel(HotbarConfig cfg, HotbarStore store)
        {
            _cfg = cfg;
            _store = store;
            Resize(cfg.SlotCount.Value);
            cfg.SlotCount.SettingChanged += (_, __) => Resize(cfg.SlotCount.Value);
        }

        internal int SlotCount => _ids.Length;

        /// <summary>第 index 槽用于显示的物品：离身引用置空，AutoRefill 开则按 id 解析身上同种，关则清链。</summary>
        internal Item GetItem(int index)
        {
            if (index < 0 || index >= _items.Length) return null;
            var body = CurrentBody();
            var cur = _items[index];
            if (cur != null && body != null && !InBody(body, cur))
            {
                cur = null;
                _items[index] = null;
                if (!_cfg.AutoRefill.Value)
                {
                    _ids[index] = "";
                    if (index == _activeIndex) ClearActive();
                    SaveForCurrentRun();
                    return null;
                }
            }
            if (cur == null && body != null && !string.IsNullOrEmpty(_ids[index])
                && body.FindByIdThorough(_ids[index], out var found))
            {
                _items[index] = found;
                cur = found;
            }
            return cur;
        }

        internal string GetId(int index)
        {
            if (index < 0 || index >= _ids.Length) return "";
            return _ids[index] ?? "";
        }

        /// <summary>身上（含容器/穿戴）同 id 物品数量。</summary>
        internal int CountInBody(int index)
        {
            string id = GetId(index);
            if (string.IsNullOrEmpty(id)) return 0;
            var body = CurrentBody();
            if (body == null) return 0;
            int n = 0;
            foreach (var it in body.GetAllItemsThorough())
            {
                if (it != null && it.id == id) n++;
            }
            return n;
        }

        /// <summary>把第 index 槽指向某物品（拖拽落点调用）。一物一槽：先清除该物品在其它槽的绑定。</summary>
        internal void Link(int index, Item item)
        {
            if (index < 0 || index >= _ids.Length) return;
            if (item != null)
            {
                for (int i = 0; i < _ids.Length; i++)
                {
                    if (i == index) continue;
                    if (_items[i] == item || (!string.IsNullOrEmpty(item.id) && _ids[i] == item.id))
                    {
                        _items[i] = null;
                        _ids[i] = "";
                    }
                }
            }
            _items[index] = item;
            _ids[index] = item != null ? item.id : "";
            SaveForCurrentRun();
        }

        internal void Clear(int index)
        {
            if (index < 0 || index >= _ids.Length) return;
            _items[index] = null;
            _ids[index] = "";
            if (_activeIndex == index) ClearActive();
            SaveForCurrentRun();
        }

        /// <summary>激活第 index 槽：空槽则收起当前快捷栏物品；否则归还上一物品并把本槽物品切到主手。</summary>
        internal bool Activate(int index)
        {
            var body = CurrentBody();
            if (body == null || !body.conscious) return false;

            Item target = Resolve(index, body);
            if (target == null)
            {
                RestorePrevious(body);
                return true;
            }
            if (_activeIndex == index && body.GetItem(body.handSlot) == target) return true;

            RestorePrevious(body);

            Item handItem = body.GetItem(body.handSlot);
            if (handItem != null && handItem != target) ItemMover.Stash(body, handItem);

            _activeOrigin = ItemMover.Probe(body, target);
            if (!ItemMover.GrabToHand(body, target))
            {
                ClearActive();
                return false;
            }
            _activeIndex = index;
            _activeItem = target;
            return true;
        }

        /// <summary>激活槽物品已离身（丢出/用尽）时按 AutoRefill 补同种到主手；被手动挪走则停止跟踪。</summary>
        internal void RefillActiveIfNeeded()
        {
            if (_activeIndex < 0 || _activeIndex >= _ids.Length) { _activeItem = null; return; }
            var body = CurrentBody();
            if (body == null || !body.conscious) return;

            if (_activeItem != null && InBody(body, _activeItem))
            {
                if (body.GetItem(body.handSlot) != _activeItem) ClearActive();
                return;
            }

            if (!_cfg.AutoRefill.Value) { ClearActive(); return; }

            string id = _ids[_activeIndex];
            if (!string.IsNullOrEmpty(id) && body.FindByIdThorough(id, out var same))
            {
                _activeOrigin = ItemMover.Probe(body, same);
                if (ItemMover.GrabToHand(body, same))
                {
                    _items[_activeIndex] = same;
                    _activeItem = same;
                    return;
                }
            }
            ClearActive();
        }

        /// <summary>把上一激活槽的物品放回其原位；放不回则掉地并清除该槽链接。</summary>
        private void RestorePrevious(Body body)
        {
            if (_activeIndex < 0 || _activeIndex >= _items.Length) { ClearActive(); return; }
            Item prev = _activeItem;
            if (prev != null && body.GetItem(body.handSlot) == prev)
            {
                if (!ItemMover.Restore(body, prev, _activeOrigin))
                {
                    Clear(_activeIndex);
                }
            }
            ClearActive();
        }

        private void ClearActive()
        {
            _activeIndex = -1;
            _activeItem = null;
            _activeOrigin = ItemMover.Origin.None;
        }

        /// <summary>逐帧触发各槽实时解析，保持运行时引用与身上物品一致。</summary>
        internal void Refresh()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                GetItem(i);
            }
        }

        private static bool InBody(Body body, Item item)
        {
            foreach (var it in body.GetAllItemsThorough())
            {
                if (it == item) return true;
            }
            return false;
        }

        internal void LoadForCurrentRun()
        {
            _runId = HotbarStore.CurrentRunId();
            var ids = _store.Load(_runId);
            for (int i = 0; i < _ids.Length; i++)
            {
                _ids[i] = i < ids.Count ? (ids[i] ?? "") : "";
                _items[i] = null;
            }
            ClearActive();
        }

        internal void SaveForCurrentRun()
        {
            if (_runId == 0) _runId = HotbarStore.CurrentRunId();
            if (_runId == 0) return;
            _store.Save(_runId, new List<string>(_ids));
        }

        private Item Resolve(int index, Body body)
        {
            if (index < 0 || index >= _items.Length) return null;
            if (_items[index] != null) return _items[index];
            if (string.IsNullOrEmpty(_ids[index])) return null;
            if (body.FindByIdThorough(_ids[index], out var found))
            {
                _items[index] = found;
                return found;
            }
            return null;
        }

        private void Resize(int count)
        {
            count = Mathf.Max(count, 1);
            var newItems = new Item[count];
            var newIds = new string[count];
            for (int i = 0; i < count; i++)
            {
                newItems[i] = _items != null && i < _items.Length ? _items[i] : null;
                newIds[i] = _ids != null && i < _ids.Length ? _ids[i] : "";
            }
            _items = newItems;
            _ids = newIds;
        }

        private static Body CurrentBody()
        {
            var cam = PlayerCamera.main;
            return cam != null ? cam.body : null;
        }
    }
}
