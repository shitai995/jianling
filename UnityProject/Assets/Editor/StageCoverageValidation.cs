using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    // Tests entry construction using the original scripts, not a replacement map loader.
    public static class StageCoverageValidation
    {
        private static StageDefinition[] stages;
        private static int index, frames;
        private static bool loading;
        private static double started;
        private static float previousScale;
        private static string previousSave;
        private static readonly List<string> checks = new List<string>();
        [MenuItem("古剑/验证全部场景入口")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            stages = OriginalAssets.Table<StageDefinition>("stages");
            checks.Clear(); index = frames = 0; loading = false;
            previousScale = Time.timeScale; previousSave = g.saveFileName;
            g.saveFileName = "validation-stages.json"; Time.timeScale = 8;
            EditorApplication.update -= Advance; EditorApplication.update += Advance;
        }
        private static void Advance()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            try
            {
                if (!EditorApplication.isPlaying || !g) throw new Exception("Play mode ended");
                if (!loading)
                {
                    // Late-game neutral state avoids invoking entry cutscenes. In the shipped
                    // 817 script an unclosed event>433 block also requires a valid late event.
                    g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = 9999;
                    g.Machine.Enqueue(stages[index].script);
                    started = EditorApplication.timeSinceStartup; frames = 0; loading = true;
                    return;
                }
                if (g.LastError != null) throw new Exception(g.LastError);
                if (EditorApplication.timeSinceStartup - started > 15) throw new Exception("Entry timeout: " + g.Machine.CurrentScript + ":" + g.Machine.Instruction);
                if (++frames < 3) return; // Let Unity dispose the previous instances and run real Update.
                var stage = stages[index];
                if (g.Map == null) return;
                if (g.Map.id != stage.map) throw new Exception("Wrong map " + g.Map.id + " expected " + stage.map);
                if (!g.Actors.ContainsKey(-1)) throw new Exception("Missing role");
                if (!g.Actors.Values.All(a => a && a.Animation != null && a.Animation.frames.All(f => f.pieces.Length <= a.GetComponentsInChildren<SpriteRenderer>(true).Length))) throw new Exception("Incomplete actor prefab");
                checks.Add(stage.script + " / " + stage.name + " / " + g.Map.id + " / " + g.Actors.Count + " actors: original entry executed, saved layout and animation references loaded");
                index++; loading = false;
                if (index == stages.Length) Finish(true, null);
            }
            catch (Exception ex) { Finish(false, stages[index].script + ": " + ex); }
        }
        private static void Finish(bool passed, string error)
        {
            EditorApplication.update -= Advance; Time.timeScale = previousScale;
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>(); if (g) g.saveFileName = previousSave;
            var report = new ReconstructionValidation.Report { passed = passed, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/stage-coverage.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_STAGE_COVERAGE " + passed + " " + checks.Count + "/" + stages.Length + " " + error);
        }
    }
}
