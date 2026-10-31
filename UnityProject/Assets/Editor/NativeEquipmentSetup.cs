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
    public static class NativeEquipmentSetup
    {
        public const string PrefabPath = "Assets/UI/NativeEquipment.prefab";
        [MenuItem("GuJian/Native UI/Create and connect equipment")]
        public static void CreateAndConnect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play before saving UI assets");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/GuJian.unity") throw new InvalidOperationException("Open GuJian.unity first");
            var game = Object.FindObjectOfType<GuJianGame>(); if (!game) throw new InvalidOperationException("Game missing");
            Directory.CreateDirectory("Assets/UI"); if (!File.Exists(PrefabPath)) CreatePrefab();
            var views = Object.FindObjectsOfType<NativeEquipmentView>().Where(v => v.gameObject.scene == scene).ToArray();
            if (views.Length > 1) throw new InvalidOperationException("Multiple equipment views");
            var view = views.Length == 1 ? views[0] : ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<NativeEquipmentView>();
            view.Bind(game); PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            var data = new SerializedObject(game); data.FindProperty("equipmentView").objectReferenceValue = view; data.ApplyModifiedProperties();
            EditorUtility.SetDirty(view); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
        }
        private static Text Label(string name, Transform parent, float x, float y, float w, float h, bool right = false)
        {
            var text = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 11; text.color = right ? new Color(.12f, .07f, .025f) : new Color(.94f, .87f, .68f);
            text.alignment = right ? TextAnchor.MiddleRight : TextAnchor.UpperLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; return text;
        }
        private static Button Button(string name, Transform parent, float x, float y, float w, float h)
        {
            var rect = Rect(name, parent, x, y, w, h); var image = rect.gameObject.AddComponent<Image>(); image.color = Color.clear;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; var nav = button.navigation; nav.mode = Navigation.Mode.None; button.navigation = nav; return button;
        }
        private static void AssignArray(SerializedObject data, string name, Object[] values)
        { var p = data.FindProperty(name); p.arraySize = values.Length; for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; }
        private static void CreatePrefab()
        {
            var root = new GameObject("Native Equipment", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(NativeEquipmentView));
            try
            {
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 190;
                var viewport = Rect("Viewport 533x320", root.transform, 0, 0, GuJianGame.ViewWidth, GuJianGame.ViewHeight);
                viewport.anchorMin = viewport.anchorMax = viewport.pivot = new Vector2(.5f, .5f); viewport.anchoredPosition = Vector2.zero;
                var panel = Rect("Equipment content", viewport, 0, 0, GuJianGame.ViewWidth, GuJianGame.ViewHeight);
                var atlas = Rect("Atlas 320x240", panel, 0, 0, 320, 240); var frame = Rect("Original atlas frame", atlas, 0, 0, 320, 240);
                var preview = Rect("Character preview", atlas, 8, 48, 68, 145);
                var mask = Rect("Equipment viewport", atlas, 82, 39, 113, 124); mask.gameObject.AddComponent<RectMask2D>();
                var scroll = mask.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
                var content = Rect("Equipment", mask, 0, 0, 98, 124); scroll.content = content; scroll.viewport = mask;
                var template = Rect("Equipment template", content, 0, 0, 98, 30); var hit = template.gameObject.AddComponent<Image>(); hit.color = Color.clear;
                var row = template.gameObject.AddComponent<NativeEquipmentRow>();
                var focus = Rect("Focus", template, 0, 0, 98, 26).gameObject.AddComponent<Image>(); focus.color = new Color(.78f, .71f, .56f); focus.raycastTarget = false;
                var icon = Rect("Icon", template, 2, 2, 22, 22).gameObject.AddComponent<RawImage>(); icon.raycastTarget = false;
                Label("Label", template, 25, 0, 72, 29); template.gameObject.SetActive(false);
                var values = new Text[7]; for (int i = 0; i < 7; i++) values[i] = Label("Attribute " + i, atlas, 258, 38 + i * 24, 46, 20, true);
                var desc = Label("Description", atlas, 15, 193, 285, 17); var skill = Label("Skill", atlas, 92, 171, 105, 20);
                var money = Label("Money", atlas, 196, 220, 64, 18);
                var equip = Button("Equip", atlas, 5, 211, 51, 23); var back = Button("Back", atlas, 263, 211, 52, 23);
                var tabs = new Button[6]; for (int i = 0; i < 6; i++) tabs[i] = Button("Tab " + i, atlas, 54 + i * 26 + (i > 1 ? 50 : 0), 2, i == 1 ? 76 : 24, 24);
                var data = new SerializedObject(root.GetComponent<NativeEquipmentView>());
                data.FindProperty("viewport").objectReferenceValue = viewport; data.FindProperty("atlas").objectReferenceValue = atlas; data.FindProperty("content").objectReferenceValue = content;
                data.FindProperty("panel").objectReferenceValue = panel.gameObject; data.FindProperty("frame").objectReferenceValue = frame; data.FindProperty("preview").objectReferenceValue = preview;
                data.FindProperty("rowTemplate").objectReferenceValue = row; data.FindProperty("description").objectReferenceValue = desc; data.FindProperty("skill").objectReferenceValue = skill; data.FindProperty("money").objectReferenceValue = money;
                data.FindProperty("equipButton").objectReferenceValue = equip; data.FindProperty("backButton").objectReferenceValue = back;
                AssignArray(data, "attributes", values); AssignArray(data, "tabs", tabs); data.ApplyModifiedPropertiesWithoutUndo();
                panel.gameObject.SetActive(false); PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
