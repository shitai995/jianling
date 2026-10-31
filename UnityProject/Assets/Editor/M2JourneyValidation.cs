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
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace GuJian.Editor
{
    public static class M2JourneyValidation
    {
        private static IEnumerator work; private static double started;
        private static readonly List<string> checks = new List<string>();
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static T Get<T>(GuJianGame g, string field) => (T)typeof(GuJianGame).GetField(field, Private).GetValue(g);
        private static IEnumerator WaitFrames(int count) { int end = Time.frameCount + count; while (Time.frameCount < end) yield return null; }
        private static IEnumerator Click(Mouse mouse, RectTransform rect)
        {
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
            var wait = WaitFrames(3); while (wait.MoveNext()) yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
            wait = WaitFrames(3); while (wait.MoveNext()) yield return null;
        }
        [MenuItem("GuJian/Native UI/Validate quest and battle")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying && work == null, "Enter Play with no active validation");
            checks.Clear(); started = EditorApplication.timeSinceStartup; work = Steps(); EditorApplication.update += Tick;
        }
        private static IEnumerator Steps()
        {
            var g = Object.FindObjectOfType<GuJianGame>(); var dialogue = Object.FindObjectOfType<NativeDialogueView>(); var choice = Object.FindObjectOfType<NativeChoiceView>();
            Require(g && dialogue && dialogue.ReplacesDialogue && choice && choice.ReplacesChoice, "Native views or game missing");
            var oldMouse = Mouse.current; Mouse mouse = null; float oldScale = Time.timeScale;
            string oldSave = g.saveFileName, fixture = "validation-m2-journey-" + Guid.NewGuid().ToString("N") + ".json";
            var recordPath = typeof(GuJianGame).GetMethod("RecordPath", Private);
            var protectedFiles = new Dictionary<string, byte[]>();
            foreach (string path in new[] { (string)recordPath.Invoke(g, new object[] { 0 }), (string)recordPath.Invoke(g, new object[] { 1 }), Path.Combine(Application.persistentDataPath, "gujian-sms-global.json") })
                foreach (string name in new[] { path, path + ".bak", path + ".tmp" }) protectedFiles[name] = File.Exists(name) ? File.ReadAllBytes(name) : null;
            var oldBackground = InputSystem.settings.backgroundBehavior; var oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
            try
            {
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent(); Time.timeScale = 1;
                g.saveFileName = fixture; g.Begin(); g.Machine.Clear();
                // Fixture starts at the original NPC's city with a level appropriate to its enemies.
                g.Machine.Values["event"] = 999; g.Machine.Values["taskID"] = -1; g.Machine.Values["days"] = 1; g.Machine.Values["lucky"] = 0;
                g.Machine.Values["stageID"] = 334; g.Machine.Enqueue("335");
                while (g.Machine.Running || Get<bool>(g, "loadingVisible")) { Require(g.LastError == null, "City script: " + g.LastError); yield return null; }
                Require(g.Actors.ContainsKey(89) && g.Actors.ContainsKey(-1), "Original quest NPC unavailable");
                g.Actors[-1].mapPosition = g.Actors[89].mapPosition + new Vector2(8, 0);
                Require(g.SubmitAction(GameAction.Interact), "Production interaction was rejected");
                int characterClicks = 0, choiceClicks = 0, directUiDispatches = 0;
                while (string.IsNullOrEmpty(g.BranchMissionTitle) || g.Machine.Running || g.DialogueVisible)
                {
                    Require(g.LastError == null, "Quest script: " + g.LastError);
                    if (g.DialogueVisible)
                    {
                        if (g.DialogueChoices != null)
                        {
                            var options = g.DialogueChoices; int index = Array.FindIndex(options, x => x == "\u6d88\u706d\u502d\u5bc71");
                            if (index < 0) index = Array.FindIndex(options, x => x == "\u662f");
                            Require(index >= 0 && index < 3, "Unexpected quest choice: " + string.Join(",", options));
                            var wait = WaitFrames(3); while (wait.MoveNext()) yield return null;
                            var row = choice.GetComponentsInChildren<NativeChoiceRow>().FirstOrDefault(r => r.name == "Option " + (index + 1));
                            Require(row, "Native quest option unavailable");
                            string script = g.Machine.CurrentScript; int instruction = g.Machine.Instruction;
                            var click = Click(mouse, (RectTransform)row.transform); while (click.MoveNext()) yield return null;
                            if (g.DialogueChoices != null && g.Machine.CurrentScript == script && g.Machine.Instruction == instruction)
                            {
                                UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(row.gameObject,
                                    new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current), UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                                directUiDispatches++;
                            }
                            choiceClicks++;
                        }
                        else if (Get<bool>(g, "originalMessage"))
                        { g.SubmitAction(GameAction.Primary); var wait = WaitFrames(3); while (wait.MoveNext()) yield return null; }
                        else
                        {
                            var wait = WaitFrames(3); while (wait.MoveNext()) yield return null;
                            var frame = dialogue.transform.Find("Viewport 533x320/Character Dialogue/Original dialogue frame") as RectTransform;
                            Require(frame && frame.gameObject.activeInHierarchy, "Native NPC dialogue unavailable");
                            string script = g.Machine.CurrentScript; int instruction = g.Machine.Instruction; int line = Get<int>(g, "characterStartLine");
                            var click = Click(mouse, frame); while (click.MoveNext()) yield return null;
                            if (g.DialogueVisible && g.DialogueChoices == null && !Get<bool>(g, "originalMessage") && g.Machine.CurrentScript == script && g.Machine.Instruction == instruction && Get<int>(g, "characterStartLine") == line)
                            {
                                UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(frame.gameObject,
                                    new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current), UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                                directUiDispatches++;
                            }
                            characterClicks++;
                        }
                    }
                    else yield return null;
                }
                Require(g.Machine.Get("taskID") == 1 && g.MissionReward(true, 0) == 20000 && g.MissionReward(true, 1) == 400 && characterClicks > 0 && choiceClicks >= 2,
                    "Actual NPC quest acceptance did not preserve task/reward state");
                checks.Add("Original city NPC -> native character dialogue and two native choices accepts task 1 with original 20-enemy objective and advertised rewards; direct UI event fallback count=" + directUiDispatches);
                // A separate higher-level fixture tests integration plumbing, not level-10 balance.
                g.Machine.Command(0x4D, new object[] { 30 });
                g.EquipmentBag.Add(EquipmentInstance.Create(7));
                Require(g.SubmitAction(GameAction.Back) && g.SelectMenuPage(2), "Cannot open equipment fixture");
                var equipmentView = Object.FindObjectOfType<NativeEquipmentView>();
                var equipWait = WaitFrames(5); while (equipWait.MoveNext()) yield return null;
                var equipRows = equipmentView.GetComponentsInChildren<NativeEquipmentRow>();
                Require(equipRows.Length >= 2, "Native equipment rows missing");
                var equipClick = Click(mouse, (RectTransform)equipRows[1].transform); while (equipClick.MoveNext()) yield return null;
                if (Get<int>(g, "atlasSelection") != 1)
                {
                    UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(equipRows[1].gameObject,
                        new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current), UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                    directUiDispatches++;
                }
                var equipButton = equipmentView.GetComponentsInChildren<Button>().First(b => b.name == "Equip");
                equipClick = Click(mouse, (RectTransform)equipButton.transform); while (equipClick.MoveNext()) yield return null;
                if (g.EquippedIndex != 1)
                {
                    UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(equipButton.gameObject,
                        new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current), UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                    directUiDispatches++;
                }
                Require(g.EquippedIndex == 1 && g.Equipment[1].id == 7, "Native equipment fixture did not equip the selected sword");
                Require(g.CloseMenu(), "Cannot close equipment fixture");
                checks.Add("Native equipment UI selects and equips a fixture-granted level-27 sword for a level-30 integration character; cumulative direct UI event fallbacks=" + directUiDispatches);
                g.Machine.Values["stageID"] = 1; g.Machine.Enqueue("400");
                while (g.Machine.Running || Get<bool>(g, "loadingVisible")) { Require(g.LastError == null, "Battle stage: " + g.LastError); yield return null; }
                Require(g.Machine.Get("stageID") == 400 && g.Actors.Values.Any(a => a.IsEnemy && a.definitionId == 207), "Original stage 400 enemies unavailable");
                var task = ((IList)Get<object>(g, "tasks"))[0]; var field = task.GetType().GetField("counts", Private | BindingFlags.Public);
                int before = ((int[])field.GetValue(task))[0]; Require(before == 20, "Quest counter did not start at 20");
                double limit = EditorApplication.timeSinceStartup + 50, nextAttack = 0; int attempts = 0;
                while (((int[])field.GetValue(task))[0] == before && EditorApplication.timeSinceStartup < limit)
                {
                    Require(g.CurrentHp > 0 && g.LastError == null, "Player died or game error during unmodified combat");
                    var target = g.Actors.Values.FirstOrDefault(a => a.IsEnemy && a.definitionId == 207 && !a.hidden && a.hp > 0);
                    if (target && EditorApplication.timeSinceStartup >= nextAttack && g.Actors[-1].LogicalAction <= 1 && !g.Actors[-1].locked)
                    {
                        var role = g.Actors[-1]; role.mapPosition = target.mapPosition + new Vector2(-36, target.Animation.footHeight - role.Animation.footHeight);
                        role.Face("r"); g.SubmitAction(GameAction.Primary); attempts++; nextAttack = EditorApplication.timeSinceStartup + .3;
                    }
                    yield return null;
                }
                int remaining = ((int[])field.GetValue(task))[0];
                Require(remaining < before, "Natural AI battle did not produce a quest kill after " + attempts + " player attack attempts");
                checks.Add("Level-30 fixture versus level-10 enemies with original HP, damage and active AI: a production primary attack reduced the quest counter to " + remaining + " (fixture repositions player)");
                // Save to a unique disposable slot and check the in-progress objective after the production load path.
                int money = g.money, inventory = g.ItemCount(0); g.Save(); g.Load();
                while (g.Machine.Running || Get<bool>(g, "loadingVisible")) { Require(g.LastError == null, "Restore: " + g.LastError); yield return null; }
                var restored = ((IList)Get<object>(g, "tasks"))[0];
                Require(((int[])restored.GetType().GetField("counts", Private | BindingFlags.Public).GetValue(restored))[0] == remaining && g.Machine.Get("taskID") == 1 && g.money == money && g.ItemCount(0) == inventory,
                    "In-progress quest or inventory did not survive Save/Load");
                checks.Add("Unique-slot production Save/Load restores quest progress, currency and inventory after combat");
                foreach (var file in protectedFiles)
                    Require(file.Value == null ? !File.Exists(file.Key) : File.Exists(file.Key) && file.Value.SequenceEqual(File.ReadAllBytes(file.Key)), "Formal save changed: " + Path.GetFileName(file.Key));
                checks.Add("Formal player records and global state remain byte-for-byte unchanged");
            }
            finally
            {
                // Do not leave fixture progress attached to the user's normal save slot.
                if (g && EditorApplication.isPlaying)
                {
                    g.Begin(); g.Machine.Clear();
                    typeof(GuJianGame).GetProperty("AtTitle").SetValue(g, true);
                }
                g.saveFileName = oldSave; Time.timeScale = oldScale;
                if (mouse != null) InputSystem.RemoveDevice(mouse); if (oldMouse != null) oldMouse.MakeCurrent();
                InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor;
                foreach (var file in protectedFiles)
                {
                    if (file.Value != null && (!File.Exists(file.Key) || !file.Value.SequenceEqual(File.ReadAllBytes(file.Key)))) File.WriteAllBytes(file.Key, file.Value);
                    else if (file.Value == null && File.Exists(file.Key)) File.Delete(file.Key);
                }
                foreach (string name in new[] { fixture, fixture + ".bak", fixture + ".tmp", Path.GetFileNameWithoutExtension(fixture) + "-slot1.json" })
                { string path = Path.Combine(Application.persistentDataPath, name); if (File.Exists(path)) File.Delete(path); }
            }
        }
        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup - started > 120) throw new Exception("Journey validation timed out or Play stopped");
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
            File.WriteAllText(Path.Combine(dir, "quest-battle.json"), JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            if (error == null) Debug.Log("GUJIAN_M2_QUEST_BATTLE_PASSED " + checks.Count); else Debug.LogError("GUJIAN_M2_QUEST_BATTLE_FAILED " + error);
        }
    }
}
