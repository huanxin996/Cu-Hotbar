using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 快捷栏运行时驱动：逐帧刷新视图与指向、处理数字键切换、按角色加载与保存。
    /// </summary>
    internal sealed class HotbarController : MonoBehaviour
    {
        private HotbarConfig _cfg;
        private HotbarModel _model;
        private HotbarView _view;
        private int _lastRunId;

        internal void Init(HotbarConfig cfg, HotbarModel model, HotbarView view)
        {
            _cfg = cfg;
            _model = model;
            _view = view;
            HotbarRuntime.Config = cfg;
            HotbarRuntime.Model = model;
            HotbarRuntime.View = view;
        }

        private void Update()
        {
            if (_cfg == null) return;

            SyncRun();
            _model.RefillActiveIfNeeded();
            _model.Refresh();
            _view.Tick();
        }

        private void LateUpdate()
        {
            if (_cfg == null) return;
            ImGuiImeRecovery.TickUpdate(textInputExpected: false);
            HandleKeys();
            HandleUseItem();
            HandleScroll();
        }

        private void HandleUseItem()
        {
            if (!_cfg.UseItemHotkey.Value.IsDown()) return;
            if (_view.TryGetSlotIndexAt(Input.mousePosition, out _)) return;
            _model.UseSelected(_view.Selected);
        }

        private void HandleScroll()
        {
            if (!_cfg.EnableScroll.Value) return;
            if (_model.SlotCount <= 0) return;
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) < 0.01f) return;
            var cam = PlayerCamera.main;
            if (cam == null || cam.body == null || !cam.body.conscious) return;
            if (PauseMenuCompat.IsPauseMenuOpen()) return;

            int n = _model.SlotCount;
            int start = _view.Selected < 0 ? 0 : _view.Selected;
            int dir = scroll > 0f ? -1 : 1;
            int idx = ((start + dir) % n + n) % n;
            if (_cfg.SafeQuickUse.Value && _model.IsConsumableSlot(idx))
            {
                _view.Selected = idx;
                return;
            }
            if (_model.Activate(idx)) _view.Selected = idx;
        }

        private void SyncRun()
        {
            int runId = HotbarStore.CurrentRunId();
            if (runId != _lastRunId)
            {
                _lastRunId = runId;
                _model.LoadForCurrentRun();
                _view.Selected = -1;
            }
        }

        private void HandleKeys()
        {
            for (int i = 0; i < _model.SlotCount; i++)
            {
                if (_cfg.SlotHotkey(i).Value.IsDown())
                {
                    var hover = HoverItemLocator.ItemUnderMouse();
                    if (hover != null) { _model.Link(i, hover); return; }
                    if (_cfg.SafeQuickUse.Value && _model.IsConsumableSlot(i))
                    {
                        _view.Selected = i;
                        return;
                    }
                    if (_model.Activate(i)) _view.Selected = i;
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            _view?.Dispose();
        }
    }
}
