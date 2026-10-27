using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ReconstructionTools
    {
        [MenuItem("古剑/创建启动场景")]
        public static void CreateStartupScene()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.orthographic = true;
            camera.orthographicSize = GuJianGame.ViewHeight / 2;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            cameraObject.transform.position = new Vector3(GuJianGame.ViewWidth / 2, -GuJianGame.ViewHeight / 2, -10);
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            var host = new GameObject("古剑魂梦");
            var game = host.AddComponent<GuJianGame>();
            var actors = new GameObject("Actors"); actors.transform.SetParent(host.transform, false);
            var maps = new GameObject("Map"); maps.transform.SetParent(host.transform, false);
            var serialized = new SerializedObject(game);
            serialized.FindProperty("actorRoot").objectReferenceValue = actors;
            serialized.FindProperty("mapRoot").objectReferenceValue = maps;
            var music = host.AddComponent<AudioSource>(); music.loop = true; music.volume = .45f; music.playOnAwake = false;
            serialized.FindProperty("music").objectReferenceValue = music;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(host, "Assets/Prefabs/GuJianGame.prefab");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GuJian.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/GuJian.unity", true) };
            PlayerSettings.productName = "古剑魂梦";
            PlayerSettings.defaultScreenWidth = 1066;
            PlayerSettings.defaultScreenHeight = 640;
            PlayerSettings.runInBackground = true;
            Debug.Log("GUJIAN_STARTUP_SCENE_READY");
        }

        private static T[] Read<T>(string name)
        {
            var text = File.ReadAllText("Assets/Resources/Original/Data/" + name + ".json");
            return JsonUtility.FromJson<Rows<T>>(text).rows;
        }

        [MenuItem("古剑/从原版 JSON 导入数据库")]
        public static void ImportDatabase()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new System.InvalidOperationException("退出播放模式后导入数据库");
            const string path = "Assets/Resources/OriginalDatabase.asset";
            var database = AssetDatabase.LoadAssetAtPath<OriginalDatabase>(path);
            if (!database) { database = ScriptableObject.CreateInstance<OriginalDatabase>(); AssetDatabase.CreateAsset(database, path); }
            Undo.RecordObject(database, "Import original database");
            database.sprites = AddMissing(database.sprites, Read<SpriteDefinition>("sprite"), v => v.id);
            database.stages = AddMissing(database.stages, Read<StageDefinition>("stages"), v => v.script);
            database.equipment = AddMissing(database.equipment, Read<EquipmentDefinition>("equip"), v => v.id);
            database.items = AddMissing(database.items, Read<ItemDefinition>("item"), v => v.id);
            database.skills = AddMissing(database.skills, Read<SkillDefinition>("skill"), v => v.id);
            database.enemies = AddMissing(database.enemies, Read<EnemyDefinition>("enemy"), v => v.id);
            if (database.roleAttributes == null || database.roleAttributes.Length == 0) database.roleAttributes = Read<AttributeDefinition>("role");
            if (database.swordSpiritAttributes == null || database.swordSpiritAttributes.Length == 0) database.swordSpiritAttributes = Read<AttributeDefinition>("ego");
            if (database.enemyAttributes == null || database.enemyAttributes.Length == 0) database.enemyAttributes = Read<AttributeDefinition>("enemybase");
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Selection.activeObject = database;
            Debug.Log("GUJIAN_DATABASE_IMPORTED");
        }
        private static T[] AddMissing<T, TKey>(T[] existing, T[] imported, System.Func<T, TKey> key)
        {
            var kept = existing ?? new T[0];
            var ids = new System.Collections.Generic.HashSet<TKey>(kept.Select(key));
            return kept.Concat(imported.Where(v => ids.Add(key(v)))).ToArray();
        }

        [MenuItem("古剑/设置原版像素纹理")]
        public static void ConfigureTextures()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Original" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                if (importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.npotScale == TextureImporterNPOTScale.None) continue;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 4096;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            Debug.Log("GUJIAN_PIXEL_TEXTURES_CONFIGURED");
        }
    }

    public sealed class OriginalAssetBrowser : EditorWindow
    {
        private int mapIndex, actorIndex;
        private int tab;
        private Vector2 scroll;

        [MenuItem("古剑/原版资源浏览器")]
        public static void Open() => GetWindow<OriginalAssetBrowser>("古剑资源");

        private void OnGUI()
        {
            tab = GUILayout.Toolbar(tab, new[] { "地图", "角色", "数据库" });
            var stages = OriginalAssets.Table<StageDefinition>("stages");
            var actors = OriginalAssets.Table<SpriteDefinition>("sprite");
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (tab == 0 && stages.Length > 0)
            {
                mapIndex = EditorGUILayout.Popup(Mathf.Clamp(mapIndex, 0, stages.Length - 1), stages.Select(s => s.script + " " + s.name).ToArray());
                var stage = stages[mapIndex];
                var entry = OriginalAssets.Catalog.maps.FirstOrDefault(m => m.id == stage.map);
                var editable = entry != null && entry.prefab ? entry.prefab.GetComponent<EditableTilemap>() : null;
                var map = entry != null ? entry.data : OriginalAssets.Json<OriginalMap>("Maps/" + stage.map);
                if (map != null) EditorGUILayout.LabelField(map.Width + " × " + map.Height + "    " + map.cellWidth + " × " + map.cellHeight + " 像素图块");
                if (editable)
                {
                    EditorGUILayout.ObjectField("地图 Prefab", entry.prefab, typeof(GameObject), false);
                    if (GUILayout.Button("编辑瓦片与碰撞")) TilemapAuthoringWindow.OpenMap(entry.prefab);
                }
                else if (map != null) for (int i = 0; i < 3; i++)
                {
                    var texture = OriginalAssets.Texture("Maps/" + stage.map + "_" + i);
                    if (!texture) continue;
                    GUILayout.Label("图层 " + i);
                    var rect = GUILayoutUtility.GetRect(position.width - 24, (position.width - 24) * map.Height / map.Width);
                    GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
                }
                if (GUILayout.Button("编辑场景剧情")) StoryAssetSetup.OpenStory(stage.script);
            }
            else if (tab == 1 && actors.Length > 0)
            {
                actorIndex = EditorGUILayout.Popup(Mathf.Clamp(actorIndex, 0, actors.Length - 1), actors.Select(a => a.id + " " + a.name).ToArray());
                foreach (string image in actors[actorIndex].images)
                {
                    var texture = OriginalAssets.Texture(image);
                    GUILayout.Label(image + "  " + texture.width + " × " + texture.height);
                    var rect = GUILayoutUtility.GetRect(Mathf.Min(position.width - 24, texture.width * 2), Mathf.Min(800, texture.height * 2));
                    GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
                }
            }
            else if (tab == 2)
            {
                if (GUILayout.Button("在 Inspector 中编辑数据库")) Selection.activeObject = Resources.Load<OriginalDatabase>("OriginalDatabase");
                EditorGUILayout.HelpBox("运行时优先读取 OriginalDatabase.asset。Scripts 和 Maps 中的 JSON 保留原版脚本指令与地图碰撞数据。", MessageType.Info);
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
