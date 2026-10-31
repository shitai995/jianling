using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;
namespace GuJian.Editor
{
    public static class M2RecordMenuValidation
    {
        private static IEnumerator work; private static double started; private static readonly List<string> checks = new List<string>();
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Require(bool ok, string message) { if (!ok) throw new Exception(message); }
        private static T Get<T>(GuJianGame g, string field) => (T)typeof(GuJianGame).GetField(field, Private).GetValue(g);
        private static IEnumerator Frames(int count) { int end = Time.frameCount + count; while (Time.frameCount < end) yield return null; }
        private static IEnumerator Press(Keyboard keyboard, Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); var wait = Frames(3); while (wait.MoveNext()) yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); wait = Frames(3); while (wait.MoveNext()) yield return null;
        }
        [MenuItem("GuJian/Native UI/Validate checkpoint and record menu")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying && work == null, "Enter Play with no active validation");
            checks.Clear(); started = EditorApplication.timeSinceStartup; work = Steps(); EditorApplication.update += Tick;
        }
        private static IEnumerator Steps()
        {
            var g = Object.FindObjectOfType<GuJianGame>(); Require(g, "Game missing");
            var oldKeyboard = Keyboard.current; Keyboard keyboard = null; float oldScale = Time.timeScale; string oldSave = g.saveFileName;
            string fixture = "validation-m2-record-" + Guid.NewGuid().ToString("N") + ".json";
            var recordPath = typeof(GuJianGame).GetMethod("RecordPath", Private); var protectedFiles = new Dictionary<string, byte[]>();
            foreach (string path in new[] { (string)recordPath.Invoke(g, new object[] { 0 }), (string)recordPath.Invoke(g, new object[] { 1 }), Path.Combine(Application.persistentDataPath, "gujian-sms-global.json") })
                foreach (string file in new[] { path, path + ".bak", path + ".tmp" }) protectedFiles[file] = File.Exists(file) ? File.ReadAllBytes(file) : null;
            var oldBackground = InputSystem.settings.backgroundBehavior; var oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
            try
            {
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard = InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent(); Time.timeScale = 1;
                g.saveFileName = fixture; g.Begin(); g.Machine.Clear();
                g.Machine.Values["event"] = 999; g.Machine.Values["stageID"] = 334; g.Machine.Values["taskID"] = 1; g.Machine.Enqueue("335");
                while (g.Machine.Running || Get<bool>(g, "loadingVisible")) { Require(g.LastError == null, "Stage failed: " + g.LastError); yield return null; }
                g.Machine.Command(0x5A, new object[] { "fixture quest", "fixture progress", new object[] { 20000, 400 }, true });
                g.Machine.Command(0x59, new object[] { new object[] { 400 }, new object[] { 207 }, new object[] { 19 }, "taskover.s" });
                g.Inventory.Clear(); g.Inventory.Add(0, 5); g.money = 1234; g.EquipmentBag.Add(EquipmentInstance.Create(0, 2));
                var select = typeof(GuJianGame).GetMethod("SelectEquipment", Private); select.Invoke(g, new object[] { 1 });
                g.Machine.Command(0x68, new object[0]);
                string path = Path.Combine(Application.persistentDataPath, fixture);
                Require(File.Exists(path), "Checkpoint opcode did not create isolated record");
                checks.Add("Original checkpoint opcode writes an isolated record containing quest, inventory, equipment and currency fixtures");
                g.Inventory.Clear(); g.money = 1; select.Invoke(g, new object[] { 0 });
                var key = Press(keyboard, Key.Escape); while (key.MoveNext()) yield return null;
                Require(Get<bool>(g, "menu"), "Keyboard did not open menu");
                // Default mission page -> Right -> system page; Down twice selects records.
                key = Press(keyboard, Key.RightArrow); while (key.MoveNext()) yield return null;
                Require(Get<int>(g, "menuPage") == 3, "Keyboard did not select system tab");
                for (int i = 0; i < 2; i++) { key = Press(keyboard, Key.DownArrow); while (key.MoveNext()) yield return null; }
                Require(Get<int>(g, "atlasSelection") == 2, "Records option was not selected");
                key = Press(keyboard, Key.Enter); while (key.MoveNext()) yield return null;
                Require(Get<object>(g, "systemMenuPage").ToString() == "Records", "Production records page did not open");
                key = Press(keyboard, Key.Enter); while (key.MoveNext()) yield return null;
                Require(Get<bool>(g, "recordOptionOpen"), "Existing record action menu did not open");
                key = Press(keyboard, Key.Enter); while (key.MoveNext()) yield return null;
                // Scene initialization can continue into an original NPC conversation after
                // pending state has been restored; observe it without choosing for the player.
                while (Get<object>(g, "pendingRestore") != null || Get<bool>(g, "loadingVisible")) { Require(g.LastError == null, "Load failed: " + g.LastError); yield return null; }
                var settle = Frames(6); while (settle.MoveNext()) yield return null;
                bool npcDialogueAfterLoad = g.DialogueVisible;
                string continuationScript = g.Machine.CurrentScript;
                var tasks = (IList)Get<object>(g, "tasks"); Require(tasks.Count == 1, "Quest task missing after records load");
                var count = (int[])tasks[0].GetType().GetField("counts", Private | BindingFlags.Public).GetValue(tasks[0]);
                Require(g.money == 1234 && g.ItemCount(0) == 5 && g.EquippedIndex == 1 && g.Equipment[1].reinforcement == 2 &&
                    g.BranchMissionTitle == "fixture quest" && count[0] == 19 && g.Machine.Get("taskID") == 1,
                    "Records menu did not restore saved state");
                checks.Add("Queued keyboard events navigate the original system/records UI and load the isolated record");
                checks.Add("Records UI load restores task progress, medicine, the selected reinforced equipment and currency");
                Require(npcDialogueAfterLoad && continuationScript == "335n0", "City checkpoint did not enter its original scripted NPC continuation");
                checks.Add("City fixture reload enters NPC script 335n0, matching the saved-stage script path in the decompiled original");
                foreach (var file in protectedFiles.Where(x => !x.Key.Contains("gujian-sms-global")))
                    Require(file.Value == null ? !File.Exists(file.Key) : File.Exists(file.Key) && file.Value.SequenceEqual(File.ReadAllBytes(file.Key)), "Formal player record changed: " + Path.GetFileName(file.Key));
                checks.Add("Formal player records and their transaction files remain unchanged");
            }
            finally
            {
                if (g && EditorApplication.isPlaying) { g.Begin(); g.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(g, true); }
                g.saveFileName = oldSave; Time.timeScale = oldScale;
                if (keyboard != null) InputSystem.RemoveDevice(keyboard); if (oldKeyboard != null) oldKeyboard.MakeCurrent();
                InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor;
                foreach (var file in protectedFiles)
                {
                    if (file.Value != null && (!File.Exists(file.Key) || !file.Value.SequenceEqual(File.ReadAllBytes(file.Key)))) File.WriteAllBytes(file.Key, file.Value);
                    else if (file.Value == null && File.Exists(file.Key)) File.Delete(file.Key);
                }
                foreach (var file in protectedFiles)
                    Require(file.Value == null ? !File.Exists(file.Key) : File.Exists(file.Key) && file.Value.SequenceEqual(File.ReadAllBytes(file.Key)), "Failed to restore protected file: " + Path.GetFileName(file.Key));
                checks.Add("Protected global state and formal records match their pre-test bytes after cleanup");
                string slot1 = Path.GetFileNameWithoutExtension(fixture) + "-slot1" + Path.GetExtension(fixture);
                foreach (string baseName in new[] { fixture, slot1 }) foreach (string suffix in new[] { "", ".bak", ".tmp" })
                { string path = Path.Combine(Application.persistentDataPath, baseName + suffix); if (File.Exists(path)) File.Delete(path); }
            }
        }
        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup - started > 45) throw new Exception("Validation timed out or Play stopped");
                if (!work.MoveNext()) Finish(null);
            }
            catch (Exception e) { Finish(e.ToString()); }
        }
        private static void Finish(string error)
        {
            EditorApplication.update -= Tick;
            try { (work as IDisposable)?.Dispose(); }
            catch (Exception cleanup) { error = (error ?? "") + "\nCleanup: " + cleanup; }
            work = null;
            string dir = Path.GetFullPath("../Reference/Validation/2026-10-31-m2"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "record-menu.json"), JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            if (error == null) Debug.Log("GUJIAN_M2_RECORD_MENU_PASSED " + checks.Count); else Debug.LogError("GUJIAN_M2_RECORD_MENU_FAILED " + error);
        }
    }
}
