using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using Object = UnityEngine.Object;

namespace GuJian.Editor
{
    public static class NativeTitleValidation
    {
        private static IEnumerator work;
        private static GuJianGame game;
        private static NativeTitleView view;
        private static string originalSave, fixture;
        private static float originalScale;
        private static double started;
        private static readonly List<string> checks = new List<string>();
        private static readonly Dictionary<string, byte[]> protectedFiles = new Dictionary<string, byte[]>();
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        private static void Require(bool ok, string message) { if (!ok) throw new Exception(message); }
        private static void Title() => typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
        private static void Submit(UnityEngine.UI.Button button) =>
            ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);

        [MenuItem("古剑/原生界面/验证 M1 输入与标题")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying && work == null, "Enter Play; no other M1 run may be active");
            game = Object.FindObjectOfType<GuJianGame>(); view = Object.FindObjectOfType<NativeTitleView>();
            Require(game && view && view.ReplacesTitle, "Native title must be connected to the production scene");
            originalSave = game.saveFileName; originalScale = Time.timeScale;
            fixture = "validation-m1-" + Guid.NewGuid().ToString("N") + ".json";
            protectedFiles.Clear(); checks.Clear();
            var recordPath = typeof(GuJianGame).GetMethod("RecordPath", Private);
            foreach (string path in new[] { (string)recordPath.Invoke(game, new object[] { 0 }),
                (string)recordPath.Invoke(game, new object[] { 1 }),
                Path.Combine(Application.persistentDataPath, "gujian-sms-global.json") })
                foreach (string file in new[] { path, path + ".bak", path + ".tmp" })
                    protectedFiles[file] = File.Exists(file) ? File.ReadAllBytes(file) : null;
            game.saveFileName = fixture; Time.timeScale = 0;
            started = EditorApplication.timeSinceStartup; work = Steps(); EditorApplication.update += Tick;
        }

        private static IEnumerator Steps()
        {
            var devices = CheckDevices();
            using (devices as IDisposable)
                while (devices.MoveNext()) yield return devices.Current;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NativeTitleSetup.PrefabPath);
            Require(prefab && prefab.GetComponent<NativeTitleView>() &&
                prefab.GetComponentInChildren<UnityEngine.UI.RawImage>(true).texture &&
                EditorUtility.IsPersistent(prefab.GetComponentInChildren<UnityEngine.UI.RawImage>(true).texture), "Title prefab or source art missing");
            var module = view.GetComponentInChildren<InputSystemUIInputModule>(true);
            Require(module && EditorUtility.IsPersistent(module.actionsAsset) && EditorUtility.IsPersistent(module.submit) &&
                EditorUtility.IsPersistent(module.point) && EditorUtility.IsPersistent(module.leftClick), "Input actions are transient");
            checks.Add("Title, original artwork and Input System UI actions are persistent assets, connected in the startup scene");
            var fontProbe = new GameObject("Inactive native font probe"); fontProbe.SetActive(false);
            try { Require(fontProbe.AddComponent<GuJianGame>().InterfaceFont != null, "Native font loading requires OnGUI"); }
            finally { Object.DestroyImmediate(fontProbe); }
            checks.Add("A fresh game instance provides its shared font outside OnGUI without creating IMGUI styles");

            Title(); for (int i = 0; i < 4; i++) yield return null;
            Require(!game.CanContinue && !view.ContinueButton.interactable, "Empty slot must disable Continue");
            Submit(view.ContinueButton);
            Require(game.AtTitle && !File.Exists(Path.Combine(Application.persistentDataPath, fixture)), "Disabled Continue changed the game or wrote a save");
            checks.Add("An empty isolated slot disables Continue; submitting the disabled native button does not start or write a game");

            Submit(view.NewGameButton);
            Require(!game.AtTitle && game.Machine.Running, "Native New Game did not start the production story VM");
            game.money = 321;
            Require(!game.SubmitAction(GameAction.NewGame) && game.money == 321, "An in-game New Game command reset progress");
            Require(!game.SubmitAction((GameAction)(-99)), "Unknown command accepted");
            checks.Add("Native New Game reaches the shared production operation; title-only and unknown actions cannot reset a running game");

            game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
            game.Machine.Enqueue("200"); game.Machine.Tick();
            foreach (var actor in game.Actors.Values) actor.locked = true;
            game.money = 8765; game.Inventory.Add(0, 3); game.Save(true);
            string path = Path.Combine(Application.persistentDataPath, fixture);
            Require(File.Exists(path) && File.ReadAllText(path).Contains("\"version\": 3"), "Version3 production save missing");
            Title(); for (int i = 0; i < 4; i++) yield return null;
            Require(view.ContinueButton.interactable, "Valid save did not enable Continue");
            Submit(view.ContinueButton); game.Machine.Tick();
            Require(!game.AtTitle && game.money == 8765 && game.Inventory.Count(0) == 3 && game.LastError == null, "Native Continue failed to restore production state");
            checks.Add("Native Continue enables for a real version3 save and restores stage, 8765 money and three inventory items through production Load");

            game.Machine.Clear();
            game.Machine.Command(0x2D, new object[] { "请选择", new object[] { "第一项", "第二项" }, false });
            game.SubmitAction(GameAction.Back);
            Require(game.DialogueVisible && game.LastChoice == -1, "Nonclosable choice leaked Back into gameplay");
            game.SubmitAction(GameAction.Down); game.SubmitAction(GameAction.Primary);
            Require(!game.DialogueVisible && game.LastChoice == 1, "Typed direction/confirm did not choose the second entry");
            Require(!game.SubmitAction(GameAction.Skill), "Closing a modal let the same-frame action leak into gameplay");
            checks.Add("Typed actions respect nonclosable choice navigation, confirm the selected entry and block same-frame gameplay after closing a modal");
            yield return null;
            game.Machine.Command(0x6A, new object[] { 6 });
            game.PressControl(6);
            Require(game.TutorialKey == -1 && !(bool)game.Machine.Command(0x6B, new object[0]), "Legacy tutorial Return code changed");
            checks.Add("The original numeric tutorial Return control remains accepted by the typed operation gateway");

            var nativeRoutes = CheckNativeRoutes(path);
            using (nativeRoutes as IDisposable)
                while (nativeRoutes.MoveNext()) yield return nativeRoutes.Current;

            Title(); File.WriteAllText(path, "{invalid json");
            Require(!game.SubmitAction(GameAction.ContinueGame) && game.AtTitle && !string.IsNullOrEmpty(game.LastActionError),
                "Corrupt title save should fail safely with a visible error");
            Require(File.ReadAllText(path) == "{invalid json", "A failed Continue rewrote the corrupted file");
            checks.Add("Corrupt Continue stays at the title, reports a user-facing error and preserves the unreadable file");

            foreach (var file in protectedFiles.Where(p => !p.Key.Contains("gujian-sms-global")))
                Require(file.Value == null ? !File.Exists(file.Key) : File.Exists(file.Key) && file.Value.SequenceEqual(File.ReadAllBytes(file.Key)),
                    "A formal player slot or its backup was changed: " + Path.GetFileName(file.Key));
            checks.Add("Both formal player slots and their transaction files remain byte-for-byte unchanged");
        }

        private static IEnumerator CheckNativeRoutes(string path)
        {
            string validSave = File.ReadAllText(path);
            var oldKeyboard = Keyboard.current; var oldMouse = Mouse.current;
            var background = InputSystem.settings.backgroundBehavior;
            var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            Keyboard keyboard = null; Mouse mouse = null;
            try
            {
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
                keyboard.MakeCurrent(); mouse.MakeCurrent();
                game.Machine.Clear(); Title();
                for (int i = 0; i < 4; i++) yield return null;
                var rect = view.NewGameButton.GetComponent<RectTransform>();
                Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
                for (int i = 0; i < 3; i++) yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
                double limit = EditorApplication.timeSinceStartup + 4;
                while (game.AtTitle && EditorApplication.timeSinceStartup < limit) yield return null;
                Require(!game.AtTitle && game.Machine.Running, "Queued pointer did not reach the native title button");
                checks.Add("A queued mouse press/release is raycast by the real InputSystemUIInputModule and starts the production new-game flow");

                game.Machine.Clear(); File.WriteAllText(path, validSave); Title();
                for (int i = 0; i < 4; i++) yield return null;
                EventSystem.current.SetSelectedGameObject(view.NewGameButton.gameObject);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.DownArrow));
                limit = EditorApplication.timeSinceStartup + 4;
                while (EventSystem.current.currentSelectedGameObject != view.ContinueButton.gameObject && EditorApplication.timeSinceStartup < limit) yield return null;
                Require(EventSystem.current.currentSelectedGameObject == view.ContinueButton.gameObject, "Native keyboard navigation did not select Continue");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                for (int i = 0; i < 3; i++) yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Enter));
                limit = EditorApplication.timeSinceStartup + 4;
                while (game.AtTitle && EditorApplication.timeSinceStartup < limit) yield return null;
                game.Machine.Tick();
                Require(!game.AtTitle && game.money == 8765 && game.Inventory.Count(0) == 3,
                    "Native keyboard submit did not restore the selected save");
                checks.Add("Queued Down/Enter events navigate and submit through the real UI input module, restoring the isolated save");
            }
            finally
            {
                if (keyboard != null) InputSystem.RemoveDevice(keyboard);
                if (mouse != null) InputSystem.RemoveDevice(mouse);
                if (oldKeyboard != null) oldKeyboard.MakeCurrent();
                if (oldMouse != null) oldMouse.MakeCurrent();
                InputSystem.settings.backgroundBehavior = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
            }
        }

        private static IEnumerator CheckDevices()
        {
            var previousKeyboard = Keyboard.current; var previousMouse = Mouse.current; var previousTouch = Touchscreen.current;
            var ui = view.GetComponentInChildren<InputSystemUIInputModule>(true); bool enabled = ui.enabled;
            var background = InputSystem.settings.backgroundBehavior;
            var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            Keyboard keyboard = null; Mouse mouse = null; Touchscreen screen = null;
            int ticks = 0;
            bool pressedSeen = false;
            bool w = false, j = false, left = false, enter = false, numpad = false, mouseHeld = false;
            Vector2 mousePosition = Vector2.zero;
            int touchCount = 0;
            Action sample = () =>
            {
                if (InputState.currentUpdateType != InputUpdateType.Dynamic) return;
                ticks++;
                pressedSeen |= GameInput.Pressed(KeyCode.W);
                // Editor callbacks use a separate device state buffer. Sample the production reader
                // during the real player update, then assert on that captured player snapshot.
                w = GameInput.Held(KeyCode.W); j = GameInput.Held(KeyCode.J);
                left = GameInput.Held(KeyCode.LeftArrow); enter = GameInput.Held(KeyCode.Return);
                numpad = GameInput.Held(KeyCode.KeypadEnter); mouseHeld = GameInput.MouseHeld;
                mousePosition = GameInput.MousePosition; touchCount = ActiveTouches();
            };
            ui.enabled = false;
            try
            {
                InputSystem.onAfterUpdate += sample;
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                // Synthetic devices must reach the player while MCP keeps the GameView unfocused.
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard = InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent();
                int target = ticks + 1;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.J, Key.LeftArrow, Key.Enter, Key.NumpadEnter));
                while (ticks < target) yield return null;
                Require(w && j && left && enter && numpad && pressedSeen,
                    "Keyboard state mismatch: W=" + w + ", J=" + j + ", Enter=" + enter + ", edge=" + pressedSeen + ", ticks=" + ticks);
                checks.Add("Simultaneous movement/attack, arrow, Enter and numpad Enter read actual queued Input System keyboard state");
                pressedSeen = false; target = ticks + 1;
                while (ticks < target) yield return null;
                Require(w && !pressedSeen, "Held key repeated its press edge");
                target = ticks + 1; InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                while (ticks < target) yield return null;
                Require(!w && !j && !GameInput.Held(KeyCode.None), "Release or unsupported-key handling failed");
                checks.Add("Holding does not repeat a press edge; releasing clears held state, and unsupported keys are safely ignored");

                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent();
                target = ticks + 1;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(120, 75) }.WithButton(MouseButton.Left));
                while (ticks < target) yield return null;
                Require(mouseHeld && mousePosition == new Vector2(120, 75), "Mouse state mismatch");
                target = ticks + 1; InputSystem.QueueStateEvent(mouse, new MouseState());
                while (ticks < target) yield return null;
                Require(!mouseHeld, "Released mouse remained held");
                checks.Add("Pointer position and mouse-button press/release use the same device adapter");

                GameInput.Initialize(); screen = InputSystem.AddDevice<Touchscreen>(); screen.MakeCurrent();
                target = ticks + 1;
                Touch(screen, 1, UnityEngine.InputSystem.TouchPhase.Began, new Vector2(8, 8));
                Touch(screen, 2, UnityEngine.InputSystem.TouchPhase.Began, new Vector2(18, 18));
                while (ticks < target) yield return null;
                Require(touchCount == 2 && !GameInput.ActiveTouch(-1, out _), "Independent touches were not preserved");
                target = ticks + 1;
                Touch(screen, 1, UnityEngine.InputSystem.TouchPhase.Ended, new Vector2(8, 8));
                Touch(screen, 2, UnityEngine.InputSystem.TouchPhase.Canceled, new Vector2(18, 18));
                while (ticks < target) yield return null;
                Require(touchCount == 0, "Ended or canceled touches remained active");
                checks.Add("Two independent touches coexist; ended/canceled touches do not remain held, and invalid touch indexes are safe");
            }
            finally
            {
                InputSystem.onAfterUpdate -= sample;
                if (keyboard != null) InputSystem.RemoveDevice(keyboard);
                if (mouse != null) InputSystem.RemoveDevice(mouse);
                if (screen != null) InputSystem.RemoveDevice(screen);
                if (previousKeyboard != null) previousKeyboard.MakeCurrent();
                if (previousMouse != null) previousMouse.MakeCurrent();
                if (previousTouch != null) previousTouch.MakeCurrent();
                InputSystem.settings.backgroundBehavior = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput; ui.enabled = enabled;
            }
        }

        private static int ActiveTouches()
        {
            int count = 0;
            for (int i = 0; i < GameInput.TouchCount; i++) if (GameInput.ActiveTouch(i, out _)) count++;
            return count;
        }
        private static void Touch(Touchscreen screen, int id, UnityEngine.InputSystem.TouchPhase phase, Vector2 position) =>
            InputSystem.QueueStateEvent(screen, new TouchState { touchId = id, phase = phase, position = position, pressure = 1 });

        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying) throw new Exception("Play ended during M1 verification");
                if (EditorApplication.timeSinceStartup - started > 40) throw new TimeoutException("M1 verification timed out");
                if (!work.MoveNext()) Finish(null);
            }
            catch (Exception error) { Finish(error.ToString()); }
        }

        private static void Finish(string error)
        {
            EditorApplication.update -= Tick;
            var previousWork = work; work = null;
            try
            {
                (previousWork as IDisposable)?.Dispose();
                if (game && EditorApplication.isPlaying)
                { Title(); game.SubmitAction(GameAction.NewGame); game.Machine.Clear(); Title(); game.saveFileName = originalSave; }
                Time.timeScale = originalScale;
                foreach (var file in protectedFiles)
                {
                    if (file.Value != null) File.WriteAllBytes(file.Key, file.Value);
                    else if (File.Exists(file.Key)) File.Delete(file.Key);
                }
                foreach (string file in new[] { fixture, fixture + ".bak", fixture + ".tmp" })
                { string path = Path.Combine(Application.persistentDataPath, file); if (File.Exists(path)) File.Delete(path); }
            }
            catch (Exception cleanup) { error = (error ?? "") + "\nCleanup: " + cleanup; }
            Directory.CreateDirectory("../Reference/Validation/2026-10-30-m1");
            File.WriteAllText("../Reference/Validation/2026-10-30-m1/input-title.json", JsonUtility.ToJson(
                new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            Debug.Log("GUJIAN_M1_INPUT_TITLE " + (error == null) + " " + error);
        }
    }
}
