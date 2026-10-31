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
    public static class NativeInventorySetup
    {
        public const string PrefabPath = "Assets/UI/NativeInventory.prefab";
        [MenuItem("GuJian/Native UI/Create and connect inventory")]
        public static void CreateAndConnect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play before saving UI assets");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/GuJian.unity") throw new InvalidOperationException("Open GuJian.unity first");
            var game = Object.FindObjectOfType<GuJianGame>(); if (!game) throw new InvalidOperationException("Game missing");
            Directory.CreateDirectory("Assets/UI"); if (!File.Exists(PrefabPath)) CreatePrefab(); else RepairShell();
            var views = Object.FindObjectsOfType<NativeInventoryView>().Where(v => v.gameObject.scene == scene).ToArray();
            if (views.Length > 1) throw new InvalidOperationException("Multiple inventory views");
            var view = views.Length == 1 ? views[0] : ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<NativeInventoryView>();
            view.Bind(game); PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            var data = new SerializedObject(game); data.FindProperty("inventoryView").objectReferenceValue = view; data.ApplyModifiedProperties();
            EditorUtility.SetDirty(view); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
        private static void RepairShell()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var data = new SerializedObject(root.GetComponent<NativeInventoryView>());
                if (!data.FindProperty("frame").objectReferenceValue)
                {
                    BuildShell(root.transform.Find("Viewport 533x320/Inventory content/Atlas 320x240"), data);
                    data.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void BuildShell(Transform atlas, SerializedObject data)
        {
            var frame = Rect("Original atlas frame", atlas, 0, 0, 320, 240); frame.SetAsFirstSibling();
            var tabs = new Button[6];
            for (int i = 0; i < tabs.Length; i++) tabs[i] = Button("Tab " + i, atlas, 54 + i * 26 + (i > 2 ? 50 : 0), 2, i == 2 ? 76 : 24, 24);
            var back = Button("Back", atlas, 263, 211, 52, 23);
            var money = Label("Money", atlas, 196, 220, 64, 18);
            data.FindProperty("frame").objectReferenceValue = frame; data.FindProperty("backButton").objectReferenceValue = back; data.FindProperty("moneyLabel").objectReferenceValue = money;
            var a = data.FindProperty("tabButtons"); a.arraySize = tabs.Length;
            for (int i = 0; i < tabs.Length; i++) a.GetArrayElementAtIndex(i).objectReferenceValue = tabs[i];
        }
        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
        }
        private static Text Label(string name, Transform parent, float x, float y, float w, float h)
        {
            var label = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 11;
            label.color = new Color(.94f, .87f, .68f); label.alignment = TextAnchor.UpperLeft; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate; return label;
        }
        private static Button Button(string name, Transform parent, float x, float y, float w, float h)
        {
            var r = Rect(name, parent, x, y, w, h); var image = r.gameObject.AddComponent<Image>(); image.color = Color.clear;
            var button = r.gameObject.AddComponent<Button>(); button.targetGraphic = image; var nav = button.navigation; nav.mode = Navigation.Mode.None; button.navigation = nav;
            return button;
        }
        private static void CreatePrefab()
        {
            var root = new GameObject("Native Inventory", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(NativeInventoryView));
            try
            {
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 190;
                var viewport = Rect("Viewport 533x320", root.transform, 0, 0, GuJianGame.ViewWidth, GuJianGame.ViewHeight);
                viewport.anchorMin = viewport.anchorMax = viewport.pivot = new Vector2(.5f, .5f); viewport.anchoredPosition = Vector2.zero;
                var panel = Rect("Inventory content", viewport, 0, 0, GuJianGame.ViewWidth, GuJianGame.ViewHeight);
                var atlas = Rect("Atlas 320x240", panel, 0, 0, 320, 240);
                var mask = Rect("Items viewport", atlas, 16, 50, 205, 145); mask.gameObject.AddComponent<RectMask2D>();
                var scroll = mask.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
                var content = Rect("Items", mask, 0, 0, 187, 145); scroll.content = content; scroll.viewport = mask;
                var template = Rect("Item template", content, 0, 0, 187, 30); var hit = template.gameObject.AddComponent<Image>(); hit.color = Color.clear;
                var row = template.gameObject.AddComponent<NativeInventoryRow>();
                var icon = Rect("Icon", template, 2, 2, 22, 22).gameObject.AddComponent<RawImage>(); icon.raycastTarget = false;
                Label("Label", template, 30, 4, 155, 24); template.gameObject.SetActive(false);
                var description = Label("Description", atlas, 232, 47, 70, 147);
                var activate = Button("Activate", atlas, 5, 211, 52, 23);
                var auto = Button("Auto medicine", atlas, 229, 194, 74, 16); var autoLabel = Label("Label", auto.transform, 0, 0, 74, 16); autoLabel.alignment = TextAnchor.MiddleCenter;
                var data = new SerializedObject(root.GetComponent<NativeInventoryView>());
                data.FindProperty("viewport").objectReferenceValue = viewport; data.FindProperty("atlas").objectReferenceValue = atlas; data.FindProperty("content").objectReferenceValue = content;
                data.FindProperty("panel").objectReferenceValue = panel.gameObject; data.FindProperty("rowTemplate").objectReferenceValue = row;
                data.FindProperty("description").objectReferenceValue = description; data.FindProperty("autoLabel").objectReferenceValue = autoLabel;
                data.FindProperty("activateButton").objectReferenceValue = activate; data.FindProperty("autoButton").objectReferenceValue = auto;
                BuildShell(atlas, data); data.ApplyModifiedPropertiesWithoutUndo();
                panel.gameObject.SetActive(false); PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
