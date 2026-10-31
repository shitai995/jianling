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
    public static class NativeDialogueSetup
    {
        public const string PrefabPath = "Assets/UI/NativeDialogue.prefab";
        [MenuItem("古剑/原生界面/创建并接入人物对话（保留已有编辑）")]
        public static void CreateAndConnect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play before saving UI assets");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/GuJian.unity") throw new InvalidOperationException("Open GuJian.unity first");
            var game = Object.FindObjectOfType<GuJianGame>();
            if (!game) throw new InvalidOperationException("Game component missing");
            Directory.CreateDirectory("Assets/UI");
            if (!File.Exists(PrefabPath)) CreatePrefab();
            var views = Object.FindObjectsOfType<NativeDialogueView>().Where(v => v.gameObject.scene == scene).ToArray();
            if (views.Length > 1) throw new InvalidOperationException("Multiple native dialogue views in scene");
            var view = views.Length == 1 ? views[0] :
                ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<NativeDialogueView>();
            view.Bind(game); PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            var data = new SerializedObject(game); data.FindProperty("characterDialogueView").objectReferenceValue = view;
            data.ApplyModifiedProperties(); PrefabUtility.RecordPrefabInstancePropertyModifications(game);
            EditorUtility.SetDirty(view); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets(); Debug.Log("GUJIAN_NATIVE_DIALOGUE_READY " + PrefabPath);
        }
        private static RectTransform Rect(string name, Transform parent)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false); return r;
        }
        private static void CreatePrefab()
        {
            var root = new GameObject("Native Character Dialogue", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(NativeDialogueView));
            try
            {
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 200;
                var viewport = Rect("Viewport 533x320", root.transform); viewport.anchorMin = viewport.anchorMax = viewport.pivot = new Vector2(.5f, .5f); viewport.sizeDelta = new Vector2(GuJianGame.ViewWidth, GuJianGame.ViewHeight);
                var panel = Rect("Character Dialogue", viewport); panel.anchorMin = Vector2.zero; panel.anchorMax = Vector2.one; panel.offsetMin = panel.offsetMax = Vector2.zero;
                var background = Rect("Original dialogue frame", panel).gameObject.AddComponent<RawImage>(); background.raycastTarget = true;
                var portrait = Rect("Portrait", panel).gameObject.AddComponent<RawImage>(); portrait.raycastTarget = false;
                var name = Rect("Speaker", panel).gameObject.AddComponent<Text>(); name.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); name.alignment = TextAnchor.UpperCenter; name.raycastTarget = false;
                var content = Rect("Clipped text", panel); content.gameObject.AddComponent<RectMask2D>();
                var template = Rect("Span template", content).gameObject.AddComponent<Text>(); template.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); template.alignment = TextAnchor.UpperLeft; template.supportRichText = false; template.horizontalOverflow = HorizontalWrapMode.Overflow; template.verticalOverflow = VerticalWrapMode.Overflow; template.raycastTarget = false;
                template.gameObject.AddComponent<Outline>().effectDistance = new Vector2(1, -1);
                template.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(1, -1);
                template.gameObject.SetActive(false);
                var view = new SerializedObject(root.GetComponent<NativeDialogueView>());
                view.FindProperty("viewport").objectReferenceValue = viewport; view.FindProperty("panel").objectReferenceValue = panel.gameObject;
                view.FindProperty("background").objectReferenceValue = background; view.FindProperty("portrait").objectReferenceValue = portrait;
                view.FindProperty("speaker").objectReferenceValue = name; view.FindProperty("content").objectReferenceValue = content;
                view.FindProperty("spanTemplate").objectReferenceValue = template; view.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
