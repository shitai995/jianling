using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GuJian.Editor
{
    public static class OriginalAssetBaker
    {
        private const string Root = "Assets/Resources/Baked/";
        private static readonly Queue<Action> jobs = new Queue<Action>();
        private static OriginalAssetCatalog catalog;
        private static readonly Dictionary<string, OriginalAssetCatalog.SpriteEntry> sprites = new Dictionary<string, OriginalAssetCatalog.SpriteEntry>();
        private static readonly List<OriginalAssetCatalog.MapEntry> maps = new List<OriginalAssetCatalog.MapEntry>();
        private static readonly List<OriginalAssetCatalog.ActorEntry> actors = new List<OriginalAssetCatalog.ActorEntry>();
        private static readonly List<OriginalAssetCatalog.EffectEntry> effects = new List<OriginalAssetCatalog.EffectEntry>();
        private static readonly List<OriginalAssetCatalog.StageEntry> stages = new List<OriginalAssetCatalog.StageEntry>();
        public static bool Running => jobs.Count > 0;
        public static string LastError { get; private set; }
        public static int Remaining => jobs.Count;
        private static OriginalSpriteLibrary library;

        [MenuItem("古剑/生成缺少的预制资源（保留已有编辑）")]
        public static void Run()
        {
            if (EditorApplication.isPlaying || Running) throw new InvalidOperationException("Exit Play mode and wait for baking to finish");
            LastError = null; OriginalAssets.ClearCache();
            foreach (string dir in new[] { "Actors", "Effects", "Maps", "Stages", "Images/pic" }) Directory.CreateDirectory(Root + dir);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            catalog = AssetDatabase.LoadAssetAtPath<OriginalAssetCatalog>("Assets/Resources/OriginalAssetCatalog.asset");
            if (!catalog) { catalog = ScriptableObject.CreateInstance<OriginalAssetCatalog>(); AssetDatabase.CreateAsset(catalog, "Assets/Resources/OriginalAssetCatalog.asset"); }
            library = AssetDatabase.LoadAssetAtPath<OriginalSpriteLibrary>(Root + "SpriteLibrary.asset");
            if (!library) { library = ScriptableObject.CreateInstance<OriginalSpriteLibrary>(); AssetDatabase.CreateAsset(library, Root + "SpriteLibrary.asset"); }
            sprites.Clear(); maps.Clear(); actors.Clear(); effects.Clear(); stages.Clear();
            if (library.sprites != null) foreach (var s in library.sprites) sprites[s.key] = s;
            if (!library.solid)
            {
                library.solid = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 2, 2), new Vector2(.5f,.5f), 2);
                library.solid.name = "Solid"; AssetDatabase.AddObjectToAsset(library.solid, library);
            }
            catalog.solid = library.solid;
            var definitions = OriginalAssets.Table<SpriteDefinition>("sprite");
            foreach (var d in definitions)
            {
                var copy = d;
                jobs.Enqueue(() => { BakeAnimationSprites(copy.animation, copy.images); });
            }
            foreach (var path in Directory.GetFiles("Assets/Resources/Original/action", "w_*.png"))
            {
                string image = "action/" + Path.GetFileNameWithoutExtension(path);
                jobs.Enqueue(() => { foreach (var r in OriginalAssets.Json<OriginalAnimation>("Animations/action/h_00").sheets[4].rects) BakeSprite(image, r); });
            }
            var extras = new Dictionary<string, string[]>
            {
                {"z_finalslash", new[]{"s_17","s_18","s_19","s_20","s_23","s_24","s_26","o_00","s_25","s_27","s_28","s_242"}},
                {"e_99",new[]{"s_99"}}, {"z_arrow",new[]{"s_35"}},
                {"s_41",new[]{"s_41","h_04"}}, {"s_36",new[]{"s_36"}},
                {"skill_05",new[]{"s_20"}},{"skill_06",new[]{"s_19"}},{"skill_07",new[]{"s_21"}},
                {"skill_08",new[]{"s_22"}},{"skill_09",new[]{"s_20"}},{"skill_10",new[]{"s_11","s_19","s_08"}},
                {"skill_11",new[]{"s_21"}},{"skill_12",new[]{"s_22"}},{"skill_14",new[]{"s_24"}}
            };
            foreach (var e in extras) { var copy = e; jobs.Enqueue(() => BakeAnimationSprites(copy.Key, copy.Value.Select(s => "action/" + s).ToArray())); }
            foreach (var path in Directory.GetFiles("Assets/Resources/Original/pic", "*.png"))
            {
                string image = "pic/" + Path.GetFileNameWithoutExtension(path);
                jobs.Enqueue(() => BakeImage(image));
            }
            foreach (string id in Directory.GetFiles("Assets/Resources/Original/Maps", "*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(s => s))
            {
                string copy = id; jobs.Enqueue(() => BakeMap(copy));
            }
            jobs.Enqueue(() => { catalog.sprites = sprites.Values.ToArray(); });
            foreach (var d in definitions)
            {
                var copy = d;
                jobs.Enqueue(() => actors.Add(new OriginalAssetCatalog.ActorEntry { id = copy.id, prefab = BakeActor(Root + "Actors/" + copy.id + ".prefab", copy) }));
            }
            foreach (var e in definitions.GroupBy(d => d.animation).Select(g => g.First()).Select(d => new KeyValuePair<string,string[]>(d.animation,d.images)).Concat(extras.Select(e => new KeyValuePair<string,string[]>(e.Key,e.Value.Select(s=>"action/"+s).ToArray()))).GroupBy(e=>e.Key).Select(g=>g.First()))
            {
                var copy = e;
                jobs.Enqueue(() => effects.Add(new OriginalAssetCatalog.EffectEntry { animation = copy.Key, prefab = BakeActor(Root + "Effects/" + copy.Key + ".prefab", new SpriteDefinition { id = -1000, type = 7, name = copy.Key, animation = copy.Key, images = copy.Value }) }));
            }
            foreach (var stage in OriginalAssets.Table<StageDefinition>("stages")) { var copy = stage; jobs.Enqueue(() => BakeStage(copy)); }
            jobs.Enqueue(BakeGroundDropAssets);
            jobs.Enqueue(Finish);
            EditorApplication.update -= Advance; EditorApplication.update += Advance;
        }
        [MenuItem("古剑/补充辅助技能预制资源（保留已有编辑）")]
        public static void BakeSupportSkillEffects()
        {
            if (Application.isPlaying || Running) throw new InvalidOperationException("Exit Play mode and wait for baking");
            catalog = AssetDatabase.LoadAssetAtPath<OriginalAssetCatalog>("Assets/Resources/OriginalAssetCatalog.asset");
            library = AssetDatabase.LoadAssetAtPath<OriginalSpriteLibrary>(Root + "SpriteLibrary.asset");
            if (!catalog || !library) throw new InvalidOperationException("Existing baked library required");
            sprites.Clear(); foreach (var entry in library.sprites) sprites[entry.key] = entry;
            string[] images = { "action/s_41", "action/h_04" };
            BakeAnimationSprites("s_41", images);
            catalog.sprites = sprites.Values.OrderBy(s => s.key).ToArray(); library.sprites = catalog.sprites;
            var prefab = BakeActor(Root + "Effects/s_41.prefab", new SpriteDefinition { id = -1000, type = 7, name = "s_41", animation = "s_41", images = images });
            var entries = catalog.effects.ToList();
            if (!entries.Any(e => e.animation == "s_41")) entries.Add(new OriginalAssetCatalog.EffectEntry { animation = "s_41", prefab = prefab });
            catalog.effects = entries.ToArray();
            EditorUtility.SetDirty(library); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }
        public static void BakeEnemyBackEffect()
        {
            if (Application.isPlaying || Running) throw new InvalidOperationException("Exit Play mode and wait for baking");
            catalog = AssetDatabase.LoadAssetAtPath<OriginalAssetCatalog>("Assets/Resources/OriginalAssetCatalog.asset");
            library = AssetDatabase.LoadAssetAtPath<OriginalSpriteLibrary>(Root + "SpriteLibrary.asset");
            if (!catalog || !library) throw new InvalidOperationException("Existing baked library required");
            sprites.Clear(); foreach (var entry in library.sprites) sprites[entry.key] = entry;
            var images = new[] { "action/s_36" };
            BakeAnimationSprites("s_36", images);
            catalog.sprites = sprites.Values.OrderBy(s => s.key).ToArray(); library.sprites = catalog.sprites;
            var prefab = BakeActor(Root + "Effects/s_36.prefab", new SpriteDefinition { id = -1001, type = 7, name = "s_36", animation = "s_36", images = images });
            var entries = catalog.effects.ToList();
            if (!entries.Any(e => e.animation == "s_36")) entries.Add(new OriginalAssetCatalog.EffectEntry { animation = "s_36", prefab = prefab });
            catalog.effects = entries.ToArray();
            EditorUtility.SetDirty(library); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }
        private static void Advance()
        {
            try
            {
                double until = EditorApplication.timeSinceStartup + .05;
                do { if (jobs.Count > 0) jobs.Dequeue()(); }
                while (jobs.Count > 0 && EditorApplication.timeSinceStartup < until);
                if (jobs.Count == 0) EditorApplication.update -= Advance;
            }
            catch (Exception e) { LastError = e.ToString(); jobs.Clear(); EditorApplication.update -= Advance; Debug.LogException(e); }
        }
        [MenuItem("古剑/补充掉落预制资源（保留已有编辑）")]
        public static void BakeGroundDrops()
        {
            if (Application.isPlaying || Running) throw new InvalidOperationException("Exit Play mode and wait for baking");
            catalog = AssetDatabase.LoadAssetAtPath<OriginalAssetCatalog>("Assets/Resources/OriginalAssetCatalog.asset");
            library = AssetDatabase.LoadAssetAtPath<OriginalSpriteLibrary>(Root + "SpriteLibrary.asset");
            if (!catalog || !library) throw new InvalidOperationException("Existing baked library required");
            sprites.Clear(); foreach (var entry in library.sprites) sprites[entry.key] = entry;
            BakeGroundDropAssets();
            library.sprites = sprites.Values.OrderBy(s=>s.key).ToArray(); catalog.sprites = library.sprites;
            EditorUtility.SetDirty(library); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }
        private static void BakeGroundDropAssets()
        {
            Directory.CreateDirectory(Root + "Drops/Items"); Directory.CreateDirectory(Root + "Drops/Weapons");
            BakeIconDrop("Coin", null);
            foreach (var item in OriginalAssets.Table<ItemDefinition>("item")) BakeIconDrop("Items/" + item.id, item);
            foreach (string image in OriginalAssets.Table<EquipmentDefinition>("equip").Select(e=>e.image.Split(',')[0]).Distinct())
            {
                var definition = new SpriteDefinition { id=-1000, type=17, name="武器掉落 " + image, animation="z_weapondrop", images=new[]{"action/"+image,"action/i_00"} };
                BakeAnimationSprites(definition.animation,definition.images); catalog.sprites=sprites.Values.ToArray();
                string path=Root+"Drops/Weapons/"+image+".prefab";
                BakeActor(path,definition);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<OriginalGroundDrop>()) continue;
                var root=PrefabUtility.LoadPrefabContents(path);
                root.AddComponent<OriginalGroundDrop>().PrepareAsset(root.GetComponentsInChildren<SpriteRenderer>(true),root.GetComponent<OriginalActor>(),image);
                PrefabUtility.SaveAsPrefabAsset(root,path); PrefabUtility.UnloadPrefabContents(root);
            }
        }
        private static void BakeIconDrop(string name, ItemDefinition item)
        {
            string path=Root+"Drops/"+name+".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path)) return;
            var root=new GameObject(name); var pieces=new List<SpriteRenderer>();
            if (item==null)
            {
                var texture=OriginalAssets.Texture("pic/coin");
                AddDropPiece(root,pieces,"银两","pic/coin",new OriginalRect{w=texture.width,h=texture.height},new Vector2(texture.width/2f,-texture.height/2f));
            }
            else
            {
                AddDropPiece(root,pieces,"底框","pic/i_0"+(item.type+1),new OriginalRect{w=26,h=26},new Vector2(11,-11));
                // The original ground renderer uses the raw item ID; slot 23 lies beyond this atlas.
                if (item.id*22+22<=OriginalAssets.Texture("pic/item").width)
                    AddDropPiece(root,pieces,"道具","pic/item",new OriginalRect{x=item.id*22,w=22,h=22},new Vector2(11,-11));
            }
            root.AddComponent<OriginalGroundDrop>().PrepareAsset(pieces.ToArray());
            PrefabUtility.SaveAsPrefabAsset(root,path); UnityEngine.Object.DestroyImmediate(root);
        }
        private static void AddDropPiece(GameObject root,List<SpriteRenderer> pieces,string name,string image,OriginalRect rect,Vector2 position)
        {
            var child=new GameObject(name); child.transform.SetParent(root.transform,false); child.transform.localPosition=position;
            var renderer=child.AddComponent<SpriteRenderer>(); renderer.sprite=BakeSprite(image,rect); pieces.Add(renderer);
        }
        private static void BakeAnimationSprites(string animation, string[] images)
        {
            var data = OriginalAssets.Json<OriginalAnimation>("Animations/action/" + animation);
            for (int sheet = 0; sheet < data.sheets.Length; sheet++)
                foreach (var r in data.sheets[sheet].rects) BakeSprite(images[sheet], r);
        }
        public static void PrepareEditedAnimation(string id, OriginalAnimation data, Dictionary<int, HashSet<string>> imageSources)
        {
            if (Application.isPlaying || Running) throw new InvalidOperationException("Exit Play mode and wait for baking");
            catalog = AssetDatabase.LoadAssetAtPath<OriginalAssetCatalog>("Assets/Resources/OriginalAssetCatalog.asset");
            library = AssetDatabase.LoadAssetAtPath<OriginalSpriteLibrary>(Root + "SpriteLibrary.asset");
            if (!catalog || !library) throw new InvalidOperationException("Existing baked catalog and sprite library required");
            sprites.Clear(); foreach (var entry in library.sprites) sprites[entry.key] = entry;
            foreach (var sheet in imageSources)
                foreach (string image in sheet.Value)
                    foreach (var rect in data.sheets[sheet.Key].rects) BakeSprite(image, rect);
            catalog.sprites = sprites.Values.OrderBy(s => s.key, StringComparer.Ordinal).ToArray(); library.sprites = catalog.sprites;
            EditorUtility.SetDirty(catalog); EditorUtility.SetDirty(library);
            int capacity = data.frames.Max(f => f.pieces.Length);
            var ids = new HashSet<int>(OriginalAssets.Table<SpriteDefinition>("sprite").Where(d => d.animation == id).Select(d => d.id));
            // Include stage-local copies and drop/effect prefabs, preserving all existing edits.
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Baked" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                bool effect = path == Root + "Effects/" + id + ".prefab";
                if (!prefab.GetComponentsInChildren<OriginalActor>(true).Any(a => (ids.Contains(a.definitionId) || (effect && a.transform == prefab.transform)) && a.FramePieceCapacity < capacity)) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var actor in root.GetComponentsInChildren<OriginalActor>(true))
                        if (ids.Contains(actor.definitionId) || (effect && actor.transform == root.transform)) actor.PrepareFramePieces(capacity);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        private static Sprite BakeSprite(string image, OriginalRect r)
        {
            if (r.w <= 0 || r.h <= 0) return null;
            string key = OriginalAssetCatalog.SpriteKey(image, r);
            if (sprites.TryGetValue(key, out var existing)) return existing.sprite;
            var texture = OriginalAssets.Texture(image);
            var sprite = Sprite.Create(texture, new Rect(r.x, texture.height - r.y - r.h, r.w, r.h), new Vector2(.5f,.5f), 1);
            sprite.name = key; AssetDatabase.AddObjectToAsset(sprite, library);
            sprites[key] = new OriginalAssetCatalog.SpriteEntry { key = key, sprite = sprite };
            return sprite;
        }
        private static GameObject BakeImage(string image)
        {
            string path = Root + "Images/" + image + ".prefab";
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (saved) return saved;
            var texture = OriginalAssets.Texture(image);
            var go = new GameObject(Path.GetFileName(image));
            go.AddComponent<SpriteRenderer>().sprite = BakeSprite(image, new OriginalRect { w = texture.width, h = texture.height });
            var result = PrefabUtility.SaveAsPrefabAsset(go, path); UnityEngine.Object.DestroyImmediate(go); return result;
        }
        private static void BakeMap(string id)
        {
            string path = Root + "Maps/" + id + ".prefab";
            var old = catalog.maps == null ? null : catalog.maps.FirstOrDefault(m => m.id == id);
            var data = old != null ? old.data : OriginalAssets.Json<OriginalMap>("Maps/" + id);
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!saved)
            {
                var root = new GameObject(id);
                for (int i = 0; i < 3; i++)
                {
                    var texture = OriginalAssets.Texture("Maps/" + id + "_" + i);
                    var layer = new GameObject("Layer " + i); layer.transform.SetParent(root.transform, false);
                    layer.transform.localPosition = new Vector3(data.Width / 2f, -data.Height / 2f, 0);
                    var renderer = layer.AddComponent<SpriteRenderer>();
                    renderer.sprite = BakeSprite("Maps/" + id + "_" + i, new OriginalRect { w = texture.width, h = texture.height });
                    renderer.sortingOrder = i == 0 ? -25000 : i == 1 ? -5000 : 28000;
                }
                saved = PrefabUtility.SaveAsPrefabAsset(root, path); UnityEngine.Object.DestroyImmediate(root);
            }
            maps.Add(new OriginalAssetCatalog.MapEntry { id = id, data = data, prefab = saved });
        }
        private static OriginalActor BakeActor(string path, SpriteDefinition definition)
        {
            var saved = AssetDatabase.LoadAssetAtPath<OriginalActor>(path);
            if (saved)
            {
                int capacity=OriginalAssets.Json<OriginalAnimation>("Animations/action/" + definition.animation).frames.Max(f=>f.pieces.Length);
                if (saved.FramePieceCapacity < capacity)
                {
                    var root=PrefabUtility.LoadPrefabContents(path);
                    root.GetComponent<OriginalActor>().PrepareFramePieces(capacity);
                    PrefabUtility.SaveAsPrefabAsset(root,path); PrefabUtility.UnloadPrefabContents(root);
                }
                return saved;
            }
            var go = new GameObject(definition.name); var actor = go.AddComponent<OriginalActor>();
            var data = OriginalAssets.Json<OriginalAnimation>("Animations/action/" + definition.animation);
            actor.PrepareFramePieces(data.frames.Max(f => f.pieces.Length));
            actor.Initialize(definition, definition.id, Vector2.zero, 2);
            var result = PrefabUtility.SaveAsPrefabAsset(go, path).GetComponent<OriginalActor>();
            UnityEngine.Object.DestroyImmediate(go); return result;
        }
        private static void BakeStage(StageDefinition stage)
        {
            string path = Root + "Stages/" + stage.script + ".prefab";
            var saved = AssetDatabase.LoadAssetAtPath<OriginalStageLayout>(path);
            if (!saved)
            {
                var go = new GameObject(stage.script + " " + stage.name);
                var layout = go.AddComponent<OriginalStageLayout>(); layout.script = stage.script; layout.mapId = stage.map;
                var sourceMap = maps.FirstOrDefault(m => m.id == stage.map) ??
                    (catalog.maps ?? new OriginalAssetCatalog.MapEntry[0]).First(m => m.id == stage.map);
                PrefabUtility.InstantiatePrefab(sourceMap.prefab, go.transform);
                var fixedActors = new List<OriginalActor>(); int depth = 0;
                var candidates = new Dictionary<int, Match>(); var duplicates = new HashSet<int>();
                foreach (string instruction in OriginalAssets.Script(stage.script))
                {
                    string line = instruction.Trim();
                    if (line.StartsWith("endif") || line == "loop") { depth--; continue; }
                    if (line.StartsWith("if(") || line.StartsWith("while(")) { depth++; continue; }
                    if (depth != 0) continue;
                    var match = Regex.Match(line, "^x30\\((-?\\d+),(\\d+),\"([lrud])\",(-?\\d+),(-?\\d+),(\\d+)\\)$");
                    if (!match.Success) continue;
                    int id = int.Parse(match.Groups[1].Value);
                    if (candidates.ContainsKey(id)) duplicates.Add(id); else candidates[id] = match;
                }
                foreach (var pair in candidates.Where(p => !duplicates.Contains(p.Key)))
                {
                    var m = pair.Value; int id = int.Parse(m.Groups[2].Value);
                    var definition = OriginalAssets.Table<SpriteDefinition>("sprite").First(d => d.id == id);
                    if (definition.type == 0 || definition.type >= 9 || pair.Key < 0) continue;
                    var a = ((GameObject)PrefabUtility.InstantiatePrefab(actors.First(e => e.id == id).prefab.gameObject, go.transform)).GetComponent<OriginalActor>();
                    a.Initialize(definition, pair.Key, new Vector2(int.Parse(m.Groups[4].Value),int.Parse(m.Groups[5].Value)),int.Parse(m.Groups[6].Value));
                    a.Face(m.Groups[3].Value); a.useLayoutTransform = true;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(a);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(a.transform);
                    fixedActors.Add(a);
                }
                layout.fixedActors = fixedActors.ToArray();
                saved = PrefabUtility.SaveAsPrefabAsset(go, path).GetComponent<OriginalStageLayout>(); UnityEngine.Object.DestroyImmediate(go);
            }
            stages.Add(new OriginalAssetCatalog.StageEntry { script = stage.script, prefab = saved });
        }
        private static void BindRoots(GuJianGame game)
        {
            var actorRoot = game.transform.Find("Actors"); if (!actorRoot) { var go = new GameObject("Actors"); go.transform.SetParent(game.transform, false); actorRoot = go.transform; }
            var mapRoot = game.transform.Find("Map"); if (!mapRoot) { var go = new GameObject("Map"); go.transform.SetParent(game.transform, false); mapRoot = go.transform; }
            var serialized = new SerializedObject(game);
            serialized.FindProperty("actorRoot").objectReferenceValue = actorRoot.gameObject;
            serialized.FindProperty("mapRoot").objectReferenceValue = mapRoot.gameObject;
            var music = game.GetComponent<AudioSource>();
            if (!music) { music = game.gameObject.AddComponent<AudioSource>(); music.loop = true; music.volume = .45f; music.playOnAwake = false; }
            serialized.FindProperty("music").objectReferenceValue = music;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Finish()
        {
            // User-authored maps and layouts are not rediscovered by the original JSON scan.
            foreach (var map in catalog.maps ?? new OriginalAssetCatalog.MapEntry[0]) if (!maps.Any(m => m.id == map.id)) maps.Add(map);
            foreach (var stage in catalog.stages ?? new OriginalAssetCatalog.StageEntry[0]) if (!stages.Any(s => s.script == stage.script)) stages.Add(stage);
            library.sprites = sprites.Values.OrderBy(s=>s.key).ToArray();
            catalog.sprites = library.sprites; catalog.maps = maps.ToArray(); catalog.actors = actors.ToArray(); catalog.effects = effects.ToArray(); catalog.stages = stages.ToArray();
            EditorUtility.SetDirty(library); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            var host = PrefabUtility.LoadPrefabContents("Assets/Prefabs/GuJianGame.prefab"); BindRoots(host.GetComponent<GuJianGame>());
            PrefabUtility.SaveAsPrefabAsset(host, "Assets/Prefabs/GuJianGame.prefab"); PrefabUtility.UnloadPrefabContents(host);
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path == "Assets/Scenes/GuJian.unity")
            {
                BindRoots(UnityEngine.Object.FindObjectOfType<GuJianGame>());
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            WorldPresentationSetup.Run();
            Debug.Log("GUJIAN_BAKED_ASSETS_READY sprites=" + sprites.Count + " maps=" + maps.Count + " actors=" + actors.Count + " stages=" + stages.Count);
        }
    }
}
