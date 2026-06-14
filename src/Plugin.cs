using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 快捷物品栏入口：装配存储、运行时模型、UGUI 视图、输入与设置面板。
    /// 检测到 SaveManager 时合并设置分页，否则注入独立菜单按钮与窗口。
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(SaveManagerGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "com.casualtiesUnknown.hotbar";
        internal const string PluginName = "CuHotbar";
        internal const string PluginVersion = "1.0.7";

        private const string SaveManagerGuid = "com.casualtiesUnknown.saveManager";

        private static ManualLogSource _log;

        private HotbarConfig _cfg;
        private HotbarStore _store;
        private HotbarModel _model;
        private HotbarView _view;
        private HotbarWindow _window;
        private InGameOverlay _overlay;
        private Harmony _harmony;
        private bool _standalone;

        private void Awake()
        {
            _log = Logger;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            ModLog.Init(_log);

            _cfg = new HotbarConfig(Config);
            HotbarRuntime.Config = _cfg;
            ModLog.ShowInConsole = _cfg.ShowLogInConsole.Value;
            _cfg.ShowLogInConsole.SettingChanged += (_, __) => ModLog.ShowInConsole = _cfg.ShowLogInConsole.Value;
            UpdateChecker.Enabled = _cfg.AcceptUpdateNotice.Value;
            _cfg.AcceptUpdateNotice.SettingChanged += (_, __) => UpdateChecker.Enabled = _cfg.AcceptUpdateNotice.Value;

            _store = new HotbarStore();
            _model = new HotbarModel(_cfg, _store);
            _view = new HotbarView(_cfg, _model);

            var controller = gameObject.AddComponent<HotbarController>();
            controller.Init(_cfg, _model, _view);

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(Plugin).Assembly);
            gameObject.AddComponent<UpdateChecker>();

            _standalone = !Chainloader.PluginInfos.ContainsKey(SaveManagerGuid);
            if (!_standalone)
            {
                if (SaveManagerTabBridge.Register(_cfg))
                {
                    ModLog.Info("检测到 SaveManager：设置已合并到其面板分页");
                }
                else
                {
                    _standalone = true;
                    ModLog.Info("SaveManager 无扩展点，回退独立模式");
                }
            }
            if (_standalone)
            {
                _window = new HotbarWindow(_cfg);
                _overlay = new InGameOverlay();
                MenuButtonInjector.Setup(() => _window.Toggle());
                ModLog.Info("独立模式：注入菜单按钮与设置窗口");
            }

            ModLog.Info($"{PluginName} v{PluginVersion} ready");
        }

        private void LateUpdate()
        {
            bool textInput = _standalone && _window != null && _window.ExpectsTextInput;
            ImGuiImeRecovery.TickUpdate(textInput);

            if (_standalone && HotbarConfig.TriggeredThisFrame(_cfg.ToggleSettingsHotkey))
            {
                _window.Toggle();
            }
            if (_standalone && _window != null && _window.Open && Input.GetKeyDown(KeyCode.Escape))
            {
                _window.Close();
            }
        }

        private void OnGUI()
        {
            bool textInput = _standalone && _window != null && _window.ExpectsTextInput;
            ImGuiImeRecovery.TickOnGui(textInput);
            if (_standalone) _overlay?.Draw(() => _window.OpenIfClosed());
            _window?.Draw();
        }

        private void OnDestroy()
        {
            try { MenuButtonInjector.Dispose(); } catch { }
            try { _harmony?.UnpatchSelf(); } catch { }
        }
    }
}
