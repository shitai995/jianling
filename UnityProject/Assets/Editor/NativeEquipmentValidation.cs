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
    public static class NativeEquipmentValidation
    {
        private static IEnumerator work; private static double started; private static readonly List<string> checks = new List<string>();
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        [MenuItem("GuJian/Native UI/Validate equipment")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying && work == null, "Enter Play with no active validation");
            checks.Clear(); started = EditorApplication.timeSinceStartup; work = Steps(); EditorApplication.update += Tick;
        }
        private static IEnumerator WaitFrames(int count) { int end = Time.frameCount + count; while (Time.frameCount < end) yield return null; }
        private static IEnumerator Click(Mouse mouse, RectTransform rect)
        {
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
            var frames = WaitFrames(3); while (frames.MoveNext()) yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
            frames = WaitFrames(3); while (frames.MoveNext()) yield return null;
        }
        private static IEnumerator Steps()
        {
            var game = Object.FindObjectOfType<GuJianGame>(); var view = Object.FindObjectOfType<NativeEquipmentView>();
            Require(game && view && view.ReplacesEquipment && AssetDatabase.LoadAssetAtPath<GameObject>(NativeEquipmentSetup.PrefabPath), "Native equipment is not connected");
            checks.Add("Equipment prefab is saved and connected to the production scene");
            var oldMouse = Mouse.current; var oldKeyboard = Keyboard.current; Mouse mouse = null; Keyboard keyboard = null;
            var background = InputSystem.settings.backgroundBehavior; var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            try
            {
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent(); game.Begin(); game.Machine.Clear(); game.level = 1;
                game.EquipmentBag.Add(EquipmentInstance.Create(0, 2));
                var locked = OriginalAssets.Table<EquipmentDefinition>("equip").First(e => e.level > game.level && !string.IsNullOrEmpty(e.image));
                game.EquipmentBag.Add(EquipmentInstance.Create(locked.id));
                typeof(GuJianGame).GetField("menu", Private).SetValue(game, true); Require(game.SelectMenuPage(2), "Cannot open equipment page");
                var frames = WaitFrames(5); while (frames.MoveNext()) yield return null;
                var rows = view.GetComponentsInChildren<NativeEquipmentRow>();
                Require(rows.Length == 3 && rows[1].GetComponentInChildren<Text>().text.Contains("+2"), "Equipment rows or reinforcement missing");
                Require(view.transform.Find("Viewport 533x320/Equipment content/Atlas 320x240/Character preview").GetComponentsInChildren<RawImage>().Length > 0, "Character preview absent");
                checks.Add("Original atlas, reinforced equipment rows and character preview are visible");
                var click = Click(mouse, (RectTransform)rows[1].transform); while (click.MoveNext()) yield return null;
                Require((int)typeof(GuJianGame).GetField("atlasSelection", Private).GetValue(game) == 1, "Second equipment row not selected");
                var attack = view.GetComponentsInChildren<Text>().First(x => x.name == "Attribute 1");
                Require(attack.text == game.Equipment[1].Attack.ToString(), "Selected equipment attributes are stale");
                checks.Add("Pointer selection updates the selected equipment and its production attributes");
                var equip = view.GetComponentsInChildren<Button>().First(b => b.name == "Equip");
                click = Click(mouse, (RectTransform)equip.transform); while (click.MoveNext()) yield return null;
                Require(game.EquippedIndex == 1 && game.Equipment[1].reinforcement == 2, "Equipment activation did not select the actual instance");
                checks.Add("Native Equip selects the actual reinforced instance through production rules");
                click = Click(mouse, (RectTransform)rows[2].transform); while (click.MoveNext()) yield return null;
                click = Click(mouse, (RectTransform)equip.transform); while (click.MoveNext()) yield return null;
                Require(game.EquippedIndex == 1, "Equipment above player level was equipped");
                checks.Add("Equipment above the player level cannot be equipped");
                keyboard = InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.DownArrow));
                frames = WaitFrames(3); while (frames.MoveNext()) yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                frames = WaitFrames(3); while (frames.MoveNext()) yield return null;
                Require((int)typeof(GuJianGame).GetField("atlasSelection", Private).GetValue(game) == 0, "Queued keyboard navigation did not wrap selection");
                checks.Add("Queued keyboard input reaches the original equipment navigation");
                click = Click(mouse, (RectTransform)rows[1].transform); while (click.MoveNext()) yield return null;
                Require((int)typeof(GuJianGame).GetField("atlasSelection", Private).GetValue(game) == 1, "Pointer selection failed after keyboard navigation");
                checks.Add("Pointer selection continues to work after keyboard navigation");
                for (int i = 0; i < 7; i++) game.EquipmentBag.Add(EquipmentInstance.Create(0));
                for (int i = 0; i < 6; i++) game.PressControl(1);
                frames = WaitFrames(4); while (frames.MoveNext()) yield return null;
                var content = (RectTransform)view.transform.Find("Viewport 533x320/Equipment content/Atlas 320x240/Equipment viewport/Equipment");
                Require(content.anchoredPosition.y > 0, "Focused equipment did not scroll into view");
                checks.Add("Directional menu actions scroll a long equipment list to its focus");
                view.enabled = false; Require(!view.ReplacesEquipment && !view.transform.Find("Viewport 533x320/Equipment content").gameObject.activeSelf, "Disabled view did not restore fallback");
                view.enabled = true; frames = WaitFrames(3); while (frames.MoveNext()) yield return null;
                var tab = view.GetComponentsInChildren<Button>().First(b => b.name == "Tab 2");
                click = Click(mouse, (RectTransform)tab.transform); while (click.MoveNext()) yield return null;
                Require((int)typeof(GuJianGame).GetField("menuPage", Private).GetValue(game) == 1 && !view.transform.Find("Viewport 533x320/Equipment content").gameObject.activeSelf, "Inventory tab did not transfer");
                checks.Add("Disabling restores the old renderer; the inventory tab switches pages");
            }
            finally
            {
                if (view) view.enabled = true;
                if (mouse != null) InputSystem.RemoveDevice(mouse); if (oldMouse != null) oldMouse.MakeCurrent();
                if (keyboard != null) InputSystem.RemoveDevice(keyboard); if (oldKeyboard != null) oldKeyboard.MakeCurrent();
                InputSystem.settings.backgroundBehavior = background; InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
            }
        }
        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup - started > 30) throw new Exception("Play stopped or validation timed out");
                if (!work.MoveNext()) Finish(null);
            }
            catch (Exception e) { Finish(e.ToString()); }
        }
        private static void Finish(string error)
        {
            EditorApplication.update -= Tick; (work as IDisposable)?.Dispose(); work = null;
            string dir = Path.GetFullPath("../Reference/Validation/2026-10-31-m2"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "native-equipment.json"), JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            if (error == null) Debug.Log("GUJIAN_NATIVE_EQUIPMENT_PASSED " + checks.Count); else Debug.LogError("GUJIAN_NATIVE_EQUIPMENT_FAILED " + error);
        }
    }
}
