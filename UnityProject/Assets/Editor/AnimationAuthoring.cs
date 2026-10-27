using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class AnimationAuthoring
    {
        public const string Root = "Assets/Resources/EditedAnimations";
        public static string PathFor(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.IndexOfAny(new[] { '/', '\\', ':' }) >= 0 || id == "." || id == "..")
                throw new ArgumentException("动作标识应为文件名");
            return Root + "/" + id + ".asset";
        }
        public static EditableAnimation GetOrCreate(string id)
        {
            if (Application.isPlaying) throw new InvalidOperationException("退出播放后编辑动作");
            string path = PathFor(id);
            var saved = AssetDatabase.LoadAssetAtPath<EditableAnimation>(path);
            if (saved) return saved;
            if (File.Exists(path)) throw new IOException("目标路径已占用：" + path);
            var original = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Original/Animations/action/" + id + ".json");
            if (!original) throw new FileNotFoundException("缺少原动作：" + id);
            var asset = ScriptableObject.CreateInstance<EditableAnimation>();
            asset.draft = JsonUtility.FromJson<OriginalAnimation>(original.text);
            Validate(id, asset.draft);
            asset.Publish();
            if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets/Resources", "EditedAnimations");
            AssetDatabase.CreateAsset(asset, path); AssetDatabase.SaveAssetIfDirty(asset);
            OriginalAssets.ClearCache();
            return asset;
        }

        public static Dictionary<int, HashSet<string>> ImageSources(string id)
        {
            var result = new Dictionary<int, HashSet<string>>();
            Action<int, string> add = (sheet, image) => {
                if (!result.TryGetValue(sheet, out var images)) result[sheet] = images = new HashSet<string>();
                images.Add(image);
            };
            var definitions = OriginalAssets.Table<SpriteDefinition>("sprite").Where(d => d.animation == id).ToArray();
            if (definitions.Length == 0) throw new InvalidOperationException("动作需先登记精灵配置：" + id);
            foreach (var definition in definitions)
            {
                for (int i = 0; i < definition.images.Length; i++) add(i, definition.images[i]);
                if (definition.type == 0)
                {
                    foreach (var weapon in OriginalAssets.Table<EquipmentDefinition>("equip")) add(4, "action/" + weapon.image);
                    add(4, "action/w_01");
                    foreach (string image in new[] { "w_97", "w_98", "w_99", "w_00" }) add(1, "action/" + image);
                }
            }
            return result;
        }

        public static void Validate(string id, OriginalAnimation data)
        {
            if (data == null || data.sheets == null || data.frames == null || data.frames.Length == 0 || data.actions == null || data.actions.Length == 0)
                throw new InvalidDataException("动作必须包含图集、帧和动作步骤");
            var source = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Original/Animations/action/" + id + ".json");
            var baseline = source ? JsonUtility.FromJson<OriginalAnimation>(source.text) : null;
            if (baseline != null && (data.actions.Length < baseline.actions.Length || data.sheets.Length != baseline.sheets.Length))
                throw new InvalidDataException("保留已有动作编号和换装图层数量；可追加动作");
            if (data.footWidth < 0 || data.footHeight < 0) throw new InvalidDataException("脚底范围不能为负数");
            var images = ImageSources(id);
            for (int i = 0; i < data.sheets.Length; i++)
            {
                if (data.sheets[i] == null || data.sheets[i].rects == null || !images.ContainsKey(i)) throw new InvalidDataException("图集缺失：" + i);
                foreach (string image in images[i])
                {
                    var texture = OriginalAssets.Texture(image);
                    foreach (var r in data.sheets[i].rects)
                        if (r == null || r.x < 0 || r.y < 0 || r.w < 0 || r.h < 0 || (long)r.x + r.w > texture.width || (long)r.y + r.h > texture.height)
                            throw new InvalidDataException("图集裁切越界：" + image + "，图层 " + i);
                }
            }
            foreach (var frame in data.frames)
            {
                if (frame == null || frame.pieces == null || frame.attackRects == null || frame.bodyRects == null || frame.edgeRect == null)
                    throw new InvalidDataException("帧片和攻防框数组不能为空");
                foreach (var piece in frame.pieces)
                {
                    if (piece == null || piece.transform < 0 || piece.transform > 7 || piece.w < 0 || piece.h < 0)
                        throw new InvalidDataException("帧片变换或尺寸无效");
                    if (piece.image >= 0 && (piece.image >= data.sheets.Length || piece.rect < 0 || piece.rect >= data.sheets[piece.image].rects.Length))
                        throw new InvalidDataException("帧片引用了不存在的图层或裁切");
                }
                foreach (var r in frame.attackRects.Concat(frame.bodyRects).Concat(new[] { frame.edgeRect }))
                    if (r == null || r.w < 0 || r.h < 0) throw new InvalidDataException("框尺寸不能为负数");
            }
            foreach (var action in data.actions)
            {
                if (action == null || action.steps == null || action.steps.Length == 0) throw new InvalidDataException("每个动作至少有一个步骤");
                foreach (var step in action.steps)
                    if (step == null || step.frame < 0 || step.frame >= data.frames.Length) throw new InvalidDataException("动作步骤引用了不存在的帧");
            }
        }

        public static void SaveAndPublish(EditableAnimation asset)
        {
            if (Application.isPlaying || OriginalAssetBaker.Running) throw new InvalidOperationException("退出播放并等待资源任务完成");
            string id = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(asset));
            if (AssetDatabase.GetAssetPath(asset) != PathFor(id)) throw new InvalidOperationException("动作资产需保存在 " + Root);
            Validate(id, asset.draft);
            // Prepare saved renderer capacity and all outfit sprite variants before exposing new data.
            OriginalAssetBaker.PrepareEditedAnimation(id, asset.draft, ImageSources(id));
            Undo.RecordObject(asset, "发布动作"); asset.Publish(); EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets(); OriginalAssets.ClearCache();
        }

        public static int DuplicateFrame(EditableAnimation asset, int action, int tick)
        {
            var step = asset.draft.actions[action].steps[tick];
            var copy = JsonUtility.FromJson<OriginalFrame>(JsonUtility.ToJson(asset.draft.frames[step.frame]));
            Undo.RecordObject(asset, "复制动作帧");
            int index = asset.draft.frames.Length; Array.Resize(ref asset.draft.frames, index + 1);
            asset.draft.frames[index] = copy; step.frame = index; EditorUtility.SetDirty(asset);
            return index;
        }
    }

    [CustomEditor(typeof(EditableAnimation))]
    public sealed class EditableAnimationInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("草稿可撤销。发布会校验共享角色与全部换装图集，在编辑期补齐 Sprite 和 Prefab 容量。游戏读取发布版本。", MessageType.Info);
            if (GUILayout.Button("打开动作编辑器")) AnimationEditorWindow.OpenAsset((EditableAnimation)target);
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                serializedObject.Update(); EditorGUILayout.PropertyField(serializedObject.FindProperty("draft"), true); serializedObject.ApplyModifiedProperties();
                if (GUILayout.Button("校验并发布"))
                    try { AnimationAuthoring.SaveAndPublish((EditableAnimation)target); }
                    catch (Exception e) { Debug.LogError(e.Message); }
            }
        }
    }

    public sealed class AnimationEditorWindow : EditorWindow
    {
        [SerializeField] private EditableAnimation asset;
        [SerializeField] private int definitionId = 200, action, tick;
        [SerializeField] private bool mirrored, showBoxes = true, showPath = true;
        [SerializeField] private float zoom = 2;
        [SerializeField] private string weapon = "action/w_01", spirit = "action/w_97";
        private SpriteDefinition[] definitions;
        private SpriteDefinition definition;
        private SerializedObject serialized;
        private Vector2 scroll;
        private bool playing;
        private double lastTick;
        private string status;
        private Texture2D[] textures;
        private OriginalAnimation preview;
        private string previewKey;
        [MenuItem("古剑/内容/动作编辑器")]
        public static void Open() => OpenDefinition(200);
        public static void OpenDefinition(int id)
        {
            var window = GetWindow<AnimationEditorWindow>("古剑动作编辑器"); window.definitionId = id; window.Reload();
        }
        public static void OpenAsset(EditableAnimation value)
        {
            var d = OriginalAssets.Table<SpriteDefinition>("sprite").FirstOrDefault(s => s.animation == value.name);
            if (d == null) throw new InvalidOperationException("未登记此动作的精灵配置");
            OpenDefinition(d.id);
        }
        private void OnEnable() { Reload(); EditorApplication.update += UpdatePlayback; Undo.undoRedoPerformed += Changed; }
        private void OnDisable() { EditorApplication.update -= UpdatePlayback; Undo.undoRedoPerformed -= Changed; }
        private void Changed() { previewKey = null; Repaint(); }
        private void Reload()
        {
            definitions = OriginalAssets.Table<SpriteDefinition>("sprite");
            definition = definitions.FirstOrDefault(d => d.id == definitionId) ?? definitions.First();
            definitionId = definition.id;
            asset = AssetDatabase.LoadAssetAtPath<EditableAnimation>(AnimationAuthoring.PathFor(definition.animation));
            serialized = asset ? new SerializedObject(asset) : null;
            playing = false; action = tick = 0; textures = null; previewKey = null; status = null;
            Repaint();
        }
        private OriginalAnimation Data => asset ? asset.draft : OriginalAssets.Json<OriginalAnimation>("Animations/action/" + definition.animation);
        private void UpdatePlayback()
        {
            if (!playing || Application.isPlaying || definition == null) return;
            double now = EditorApplication.timeSinceStartup;
            if (now - lastTick < .08) return;
            int steps = (int)((now - lastTick) / .08); lastTick += steps * .08;
            if (Data.actions.Length > action && Data.actions[action].steps.Length > 0) tick = (tick + steps) % Data.actions[action].steps.Length;
            Repaint();
        }
        private void OnGUI()
        {
            if (definition == null) { Reload(); return; }
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                int selected = Array.FindIndex(definitions, d => d.id == definitionId);
                int next = EditorGUILayout.Popup("精灵", selected, definitions.Select(d => d.id + "  " + d.name).ToArray());
                if (next != selected) { definitionId = definitions[next].id; Reload(); }
                EditorGUILayout.LabelField("共享动作", definition.animation + "  /  " + string.Join("、", definitions.Where(d => d.animation == definition.animation).Select(d => d.id.ToString()).ToArray()));
                EditorGUILayout.HelpBox("80ms／步。红：攻击框；绿：受击框；黄：边界；蓝：位移轨迹。帧片与框可被多个动作步骤共用。", MessageType.Info);
                if (!asset)
                {
                    if (GUILayout.Button("创建可编辑动作资产"))
                        try { asset = AnimationAuthoring.GetOrCreate(definition.animation); serialized = new SerializedObject(asset); }
                        catch (Exception e) { status = e.Message; }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("编辑资产", asset, typeof(EditableAnimation), false);
                    if (GUILayout.Button("校验并发布", GUILayout.Width(110)))
                        try { serialized.ApplyModifiedProperties(); AnimationAuthoring.SaveAndPublish(asset); status = "已发布；重新进入场景或开局后读取。"; }
                        catch (Exception e) { status = e.Message; }
                    EditorGUILayout.EndHorizontal();
                }
                if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.Info);
                var data = Data;
                if (data == null || data.actions == null || data.actions.Length == 0) return;
                action = EditorGUILayout.IntSlider("动作编号", action, 0, data.actions.Length - 1);
                var current = data.actions[action];
                if (current == null || current.steps == null || current.steps.Length == 0) { EditorGUILayout.HelpBox("动作缺少步骤，请在 Inspector 修复草稿。", MessageType.Warning); return; }
                tick = Mathf.Clamp(tick, 0, current.steps.Length - 1);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(playing ? "暂停" : "播放", GUILayout.Width(55))) { playing = !playing; lastTick = EditorApplication.timeSinceStartup; }
                if (GUILayout.Button("上一步", GUILayout.Width(60))) { playing = false; tick = (tick + current.steps.Length - 1) % current.steps.Length; }
                if (GUILayout.Button("下一步", GUILayout.Width(60))) { playing = false; tick = (tick + 1) % current.steps.Length; }
                tick = EditorGUILayout.IntSlider(tick, 0, current.steps.Length - 1);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                mirrored = GUILayout.Toggle(mirrored, "镜像朝向"); showBoxes = GUILayout.Toggle(showBoxes, "攻防框"); showPath = GUILayout.Toggle(showPath, "位移轨迹");
                zoom = EditorGUILayout.Slider(zoom, .5f, 4); EditorGUILayout.EndHorizontal();
                if (definition.type == 0)
                {
                    string nextWeapon = EditorGUILayout.TextField("普通武器图层4", weapon);
                    string nextSpirit = EditorGUILayout.TextField("剑灵图层1", spirit);
                    if (nextWeapon != weapon || nextSpirit != spirit) { weapon = nextWeapon; spirit = nextSpirit; textures = null; }
                }
                var bounds = GUILayoutUtility.GetRect(200, 260, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(bounds, new Color(.12f, .13f, .15f));
                try { DrawPreview(bounds, data); }
                catch (Exception e) { GUI.Label(bounds, "草稿预览：" + e.Message); }
                if (!asset) return;
                serialized.Update(); scroll = EditorGUILayout.BeginScrollView(scroll);
                var draft = serialized.FindProperty("draft");
                var actions = draft.FindPropertyRelative("actions");
                var steps = actions.GetArrayElementAtIndex(action).FindPropertyRelative("steps");
                EditorGUILayout.PropertyField(steps.GetArrayElementAtIndex(tick), new GUIContent("当前步骤（80ms）"), true);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("复制当前步骤")) { steps.InsertArrayElementAtIndex(tick); tick++; }
                using (new EditorGUI.DisabledScope(steps.arraySize <= 1)) if (GUILayout.Button("删除当前步骤")) { steps.DeleteArrayElementAtIndex(tick); tick = Mathf.Max(0, tick - 1); }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("复制当前动作并追加")) { int index = actions.arraySize; actions.InsertArrayElementAtIndex(action); actions.MoveArrayElement(action, index); action = index; tick = 0; }
                // Resolve after applying step edits to avoid indexing an old frame value.
                if (serialized.ApplyModifiedProperties()) previewKey = null;
                serialized.Update(); draft = serialized.FindProperty("draft");
                int frameIndex = Data.actions[action].steps[Mathf.Min(tick, Data.actions[action].steps.Length - 1)].frame;
                var frames = draft.FindPropertyRelative("frames");
                if (frameIndex >= 0 && frameIndex < frames.arraySize)
                {
                    EditorGUILayout.PropertyField(frames.GetArrayElementAtIndex(frameIndex), new GUIContent("共享帧 " + frameIndex + "（帧片／攻防／边界）"), true);
                    if (GUILayout.Button("复制为独立帧并用于当前步骤"))
                    {
                        serialized.ApplyModifiedProperties();
                        AnimationAuthoring.DuplicateFrame(asset, action, tick); serialized.Update(); previewKey = null;
                    }
                }
                EditorGUILayout.PropertyField(draft.FindPropertyRelative("sheets"), new GUIContent("图集裁切（共享）"), true);
                EditorGUILayout.PropertyField(draft.FindPropertyRelative("footWidth"), new GUIContent("镜像轴宽度"));
                EditorGUILayout.PropertyField(draft.FindPropertyRelative("footHeight"), new GUIContent("脚底高度"));
                if (serialized.ApplyModifiedProperties()) previewKey = null;
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawPreview(Rect bounds, OriginalAnimation data)
        {
            var step = data.actions[action].steps[tick];
            string key = JsonUtility.ToJson(data.frames[step.frame]) + mirrored;
            if (key != previewKey)
            {
                // Use the same piece transform/mirror rules as OriginalActor, including solid pieces.
                preview = new OriginalAnimation { sheets = data.sheets, frames = new[] { JsonUtility.FromJson<OriginalFrame>(JsonUtility.ToJson(data.frames[step.frame])) } };
                if (mirrored)
                    foreach (var p in preview.frames[0].pieces)
                        if (p.image >= 0) { p.x = -p.x - data.sheets[p.image].rects[p.rect].w + data.footWidth; p.transform = OriginalActor.MirrorTransform(p.transform); }
                previewKey = key;
            }
            preview.sheets = data.sheets;
            if (textures == null)
            {
                var loaded = definition.images.Select(OriginalAssets.Texture).ToArray();
                if (definition.type == 0) { loaded[4] = OriginalAssets.Texture(weapon); loaded[1] = OriginalAssets.Texture(spirit); }
                textures = loaded;
            }
            Vector2 origin = new Vector2(bounds.width / (2 * zoom) - data.footWidth / 2f, bounds.height / (zoom * 1.4f));
            GUI.BeginClip(bounds); var matrix = GUI.matrix;
            try
            {
                GUI.matrix = matrix * Matrix4x4.Scale(new Vector3(zoom, zoom, 1));
                OriginalGui.Frame(preview, textures, 0, origin);
                Handles.BeginGUI();
                if (showPath)
                {
                    Vector2 point = origin;
                    foreach (var s in data.actions[action].steps)
                    {
                        var next = point + new Vector2(mirrored ? -s.vx : s.vx, s.vy);
                        Handles.color = Color.cyan; Handles.DrawLine(point, next); Handles.DrawSolidDisc(next, Vector3.forward, 1.2f); point = next;
                    }
                }
                if (showBoxes)
                {
                    var frame = data.frames[step.frame];
                    foreach (var r in frame.attackRects) DrawBox(r, Color.red, origin, data.footWidth);
                    foreach (var r in frame.bodyRects) DrawBox(r, Color.green, origin, data.footWidth);
                    DrawBox(frame.edgeRect, Color.yellow, origin, data.footWidth);
                }
                Handles.EndGUI();
            }
            finally { GUI.matrix = matrix; GUI.EndClip(); }
            GUI.Label(new Rect(bounds.x + 6, bounds.y + 5, bounds.width - 12, 22), "步骤 " + tick + "  帧 " + step.frame + "  攻击=" + step.attack + "  可受击=" + step.vulnerable + "  Δ(" + step.vx + "," + step.vy + ")");
        }
        private void DrawBox(OriginalRect r, Color color, Vector2 origin, int footWidth)
        {
            float x = origin.x + (mirrored ? -r.x - r.w + footWidth : r.x), y = origin.y + r.y;
            Handles.color = color;
            Handles.DrawPolyLine(new Vector3(x, y), new Vector3(x + r.w, y), new Vector3(x + r.w, y + r.h), new Vector3(x, y + r.h), new Vector3(x, y));
        }
    }
}
