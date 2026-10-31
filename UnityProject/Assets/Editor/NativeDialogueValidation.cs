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
    public static class NativeDialogueValidation
    {
        private static IEnumerator work;
        private static double started;
        private static readonly List<string> checks = new List<string>();
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        [MenuItem("古剑/原生界面/验证 M2 人物对话")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying && work == null, "Enter Play and wait for any previous run");
            checks.Clear(); started = EditorApplication.timeSinceStartup; work = Steps(); EditorApplication.update += Tick;
        }
        private static IEnumerator Steps()
        {
            var game = Object.FindObjectOfType<GuJianGame>(); var view = Object.FindObjectOfType<NativeDialogueView>();
            Require(game && view && view.ReplacesDialogue && AssetDatabase.LoadAssetAtPath<GameObject>(NativeDialogueSetup.PrefabPath).GetComponent<NativeDialogueView>(), "Production scene/prefab is not connected");
            checks.Add("Saved native dialogue prefab is connected to the production scene");
            var oldMouse = Mouse.current; Mouse mouse = null; float originalScale = Time.timeScale;
            var background = InputSystem.settings.backgroundBehavior; var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            try
            {
                Time.timeScale = 0;
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent();
                game.Begin(); game.Machine.Clear();
                game.Machine.Command(0x27, new object[] { "展云飞", "少侠，此去山高路远。<c=ffff00,000000,1>江湖凶险，千万小心。</c>让我们先去藏剑山庄一探究竟。", "", false });
                for (int i = 0; i < 5; i++) yield return null;
                var panel = view.transform.Find("Viewport 533x320/Character Dialogue");
                var frame = panel.Find("Original dialogue frame").GetComponent<RawImage>();
                var texts = panel.GetComponentsInChildren<Text>().Where(x => x.name.StartsWith("Span template")).ToArray();
                Require(game.DialogueVisible && panel.gameObject.activeInHierarchy && frame.texture && texts.Length > 1 && texts.Any(x => x.color.r > .9f && x.color.g > .9f && x.color.b < .1f), "Frame, formatted spans or dialogue state missing");
                checks.Add("Original character command displays the original frame and formatted colored spans in native UI");
                var startField = typeof(GuJianGame).GetField("characterStartLine", BindingFlags.Instance | BindingFlags.NonPublic);
                int first = (int)startField.GetValue(game);
                Vector2 point = RectTransformUtility.WorldToScreenPoint(null, frame.rectTransform.TransformPoint(frame.rectTransform.rect.center));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
                for (int i = 0; i < 3; i++) yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
                double limit = EditorApplication.timeSinceStartup + 4;
                while ((int)startField.GetValue(game) == first && EditorApplication.timeSinceStartup < limit) yield return null;
                Require((int)startField.GetValue(game) > first && game.DialogueVisible, "Real UI pointer routing did not advance exactly to the next page");
                checks.Add("Queued pointer press/release passes through the Input System UI raycaster and advances a multipage dialogue");
                for (int i = 0; i < 4; i++) yield return null;
                game.SubmitAction(GameAction.Primary);
                Require(!game.DialogueVisible, "Last page did not close through the shared production operation");
                Require(!game.SubmitAction(GameAction.Skill), "Closing dialogue leaked a same-frame gameplay action");
                checks.Add("Final confirmation closes the dialogue and blocks same-frame gameplay leakage");
                yield return null;
                game.Machine.Command(0x27, new object[] { "古小天", "左侧立绘", "f_00", true });
                for (int i = 0; i < 3; i++) yield return null;
                var portrait = panel.Find("Portrait").GetComponent<RawImage>();
                Require(portrait.gameObject.activeInHierarchy && portrait.texture && frame.uvRect.width < 0 && portrait.rectTransform.anchoredPosition.x == 132,
                    "Left portrait or mirrored source frame is wrong");
                game.ContinueDialogue(); yield return null;
                game.Machine.Command(0x27, new object[] { "古小天", "", "f_00", false });
                for (int i = 0; i < 3; i++) yield return null;
                Require(portrait.gameObject.activeInHierarchy && frame.uvRect.width > 0 && game.DialogueVisible,
                    "Right portrait or empty dialogue is wrong");
                checks.Add("Original portrait renders on either side with corresponding frame orientation; empty text remains dismissible");
                game.ContinueDialogue(); yield return null;
                game.Machine.Command(0x27, new object[] { "测试", "回退测试", "", false });
                for (int i = 0; i < 3; i++) yield return null;
                // Freeze the game controller while toggling synthetic states; these flags normally
                // carry additional rendering/VM state initialized by their real transitions.
                game.enabled = false;
                foreach (string fieldName in new[] { "loadingVisible", "originalBlackScreen", "miniMapOpen", "menu" })
                {
                    var field = typeof(GuJianGame).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                    bool previous = (bool)field.GetValue(game);
                    Require(!previous, fieldName + " was already active before overlay test");
                    try
                    {
                        field.SetValue(game, true);
                        for (int i = 0; i < 3; i++) yield return null;
                        Require(!panel.gameObject.activeSelf && game.DialogueVisible, fieldName + " did not hide the native panel while preserving dialogue");
                    }
                    finally { field.SetValue(game, previous); }
                    for (int i = 0; i < 3; i++) yield return null;
                    Require(panel.gameObject.activeSelf, fieldName + " did not restore the native panel");
                }
                checks.Add("Loading, black screen, minimap and menu suppress the dialogue canvas and restore it afterward");
                game.enabled = true;
                view.enabled = false;
                Require(!panel.gameObject.activeSelf && !view.ReplacesDialogue && game.DialogueVisible, "Disabling the native view did not restore IMGUI eligibility");
                view.enabled = true;
                checks.Add("Disabling the native view hides its panel and leaves dialogue state available for the original renderer");
                game.ContinueDialogue();
            }
            finally
            {
                Time.timeScale = originalScale;
                if (game) game.enabled = true;
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
                if (!EditorApplication.isPlaying) throw new Exception("Play mode stopped during validation");
                if (EditorApplication.timeSinceStartup - started > 30) throw new Exception("Timed out");
                if (!work.MoveNext()) Finish(null);
            }
            catch (Exception e) { Finish(e.ToString()); }
        }
        private static void Finish(string error)
        {
            EditorApplication.update -= Tick;
            (work as IDisposable)?.Dispose(); work = null;
            string directory = Path.GetFullPath("../Reference/Validation/2026-10-31-m2"); Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "character-dialogue.json"), JsonUtility.ToJson(
                new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            if (error == null) Debug.Log("GUJIAN_NATIVE_DIALOGUE_VALIDATION_PASSED " + checks.Count);
            else Debug.LogError("GUJIAN_NATIVE_DIALOGUE_VALIDATION_FAILED " + error);
        }
    }
}
