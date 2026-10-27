using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class MapEventValidation
    {
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static string Hash(string path)
        {
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path)));
        }
        [MenuItem("古剑/内容/验证地图事件编辑")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("退出播放模式后验证地图编辑");
            const string id = "__map_event_authoring_validation__";
            string path = StoryAssetSetup.AssetPath(id);
            if (File.Exists(path)) throw new InvalidOperationException("测试路径已有资产：" + path);
            var sources = Directory.GetFiles(StoryAssetSetup.Root, "*.asset").ToDictionary(p => p, Hash);
            var checks = new List<string>(); string error = null; StorySequence fixture = null;
            Undo.IncrementCurrentGroup(); int undoGroup = Undo.GetCurrentGroup();
            try
            {
                MapEventSetup.Rebuild(); var audit = MapEventSetup.AuditCatalog();
                int exits = 0, entries = 0, callbacks = 0;
                foreach (var stage in OriginalAssets.Table<StageDefinition>("stages"))
                    foreach (string line in OriginalAssets.Script(stage.script))
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("x24(")) exits++;
                        if (trimmed.StartsWith("x26(")) entries++;
                        if (trimmed.StartsWith("x37(")) callbacks++;
                    }
                Require(audit.exits == exits && audit.entries == entries &&
                    audit.interactions + audit.deaths + audit.specials + audit.otherCallbacks == callbacks, "Stage command coverage mismatch");
                checks.Add("All " + audit.stages + " stage sources indexed: " + audit.exits + " exits, " + audit.entries + " entries, " +
                    audit.interactions + " interactions, " + audit.deaths + " deaths, " + audit.specials + " special callbacks");
                string indexHash = Hash(MapEventSetup.AssetPath); MapEventSetup.Rebuild();
                Require(Hash(MapEventSetup.AssetPath) == indexHash, "Repeated scan changed index bytes");
                checks.Add("Repeated generation is byte-identical and serialized index has a valid dedicated ScriptableObject type");
                var catalog = OriginalAssets.Catalog;
                foreach (var map in catalog.maps)
                {
                    Require(map.prefab && map.data != null && map.data.cells.Length == map.data.columns * map.data.rows, "Map data/layout missing: " + map.id);
                    var editable = map.prefab.GetComponent<EditableTilemap>();
                    if (editable)
                    {
                        Require(editable.background && editable.decoration && editable.foreground && editable.collision &&
                            editable.columns == map.data.columns && editable.rows == map.data.rows, "Saved Tilemap layers/dimensions missing: " + map.id);
                    }
                    else
                    {
                        var renderers = map.prefab.GetComponentsInChildren<SpriteRenderer>(true);
                        Require(renderers.Length == 3 && renderers.All(r => r.sprite), "Saved map layers missing: " + map.id);
                    }
                }
                Require(catalog.maps.Length >= 52, "Original maps were lost");
                checks.Add(catalog.maps.Length + " map prefabs retain saved legacy/Tilemap layers and complete collision grids");
                foreach (var stage in OriginalAssets.Table<StageDefinition>("stages"))
                    Require(catalog.stages.Any(s => s.script == stage.script && s.prefab), "Missing saved stage layout: " + stage.script);
                var minimap = Resources.Load<OriginalMiniMapLayout>("OriginalMiniMapLayout");
                Require(minimap && minimap.areas.Length >= 13 && minimap.areas.Sum(a => a.rooms.Length) >= 174, "Original minimap areas/rooms were lost");
                checks.Add(audit.stages + " saved stage layouts and at least 13-area / 174-room minimap remain referenced");
                var original = new[] { "if(x0A(\"event\")=1)", "  x24(2,3,\"200.s\")  ", "x26(52,251,\"r\")",
                    "x30(17,84,\"l\",238,242,2)", "x37(17,\"200.s\",1)", "x37(17,\"200.s\",2)",
                    "else", "x26(227,247,\"l\")", "endif" };
                fixture = ScriptableObject.CreateInstance<StorySequence>(); fixture.Import(id, "地图编辑验证", original);
                var meta = new SerializedObject(fixture); meta.FindProperty("notes").stringValue = "保留开发备注"; meta.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(fixture, path);
                var parsed = MapEventSetup.Parse("map200", id, original);
                var callback = parsed.Where(e => e.kind == MapEventKind.ActorCallback).ToArray();
                Require(callback.Length == 2 && callback[0] != callback[1] && callback[0].callbackMode == 1 && callback[1].callbackMode == 2 &&
                    callback[0].sourceLine == 4 && callback[1].sourceLine == 5 && callback.All(e => e.instanceId == 17 && e.actorId == 84 && e.actorSourceLine == 3), "Callback alias/mode/source mismatch");
                Require(parsed.Last().condition.Contains("else of") && !parsed[0].condition.Contains("else of"), "Else branch indistinguishable");
                checks.Add("Separate actor callbacks retain instance, definition, creation/source lines and mode; else branches are distinguished");
                var exit = parsed.First(); MapEventSetup.Save(exit, 8, 9, "201.s", "r");
                var expected = (string[])original.Clone(); expected[1] = "  x24(8,9,\"201.s\")  ";
                Require(OriginalAssets.Script(id).SequenceEqual(expected), "Exit save changed unrelated instructions");
                meta.Update(); Require(fixture.ScriptId == id && fixture.DisplayName == "地图编辑验证" && meta.FindProperty("notes").stringValue == "保留开发备注", "Metadata changed");
                Require(File.ReadAllText(path).Contains("x24(8,9"), "Exit was not persisted to disk");
                checks.Add("Exit edit persists through production script lookup, preserving whitespace, all other lines and story metadata");
                bool stale = false; try { MapEventSetup.Save(exit, 1, 1, "200.s", "r"); } catch (InvalidOperationException) { stale = true; }
                bool missing = false; try { MapEventSetup.Save(parsed[1], 1, 1, "", "bad"); } catch (ArgumentException) { missing = true; }
                bool invalidTarget = false; try { MapEventSetup.Save(parsed[2], 1, 1, "__missing_map_target__", "r"); } catch (ArgumentException) { invalidTarget = true; }
                Require(stale && missing && invalidTarget && fixture.ReadInstructions().SequenceEqual(expected), "Invalid/stale edit protection failed");
                checks.Add("Stale source selections, invalid facing and missing targets cannot overwrite the source");
                MapEventSetup.Save(parsed[1], 60, 260, "", "u"); expected[2] = "x26(60,260,\"u\")";
                MapEventSetup.Save(parsed[2], 0, 0, "201", "r"); expected[4] = "x37(17,\"201.s\",1)";
                Require(OriginalAssets.Script(id).SequenceEqual(expected), "Entry or callback save changed other commands/mode");
                checks.Add("Entry position/facing and callback target persist while preserving the explicit death mode");
                object[] observedExit = null, observedEntry = null, observedCallback = null;
                var vm = new OriginalScriptMachine { LoadScript = OriginalAssets.Script,
                    Command = (command, args) => { if (command == 0x24) observedExit = args; if (command == 0x26) observedEntry = args;
                        if (command == 0x37 && OriginalScriptMachine.Int(args[2]) == 1) observedCallback = args; return null; } };
                vm.Values["event"] = 1; vm.Enqueue(id); vm.Tick();
                Require(vm.Error == null && !vm.Running && OriginalScriptMachine.Int(observedExit[0]) == 8 &&
                    Convert.ToString(observedExit[2]) == "201.s" && OriginalScriptMachine.Int(observedEntry[0]) == 60 &&
                    Convert.ToString(observedEntry[2]) == "u" && Convert.ToString(observedCallback[1]) == "201.s", "VM did not consume edited source");
                checks.Add("Production VM reads the saved edits and dispatches updated exit, entry and death callback arguments under the original condition");
                Undo.FlushUndoRecordObjects(); Undo.RevertAllDownToGroup(undoGroup);
                Require(fixture.ReadInstructions().SequenceEqual(original), "Undo did not restore original fixture commands");
                checks.Add("Unity Undo restores all fixture edits to their original commands");
                Require(audit.missingReferences.Length == 8 && audit.missingReferences.Select(r => r.target).Distinct().OrderBy(s => s)
                    .SequenceEqual(new[] { "1401_1400.s", "1436_0.s" }), "Missing-reference baseline changed");
                foreach (string sample in new[] { "200", "334", "1401", "1436" })
                    Require(MapEventSetup.Load().stages.Any(s => s.stageScript == sample && s.events.Length > 0), "Sample stage missing: " + sample);
                checks.Add("200 / 334 / 1401 / 1436 indexed; eight unresolved original exits to two absent scripts are explicitly reported");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                if (fixture) { Undo.ClearUndo(fixture); AssetDatabase.DeleteAsset(path); }
                OriginalAssets.ClearCache(); MapEventSetup.Rebuild();
            }
            if (sources.Any(p => !File.Exists(p.Key) || Hash(p.Key) != p.Value)) error = (error ?? "") + " Production story files changed";
            else checks.Add("All production story files remain byte-identical after the isolated authoring test; temporary fixture removed");
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/map-event-authoring.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_MAP_EVENT_AUTHORING passed=" + report.passed + " " + error);
        }
    }
}
