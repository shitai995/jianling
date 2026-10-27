using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GuJian.Editor
{
    public static class TilemapAuthoring
    {
        public const string Root = "Assets/Resources/Maps";
        public static GameObject Create(string id, int columns, int rows, int width, int height)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("退出播放模式后创建地图");
            if (string.IsNullOrEmpty(id) || !System.Text.RegularExpressions.Regex.IsMatch(id, "^[A-Za-z][A-Za-z0-9_]*$"))
                throw new ArgumentException("地图编号应以字母开头，只含字母、数字或下划线");
            if (columns < 1 || rows < 1 || columns > 256 || rows > 256 || width < 1 || height < 1) throw new ArgumentException("地图尺寸无效");
            string path = Root + "/" + id + ".prefab";
            var catalog = OriginalAssets.Catalog;
            if (!catalog || catalog.maps.Any(m => m.id == id) || File.Exists(path)) throw new InvalidOperationException("地图编号已存在或目录缺失：" + id);
            Directory.CreateDirectory(Root); AssetDatabase.Refresh();
            var root = new GameObject(id);
            try
            {
                var grid = root.AddComponent<Grid>(); grid.cellSize = new Vector3(width, height, 1);
                var map = root.AddComponent<EditableTilemap>(); map.mapId = id; map.columns = columns; map.rows = rows; map.cellWidth = width; map.cellHeight = height;
                map.background = Layer(root, "地面", -25000, true);
                map.decoration = Layer(root, "装饰", -5000, true);
                map.foreground = Layer(root, "前景遮挡", 28000, true);
                map.collision = Layer(root, "阻挡", 0, false);
                var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
                Publish(saved);
                return saved;
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        private static Tilemap Layer(GameObject root, string name, int order, bool visible)
        {
            var go = new GameObject(name); go.transform.SetParent(root.transform, false);
            var tiles = go.AddComponent<Tilemap>(); var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = order; renderer.enabled = visible;
            return tiles;
        }
        public static void Publish(GameObject prefab)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("退出播放模式后保存地图");
            var map = prefab ? prefab.GetComponent<EditableTilemap>() : null;
            if (!map || !map.collision || map.columns <= 0 || map.rows <= 0 || map.columns > 256 || map.rows > 256 || map.cellWidth <= 0 || map.cellHeight <= 0)
                throw new ArgumentException("地图需要有效尺寸与阻挡 Tilemap");
            var data = new OriginalMap { id = map.mapId, columns = map.columns, rows = map.rows, cellWidth = map.cellWidth, cellHeight = map.cellHeight };
            data.cells = new OriginalCell[data.columns * data.rows];
            for (int x = 0; x < data.columns; x++) for (int y = 0; y < data.rows; y++)
                data.cells[x * data.rows + y] = new OriginalCell { x = x, y = y, blocked = map.collision.HasTile(new Vector3Int(x, -y - 1, 0)) };
            var catalog = OriginalAssets.Catalog;
            var existing = catalog.maps.FirstOrDefault(m => m.id == map.mapId);
            if (existing != null && existing.prefab != prefab) throw new InvalidOperationException("地图编号被另一 Prefab 占用");
            Undo.RecordObject(catalog, "同步瓦片地图碰撞");
            if (existing == null) catalog.maps = catalog.maps.Concat(new[] { new OriginalAssetCatalog.MapEntry { id = map.mapId, data = data, prefab = prefab } }).ToArray();
            else { existing.data = data; existing.prefab = prefab; }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog);
        }
        public static void CreateStage(GameObject mapPrefab, int stageId, string title, int x, int y)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("退出播放模式后创建阶段");
            var map = mapPrefab ? mapPrefab.GetComponent<EditableTilemap>() : null;
            if (!map || stageId < 1 || x < 0 || y < 0 || x >= map.columns * map.cellWidth || y >= map.rows * map.cellHeight)
                throw new ArgumentException("阶段编号或入口像素坐标无效");
            var db = Resources.Load<OriginalDatabase>("OriginalDatabase"); string id = stageId.ToString();
            string storyPath = StoryAssetSetup.AssetPath(id), prefabPath = "Assets/Resources/Baked/Stages/" + id + ".prefab";
            if (db.Get<StageDefinition>().Any(s => s.script == id) || OriginalAssets.HasScript(id) || File.Exists(storyPath) || File.Exists(prefabPath))
                throw new InvalidOperationException("阶段编号已被使用：" + id);
            if (string.IsNullOrWhiteSpace(title) || title.Contains("\"") || title.Contains("\\") || title.Contains("\n")) throw new ArgumentException("阶段名称不能含引号、换行或反斜线");
            Publish(mapPrefab);
            var story = ScriptableObject.CreateInstance<StorySequence>();
            story.Import(id, title, new[] { "x21(\"" + title + "\")", "x23(\"/" + map.mapId + ".m\")",
                "x26(" + x + "," + y + ",\"r\")", "x09(\"stageID\"," + stageId + ")", "x22(0)" });
            AssetDatabase.CreateAsset(story, storyPath);
            var root = new GameObject(title);
            OriginalStageLayout saved;
            try
            {
                var layout = root.AddComponent<OriginalStageLayout>(); layout.script = id; layout.mapId = map.mapId; layout.fixedActors = new OriginalActor[0];
                PrefabUtility.InstantiatePrefab(mapPrefab, root.transform);
                saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath).GetComponent<OriginalStageLayout>();
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            Undo.RecordObject(db, "登记扩展阶段");
            db.addedStages = (db.addedStages ?? new StageDefinition[0]).Concat(new[] { new StageDefinition { script = id, map = map.mapId, name = title } }).ToArray();
            db.ClearTableCache(); EditorUtility.SetDirty(db); AssetDatabase.SaveAssetIfDirty(db);
            var catalog = OriginalAssets.Catalog; Undo.RecordObject(catalog, "登记扩展阶段布局");
            catalog.stages = catalog.stages.Concat(new[] { new OriginalAssetCatalog.StageEntry { script = id, prefab = saved } }).ToArray();
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog); OriginalAssets.ClearCache();
        }
        [MenuItem("古剑/内容/保存选中瓦片地图的碰撞")]
        public static void PublishSelected()
        {
            var prefab = Selection.activeGameObject;
            if (!prefab || !AssetDatabase.Contains(prefab)) throw new InvalidOperationException("请在 Project 中选择瓦片地图 Prefab");
            Publish(prefab);
        }
    }

    public sealed class TilemapAuthoringWindow : EditorWindow
    {
        private string id = "map_new", message;
        private int columns = 32, rows = 24, width = 32, height = 16, layer;
        private int stageId = 10000, entryX = 16, entryY = 8;
        private string stageName = "新区域";
        private GameObject saved, editing;
        private Sprite sprite; private Tile tile; private bool erase;
        private Vector2 scroll;
        [MenuItem("古剑/内容/瓦片地图编辑器")]
        public static void Open() => GetWindow<TilemapAuthoringWindow>("瓦片地图");
        public static void OpenMap(GameObject prefab) { var window = GetWindow<TilemapAuthoringWindow>("瓦片地图"); window.Load(prefab); }
        private void OnEnable() { saveChangesMessage = "瓦片地图尚未保存，是否保存地图与碰撞？"; }
        private void OnDisable() { if (editing) PrefabUtility.UnloadPrefabContents(editing); }
        private void SaveCurrent()
        {
            if (!editing || !saved) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("退出播放模式后保存地图");
            PrefabUtility.SaveAsPrefabAsset(editing, AssetDatabase.GetAssetPath(saved));
            TilemapAuthoring.Publish(saved); hasUnsavedChanges = false; message = "地图和碰撞已保存";
        }
        public override void SaveChanges() { SaveCurrent(); base.SaveChanges(); }
        private void Load(GameObject prefab)
        {
            if (prefab && (!AssetDatabase.Contains(prefab) || !prefab.GetComponent<EditableTilemap>()))
                throw new ArgumentException("请选择已保存的 EditableTilemap 地图 Prefab");
            if (hasUnsavedChanges)
            {
                int choice = EditorUtility.DisplayDialogComplex("切换瓦片地图", saveChangesMessage, "保存", "取消", "放弃修改");
                if (choice == 1) return;
                if (choice == 0) SaveCurrent();
            }
            if (editing) PrefabUtility.UnloadPrefabContents(editing);
            saved = prefab; editing = saved ? PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(saved)) : null;
            hasUnsavedChanges = false;
            var map = editing ? editing.GetComponent<EditableTilemap>() : null;
            if (map) { columns = map.columns; rows = map.rows; width = map.cellWidth; height = map.cellHeight; }
        }
        private void OnGUI()
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                EditorGUILayout.HelpBox("旧地图使用烘焙图层；新地图使用已保存的 Tilemap。画笔 Y 向下，与剧情像素坐标一致；阻挡层在保存时同步现有游戏碰撞。", MessageType.Info);
                id = EditorGUILayout.TextField("新地图编号", id);
                columns = EditorGUILayout.IntField("列", columns); rows = EditorGUILayout.IntField("行", rows);
                width = EditorGUILayout.IntField("格宽（像素）", width); height = EditorGUILayout.IntField("格高（像素）", height);
                if (GUILayout.Button("创建并登记新地图")) Run(() => Load(TilemapAuthoring.Create(id, columns, rows, width, height)));
                var selected = (GameObject)EditorGUILayout.ObjectField("地图 Prefab", saved, typeof(GameObject), false);
                if (selected != saved) Run(() => Load(selected));
                if (!editing) return;
                var map = editing.GetComponent<EditableTilemap>();
                if (!map) { EditorGUILayout.HelpBox("请选择 EditableTilemap 地图 Prefab", MessageType.Warning); return; }
                if (GUILayout.Button("按上方尺寸扩建当前地图（保存后生效）")) Run(() =>
                {
                    if (columns < map.columns || rows < map.rows || columns > 256 || rows > 256 || width != map.cellWidth || height != map.cellHeight)
                        throw new ArgumentException("扩建只能增加行列数，不能缩小或改变已有格子的尺寸");
                    Undo.RecordObject(map, "扩建瓦片地图"); map.columns = columns; map.rows = rows; hasUnsavedChanges = true;
                    message = "尺寸已扩展，请绘制后保存地图与碰撞";
                });
                layer = GUILayout.Toolbar(layer, new[] { "地面", "装饰", "前景", "阻挡" });
                tile = (Tile)EditorGUILayout.ObjectField("瓦片画笔", tile, typeof(Tile), false);
                sprite = (Sprite)EditorGUILayout.ObjectField("创建瓦片用 Sprite", sprite, typeof(Sprite), false);
                if (GUILayout.Button("从 Sprite 创建持久瓦片") && sprite) Run(() =>
                {
                    string path = EditorUtility.SaveFilePanelInProject("保存瓦片", sprite.name + ".asset", "asset", "选择瓦片保存位置");
                    if (string.IsNullOrEmpty(path)) return;
                    var created = ScriptableObject.CreateInstance<Tile>(); created.sprite = sprite;
                    created.colliderType = Tile.ColliderType.None; AssetDatabase.CreateAsset(created, path); tile = created;
                });
                erase = EditorGUILayout.Toggle("擦除", erase);
                if (GUILayout.Button("保存地图与碰撞")) Run(SaveCurrent);
                stageId = EditorGUILayout.IntField("新阶段编号", stageId);
                stageName = EditorGUILayout.TextField("阶段名称", stageName);
                entryX = EditorGUILayout.IntField("入口 X（像素）", entryX); entryY = EditorGUILayout.IntField("入口 Y（像素）", entryY);
                if (GUILayout.Button("为地图创建剧情入口与阶段布局")) Run(() =>
                {
                    SaveCurrent();
                    TilemapAuthoring.CreateStage(saved, stageId, stageName, entryX, entryY);
                    message = "入口与阶段布局已创建；可在剧情调试器载入 " + stageId;
                });
                if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
                scroll = EditorGUILayout.BeginScrollView(scroll);
                const float cell = 20;
                Rect bounds = GUILayoutUtility.GetRect(map.columns * cell, map.rows * cell, GUILayout.ExpandWidth(false));
                EditorGUI.DrawRect(bounds, new Color(.16f,.16f,.16f));
                Tilemap[] layers = { map.background, map.decoration, map.foreground, map.collision };
                for (int x = 0; x < map.columns; x++) for (int y = 0; y < map.rows; y++)
                {
                    var p = new Vector3Int(x, -y - 1, 0); Rect r = new Rect(bounds.x + x * cell, bounds.y + y * cell, cell, cell);
                    for (int z = 0; z < 3; z++)
                    {
                        var image = layers[z].GetSprite(p); if (!image) continue;
                        Rect uv = image.textureRect;
                        GUI.DrawTextureWithTexCoords(r, image.texture, new Rect(uv.x / image.texture.width, uv.y / image.texture.height, uv.width / image.texture.width, uv.height / image.texture.height));
                    }
                    if (map.collision.HasTile(p)) EditorGUI.DrawRect(r, new Color(1,0,0,.4f));
                    EditorGUI.DrawRect(new Rect(r.x,r.y,r.width,1), new Color(1,1,1,.15f));
                    EditorGUI.DrawRect(new Rect(r.x,r.y,1,r.height), new Color(1,1,1,.15f));
                }
                Event e = Event.current;
                if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && bounds.Contains(e.mousePosition) && (erase || tile))
                {
                    int x = (int)((e.mousePosition.x - bounds.x) / cell), y = (int)((e.mousePosition.y - bounds.y) / cell);
                    Undo.RecordObject(layers[layer], "绘制瓦片"); layers[layer].SetTile(new Vector3Int(x,-y-1,0), erase ? null : tile); hasUnsavedChanges = true; e.Use(); Repaint();
                }
                EditorGUILayout.EndScrollView();
            }
        }
        private void Run(Action action) { try { action(); } catch (Exception ex) { message = ex.Message; } }
    }
}
