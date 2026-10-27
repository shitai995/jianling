using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class EditorReloadValidation
    {
        private const string Pending = "GuJian.Validation.EditorReload";
        private static double deadline;
        [MenuItem("古剑/验证播放中脚本重载")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (!g.startAtTitle) throw new InvalidOperationException("Validation requires the default title-screen setting");
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999; g.Machine.Enqueue("200"); g.Machine.Tick();
            if (g.Map == null) throw new Exception("Reload stage fixture failed");
            SessionState.SetBool(Pending, true);
            EditorUtility.RequestScriptReload();
        }
        [InitializeOnLoadMethod]
        private static void Resume()
        {
            if (!SessionState.GetBool(Pending, false)) return;
            SessionState.SetBool(Pending, false);
            // Assembly callbacks can run before Unity has re-enabled scene behaviours.
            deadline = EditorApplication.timeSinceStartup + 10;
            EditorApplication.update += AwaitRuntime;
        }
        private static void AwaitRuntime()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (EditorApplication.timeSinceStartup < deadline && (!EditorApplication.isPlaying || !g || g.Machine == null)) return;
            EditorApplication.update -= AwaitRuntime;
            Verify();
        }
        private static void Verify()
        {
            string error = null;
            var checks = new System.Collections.Generic.List<string>();
            try
            {
                var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
                if (!EditorApplication.isPlaying || !g || g.Machine == null || !g.AtTitle || g.LastError != null)
                    throw new Exception("Script reload did not restore the VM and title screen");
                checks.Add("Actual play-mode domain reload restores runtime dependencies and returns to the title screen without an empty VM");
                g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999; g.Machine.Enqueue("200"); g.Machine.Tick();
                typeof(GuJianGame).GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(g, null);
                if (g.Map == null || g.LastError != null || !g.Actors.ContainsKey(-1)) throw new Exception("Restart after reload failed");
                checks.Add("Starting again after reload loads real stage 200, registers the player and executes Update without the prior null-reference failure");
            }
            catch (Exception e) { error = e.ToString(); }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/editor-reload.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_EDITOR_RELOAD_VALIDATION " + report.passed + " " + error);
        }
    }
}
