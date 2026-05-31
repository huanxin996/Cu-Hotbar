using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace CasualtiesUnknown.Hotbar
{
    /// <summary>
    /// 按角色（runId=cId）持久化快捷栏槽位指向。每个 runId 一份 json，存物品 id 列表，空槽为空串。
    /// </summary>
    internal sealed class HotbarStore
    {
        private readonly string _dataRoot;

        internal HotbarStore()
        {
            _dataRoot = ResolveDataRoot();
            try { Directory.CreateDirectory(_dataRoot); }
            catch (Exception ex) { ModLog.Warning($"创建 hotbarData 目录失败：{ex.Message}"); }
        }

        /// <summary>当前角色 runId，取自 WoundView.cInfo[2]；读不到返回 0（未识别）。</summary>
        internal static int CurrentRunId()
        {
            try
            {
                var view = WoundView.view;
                if (view != null && view.cInfo != null && view.cInfo.Length >= 3)
                    return view.cInfo[2];
            }
            catch (Exception ex)
            {
                ModLog.Warning($"读取 runId 失败：{ex.Message}");
            }
            return 0;
        }

        internal List<string> Load(int runId)
        {
            string path = SlotsPath(runId);
            try
            {
                if (!File.Exists(path)) return new List<string>();
                var ids = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
                return ids ?? new List<string>();
            }
            catch (Exception ex)
            {
                ModLog.Warning($"加载快捷栏配置失败 runId={runId}：{ex.Message}");
                return new List<string>();
            }
        }

        internal void Save(int runId, List<string> slotItemIds)
        {
            string path = SlotsPath(runId);
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(slotItemIds ?? new List<string>()));
            }
            catch (Exception ex)
            {
                ModLog.Warning($"保存快捷栏配置失败 runId={runId}：{ex.Message}");
            }
        }

        private string SlotsPath(int runId) => Path.Combine(_dataRoot, runId + ".json");

        private static string ResolveDataRoot()
        {
            var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                          ?? Application.persistentDataPath;
            return Path.Combine(asmDir, "hotbarData");
        }
    }
}
