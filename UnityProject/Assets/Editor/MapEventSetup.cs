using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class MapEventSetup
    {
        public const string AssetPath = "Assets/Editor/MapEventCatalog.asset";
        private static readonly Regex Exit = new Regex("^x24\\((-?\\d+),(-?\\d+),\"([^\"]+)\"\\)$");
        private static readonly Regex Entry = new Regex("^x26\\((-?\\d+),(-?\\d+),\"([lrud])\"\\)$");
        private static readonly Regex Actor = new Regex("^x30\\((-?\\d+),(-?\\d+),\"([lrud])\",(-?\\d+),(-?\\d+),(-?\\d+)\\)$");
        private static readonly Regex Callback = new Regex("^x37\\((-?\\d+),\"([^\"]+)\"(?:,(-?\\d+))?\\)$");

        [MenuItem("古剑/内容/生成地图事件资产（保留剧情原文）")]
        public static void Rebuild()
        {
            RequireEditMode();
            var stages = (OriginalAssets.Table<StageDefinition>("stages") ?? new StageDefinition[0])
                .Select(d => new MapEventStage { mapId = d.map, stageScript = d.script,
                    events = Parse(d.map, d.script, OriginalAssets.Script(d.script)).ToArray() }).ToArray();
            var catalog = Load();
            if (!catalog)
            {
                if (File.Exists(AssetPath)) throw new InvalidDataException("地图事件路径已被其他资产占用");
                catalog = ScriptableObject.CreateInstance<MapEventCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }
            catalog.stages = stages;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssetIfDirty(catalog);
            Debug.Log("GUJIAN_MAP_EVENTS stages=" + stages.Length + " events=" + stages.Sum(s => s.events.Length));
        }

        public static List<MapEventRecord> Parse(string mapId, string script, string[] lines)
        {
            var result = new List<MapEventRecord>();
            var conditions = new Stack<string>();
            var actors = new Dictionary<int, MapEventRecord>();
            for (int i = 0; i < (lines == null ? 0 : lines.Length); i++)
            {
                string line = (lines[i] ?? "").Trim();
                if (line.StartsWith("if(") || line.StartsWith("while(")) { conditions.Push(line); continue; }
                if (line == "else")
                {
                    if (conditions.Count > 0) conditions.Push("else of " + conditions.Pop());
                    continue;
                }
                if (line == "endif" || line == "loop") { if (conditions.Count > 0) conditions.Pop(); continue; }
                var record = new MapEventRecord { mapId = mapId, stageScript = script, sourceLine = i,
                    sourceText = lines[i], condition = string.Join(" → ", conditions.Reverse()) };
                var m = Exit.Match(line);
                if (m.Success)
                {
                    record.kind = MapEventKind.Exit; record.x = int.Parse(m.Groups[1].Value);
                    record.y = int.Parse(m.Groups[2].Value); record.targetScript = m.Groups[3].Value;
                    result.Add(record); continue;
                }
                m = Entry.Match(line);
                if (m.Success)
                {
                    record.kind = MapEventKind.Entry; record.x = int.Parse(m.Groups[1].Value);
                    record.y = int.Parse(m.Groups[2].Value); record.facing = m.Groups[3].Value;
                    result.Add(record); continue;
                }
                m = Actor.Match(line);
                if (m.Success)
                {
                    record.instanceId = int.Parse(m.Groups[1].Value); record.actorId = int.Parse(m.Groups[2].Value);
                    record.x = int.Parse(m.Groups[4].Value); record.y = int.Parse(m.Groups[5].Value);
                    record.facing = m.Groups[3].Value; record.actorSourceLine = i; record.hasActorPosition = true;
                    actors[record.instanceId] = record; continue;
                }
                m = Callback.Match(line);
                if (m.Success)
                {
                    record.kind = MapEventKind.ActorCallback; record.instanceId = int.Parse(m.Groups[1].Value);
                    record.actorScript = m.Groups[2].Value;
                    record.callbackMode = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;
                    if (actors.TryGetValue(record.instanceId, out var actor))
                    {
                        record.actorId = actor.actorId; record.x = actor.x; record.y = actor.y;
                        record.facing = actor.facing; record.actorSourceLine = actor.sourceLine; record.hasActorPosition = true;
                    }
                    result.Add(record);
                }
            }
            return result;
        }

        public static void OpenMap(string mapId) => MapEventEditor.OpenMap(mapId);
        public static MapEventCatalog Load() => AssetDatabase.LoadAssetAtPath<MapEventCatalog>(AssetPath);
        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("退出播放模式后编辑地图事件");
        }

        // Write back one source command, retaining metadata, branches, and callback mode.
        public static void Save(MapEventRecord record, int x, int y, string target, string facing)
        {
            RequireEditMode();
            var story = AssetDatabase.LoadAssetAtPath<StorySequence>(StoryAssetSetup.AssetPath(record.stageScript));
            if (!story) throw new InvalidOperationException("请先补充该剧情的 StorySequence 资产");
            string replacement;
            if (record.kind == MapEventKind.Entry)
            {
                if (string.IsNullOrEmpty(facing) || facing.Length != 1 || "lrud".IndexOf(facing, StringComparison.Ordinal) < 0)
                    throw new ArgumentException("朝向必须是 l/r/u/d");
                replacement = "x26(" + x + "," + y + ",\"" + facing + "\")";
            }
            else
            {
                string id = StorySequence.NormalizeId(target);
                if (!OriginalAssets.HasScript(id)) throw new ArgumentException("目标剧情不存在：" + id);
                if (record.kind == MapEventKind.Exit) replacement = "x24(" + x + "," + y + ",\"" + id + ".s\")";
                else
                {
                    var match = Callback.Match(record.sourceText.Trim());
                    if (!match.Success) throw new InvalidOperationException("角色回调原文已失效，请重新扫描");
                    var group = match.Groups[2];
                    replacement = match.Value.Remove(group.Index, group.Length).Insert(group.Index, id + ".s");
                }
            }
            string raw = record.sourceText;
            int offset = raw.IndexOf(raw.Trim(), StringComparison.Ordinal);
            replacement = raw.Substring(0, offset) + replacement + raw.Substring(offset + raw.Trim().Length);
            // Guard before Undo so a stale selection cannot overwrite another editor's changes.
            var lines = story.ReadInstructions();
            if (record.sourceLine < 0 || record.sourceLine >= lines.Length || lines[record.sourceLine] != raw)
                throw new InvalidOperationException("剧情步骤已改变，请重新扫描地图事件后编辑");
            Undo.RecordObject(story, "编辑地图事件");
            story.ReplaceInstruction(record.sourceLine, raw, replacement);
            EditorUtility.SetDirty(story); AssetDatabase.SaveAssetIfDirty(story); OriginalAssets.ClearCache(); Rebuild();
        }

        [Serializable] public sealed class ReferenceIssue { public string stage, target; public int line; }
        [Serializable] public sealed class Audit
        {
            public int stages, maps, exits, entries, interactions, deaths, specials, otherCallbacks;
            public ReferenceIssue[] missingReferences;
        }
        public static Audit AuditCatalog()
        {
            RequireEditMode();
            var catalog = Load(); if (!catalog) throw new InvalidOperationException("请先生成地图事件资产");
            var maps = new HashSet<string>(OriginalAssets.Catalog.maps.Select(m => m.id));
            var missing = new List<ReferenceIssue>();
            var audit = new Audit { stages = catalog.stages.Length, maps = catalog.stages.Select(s => s.mapId).Distinct().Count() };
            foreach (var stage in catalog.stages)
            {
                if (!maps.Contains(stage.mapId)) throw new InvalidDataException("无效地图引用：" + stage.mapId);
                var source = OriginalAssets.Script(stage.stageScript);
                if (JsonUtility.ToJson(new MapEventStage { mapId = stage.mapId, stageScript = stage.stageScript,
                    events = Parse(stage.mapId, stage.stageScript, source).ToArray() }) != JsonUtility.ToJson(stage))
                    throw new InvalidDataException("地图事件索引已过期，请重新扫描：" + stage.stageScript);
                foreach (var e in stage.events)
                {
                    if (e.kind == MapEventKind.Exit) audit.exits++;
                    else if (e.kind == MapEventKind.Entry) audit.entries++;
                    else if (e.callbackMode == 0) audit.interactions++;
                    else if (e.callbackMode == 1) audit.deaths++;
                    else if (e.callbackMode == 2) audit.specials++;
                    else audit.otherCallbacks++;
                    string target = e.kind == MapEventKind.Exit ? e.targetScript : e.actorScript;
                    if (e.kind != MapEventKind.Entry && (string.IsNullOrEmpty(target) || !OriginalAssets.HasScript(target)))
                        missing.Add(new ReferenceIssue { stage = stage.stageScript, line = e.sourceLine + 1, target = target });
                }
            }
            audit.missingReferences = missing.ToArray();
            return audit;
        }
        [MenuItem("古剑/内容/校验地图事件引用")]
        public static void Validate()
        {
            var audit = AuditCatalog();
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/map-events.json", JsonUtility.ToJson(audit, true));
            Debug.Log("GUJIAN_MAP_EVENTS_VALIDATION " + JsonUtility.ToJson(audit));
        }
    }

    public sealed class MapEventEditor : EditorWindow
    {
        private string mapFilter = "", search = "", target = "", facing = "r", feedback;
        private Vector2 scroll; private MapEventCatalog catalog; private MapEventRecord selected;
        private int x, y;
        public static void OpenMap(string id)
        {
            var window = GetWindow<MapEventEditor>("地图事件"); window.mapFilter = id; window.Reload();
        }
        private void Reload() { catalog = MapEventSetup.Load(); selected = null; Repaint(); }
        private void OnEnable() { Reload(); Undo.undoRedoPerformed += Reload; }
        private void OnDisable() { Undo.undoRedoPerformed -= Reload; }
        private void OnGUI()
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                mapFilter = EditorGUILayout.TextField(mapFilter, EditorStyles.toolbarTextField);
                search = EditorGUILayout.TextField(search, EditorStyles.toolbarTextField);
                if (GUILayout.Button("重新扫描", EditorStyles.toolbarButton, GUILayout.Width(65)))
                    Run(() => { MapEventSetup.Rebuild(); Reload(); });
                if (GUILayout.Button("校验", EditorStyles.toolbarButton, GUILayout.Width(45))) Run(MapEventSetup.Validate);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("出口坐标是地图格子；入口是原像素坐标。条件仅供定位，未求值。角色坐标为当前脚本中最近创建位置，实际布局请编辑对应阶段 Prefab。", MessageType.Info);
                if (!catalog) { EditorGUILayout.HelpBox("点击重新扫描生成地图事件索引。", MessageType.Info); return; }
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(100));
                foreach (var stage in catalog.stages ?? new MapEventStage[0])
                {
                    if (!string.IsNullOrEmpty(mapFilter) && stage.mapId.IndexOf(mapFilter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    var events = (stage.events ?? new MapEventRecord[0]).Where(e => string.IsNullOrEmpty(search) ||
                        (stage.stageScript + e.targetScript + e.actorScript + e.condition).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                    if (events.Length == 0) continue;
                    EditorGUILayout.LabelField(stage.mapId + " / " + stage.stageScript + "  (" + events.Length + ")", EditorStyles.boldLabel);
                    foreach (var e in events)
                    {
                        string label = e.kind == MapEventKind.Exit ? "出口（格）" : e.kind == MapEventKind.Entry ? "入口（像素）" :
                            "角色 " + e.instanceId + (e.callbackMode == 1 ? " 死亡" : e.callbackMode == 2 ? " 特殊" : e.callbackMode == 0 ? " 交谈" : " 回调" + e.callbackMode);
                        if (GUILayout.Toggle(selected == e, label + "  行" + (e.sourceLine + 1) + "  (" + e.x + "," + e.y + ")  " +
                            (e.targetScript ?? e.actorScript ?? e.facing) + "  " + e.condition, "Button") && selected != e)
                        { selected = e; x = e.x; y = e.y; target = e.targetScript ?? e.actorScript ?? ""; facing = e.facing ?? "r"; feedback = null; }
                    }
                }
                EditorGUILayout.EndScrollView();
                if (selected != null)
                {
                    EditorGUILayout.LabelField(selected.stageScript + " 第" + (selected.sourceLine + 1) + "行", EditorStyles.boldLabel);
                    EditorGUILayout.SelectableLabel(selected.sourceText, GUILayout.Height(20));
                    if (selected.kind != MapEventKind.ActorCallback)
                    { x = EditorGUILayout.IntField("X", x); y = EditorGUILayout.IntField("Y", y); }
                    if (selected.kind == MapEventKind.Entry) facing = EditorGUILayout.TextField("朝向 l/r/u/d", facing);
                    else target = EditorGUILayout.TextField("目标剧情", target);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("定位剧情资产")) StoryAssetSetup.OpenStory(selected.stageScript);
                    if (GUILayout.Button("写回剧情并更新索引")) Run(() => { MapEventSetup.Save(selected, x, y, target, facing); Reload(); feedback = "已保存；可通过 Unity 撤销还原剧情。撤销后请重新扫描索引。"; });
                    EditorGUILayout.EndHorizontal();
                }
                if (!string.IsNullOrEmpty(feedback)) EditorGUILayout.HelpBox(feedback, MessageType.Info);
            }
        }
        private void Run(Action action)
        {
            try { action(); }
            catch (Exception ex) { feedback = ex.Message; }
        }
    }
}
