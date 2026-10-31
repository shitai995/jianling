using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GuJian.Editor
{
    public static class NativeTitleSetup
    {
        public const string PrefabPath = "Assets/UI/NativeTitle.prefab";
        public const string ActionsPath = "Assets/UI/TitleInput.inputactions";

        [MenuItem("古剑/原生界面/创建并接入标题（保留已有编辑）")]
        public static void CreateAndConnect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play before saving UI assets");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/GuJian.unity") throw new InvalidOperationException("Open GuJian.unity first");
            var game = Object.FindObjectOfType<GuJianGame>();
            if (!game) throw new InvalidOperationException("Game component missing");
            Directory.CreateDirectory("Assets/UI");
            AssetDatabase.Refresh();
            if (!File.Exists(ActionsPath))
            {
                var defaults = new DefaultInputActions();
                File.WriteAllText(ActionsPath, defaults.asset.ToJson());
                Object.DestroyImmediate(defaults.asset);
                AssetDatabase.ImportAsset(ActionsPath, ImportAssetOptions.ForceSynchronousImport);
            }
            if (!File.Exists(PrefabPath)) CreatePrefab();
            else
            {
                var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
                try
                {
                    if (ConfigureInput(contents.GetComponentInChildren<InputSystemUIInputModule>(true)))
                        PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }

            var views = Object.FindObjectsOfType<NativeTitleView>().Where(v => v.gameObject.scene == scene).ToArray();
            if (views.Length > 1) throw new InvalidOperationException("Multiple native title views in scene");
            var view = views.Length == 1 ? views[0] :
                ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<NativeTitleView>();
            view.Bind(game);
            var liveModule = view.GetComponentInChildren<InputSystemUIInputModule>(true);
            if (ConfigureInput(liveModule)) PrefabUtility.RecordPrefabInstancePropertyModifications(liveModule);
            PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            var data = new SerializedObject(game);
            data.FindProperty("titleView").objectReferenceValue = view;
            data.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(game);
            EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("GUJIAN_NATIVE_TITLE_READY " + PrefabPath);
        }

        private static void CreatePrefab()
        {
            var root = new GameObject("Native UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(NativeTitleView));
            try
            {
                root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                root.GetComponent<Canvas>().sortingOrder = 100;
                var panel = Rect("Title", root.transform, Vector2.zero, Vector2.zero);
                panel.anchorMin = Vector2.zero; panel.anchorMax = Vector2.one; panel.sizeDelta = Vector2.zero;
                var black = panel.gameObject.AddComponent<Image>(); black.color = Color.black; black.raycastTarget = false;
                var viewport = Rect("Viewport 533x320", panel, Vector2.zero, new Vector2(GuJianGame.ViewWidth, GuJianGame.ViewHeight));
                var art = Rect("Original title artwork", viewport, Vector2.zero, viewport.sizeDelta).gameObject.AddComponent<RawImage>();
                art.texture = OriginalAssets.Texture("pic/t_00"); art.raycastTarget = false;
                float sourceAspect = (float)art.texture.width / art.texture.height;
                float targetAspect = GuJianGame.ViewWidth / GuJianGame.ViewHeight;
                if (sourceAspect > targetAspect)
                { float width = targetAspect / sourceAspect; art.uvRect = new Rect((1 - width) / 2, 0, width, 1); }
                else
                { float height = sourceAspect / targetAspect; art.uvRect = new Rect(0, (1 - height) / 2, 1, height); }

                var start = Button("New Game", "开始游戏", viewport, new Vector2(0, -41));
                var resume = Button("Continue", "继续游戏", viewport, new Vector2(0, -84));
                var status = Label("Status", "", viewport, new Vector2(0, -130), new Vector2(420, 28));
                status.fontSize = 14; status.color = new Color(1, .85f, .55f);
                var eventObject = new GameObject("UI EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventObject.transform.SetParent(root.transform, false);
                var events = eventObject.GetComponent<EventSystem>(); events.firstSelectedGameObject = start.gameObject;
                ConfigureInput(eventObject.GetComponent<InputSystemUIInputModule>());

                var view = new SerializedObject(root.GetComponent<NativeTitleView>());
                view.FindProperty("viewport").objectReferenceValue = viewport;
                view.FindProperty("titlePanel").objectReferenceValue = panel.gameObject;
                view.FindProperty("newGameButton").objectReferenceValue = start;
                view.FindProperty("continueButton").objectReferenceValue = resume;
                view.FindProperty("status").objectReferenceValue = status;
                view.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static bool ConfigureInput(InputSystemUIInputModule module)
        {
            if (!module) throw new InvalidOperationException("Title UI input module missing");
            var actions = module.actionsAsset && EditorUtility.IsPersistent(module.actionsAsset) ? module.actionsAsset :
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            var refs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(actions)).OfType<InputActionReference>()
                .Where(r => !r.hideFlags.HasFlag(HideFlags.HideInHierarchy)).ToArray();
            var serialized = new SerializedObject(module);
            serialized.FindProperty("m_ActionsAsset").objectReferenceValue = actions;
            var fields = new[] { "m_PointAction", "m_LeftClickAction", "m_RightClickAction", "m_MiddleClickAction", "m_ScrollWheelAction",
                "m_MoveAction", "m_SubmitAction", "m_CancelAction", "m_TrackedDevicePositionAction", "m_TrackedDeviceOrientationAction" };
            var names = new[] { "Point", "Click", "RightClick", "MiddleClick", "ScrollWheel", "Navigate", "Submit", "Cancel",
                "TrackedDevicePosition", "TrackedDeviceOrientation" };
            for (int i = 0; i < fields.Length; i++)
            {
                var property = serialized.FindProperty(fields[i]);
                if (property.objectReferenceValue && EditorUtility.IsPersistent(property.objectReferenceValue)) continue;
                // Property setters skip replacing a transient wrapper around the same logical action.
                // Store the importer's persistent subasset directly so reopening cannot lose it.
                property.objectReferenceValue = refs.First(r => r.action != null && r.action.actionMap.name == "UI" && r.action.name == names[i]);
            }
            return serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }

        private static Text Label(string name, string text, Transform parent, Vector2 position, Vector2 size)
        {
            var label = Rect(name, parent, position, size).gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 18; label.text = text; label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white; label.raycastTarget = false;
            return label;
        }

        private static Button Button(string name, string text, Transform parent, Vector2 position)
        {
            var rect = Rect(name, parent, position, new Vector2(153, 32));
            var image = rect.gameObject.AddComponent<Image>(); image.color = Color.white;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = new Color(.06f, .17f, .22f, .85f);
            colors.highlightedColor = colors.selectedColor = new Color(.16f, .38f, .42f, .95f);
            colors.pressedColor = new Color(.03f, .1f, .14f, 1);
            colors.disabledColor = new Color(.12f, .17f, .19f, .5f);
            button.colors = colors;
            var navigation = button.navigation; navigation.mode = Navigation.Mode.Vertical; button.navigation = navigation;
            Label("Label", text, rect, Vector2.zero, rect.sizeDelta);
            return button;
        }
    }
}
