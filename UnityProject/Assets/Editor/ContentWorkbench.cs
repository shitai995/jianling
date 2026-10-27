using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class StoryAssetSetup
    {
        public const string Root = "Assets/Resources/Stories";
        [MenuItem("古剑/内容/补充剧情资产（保留已有编辑）")]
        public static void ImportMissing()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("退出播放模式后导入剧情资产");
            if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets/Resources", "Stories");
            var titles = OriginalAssets.Table<StageDefinition>("stages").GroupBy(s => s.script).ToDictionary(g => g.Key, g => g.First().name);
            int created = 0, preserved = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var file in Directory.GetFiles("Assets/Resources/Original/Scripts", "*.json").OrderBy(s => s, StringComparer.Ordinal))
                {
                    string id = Path.GetFileNameWithoutExtension(file), path = Root + "/" + id + ".asset";
                    if (AssetDatabase.LoadAssetAtPath<StorySequence>(path)) { preserved++; continue; }
                    // A path occupied by another asset must never be overwritten.
                    if (File.Exists(path)) throw new InvalidDataException("剧情目标路径已被其他资产占用：" + path);
                    var source = JsonUtility.FromJson<ScriptData>(File.ReadAllText(file));
                    if (source == null || source.instructions == null) throw new InvalidDataException("剧情数据无效：" + file);
                    var story = ScriptableObject.CreateInstance<StorySequence>();
                    story.Import(id, titles.TryGetValue(id, out var title) ? title : id, source.instructions);
                    AssetDatabase.CreateAsset(story, path); created++;
                }
            }
            finally { AssetDatabase.StopAssetEditing(); AssetDatabase.SaveAssets(); }
            OriginalAssets.ClearCache();
            Debug.Log("GUJIAN_STORY_ASSETS created=" + created + " preserved=" + preserved);
        }
        public static string AssetPath(string id) => Root + "/" + StorySequence.NormalizeId(id) + ".asset";
        public static void OpenStory(string id)
        {
            var native = AssetDatabase.LoadAssetAtPath<StorySequence>(AssetPath(id));
            UnityEngine.Object target = native ? (UnityEngine.Object)native : AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Original/Scripts/" + StorySequence.NormalizeId(id) + ".json");
            if (target) { Selection.activeObject = target; EditorGUIUtility.PingObject(target); }
        }
    }

    [CustomEditor(typeof(StorySequence))]
    public sealed class StorySequenceInspector : UnityEditor.Editor
    {
        private int page;
        private const int PageSize = 30;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var sequence = (StorySequence)target;
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("scriptId"), new GUIContent("脚本标识"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"), new GUIContent("显示名称"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("notes"), new GUIContent("开发备注"));
                string expectedId = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sequence));
                if (!string.IsNullOrEmpty(sequence.ScriptId) && sequence.ScriptId != expectedId)
                    EditorGUILayout.HelpBox("脚本标识应与资产文件名一致：" + expectedId, MessageType.Error);
                EditorGUILayout.HelpBox("此资产是运行时剧情来源。每个步骤保留原脚本语法及命中时序；修改支持撤销。新增剧情使用独立标识，x20 可调用该标识。", MessageType.Info);
                var lines = serializedObject.FindProperty("instructions");
                int pages = Mathf.Max(1, (lines.arraySize + PageSize - 1) / PageSize);
                page = Mathf.Clamp(page, 0, pages - 1);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("上一页")) page = Mathf.Max(0, page - 1);
                EditorGUILayout.LabelField("步骤 " + lines.arraySize + " / 页 " + (page + 1) + " / " + pages, GUILayout.Width(175));
                if (GUILayout.Button("下一页")) page = Mathf.Min(pages - 1, page + 1);
                EditorGUILayout.EndHorizontal();
                int end = Mathf.Min(lines.arraySize, (page + 1) * PageSize);
                for (int i = page * PageSize; i < end; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(32));
                    var line = lines.GetArrayElementAtIndex(i);
                    line.stringValue = EditorGUILayout.TextArea(line.stringValue, GUILayout.MinHeight(30));
                    if (GUILayout.Button("+", GUILayout.Width(24)))
                    {
                        lines.InsertArrayElementAtIndex(i + 1); lines.GetArrayElementAtIndex(i + 1).stringValue = "";
                        EditorGUILayout.EndHorizontal(); break;
                    }
                    if (GUILayout.Button("−", GUILayout.Width(24)))
                    { lines.DeleteArrayElementAtIndex(i); EditorGUILayout.EndHorizontal(); break; }
                    EditorGUILayout.EndHorizontal();
                }
                if (GUILayout.Button("添加步骤")) { int i = lines.arraySize++; lines.GetArrayElementAtIndex(i).stringValue = ""; page = i / PageSize; }
                serializedObject.ApplyModifiedProperties();
                if (GUILayout.Button("保存剧情资产")) { AssetDatabase.SaveAssetIfDirty(sequence); OriginalAssets.ClearCache(); }
            }
            if (GUILayout.Button("查看原始 JSON"))
            {
                var original = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Original/Scripts/" + expectedFileName(sequence) + ".json");
                if (original) AssetDatabase.OpenAsset(original);
            }
        }
        private static string expectedFileName(StorySequence story) => Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(story));
    }

    public sealed class ContentWorkbench : EditorWindow
    {
        private int tab, selected;
        private string search = "";
        private Vector2 listScroll, detailScroll;
        private OriginalAssetCatalog catalog;
        private StageDefinition[] stages = new StageDefinition[0];
        private SpriteDefinition[] actors = new SpriteDefinition[0];
        private StorySequence[] stories = new StorySequence[0];
        private sealed class Row { public string id, label; public object value; }
        [MenuItem("古剑/内容/内容工作台")]
        public static void Open() => GetWindow<ContentWorkbench>("古剑内容工作台");
        private void OnEnable() { Reload(); EditorApplication.projectChanged += Reload; }
        private void OnDisable() { EditorApplication.projectChanged -= Reload; }
        private void Reload()
        {
            catalog = AssetDatabase.LoadAssetAtPath<OriginalAssetCatalog>("Assets/Resources/OriginalAssetCatalog.asset");
            stages = OriginalAssets.Table<StageDefinition>("stages") ?? new StageDefinition[0];
            actors = OriginalAssets.Table<SpriteDefinition>("sprite") ?? new SpriteDefinition[0];
            stories = AssetDatabase.IsValidFolder(StoryAssetSetup.Root) ? AssetDatabase.FindAssets("t:StorySequence", new[] { StoryAssetSetup.Root }).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<StorySequence>).Where(s => s).OrderBy(s => s.name, StringComparer.Ordinal).ToArray() : new StorySequence[0];
            Repaint();
        }
        private List<Row> Rows()
        {
            IEnumerable<Row> rows;
            if (tab == 0) rows = (catalog && catalog.maps != null ? catalog.maps : new OriginalAssetCatalog.MapEntry[0]).Select(m => new Row { id = m.id, label = m.id + "  " + string.Join(" / ", stages.Where(s => s.map == m.id).Select(s => s.name).Distinct().ToArray()), value = m });
            else if (tab == 1) rows = stories.Select(s => new Row { id = s.name, label = s.name + "  " + s.DisplayName, value = s });
            else rows = actors.Select(a => new Row { id = a.id.ToString(), label = a.id + "  " + a.name, value = a });
            return rows.Where(r => string.IsNullOrEmpty(search) || r.label.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            int next = GUILayout.Toolbar(tab, new[] { "地图", "剧情", "精灵" }, EditorStyles.toolbarButton);
            if (next != tab) { tab = next; selected = 0; }
            string nextSearch = GUILayout.TextField(search, EditorStyles.toolbarTextField);
            if (nextSearch != search) { search = nextSearch; selected = 0; }
            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(45))) Reload();
            if (GUILayout.Button("物品／战斗配置", EditorStyles.toolbarButton)) ContentConfigurationWindow.Open();
            if (GUILayout.Button("瓦片地图", EditorStyles.toolbarButton)) TilemapAuthoringWindow.Open();
            EditorGUILayout.EndHorizontal();
            var rows = Rows(); selected = Mathf.Clamp(selected, 0, Mathf.Max(0, rows.Count - 1));
            EditorGUILayout.BeginHorizontal();
            listScroll = EditorGUILayout.BeginScrollView(listScroll, GUILayout.Width(Mathf.Clamp(position.width * .4f, 220, 380)));
            for (int i = 0; i < rows.Count; i++) if (GUILayout.Toggle(i == selected, rows[i].label, "Button")) selected = i;
            EditorGUILayout.EndScrollView();
            detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
            if (rows.Count == 0) EditorGUILayout.HelpBox("没有匹配内容。剧情资产可通过下方按钮补充。", MessageType.Info);
            else if (tab == 0) DrawMap((OriginalAssetCatalog.MapEntry)rows[selected].value);
            else if (tab == 1) DrawStory((StorySequence)rows[selected].value);
            else DrawActor((SpriteDefinition)rows[selected].value);
            if (tab == 1)
                using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                    if (GUILayout.Button("补充缺少的剧情资产（保留已有编辑）")) { StoryAssetSetup.ImportMissing(); Reload(); }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }
        private void DrawMap(OriginalAssetCatalog.MapEntry entry)
        {
            EditorGUILayout.LabelField("地图 " + entry.id, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(entry.data.Width + " × " + entry.data.Height + "  /  格子 " + entry.data.columns + " × " + entry.data.rows);
            EditorGUILayout.ObjectField("地图布局", entry.prefab, typeof(GameObject), false);
            if (entry.prefab && GUILayout.Button("编辑地图图层 Prefab")) AssetDatabase.OpenAsset(entry.prefab);
            if (GUILayout.Button("编辑本图碰撞"))
            {
                if (entry.prefab && entry.prefab.GetComponent<EditableTilemap>()) TilemapAuthoringWindow.OpenMap(entry.prefab);
                else MapCollisionEditor.OpenMap(entry.id);
            }
            if (GUILayout.Button("编辑出口 / 入口 / NPC 事件")) MapEventSetup.OpenMap(entry.id);
            EditorGUILayout.Space(); EditorGUILayout.LabelField("使用本地图的剧情入口", EditorStyles.boldLabel);
            foreach (var stage in stages.Where(s => s.map == entry.id))
            {
                EditorGUILayout.LabelField(stage.script + "  " + stage.name);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("布局 / NPC"))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Baked/Stages/" + stage.script + ".prefab");
                    if (prefab) AssetDatabase.OpenAsset(prefab);
                }
                if (GUILayout.Button("剧情")) StoryAssetSetup.OpenStory(stage.script);
                EditorGUILayout.EndHorizontal();
            }
        }
        private void DrawStory(StorySequence story)
        {
            EditorGUILayout.LabelField(story.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("标识", story.name);
            EditorGUILayout.LabelField("步骤数", story.InstructionCount.ToString());
            if (GUILayout.Button("编辑剧情步骤")) { Selection.activeObject = story; EditorGUIUtility.PingObject(story); }
            var stage = stages.FirstOrDefault(s => s.script == story.name);
            if (stage != null)
            {
                EditorGUILayout.LabelField("地图", stage.map);
                if (GUILayout.Button("定位地图")) { tab = 0; search = stage.map; selected = 0; }
            }
            if (GUILayout.Button("打开剧情调试器")) StoryDebugger.Open();
            EditorGUILayout.HelpBox("编辑资产保存后，新开局或重新载入剧情会读取新步骤；原始 JSON 用于核对。", MessageType.Info);
        }
        private void DrawActor(SpriteDefinition actor)
        {
            EditorGUILayout.LabelField(actor.id + "  " + actor.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("原动作", actor.animation);
            var saved = catalog && catalog.actors != null ? catalog.actors.FirstOrDefault(a => a.id == actor.id) : null;
            if (saved != null && saved.prefab)
            {
                EditorGUILayout.ObjectField("角色 Prefab", saved.prefab, typeof(OriginalActor), false);
                if (GUILayout.Button("编辑角色 Prefab")) AssetDatabase.OpenAsset(saved.prefab);
            }
            var animation = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Original/Animations/action/" + actor.animation + ".json");
            if (animation && GUILayout.Button("查看动作 / 命中框数据")) AssetDatabase.OpenAsset(animation);
            if (GUILayout.Button("编辑动作、换装与攻防框")) AnimationEditorWindow.OpenDefinition(actor.id);
            foreach (var image in actor.images ?? new string[0])
                EditorGUILayout.ObjectField(image, AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Original/" + image + ".png"), typeof(Texture2D), false);
            EditorGUILayout.HelpBox("动作编辑器提供80ms预览、逐帧编辑、朝向、换装、位移轨迹和攻防框。共享同一动作标识的精灵会使用同一发布版本。", MessageType.Info);
        }
    }
}
