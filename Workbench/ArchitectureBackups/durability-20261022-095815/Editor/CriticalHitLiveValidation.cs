using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class CriticalHitLiveValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags S = BindingFlags.Static | BindingFlags.NonPublic;
        private static GuJianGame game;
        private static int phase, frames, initialFrame, maxCounter, pausedFrames;
        private static double started;
        private static float scale;
        private static string save;
        private static UnityEngine.Random.State random;
        private static readonly List<string> checks = new List<string>();
        public static bool Running { get; private set; }
        public static string Status => "phase=" + phase + ", observed=" + frames + ", maxCounter=" + maxCounter + ", breakPaused=" + pausedFrames;
        private static object Get(string n) => typeof(GuJianGame).GetField(n, F).GetValue(game);
        private static void Set(string n, object v) => typeof(GuJianGame).GetField(n, F).SetValue(game, v);
        private static object Call(string n, params object[] a) => typeof(GuJianGame).GetMethod(n, F).Invoke(game, a);

        public static void Run()
        {
            if (Running || !EditorApplication.isPlaying) throw new InvalidOperationException("Idle Play required");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            scale = Time.timeScale; save = game.saveFileName; random = UnityEngine.Random.state;
            Time.timeScale = 1; phase = 0; checks.Clear(); Running = true;
            Prepare(); EditorApplication.update += Advance;
        }
        private static void Prepare()
        {
            // The loading case deliberately holds x22 unfinished; release that
            // validation state before constructing the next isolated scene.
            Set("loadingVisible", false); Set("loadingFinished", false);
            Call("ResetOriginalBlackScreen");
            typeof(ComboValidation).GetMethod("Stage", S).Invoke(null, new object[] { game, false });
            game.Actors[-1].frozen = true;
            if (phase == 0) Set("menu", true);
            if (phase == 1) Call("OpenOriginalBlackScreen", "原版暴击全局时钟验证：黑幕期间仍按80ms继续计时。", false, 60, 0, "", 0);
            if (phase == 2) { Set("loadingVisible", true); Set("loadingFinished", false); }
            Call("SetCriticalHitEffect", 5);
            if (phase == 3) Call("DestroyOrdinarySword");
            initialFrame = game.Actors[-1].FrameIndex;
            frames = maxCounter = pausedFrames = 0; started = EditorApplication.timeSinceStartup;
        }
        private static void Advance()
        {
            try
            {
                if (!EditorApplication.isPlaying || !game || game.LastError != null) throw new Exception("Game stopped or error");
                if (EditorApplication.timeSinceStartup - started > 15) throw new TimeoutException(Status);
                frames++; int counter = (int)Get("criticalHitCounter"); maxCounter = Mathf.Max(maxCounter, counter);
                if (game.Actors[-1].FrameIndex != initialFrame) throw new Exception("Frozen actor advanced");
                if (phase == 0 && !(bool)Get("menu") || phase == 1 && !(bool)Get("originalBlackScreen") || phase == 2 && !(bool)Get("loadingVisible"))
                    throw new Exception("Modal state ended before effect");
                if (phase == 3 && game.SwordBreakVisible)
                {
                    pausedFrames++;
                    if (counter != 0) throw new Exception("Break scene pause advanced critical effect");
                }
                if ((int)Get("criticalHitDuration") > 0) return;
                if (frames < 2 || maxCounter == 0 || phase == 3 && pausedFrames < 2) throw new Exception("Insufficient live cadence evidence: " + Status);
                checks.Add(new[] { "Menu", "Black screen", "Loading", "Sword-break scene pause" }[phase] +
                    ": real Unity Update observed " + frames + " editor callbacks, peak counter=" + maxCounter +
                    ", break-paused callbacks=" + pausedFrames + "; frozen actor unchanged and five-cycle effect naturally cleared without manual ticks");
                if (++phase < 4) Prepare(); else Finish(null);
            }
            catch (Exception e) { Finish(e.ToString()); }
        }
        private static void Finish(string error)
        {
            EditorApplication.update -= Advance; Running = false;
            if (game)
            {
                Set("loadingVisible", false); Set("loadingFinished", false);
                game.Begin(); game.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = save;
            }
            Time.timeScale = scale; UnityEngine.Random.state = random;
            File.WriteAllText("../Reference/Validation/critical-hit-live.json", JsonUtility.ToJson(new ReconstructionValidation.Report
                { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            Debug.Log("GUJIAN_CRITICAL_HIT_LIVE " + (error == null) + " " + error); game = null;
        }
    }
}
