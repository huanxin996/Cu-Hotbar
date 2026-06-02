using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 快捷栏 UGUI 视图：在游戏主 Canvas 下构建槽位，逐帧刷新图标、数量、状态条、选中加粗白边与位置。
    /// </summary>
    internal sealed class HotbarView
    {
        private const float SlotSize = 64f;
        private const float SlotGap = 6f;
        private const float SelFrameThickness = 4f;
        private const float StatusBarHeight = 5f;

        private static readonly Color SlotBg = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color SlotBorder = new Color(1f, 1f, 1f, 0.35f);
        private static readonly Color SelFrameColor = Color.white;

        private readonly HotbarConfig _cfg;
        private readonly HotbarModel _model;

        private Canvas _canvas;
        private RectTransform _root;
        private readonly List<Slot> _slots = new List<Slot>();
        private int _builtCount = -1;
        private int _selected = -1;

        internal HotbarView(HotbarConfig cfg, HotbarModel model)
        {
            _cfg = cfg;
            _model = model;
        }

        internal int Selected
        {
            get => _selected;
            set => _selected = value;
        }

        internal void Tick()
        {
            if (!_cfg.Visible.Value)
            {
                if (_root != null) _root.gameObject.SetActive(false);
                return;
            }

            var cam = PlayerCamera.main;
            if (cam == null || cam.mainCanvas == null || cam.body == null)
            {
                if (_root != null) _root.gameObject.SetActive(false);
                return;
            }

            if (_canvas != cam.mainCanvas || _root == null || _builtCount != _model.SlotCount)
            {
                Rebuild(cam.mainCanvas);
            }

            _root.gameObject.SetActive(true);
            Layout();
            UpdateGraphics();
        }

        internal bool TryGetSlotIndexAt(Vector2 screenPos, out int index)
        {
            index = -1;
            if (_root == null || !_root.gameObject.activeInHierarchy) return false;
            Camera cam = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? _canvas.worldCamera
                : null;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(_slots[i].Rect, screenPos, cam))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        internal void Dispose()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
            _root = null;
            _canvas = null;
            _slots.Clear();
            _builtCount = -1;
        }

        private void Rebuild(Canvas canvas)
        {
            Dispose();
            _canvas = canvas;
            _builtCount = _model.SlotCount;

            var rootGo = new GameObject("Hotbar_Root", typeof(RectTransform));
            _root = rootGo.GetComponent<RectTransform>();
            _root.SetParent(canvas.transform, false);

            for (int i = 0; i < _builtCount; i++)
            {
                _slots.Add(BuildSlot(i));
            }
        }

        private Slot BuildSlot(int index)
        {
            var slotGo = new GameObject("Hotbar_Slot" + index, typeof(RectTransform), typeof(Image));
            var marker = slotGo.AddComponent<HotbarSlotMarker>();
            marker.Index = index;
            var rect = slotGo.GetComponent<RectTransform>();
            rect.SetParent(_root, false);
            rect.sizeDelta = new Vector2(SlotSize, SlotSize);

            var bg = slotGo.GetComponent<Image>();
            bg.color = SlotBg;
            bg.raycastTarget = true;

            var border = NewImage("Border", rect, SlotBorder);
            StretchFull(border.rectTransform);
            border.type = Image.Type.Sliced;

            var icon = NewImage("Icon", rect, Color.white);
            icon.rectTransform.anchoredPosition = Vector2.zero;
            icon.preserveAspect = true;
            icon.enabled = false;

            var statusBar = NewImage("Status", rect, Color.white);
            var sbRect = statusBar.rectTransform;
            sbRect.anchorMin = new Vector2(0f, 0f);
            sbRect.anchorMax = new Vector2(0f, 0f);
            sbRect.pivot = new Vector2(0f, 0f);
            sbRect.anchoredPosition = new Vector2(2f, 2f);
            sbRect.sizeDelta = new Vector2(SlotSize - 4f, StatusBarHeight);
            statusBar.enabled = false;

            var count = NewText("Count", rect, 16, TextAnchor.LowerRight);
            var cRect = count.rectTransform;
            cRect.anchorMin = new Vector2(1f, 0f);
            cRect.anchorMax = new Vector2(1f, 0f);
            cRect.pivot = new Vector2(1f, 0f);
            cRect.anchoredPosition = new Vector2(-3f, 7f);
            cRect.sizeDelta = new Vector2(40f, 20f);
            count.text = "";

            var key = NewText("Key", rect, 14, TextAnchor.UpperLeft);
            var kRect = key.rectTransform;
            kRect.anchorMin = new Vector2(0f, 1f);
            kRect.anchorMax = new Vector2(0f, 1f);
            kRect.pivot = new Vector2(0f, 1f);
            kRect.anchoredPosition = new Vector2(3f, -2f);
            kRect.sizeDelta = new Vector2(48f, 18f);
            key.color = new Color(1f, 1f, 1f, 0.8f);
            key.text = KeyLabel(index);

            var selFrame = BuildSelFrame(rect);

            return new Slot
            {
                Rect = rect, Bg = bg, Border = border, Icon = icon,
                Status = statusBar, Count = count, Key = key, SelFrame = selFrame,
            };
        }

        private GameObject BuildSelFrame(RectTransform parent)
        {
            var frameGo = new GameObject("SelFrame", typeof(RectTransform));
            var frameRect = frameGo.GetComponent<RectTransform>();
            frameRect.SetParent(parent, false);
            StretchFull(frameRect);
            AddEdge(frameRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, SelFrameThickness));
            AddEdge(frameRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, SelFrameThickness));
            AddEdge(frameRect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(SelFrameThickness, 0f));
            AddEdge(frameRect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(SelFrameThickness, 0f));
            frameGo.SetActive(false);
            return frameGo;
        }

        private void AddEdge(RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            var edge = NewImage("Edge", parent, SelFrameColor);
            var r = edge.rectTransform;
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size;
            r.anchoredPosition = Vector2.zero;
        }

        private void Layout()
        {
            int n = _slots.Count;
            float scale = _cfg.Scale.Value;
            _root.localScale = new Vector3(scale, scale, 1f);

            _root.anchorMin = _root.anchorMax = new Vector2(_cfg.AnchorX.Value, _cfg.AnchorY.Value);
            _root.pivot = new Vector2(0.5f, 0.5f);
            _root.anchoredPosition = new Vector2(_cfg.OffsetX.Value, _cfg.OffsetY.Value);

            if (_cfg.Vertical.Value) LayoutVertical(n);
            else LayoutHorizontal(n);
        }

        private void LayoutHorizontal(int n)
        {
            int rows = Mathf.Clamp(_cfg.RowCount.Value, 1, Mathf.Max(1, n));
            int cols = Mathf.CeilToInt((float)n / rows);

            float fullW = cols * SlotSize + (cols - 1) * SlotGap;
            float totalH = rows * SlotSize + (rows - 1) * SlotGap;
            _root.sizeDelta = new Vector2(fullW, totalH);

            float bottomY = -totalH / 2f + SlotSize / 2f;
            for (int i = 0; i < n; i++)
            {
                int row = i / cols;
                int col = i % cols;
                int rowCount = Mathf.Min(cols, n - row * cols);
                float rowW = rowCount * SlotSize + (rowCount - 1) * SlotGap;
                float startX = -rowW / 2f + SlotSize / 2f;
                float y = bottomY + (rows - 1 - row) * (SlotSize + SlotGap);

                _slots[i].Rect.anchorMin = _slots[i].Rect.anchorMax = new Vector2(0.5f, 0.5f);
                _slots[i].Rect.anchoredPosition = new Vector2(startX + col * (SlotSize + SlotGap), y);
            }
        }

        private void LayoutVertical(int n)
        {
            int cols = Mathf.Clamp(_cfg.RowCount.Value, 1, Mathf.Max(1, n));
            int perCol = Mathf.CeilToInt((float)n / cols);

            float fullW = cols * SlotSize + (cols - 1) * SlotGap;
            float totalH = perCol * SlotSize + (perCol - 1) * SlotGap;
            _root.sizeDelta = new Vector2(fullW, totalH);

            float rightX = fullW / 2f - SlotSize / 2f;
            float topY = totalH / 2f - SlotSize / 2f;
            for (int i = 0; i < n; i++)
            {
                int col = i / perCol;
                int row = i % perCol;
                float x = rightX - col * (SlotSize + SlotGap);
                float y = topY - row * (SlotSize + SlotGap);

                _slots[i].Rect.anchorMin = _slots[i].Rect.anchorMax = new Vector2(0.5f, 0.5f);
                _slots[i].Rect.anchoredPosition = new Vector2(x, y);
            }
        }

        private void UpdateGraphics()
        {
            float bgA = Mathf.Clamp01(_cfg.BackgroundAlpha.Value);
            for (int i = 0; i < _slots.Count; i++)
            {
                var item = _model.GetItem(i);
                var slot = _slots[i];
                if (item != null) UpdateItemSlot(slot, item, i);
                else ClearItemSlot(slot);

                slot.Bg.color = new Color(SlotBg.r, SlotBg.g, SlotBg.b, bgA);
                slot.Border.color = new Color(SlotBorder.r, SlotBorder.g, SlotBorder.b, bgA);
                slot.Key.text = KeyLabel(i);
                slot.SelFrame.SetActive(i == _selected);
            }
        }

        private void UpdateItemSlot(Slot slot, Item item, int index)
        {
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                slot.Icon.sprite = sr.sprite;
                slot.Icon.rectTransform.sizeDelta = PlayerCamera.ImageSizeDelta(sr.sprite.texture, 3f, SlotSize - 12f);
                slot.Icon.enabled = true;
            }
            else
            {
                slot.Icon.enabled = false;
            }

            ApplyStatusBar(slot, item);

            int count = _model.CountInBody(index);
            slot.Count.text = count > 1 ? "x" + count : "";
        }

        private void ClearItemSlot(Slot slot)
        {
            slot.Icon.enabled = false;
            slot.Status.enabled = false;
            slot.Count.text = "";
        }

        /// <summary>状态条：电量物品按电量、弹匣按子弹、其余按耐久填充宽度并着色。</summary>
        private void ApplyStatusBar(Slot slot, Item item)
        {
            float fill = -1f;
            if (item.battery != null && item.battery.hasBattery && item.battery.maxCharge > 0f)
            {
                fill = Mathf.Clamp01(item.battery.GetCharge() / item.battery.maxCharge);
            }
            else if (item.TryGetComponent<AmmoScript>(out var ammo)
                     && ammo.itemType == AmmoScript.AmmoItemType.Magazine && ammo.maxRounds > 0)
            {
                fill = Mathf.Clamp01((float)ammo.rounds / ammo.maxRounds);
            }
            else
            {
                fill = Mathf.Clamp01(item.condition);
            }

            if (fill < 0f)
            {
                slot.Status.enabled = false;
                return;
            }
            float innerW = SlotSize - 4f;
            slot.Status.rectTransform.sizeDelta = new Vector2(innerW * fill, StatusBarHeight);
            slot.Status.color = PlayerCamera.ConditionToColor(fill);
            slot.Status.enabled = true;
        }

        private string KeyLabel(int index)
        {
            var sc = _cfg.SlotHotkey(index).Value;
            if (sc.MainKey == KeyCode.None) return "";
            string label = ShortKey(sc.MainKey);
            foreach (var mod in sc.Modifiers)
            {
                label = ModPrefix(mod) + "+" + label;
            }
            return label;
        }

        private static string ShortKey(KeyCode key)
        {
            if (key >= KeyCode.Keypad0 && key <= KeyCode.Keypad9) return ((int)(key - KeyCode.Keypad0)).ToString();
            if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9) return ((int)(key - KeyCode.Alpha0)).ToString();
            switch (key)
            {
                case KeyCode.Mouse0: return "LMB";
                case KeyCode.Mouse1: return "RMB";
                case KeyCode.Mouse2: return "MMB";
                default: return key.ToString();
            }
        }

        private static string ModPrefix(KeyCode mod)
        {
            switch (mod)
            {
                case KeyCode.LeftShift:
                case KeyCode.RightShift: return "S";
                case KeyCode.LeftControl:
                case KeyCode.RightControl: return "C";
                case KeyCode.LeftAlt:
                case KeyCode.RightAlt: return "A";
                default: return mod.ToString();
            }
        }

        private static Image NewImage(string name, RectTransform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.GetComponent<RectTransform>().SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private static Text NewText(string name, RectTransform parent, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.GetComponent<RectTransform>().SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = fontSize;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = anchor;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private struct Slot
        {
            public RectTransform Rect;
            public Image Bg;
            public Image Border;
            public Image Icon;
            public Image Status;
            public Text Count;
            public Text Key;
            public GameObject SelFrame;
        }
    }
}
