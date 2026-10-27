using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ContentWorkflowValidation
    {
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        [MenuItem("古剑/内容/验证剧情资产迁移")]
        public static void Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("退出播放模式后验证迁移");
            var checks = new List<string>();
            string error = null;
            try
            {
                var files = Directory.GetFiles("Assets/Resources/Original/Scripts", "*.json");
                int instructions = 0;
                foreach (var file in files)
                {
                    string id = Path.GetFileNameWithoutExtension(file);
                    var story = AssetDatabase.LoadAssetAtPath<StorySequence>(StoryAssetSetup.AssetPath(id));
                    Require(story && story.ScriptId == id, "Native story identity: " + id);
                    var source = JsonUtility.FromJson<ScriptData>(File.ReadAllText(file)).instructions;
                    Require(source.SequenceEqual(OriginalAssets.Script(id)), "Source instruction drift: " + id);
                    Require(source.SequenceEqual(OriginalAssets.Script(id + ".s")), "Legacy extension: " + id);
                    Require(OriginalAssets.HasScript(id), "Production lookup: " + id);
                    instructions += source.Length;
                }
                checks.Add(files.Length + " native stories preserve all " + instructions + " source instructions and .s aliases through production lookup");
                var catalog = OriginalAssets.Catalog;
                foreach (var stage in OriginalAssets.Table<StageDefinition>("stages"))
                    Require(OriginalAssets.HasScript(stage.script) && catalog.maps.Any(m => m.id == stage.map && m.prefab) && catalog.stages.Any(s => s.script == stage.script && s.prefab), "Stage references: " + stage.script);
                checks.Add("All stage definitions retain valid story, map and saved layout references");
                foreach (var actor in OriginalAssets.Table<SpriteDefinition>("sprite"))
                    Require(catalog.actors.Any(a => a.id == actor.id && a.prefab), "Actor prefab: " + actor.id);
                checks.Add("All actor definitions retain saved prefab references");
                var start = Resources.Load<StorySequence>("Stories/start");
                string path = AssetDatabase.GetAssetPath(start), snapshot = EditorJsonUtility.ToJson(start);
                byte[] before = File.ReadAllBytes(path);
                try
                {
                    start.Import("start", "编辑入口验证", new[] { "x09(\"contentProbe\",73)" });
                    EditorUtility.SetDirty(start); AssetDatabase.SaveAssetIfDirty(start); OriginalAssets.ClearCache();
                    var vm = new OriginalScriptMachine { LoadScript = OriginalAssets.Script };
                    vm.Enqueue("start"); vm.Tick();
                    Require(!vm.Running && vm.Get("contentProbe") == 73, "VM ignored saved native source");
                    checks.Add("Saved native story edits reach the independent production VM without modifying JSON or GuJianGame");
                    var owned = OriginalAssets.Script("start"); owned[0] = "x09(\"contentProbe\",99)";
                    Require(OriginalAssets.Script("start")[0] != owned[0], "VM caller modified source through shared array");
                    checks.Add("Execution snapshots isolate caller mutations from saved story data");
                    byte[] edited = File.ReadAllBytes(path);
                    StoryAssetSetup.ImportMissing();
                    Require(start.ReadInstructions()[0] == "x09(\"contentProbe\",73)" && File.ReadAllBytes(path).SequenceEqual(edited), "Repeated import overwrote edits");
                    checks.Add("Repeated missing-only import preserves edited instructions and native identity");
                }
                finally
                {
                    EditorJsonUtility.FromJsonOverwrite(snapshot, start);
                    EditorUtility.SetDirty(start); AssetDatabase.SaveAssetIfDirty(start); OriginalAssets.ClearCache();
                }
                Require(before.SequenceEqual(File.ReadAllBytes(path)), "Story source was not restored byte-for-byte");
                checks.Add("Temporary source edit restored byte-for-byte");
                Require(!OriginalAssets.HasScript("__content_missing__"), "Missing source accepted");
                foreach (var invalid in new[] { "", "../start", "path/start" })
                {
                    bool rejected = false;
                    try { OriginalAssets.HasScript(invalid); } catch (ArgumentException) { rejected = true; }
                    Require(rejected, "Invalid story identity accepted: " + invalid);
                }
                checks.Add("Missing source and malformed identifiers are rejected through the shared content interface");
            }
            catch (Exception e) { error = e.ToString(); }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/content-workflow.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_CONTENT_WORKFLOW " + report.passed + " " + error);
        }
    }
}
