using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class BlackScreenValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> checks = new List<string>();
        private static GuJianGame game;
        private static Func<string, string[]> previousLoader;
        private static string previousSave;
        private static float previousScale;
        private static UnityEngine.Random.State previousRandom;
        private static double liveStarted;
        private static int liveFrame, liveAction, liveUpdates;
        private static Vector2 livePosition;
        private static object Get(string name) => typeof(GuJianGame).GetField(name, F).GetValue(game);
        private static void Set(string name, object value) => typeof(GuJianGame).GetField(name, F).SetValue(game, value);
        private static object Call(string name, params object[] args) => typeof(GuJianGame).GetMethod(name, F).Invoke(game, args);
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Tick(float delta) => Call("TickOriginalBlackScreen", delta);
        private static int Width => (int)Get("originalBlackWidth");
        private static int Line => (int)Get("originalBlackLine");
        private static bool Active => (bool)Get("originalBlackScreen");
        private static void Open(params object[] args)
        {
            game.Machine.Clear(); game.Machine.Command(0x5D, args);
        }
        private static void Script(string instruction)
        {
            game.Machine.Clear(); game.Machine.Values["blackDone"] = 0;
            game.Machine.LoadScript = _ => new[] { instruction, "x09(\"blackDone\",x0A(\"blackDone\")+1)" };
            game.Machine.Enqueue("black-screen-probe"); game.Machine.Tick();
        }
        private static void Capture(string name)
        {
            var render = typeof(HudFeedbackValidation).GetMethod("Render", BindingFlags.Static | BindingFlags.NonPublic);
            var image = (Texture2D)render.Invoke(null, null);
            try { File.WriteAllBytes("../Reference/Validation/" + name + ".png", image.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }

        [MenuItem("古剑/验证原版黑幕旁白")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            if (game) throw new InvalidOperationException("Black-screen validation is already running");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            previousLoader = game.Machine.LoadScript; previousSave = game.saveFileName;
            previousScale = Time.timeScale; previousRandom = UnityEngine.Random.state;
            checks.Clear(); Time.timeScale = 0;
            Directory.CreateDirectory("../Reference/Validation");
            try
            {
                game.saveFileName = "validation-black-screen.json";
                game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
                game.Machine.Enqueue("200"); game.Machine.Tick();
                var original = OriginalAssets.Json<ScriptData>("Scripts/100e0").instructions.First(s => s.StartsWith("x5D("));
                Script(original);
                Require(Active && game.DialogueVisible && game.Machine.IsWaiting() && game.Machine.Get("blackDone") == 0, "Original x5D did not block the production VM");
                Require(((string[])Get("originalBlackLines")).SequenceEqual(new[] { "江湖第一剑派", "藏剑山庄", "一年一度的斗剑大会" }), "Original newline semantics mismatch");
                Require((int)Get("originalBlackY") == 60 && Line == 0 && Width == 0, "Original initial position or reveal mismatch");
                Capture("black-screen-start");
                Tick(.079f); Require(Width == 0, "Reveal advanced before 80ms");
                Tick(.001f); Require(Width == 2, "First 80ms must reveal exactly 2px");
                Tick(.08f); Require(Width == 4, "Second 80ms must reveal exactly 4px");
                Tick(0); Require(Width == 4, "Paused reveal advanced");
                Tick(.64f); Require(Width == 20, "Long frame lost reveal cycles");
                Capture("black-screen-partial");
                checks.Add("Original 100e0 x5D runs on production VM, keeps three original lines, starts at y60 and reveals 2px per 80ms with partial/paused/long deltas");

                game.PressControl(5); Require(Line == 0 && Width == 20 && Active, "Back key canceled the original black screen");
                game.PressControl(4); Require(Line == 1 && Width == 0, "OK should skip a nonterminal line");
                game.ContinueDialogue(); Require(Line == 2 && Width == 0, "ContinueDialogue bypassed black-screen input");
                game.ContinueDialogue(); Require(Line == 2 && Width == ((int[])Get("originalBlackWidths"))[2] && Active, "Final line should reveal rather than close");
                Tick(.08f); Require(Line == 3 && Active, "Final line did not finish on the next cycle");
                Capture("black-screen-complete");
                Tick(1.6f); Require(Active && (int)Get("originalBlackStay") == 20, "Black screen closed before twenty stay cycles");
                Tick(.08f); game.Machine.Tick();
                Require(!Active && !game.DialogueVisible && !game.Machine.Running && game.Machine.Get("blackDone") == 1, "Black screen did not resume VM after the twenty-first stay cycle");
                game.Machine.Tick(); Require(game.Machine.Get("blackDone") == 1, "VM resumed twice");
                checks.Add("Back is consumed; OK skips intermediate lines, keeps the final line, and all text remains for 20 cycles before close on cycle21; production VM resumes exactly once");

                Open("首行\n末行\n第三行", false, 73, 2, "t_00", 17);
                Require((int)Get("originalBlackY") == 73 && (int)Get("originalBlackImageY") == 17 &&
                    (Texture2D)Get("originalBlackImage") == OriginalAssets.Texture("pic/t_00"), "Explicit position/image parameters lost");
                game.ContinueDialogue(); Require(Line == 1, "First page line skip");
                game.ContinueDialogue(); Require(Line == 1 && Width == ((int[])Get("originalBlackWidths"))[1], "Last page line should reveal, not skip");
                Tick(.08f); Require(Line == 2 && (int)Get("originalBlackPage") == 1 && Width == 0, "Page advance mismatch");
                game.ContinueDialogue(); Capture("black-screen-page-image");
                Require(Line == 2 && Width > 0, "Last overall line input mismatch");
                Tick(1.76f); Require(!Active && Get("originalBlackImage") == null, "Image not released on finish");
                checks.Add("All six x5D parameters: y73, two-line pages, original t_00 image at y17, page terminal confirmation and image cleanup");

                Open("自动结束", false);
                int textWidth = ((int[])Get("originalBlackWidths"))[0];
                int revealCycles = Math.Max(0, (int)Math.Ceiling((textWidth - 2) / 2.0)) + 1;
                Tick(revealCycles * .08f); Require(Line == 1 && Active, "Natural reveal threshold mismatch");
                Tick(1.68f); Require(!Active && !(bool)Get("originalBlackScriptPaused"), "Natural end needs a confirmation or remains blocked");
                checks.Add("No-input natural reveal reaches the measured width boundary, then automatically closes and releases script wait");

                Script("x5D(\"返回场景\",true)");
                game.ContinueDialogue(); Tick(1.76f); game.Machine.Tick();
                Require(!Active && !game.DialogueVisible && game.Machine.Running && game.Machine.IsWaiting() &&
                    game.Machine.Get("blackDone") == 0, "Return-to-scene branch must leave the script paused");
                game.Machine.Command(0x28, new object[] { "新消息", 0 }); game.ContinueDialogue(); game.Machine.Tick();
                Require(!game.Machine.Running && game.Machine.Get("blackDone") == 1, "Replacement message did not clear stale black-screen wait");
                checks.Add("isReturn=true returns to scene while preserving the original script pause; a replacement modal clears the old wait and resumes after confirmation");

                string longText = string.Concat(Enumerable.Repeat("混合中文ABC123", 16));
                Open(longText);
                var lines = (string[])Get("originalBlackLines");
                var style = (GUIStyle)Get("originalBlackStyle");
                Require(string.Concat(lines) == longText && lines.Length > 1 &&
                    lines.All(s => (int)style.CalcSize(new GUIContent(s)).x < 513), "Width wrapping lost text or exceeded original 513px boundary");
                Open("尾行\n"); Require(((string[])Get("originalBlackLines")).SequenceEqual(new[] { "尾行", "" }), "Trailing newline did not preserve an empty line");
                checks.Add("Mixed CJK/ASCII wraps at the original >=513px rule without dropping any characters");
                game.Begin(); game.Machine.Clear();
                Require(!Active && !(bool)Get("originalBlackScriptPaused") && Get("originalBlackImage") == null, "New game retained black-screen state");
                checks.Add("Begin clears reveal, image and script-pause state without saving test data");

                game.Machine.LoadScript = previousLoader;
                game.Machine.Values["event"] = -999; game.Machine.Enqueue("200"); game.Machine.Tick();
                var role = game.Actors[-1]; role.Act(4, true);
                liveFrame = role.FrameIndex; liveAction = role.actionIndex; livePosition = role.mapPosition;
                Set("timerScript", "unused-black-screen-timeout"); Set("timerSeconds", 30f);
                Script(original);
                liveUpdates = 0; liveStarted = EditorApplication.timeSinceStartup;
                Time.timeScale = 16;
                EditorApplication.update -= AdvanceLive;
                EditorApplication.update += AdvanceLive;
            }
            catch (Exception error) { Finish(error.ToString()); }
        }

        private static void AdvanceLive()
        {
            try
            {
                Require(EditorApplication.isPlaying && game && game.LastError == null, "Unity play/host failed during real Update");
                if (EditorApplication.timeSinceStartup - liveStarted > 30) throw new TimeoutException("Natural black-screen Update timed out");
                if (Active)
                {
                    liveUpdates++;
                    Require(game.Actors[-1].FrameIndex == liveFrame && game.Actors[-1].actionIndex == liveAction &&
                        game.Actors[-1].mapPosition == livePosition && (float)Get("timerSeconds") == 30f, "Black screen advanced scene animation, movement or timer");
                    Require(game.Machine.Get("blackDone") == 0, "VM advanced while black-screen text was visible");
                    return;
                }
                Require(liveUpdates > 1 && game.Machine.Get("blackDone") == 1 && !game.Machine.Running, "Real Update failed to finish and resume exactly once");
                checks.Add("Real Unity Update naturally completes the original 100e0 text at timeScale16; observed " + liveUpdates +
                    " editor callbacks, scene actor frame/position and timer stay fixed while visible, then production VM continues exactly once");
                Finish(null);
            }
            catch (Exception error) { Finish(error.ToString()); }
        }

        private static void Finish(string error)
        {
            EditorApplication.update -= AdvanceLive;
            if (game)
            {
                game.Machine.LoadScript = previousLoader;
                game.Begin(); game.Machine.Clear();
                typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = previousSave;
            }
            Time.timeScale = previousScale; UnityEngine.Random.state = previousRandom;
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/black-screen.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_BLACK_SCREEN_VALIDATION " + report.passed + " " + error);
            game = null;
        }
    }
}
