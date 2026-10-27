using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class CriticalHitFeedbackValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags S = BindingFlags.Static | BindingFlags.NonPublic;
        private static GuJianGame game;
        private static object Get(object o, string name) => o.GetType().GetField(name, F).GetValue(o);
        private static void Set(object o, string name, object value) => o.GetType().GetField(name, F).SetValue(o, value);
        private static object Call(string name, params object[] args) => typeof(GuJianGame).GetMethod(name, F).Invoke(game, args);
        private static object Fighter(int id) => ((IDictionary)Get(game, "fighters"))[id];
        private static void Require(bool yes, string reason) { if (!yes) throw new Exception(reason); }
        private static AudioSource Audio(string name) => (AudioSource)Get(game, name + "Audio");
        private static void Stage()
        {
            typeof(ComboValidation).GetMethod("Stage", S).Invoke(null, new object[] { game, false });
            foreach (string name in new[] { "swing", "hit", "critical" }) Audio(name).Stop();
            Set(game, "soundVolumeStep", 2); Call("ApplySoundVolume");
        }
        private static void Hit() => typeof(ComboValidation).GetMethod("Hit", S).Invoke(null, new object[] { game, 0, 100000, 0, true });
        private static OriginalActor BossStage(int id)
        {
            var boss = (OriginalActor)typeof(BossFamilyValidation).GetMethod("Stage", S).Invoke(null, new object[] { game, id, false });
            foreach (string name in new[] { "swing", "hit", "critical" }) Audio(name).Stop();
            return boss;
        }
        private static void BossHit(OriginalActor boss, int frame)
        {
            game.Actors[-1].ResetAction(); boss.Sample(24, frame);
            typeof(BossFamilyValidation).GetMethod("Align", S).Invoke(null, new object[] { boss, game.Actors[-1] });
            Call("ResolveBossCriticalHit", boss);
        }
        private static void Tick(int count)
        { for (int i = 0; i < count; i++) Call("TickCriticalHitEffect", .080001f); }
        private static Texture2D Capture(string name)
        {
            var image = (Texture2D)typeof(HudFeedbackValidation).GetMethod("Render", S).Invoke(null, null);
            File.WriteAllBytes("../Reference/Validation/critical-hit-" + name + ".png", image.EncodeToPNG());
            return image;
        }

        [MenuItem("古剑/验证暴击特效与命中音效")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play first");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var checks = new List<string>(); string error = null, save = game.saveFileName;
            float scale = Time.timeScale, listener = AudioListener.volume;
            int soundStep = (int)Get(game, "soundVolumeStep"); var random = UnityEngine.Random.state;
            var images = new List<Texture2D>(); Time.timeScale = 0;
            try
            {
                var ui = Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                var overlay = ui.criticalHitOverlay;
                Require(overlay != null && overlay.animation != null && overlay.sheets.Length == 1 &&
                    overlay.sheets[0] && AssetDatabase.Contains(overlay.sheets[0]) && overlay.sheets[0].name == "s_34", "Saved critical overlay missing");
                Require(overlay.animation.actions[0].steps.Select(s => s.frame).SequenceEqual(new[] { 0, 1, 0, 1, 1 }) &&
                    overlay.animation.frames[0].pieces.Length == 15 && overlay.animation.frames[1].pieces.Length == 0, "Original s_32 data mismatch");
                checks.Add("Saved s_32 action uses original 0/1/0/1/1 sequence, 15 tiles, empty alternate frame and persistent s_34 texture; no runtime asset construction");

                Stage(); Hit();
                Require(Audio("hit").isPlaying && !Audio("critical").isPlaying && (int)Get(game, "criticalHitDuration") == 0, "Ordinary hit feedback");
                checks.Add("Real original-frame noncritical hit starts hit audio without starting critical overlay/audio");

                Stage(); Set(Fighter(-1), "critical", 1000); Hit();
                Require(Audio("critical").isPlaying && !Audio("hit").isPlaying && (int)Get(game, "criticalHitDuration") == 5 &&
                    (int)Get(game, "criticalHitCounter") == 0 && (int)Get(game, "shakeTicks") == 4 && (int)Get(game, "shakeType") == 0, "Critical hit feedback");
                checks.Add("Real original-frame guaranteed critical starts five-cycle overlay, original four-cycle shake and critical audio, with normal hit audio absent");

                Call("TickCriticalHitEffect", .039f); Require((int)Get(game, "criticalHitCounter") == 0, "Fraction advanced early");
                Call("TickCriticalHitEffect", .041001f); Require((int)Get(game, "criticalHitCounter") == 1, "Fraction cadence");
                Set(game, "menu", true); Tick(2);
                Require((int)Get(game, "criticalHitCounter") == 3, "Menu stopped global effect");
                Call("SetCriticalHitEffect", 5);
                Require((int)Get(game, "criticalHitCounter") == 3, "Retrigger reset original counter");
                Tick(2); Require((int)Get(game, "criticalHitCounter") == 5 && (int)Get(game, "criticalHitDuration") == 5, "Lost fifth cycle");
                Tick(1); Require((int)Get(game, "criticalHitCounter") == 0 && (int)Get(game, "criticalHitDuration") == 0, "Sixth cycle failed to clear");
                checks.Add("80 ms accumulated timing, global progress under menu, retrigger preserving current counter, fifth-cycle retention and sixth-cycle cleanup match GameCanvas.cycle");

                foreach (bool critical in new[] { false, true })
                {
                    Stage(); game.Actors[900].hidden = true;
                    var boss = (OriginalActor)typeof(ComboValidation).GetMethod("Spawn", S).Invoke(null, new object[] { game, 900, 231 });
                    boss.hp = 1; Set(Fighter(-1), "critical", critical ? 1000 : 0); Hit();
                    Require(boss.hp == 0 && (bool)Get(Fighter(900), "dead"), "Boss lethal fixture failed");
                    Require(!Audio("hit").isPlaying && Audio("critical").isPlaying == critical &&
                        ((int)Get(game, "criticalHitDuration") == 5) == critical, "Lethal Boss sound branch " + critical);
                }
                checks.Add("Actual lethal hits on type10 Boss skip ordinary hit audio after the original early return; lethal critical still starts critical audio/effect");
                Stage(); game.Actors[900].hp = 1; Hit();
                Require(game.Actors[900].hp == 0 && Audio("hit").isPlaying, "Ordinary enemy death lost hit sound");
                checks.Add("Actual lethal non-Boss hit retains normal audio; Boss suppression does not affect ordinary enemies");

                Stage(); Set(Fighter(-1), "hit", -100000); UnityEngine.Random.InitState(2026); Hit();
                Require(!Audio("hit").isPlaying && !Audio("critical").isPlaying && (int)Get(game, "criticalHitDuration") == 0, "Dodge made hit feedback");
                Call("Damage", Fighter(-1), 1, false, -1);
                Require(!Audio("hit").isPlaying && !Audio("critical").isPlaying && (int)Get(game, "criticalHitDuration") == 0, "Enemy damage made role feedback");
                checks.Add("Actual dodged attack and enemy-damage path do not trigger player hit or critical feedback");

                foreach (int id in new[] { 232, 233 })
                {
                    var boss = BossStage(id); int hp = game.CurrentHp;
                    game.Machine.Command(0x7E, new object[] { 900 });
                    for (int i = 0; i < 100 && boss.actionIndex == 22; i++) boss.Tick(.080001f);
                    Require(boss.actionIndex == 24 && game.CurrentHp == hp && Audio("critical").isPlaying &&
                        (int)Get(game, "criticalHitDuration") == 5, "Boss intro trigger " + id);
                    Call("ResetCombatFeedback"); Audio("critical").Stop();
                    int frame = Array.FindIndex(boss.Animation.actions[24].steps, s => s.attack);
                    BossHit(boss, frame);
                    Require(game.CurrentHp == hp - game.MaximumHp / 2 && !Audio("critical").isPlaying &&
                        (int)Get(game, "criticalHitDuration") == 0, "Boss half-HP hit retriggered effect " + id);
                }
                checks.Add("Boss232/233 actual intro22→24 starts critical effect/audio before damage; subsequent original half-max-HP hit does not retrigger either");
                foreach (int id in new[] { 235, 236 })
                {
                    var boss = BossStage(id); game.Machine.Command(0x7E, new object[] { 900 });
                    for (int i = 0; i < 100 && boss.actionIndex == 22; i++) boss.Tick(.080001f);
                    Require((int)Get(game, "criticalHitDuration") == 0, "Drain Boss intro triggered effect " + id);
                    BossHit(boss, 26);
                    Require(Audio("critical").isPlaying && (int)Get(game, "criticalHitDuration") == 5, "Drain Boss frame26 failed " + id);
                }
                checks.Add("Boss235/236 intro stays silent; original action24 frame26 valid hit starts critical effect/audio");
                {
                    var boss = BossStage(244); Set(Fighter(900), "critical", 1000);
                    game.Machine.Command(0x7E, new object[] { 900 });
                    for (int i = 0; i < 100 && boss.actionIndex == 22; i++) boss.Tick(.080001f);
                    int frame = Array.FindIndex(boss.Animation.actions[24].steps, s => s.attack);
                    int hp = game.CurrentHp; BossHit(boss, frame);
                    Require(game.CurrentHp < hp && !Audio("critical").isPlaying && (int)Get(game, "criticalHitDuration") == 0,
                        "Enemy random critical incorrectly used player effect");
                }
                checks.Add("Boss244 guaranteed random critical retains damage without player critical effect/audio; Enemy.processAttack only triggers effects at its authored family call sites");

                Stage(); Set(game, "soundVolumeStep", 0); Call("ApplySoundVolume");
                Set(Fighter(-1), "critical", 1000); Hit();
                Require(!Audio("critical").isPlaying && (int)Get(game, "criticalHitDuration") == 5, "Mute suppressed visual effect");
                checks.Add("Original mute gate silences critical audio while preserving visual overlay and shake");

                Stage(); Set(game, "toastUntil", 0f); ((IList)Get(game, "numbers")).Clear();
                Call("SetCriticalHitEffect", 5);
                int nodes = UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
                // RenderView updates the camera projection on its first capture.
                var warmup = (Texture2D)typeof(HudFeedbackValidation).GetMethod("Render", S).Invoke(null, null);
                UnityEngine.Object.DestroyImmediate(warmup);
                for (int i = 0; i <= 6; i++)
                {
                    images.Add(Capture(i.ToString()));
                    if (i < 6) Tick(1);
                }
                var baseline = images[0].GetPixels32(); var flash = images[1].GetPixels32();
                int changed = 0;
                for (int i = 0; i < baseline.Length; i++) if (!baseline[i].Equals(flash[i])) changed++;
                Require(changed > 1000, "Overlay absent from actual GameView");
                foreach (int i in new[] { 2, 4, 5, 6 }) Require(baseline.SequenceEqual(images[i].GetPixels32()), "Blank phase changed GameView " + i);
                Require(flash.SequenceEqual(images[3].GetPixels32()), "Flash phase pixels differ");
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length == nodes, "Fixed effect created GameObjects");
                checks.Add("Seven unfocused GameView captures verify blank/flash/blank/flash/blank/blank/closed, identical repeated phases and " + changed + " changed pixels; node count unchanged");

                Call("SetCriticalHitEffect", 5); game.Begin();
                Require((int)Get(game, "criticalHitDuration") == 0 && (int)Get(game, "criticalHitCounter") == 0 && (float)Get(game, "criticalHitElapsed") == 0, "New game retained effect");
                checks.Add("New game clears duration, phase and fractional accumulator");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                foreach (var image in images) UnityEngine.Object.DestroyImmediate(image);
                game.Begin(); game.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = save; Set(game, "soundVolumeStep", soundStep); Call("ApplySoundVolume");
                AudioListener.volume = listener; Time.timeScale = scale; UnityEngine.Random.state = random;
            }
            File.WriteAllText("../Reference/Validation/critical-hit-feedback.json", JsonUtility.ToJson(new ReconstructionValidation.Report
                { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            Debug.Log("GUJIAN_CRITICAL_HIT_FEEDBACK " + (error == null) + " " + error); game = null;
        }
    }
}
