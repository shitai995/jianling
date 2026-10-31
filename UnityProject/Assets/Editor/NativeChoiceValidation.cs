using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace GuJian.Editor
{
    public static class NativeChoiceValidation
    {
        private static IEnumerator work; private static double started;
        private static readonly List<string> checks = new List<string>();
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        [MenuItem("GuJian/Native UI/Validate choice view")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying && work == null, "Enter Play with no active validation");
            checks.Clear(); started = EditorApplication.timeSinceStartup; work = Steps(); EditorApplication.update += Tick;
        }
        private static IEnumerator Steps()
        {
            var game = Object.FindObjectOfType<GuJianGame>(); var view = Object.FindObjectOfType<NativeChoiceView>();
            Require(game && view && view.ReplacesChoice && AssetDatabase.LoadAssetAtPath<GameObject>(NativeChoiceSetup.PrefabPath), "Prefab or production scene connection missing");
            checks.Add("Native choice prefab is saved and connected in the production scene");
            var oldMouse = Mouse.current; Mouse mouse = null;
            var background = InputSystem.settings.backgroundBehavior; var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            try
            {
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent(); game.Begin(); game.Machine.Clear();
                game.Machine.Command(0x2D, new object[] { "<c=ffff00>Prompt</c>", new object[] { "one", "two", "three", "four" }, false });
                int targetFrame = Time.frameCount + 5; while (Time.frameCount < targetFrame) yield return null;
                var rows = view.GetComponentsInChildren<NativeChoiceRow>();
                var borders = view.GetComponentsInChildren<NativeChoicePanel>();
                Require(rows.Length == 3 && borders.Length == 2 && borders.All(x => x.mainTexture && x.GetComponent<CanvasRenderer>()) && rows[1].GetComponentInChildren<Text>().text == "two", "Native rows or source borders are missing");
                checks.Add("The native view renders three visible rows and original atlas borders");
                game.SubmitAction(GameAction.Down); game.SubmitAction(GameAction.Down); game.SubmitAction(GameAction.Down);
                targetFrame = Time.frameCount + 3; while (Time.frameCount < targetFrame) yield return null;
                Require(game.CurrentChoice.FirstVisible == 1 && rows[2].GetComponentInChildren<Text>().text == "four", "focus scrolling was not reflected in the UI");
                checks.Add("Production focus navigation updates the three-row viewport");
                Vector2 point = RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)rows[2].transform).TransformPoint(((RectTransform)rows[2].transform).rect.center));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
                targetFrame = Time.frameCount + 3; while (Time.frameCount < targetFrame) yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
                double limit = EditorApplication.timeSinceStartup + 4;
                while (game.DialogueVisible && EditorApplication.timeSinceStartup < limit) yield return null;
                Require(!game.DialogueVisible && game.LastChoice == 3, "UI raycast did not select the fourth production option");
                checks.Add("Queued mouse click through the real UI module selects the scrolled fourth option");
                yield return null;
                game.Machine.Command(0x2D, new object[] { "", new object[] { "cancel" }, true });
                targetFrame = Time.frameCount + 3; while (Time.frameCount < targetFrame) yield return null;
                var serialized = new SerializedObject(view);
                var rowProperty = serialized.FindProperty("rows").GetArrayElementAtIndex(0);
                var originalRow = rowProperty.objectReferenceValue;
                try
                {
                    rowProperty.objectReferenceValue = null; serialized.ApplyModifiedPropertiesWithoutUndo();
                    targetFrame = Time.frameCount + 3; while (Time.frameCount < targetFrame) yield return null;
                    Require(!view.ReplacesChoice && !view.transform.Find("Viewport 533x320/Choice").gameObject.activeSelf && game.DialogueVisible,
                        "invalid view did not hide and restore the original fallback");
                }
                finally { rowProperty.objectReferenceValue = originalRow; serialized.ApplyModifiedPropertiesWithoutUndo(); }
                checks.Add("Missing prefab references hide the native panel and restore original rendering");
                view.enabled = false;
                Require(!view.ReplacesChoice && game.DialogueVisible, "disabled native view did not retain the original fallback state");
                view.enabled = true; game.SubmitAction(GameAction.Back);
                Require(!game.DialogueVisible && game.LastChoice == -1, "cancel behavior changed");
                checks.Add("Disabled view retains original fallback and Back cancels a closable choice");
            }
            finally
            {
                if (view) view.enabled = true;
                if (mouse != null) InputSystem.RemoveDevice(mouse);
                if (oldMouse != null) oldMouse.MakeCurrent();
                InputSystem.settings.backgroundBehavior = background; InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
            }
        }
        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup-started > 30) throw new Exception("Play stopped or validation timed out");
                if (!work.MoveNext()) Finish(null);
            }
            catch (Exception e) { Finish(e.ToString()); }
        }
        private static void Finish(string error)
        {
            EditorApplication.update -= Tick; (work as IDisposable)?.Dispose(); work = null;
            string dir = Path.GetFullPath("../Reference/Validation/2026-10-31-m2"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "native-choice.json"), JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            if (error == null) Debug.Log("GUJIAN_NATIVE_CHOICE_PASSED " + checks.Count); else Debug.LogError("GUJIAN_NATIVE_CHOICE_FAILED " + error);
        }
    }
}
