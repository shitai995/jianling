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
    public static class NativeInventoryValidation
    {
        private static IEnumerator work; private static double started; private static readonly List<string> checks = new List<string>();
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        [MenuItem("GuJian/Native UI/Validate inventory")]
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
            var game = Object.FindObjectOfType<GuJianGame>(); var view = Object.FindObjectOfType<NativeInventoryView>();
            Require(game && view && view.ReplacesInventory && AssetDatabase.LoadAssetAtPath<GameObject>(NativeInventorySetup.PrefabPath), "Native inventory is not connected");
            checks.Add("Inventory prefab is saved and connected in the production scene");
            var oldMouse = Mouse.current; Mouse mouse = null;
            var oldBackground = InputSystem.settings.backgroundBehavior; var oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
            try
            {
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent(); game.Begin(); game.Machine.Clear(); game.Inventory.Clear();
                game.Inventory.Add(0, 103); game.Inventory.Add(1, 3); game.Actors[-1].hp = 1;
                typeof(GuJianGame).GetField("menu", Private).SetValue(game, true); Require(game.SelectMenuPage(1), "Cannot open inventory");
                var frames = WaitFrames(5); while (frames.MoveNext()) yield return null;
                var rows = view.GetComponentsInChildren<NativeInventoryRow>();
                Require(rows.Length == 3 && rows[0].GetComponentInChildren<Text>().text.Contains("99") && rows[1].GetComponentInChildren<Text>().text.Contains("4"), "Stack rows missing or incorrectly split");
                Require(view.GetComponentsInChildren<RawImage>().Any(x => x.name == "Atlas piece" && x.texture), "Original atlas frame is absent");
                checks.Add("Native inventory shows original atlas and correctly split 99-item stack rows");
                var click = Click(mouse, (RectTransform)rows[1].transform); while (click.MoveNext()) yield return null;
                int selected = (int)typeof(GuJianGame).GetField("atlasSelection", Private).GetValue(game);
                Require(selected == 1, "Native row click did not select the second stack");
                checks.Add("A pointer click through the UI module selects the second item stack");
                var action = view.GetComponentsInChildren<Button>().First(b => b.name == "Activate");
                click = Click(mouse, (RectTransform)action.transform); while (click.MoveNext()) yield return null;
                var itemAction = (InventoryItemAction)typeof(GuJianGame).GetField("itemAction", Private).GetValue(game);
                Require(itemAction.IsOpen && itemAction.ItemId == 0 && itemAction.Stack == 1, "Native activation did not open original item confirmation for the selected stack");
                Require(!view.transform.Find("Viewport 533x320/Inventory content").gameObject.activeSelf, "Native panel covered the original confirmation");
                checks.Add("Native activation opens the original confirmation and yields rendering to it");
                game.PressControl(4);
                Require(!itemAction.IsOpen && game.ItemCount(0) == 102 && game.Actors[-1].hp > 1, "Original confirmation did not consume the selected stack and heal");
                checks.Add("Original confirmation consumes one item and applies healing through production rules");
                frames = WaitFrames(4); while (frames.MoveNext()) yield return null;
                Require(view.transform.Find("Viewport 533x320/Inventory content").gameObject.activeSelf, "Inventory did not return after confirmation");
                int prior = game.AutoRecoverPercent;
                var auto = view.GetComponentsInChildren<Button>().First(b => b.name == "Auto medicine");
                click = Click(mouse, (RectTransform)auto.transform); while (click.MoveNext()) yield return null;
                Require(game.AutoRecoverPercent == (prior == 90 ? 10 : prior + 10), "Auto medicine threshold did not change");
                checks.Add("Auto medicine threshold cycles through the production setting");
                view.enabled = false; Require(!view.ReplacesInventory && !view.transform.Find("Viewport 533x320/Inventory content").gameObject.activeSelf, "Disabled view did not return to original menu");
                view.enabled = true; checks.Add("Disabled native inventory hides and leaves the old atlas renderer available");
                frames = WaitFrames(4); while (frames.MoveNext()) yield return null;
                for (int id = 2; id <= 7; id++) game.Inventory.Add(id, 1);
                for (int i = 0; i < 7; i++) game.PressControl(1);
                frames = WaitFrames(4); while (frames.MoveNext()) yield return null;
                var content = (RectTransform)view.transform.Find("Viewport 533x320/Inventory content/Atlas 320x240/Items viewport/Items");
                Require(content.anchoredPosition.y > 0, "Focused last row is outside the scroll viewport");
                checks.Add("Keyboard navigation scrolls the native list to the focused item");
                var tab = view.GetComponentsInChildren<Button>().First(b => b.name == "Tab 1");
                click = Click(mouse, (RectTransform)tab.transform); while (click.MoveNext()) yield return null;
                Require((int)typeof(GuJianGame).GetField("menuPage", Private).GetValue(game) == 2 && !view.transform.Find("Viewport 533x320/Inventory content").gameObject.activeSelf, "Equipment tab failed to transfer to original page");
                Require(game.SelectMenuPage(1), "Cannot return to inventory"); frames = WaitFrames(4); while (frames.MoveNext()) yield return null;
                var back = view.GetComponentsInChildren<Button>().First(b => b.name == "Back");
                click = Click(mouse, (RectTransform)back.transform); while (click.MoveNext()) yield return null;
                Require(!(bool)typeof(GuJianGame).GetField("menu", Private).GetValue(game), "Native Back did not close the menu");
                checks.Add("Native tab opens the existing equipment page and native Back closes the menu");
            }
            finally
            {
                if (view) view.enabled = true;
                if (mouse != null) InputSystem.RemoveDevice(mouse); if (oldMouse != null) oldMouse.MakeCurrent();
                InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor;
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
            File.WriteAllText(Path.Combine(dir, "native-inventory.json"), JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            if (error == null) Debug.Log("GUJIAN_NATIVE_INVENTORY_PASSED " + checks.Count); else Debug.LogError("GUJIAN_NATIVE_INVENTORY_FAILED " + error);
        }
    }
}
