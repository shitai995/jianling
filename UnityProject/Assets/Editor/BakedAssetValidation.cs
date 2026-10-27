using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class BakedAssetValidation
    {
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        [MenuItem("古剑/验证预制资源与固定布局")]
        public static void Run()
        {
            var checks = new List<string>();
            var report = new ReconstructionValidation.Report { utc = DateTime.UtcNow.ToString("O") };
            try
            {
                var catalog = OriginalAssets.Catalog;
                Require(catalog && catalog.solid, "Missing catalog/solid sprite");
                Require(catalog.maps.Length == 52 && catalog.actors.Length == 137 && catalog.stages.Length == 209, "Asset coverage mismatch");
                foreach (var s in catalog.sprites) Require(s.sprite && AssetDatabase.Contains(s.sprite), "Transient or missing Sprite " + s.key);
                checks.Add(catalog.sprites.Length + " atlas/full-image Sprites are persistent Unity assets; solid-color frame Sprite is also saved");
                foreach (var map in catalog.maps)
                {
                    Require(map.prefab && map.prefab.GetComponentsInChildren<SpriteRenderer>().Length == 3, "Missing map layers " + map.id);
                    foreach (var r in map.prefab.GetComponentsInChildren<SpriteRenderer>()) Require(r.sprite && AssetDatabase.Contains(r.sprite), "Transient layer " + map.id);
                    Require(map.data.cells.Length == map.data.columns * map.data.rows, "Map collision data " + map.id);
                }
                checks.Add("52 map Prefabs have three saved sprite layers and serialized, editable collision data in the catalog");
                var definitions = OriginalAssets.Table<SpriteDefinition>("sprite");
                foreach (var entry in catalog.actors)
                {
                    var d = definitions.First(a => a.id == entry.id);
                    var data = OriginalAssets.Json<OriginalAnimation>("Animations/action/" + d.animation);
                    Require(entry.prefab && entry.prefab.FramePieceCapacity >= data.frames.Max(f => f.pieces.Length), "Actor frame-piece capacity " + entry.id);
                    foreach (var sheet in data.sheets.Select((s,i) => new { sheet=s, image=d.images[i] }))
                        foreach (var rect in sheet.sheet.rects.Where(r => r.w > 0 && r.h > 0)) Require(catalog.FindSprite(sheet.image,rect), "Missing atlas slice " + sheet.image);
                }
                checks.Add("137 actor Prefabs contain enough saved SpriteRenderer nodes for every animation frame; every atlas crop resolves without Sprite.Create");
                foreach (var effect in catalog.effects)
                {
                    var data = OriginalAssets.Json<OriginalAnimation>("Animations/action/" + effect.animation);
                    Require(effect.prefab && effect.prefab.FramePieceCapacity >= data.frames.Max(f => f.pieces.Length), "Effect capacity " + effect.animation);
                }
                checks.Add(catalog.effects.Length + " effect/animation Prefabs contain their saved frame nodes, including Boss critical, finalslash, faces and arrows");
                int fixedActors = 0;
                foreach (var stage in catalog.stages)
                {
                    Require(stage.prefab && stage.prefab.script == stage.script && catalog.maps.Any(m => m.id == stage.prefab.mapId), "Stage reference " + stage.script);
                    foreach (var actor in stage.prefab.fixedActors)
                    {
                        Require(actor && actor.useLayoutTransform && actor.actorId >= 0 && actor.transform.IsChildOf(stage.prefab.transform), "Invalid layout actor " + stage.script);
                        fixedActors++;
                    }
                }
                checks.Add("209 stage layout Prefabs include nested map Prefabs and " + fixedActors + " unconditional fixed actors; conditional story actors remain controlled by scripts");
                if (Application.isPlaying)
                {
                    var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
                    g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999; g.Machine.Enqueue("200"); g.Machine.Tick();
                    Require(g.LastError == null && !g.Machine.Running, "Stage 200 failed");
                    // Destroy is deferred; identify the layout registered by this fixture, not an older instance awaiting disposal.
                    var layout = UnityEngine.Object.FindObjectsOfType<OriginalStageLayout>().Single(l => l.script == "200" && l.fixedActors.Length > 0
                        && g.Actors.TryGetValue(l.fixedActors[0].actorId, out var first) && first == l.fixedActors[0]);
                    Require(layout.fixedActors.Length > 0, "No runtime layout");
                    foreach (var actor in layout.fixedActors)
                        Require(g.Actors.TryGetValue(actor.actorId, out var registered) && registered == actor && actor.gameObject.activeSelf, "Fixed actor was recreated rather than reused");
                    Require(layout.GetComponentsInChildren<SpriteRenderer>(true).All(r => !r.sprite || AssetDatabase.Contains(r.sprite)), "Runtime layout created transient sprites");
                    checks.Add("Real script 200 reuses the saved layout actor instances and map renderers; all active frame Sprites still reference persistent assets");
                    var a = layout.fixedActors[0];
                    a.transform.position += new Vector3(13, -7, 0); Vector3 edited = a.transform.position;
                    var template = catalog.stages.First(s=>s.script=="200").prefab;
                    var instance = UnityEngine.Object.Instantiate(template);
                    var fixedActor = instance.fixedActors[0]; fixedActor.transform.position = edited;
                    var pending = (IDictionary<int,OriginalActor>)typeof(GuJianGame).GetField("pendingFixedActors", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(g);
                    pending[fixedActor.actorId] = fixedActor;
                    g.Machine.Command(0x30, new object[] { fixedActor.actorId, fixedActor.definitionId, "l", 0, 0, fixedActor.layer });
                    Require(g.Actors[fixedActor.actorId] == fixedActor && fixedActor.mapPosition == new Vector2(edited.x,-edited.y), "Layout edits overwritten by x30 coordinates");
                    UnityEngine.Object.Destroy(instance.gameObject);
                    checks.Add("A changed fixed-layout transform survives activation even when x30 provides different original coordinates");
                    g.Begin();
                }
                report.passed = true;
            }
            catch (Exception e) { report.error = e.ToString(); }
            report.checks = checks.ToArray(); Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/baked-assets.json", JsonUtility.ToJson(report,true));
            Debug.Log("GUJIAN_BAKED_ASSET_VALIDATION " + report.passed + " " + report.error);
        }
    }
}
