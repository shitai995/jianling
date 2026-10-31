using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GuJian.Editor
{
    public static class NativeChoiceSetup
    {
        public const string PrefabPath = "Assets/UI/NativeChoice.prefab";
        [MenuItem("GuJian/Native UI/Create and connect choice")]
        public static void CreateAndConnect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play before saving UI assets");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/GuJian.unity") throw new InvalidOperationException("Open GuJian.unity first");
            var game = Object.FindObjectOfType<GuJianGame>(); if (!game) throw new InvalidOperationException("Game missing");
            Directory.CreateDirectory("Assets/UI"); if (!File.Exists(PrefabPath)) CreatePrefab(); else RepairRows();
            var views = Object.FindObjectsOfType<NativeChoiceView>().Where(v => v.gameObject.scene == scene).ToArray();
            if (views.Length > 1) throw new InvalidOperationException("Multiple native choice views");
            var view = views.Length == 1 ? views[0] : ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<NativeChoiceView>();
            view.Bind(game); PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            var data = new SerializedObject(game); data.FindProperty("choiceView").objectReferenceValue = view; data.ApplyModifiedProperties();
            EditorUtility.SetDirty(view); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
        private static void RepairRows()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var panel = root.transform.Find("Viewport 533x320/Choice");
                var rows = new NativeChoiceRow[3]; bool changed = false;
                for (int i = 0; i < rows.Length; i++)
                {
                    var row = panel.Find("Option " + (i + 1)).gameObject;
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(row) > 0)
                    { GameObjectUtility.RemoveMonoBehavioursWithMissingScript(row); changed = true; }
                    rows[i] = row.GetComponent<NativeChoiceRow>();
                    if (!rows[i]) { rows[i] = row.AddComponent<NativeChoiceRow>(); changed = true; }
                }
                foreach (string name in new[] { "List panel", "Information panel" })
                {
                    var frame = panel.Find(name + "/Source atlas border").gameObject;
                    if (!frame.GetComponent<CanvasRenderer>()) { frame.AddComponent<CanvasRenderer>(); changed = true; }
                }
                var data = new SerializedObject(root.GetComponent<NativeChoiceView>());
                var property = data.FindProperty("rows");
                for (int i = 0; i < rows.Length; i++)
                    if (property.GetArrayElementAtIndex(i).objectReferenceValue != rows[i])
                    { property.GetArrayElementAtIndex(i).objectReferenceValue = rows[i]; changed = true; }
                data.ApplyModifiedPropertiesWithoutUndo();
                if (changed) PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static RectTransform Rect(string name, Transform parent)
        { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false); return r; }
        private static NativeChoicePanel Frame(string name, Transform parent, out RectTransform root)
        {
            root = Rect(name, parent); var fill = root.gameObject.AddComponent<Image>(); fill.color = new Color32(214, 140, 90, 255); fill.raycastTarget = false;
            var frame = Rect("Source atlas border", root); frame.anchorMin = Vector2.zero; frame.anchorMax = Vector2.one; frame.offsetMin = frame.offsetMax = Vector2.zero;
            frame.gameObject.AddComponent<CanvasRenderer>(); var graphic = frame.gameObject.AddComponent<NativeChoicePanel>(); graphic.raycastTarget = false; return graphic;
        }
        private static Text TextObject(string name, Transform parent)
        {
            var text = Rect(name, parent).gameObject.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18; text.alignment = TextAnchor.UpperLeft; text.raycastTarget = false; text.verticalOverflow = VerticalWrapMode.Overflow; return text;
        }
        private static void AssignArray(SerializedObject data, string name, Object[] values)
        { var p = data.FindProperty(name); p.arraySize = values.Length; for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; }
        private static void CreatePrefab()
        {
            var root = new GameObject("Native Choice", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(NativeChoiceView));
            try
            {
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 210;
                var viewport = Rect("Viewport 533x320", root.transform); viewport.anchorMin = viewport.anchorMax = viewport.pivot = new Vector2(.5f, .5f); viewport.sizeDelta = new Vector2(GuJianGame.ViewWidth, GuJianGame.ViewHeight);
                var panel = Rect("Choice", viewport); panel.anchorMin = Vector2.zero; panel.anchorMax = Vector2.one; panel.offsetMin = panel.offsetMax = Vector2.zero;
                var listFrame = Frame("List panel", panel, out var list); var infoFrame = Frame("Information panel", panel, out var info);
                var clip = Rect("Information clip", panel); clip.gameObject.AddComponent<RectMask2D>(); var infoText = TextObject("Information", clip); infoText.supportRichText = true; infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
                var rows = new NativeChoiceRow[3]; var highlights = new Image[3]; var labels = new Text[3];
                for (int i = 0; i < rows.Length; i++)
                {
                    var row = Rect("Option " + (i + 1), panel); rows[i] = row.gameObject.AddComponent<NativeChoiceRow>();
                    var raycast = row.gameObject.AddComponent<Image>(); raycast.color = new Color(0, 0, 0, 0); raycast.raycastTarget = true; row.gameObject.AddComponent<RectMask2D>();
                    var highlight = Rect("Focus", row); highlight.anchorMin = Vector2.zero; highlight.anchorMax = Vector2.one; highlight.offsetMin = new Vector2(2, 2); highlight.offsetMax = new Vector2(-2, -2);
                    highlights[i] = highlight.gameObject.AddComponent<Image>(); highlights[i].color = new Color32(131, 104, 159, 255); highlights[i].raycastTarget = false;
                    labels[i] = TextObject("Label", row); labels[i].horizontalOverflow = HorizontalWrapMode.Overflow; labels[i].supportRichText = false;
                    var shadow = labels[i].gameObject.AddComponent<Shadow>(); shadow.effectColor = Color.black; shadow.effectDistance = new Vector2(-1, 1);
                }
                var bar = Rect("Scrollbar", panel); var barImage = bar.gameObject.AddComponent<Image>(); barImage.color = new Color32(210, 214, 205, 255); barImage.raycastTarget = false;
                var thumb = Rect("Thumb", bar); var thumbImage = thumb.gameObject.AddComponent<Image>(); thumbImage.color = new Color32(87, 91, 101, 255); thumbImage.raycastTarget = false;
                var data = new SerializedObject(root.GetComponent<NativeChoiceView>());
                data.FindProperty("viewport").objectReferenceValue = viewport; data.FindProperty("panel").objectReferenceValue = panel.gameObject;
                data.FindProperty("listPanel").objectReferenceValue = list; data.FindProperty("infoPanel").objectReferenceValue = info; data.FindProperty("infoClip").objectReferenceValue = clip;
                data.FindProperty("listFrame").objectReferenceValue = listFrame; data.FindProperty("infoFrame").objectReferenceValue = infoFrame; data.FindProperty("infoText").objectReferenceValue = infoText;
                data.FindProperty("scrollbar").objectReferenceValue = bar; data.FindProperty("thumb").objectReferenceValue = thumb;
                AssignArray(data, "rows", rows); AssignArray(data, "highlights", highlights); AssignArray(data, "labels", labels); data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
