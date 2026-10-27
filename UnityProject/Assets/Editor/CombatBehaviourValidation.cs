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
    public static class CombatBehaviourValidation
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<string> checks = new List<string>();
        private static readonly HashSet<int> actions = new HashSet<int>();
        private static readonly List<int> capturedHp = new List<int>();
        private static float previousScale;
        private static UnityEngine.Random.State previousRandom;
        private static double started;
        private static int phase, initialHp, lastHit;
        private static OriginalActor observedProjectile;
        private static object Get(object o, string name) => ValidationState.Get(o, name);
        private static void Set(object o, string name, object value) => ValidationState.Set(o, name, value);
        private static object Fighter(GuJianGame g, int id) => ((IDictionary)Get(g, "fighters"))[id];
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        private static void CombatTick(GuJianGame g, int count = 1)
        {
            for (int i = 0; i < count; i++) typeof(GuJianGame).GetMethod("TickCombat", Private).Invoke(g, new object[] { .08f });
        }
        private static void Stage(GuJianGame g)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999;
            g.Machine.Enqueue("200"); g.Machine.Tick();
            Require(!g.Machine.Running && !g.DialogueVisible && g.Map != null, "Test stage failed to load");
            g.Machine.Command(0x4D, new object[] { 40 });
            g.Machine.Command(0x65, new object[] { true });
            Set(Fighter(g, -1), "hit", 100000);
        }
        private static OriginalActor Spawn(GuJianGame g, int definition, bool locked = true)
        {
            g.Machine.Command(0x31, new object[] { 900 });
            g.Machine.Command(0x30, new object[] { 900, definition, "l", 250, 250, 2 });
            var a = g.Actors[900]; a.locked = locked;
            Set(Fighter(g, 900), "cooldowns", Enumerable.Repeat(100000, ((int[])Get(Fighter(g, 900), "cooldowns")).Length).ToArray());
            return a;
        }
        private static void PlaceNear(GuJianGame g, OriginalActor a, float dx = 55)
        {
            var role = g.Actors[-1]; role.mapPosition = new Vector2(180, 250); role.ResetAction(); role.Face("r");
            a.mapPosition = role.mapPosition + new Vector2(dx + (role.Animation.footWidth - a.Animation.footWidth) / 2f, role.Animation.footHeight - a.Animation.footHeight);
        }
        // Real UseSkill + original animation attack rectangles; only starting stats/poses are fixtures.
        private static OriginalActor Hit(GuJianGame g, int skill, int definition = 201, int hold = 0, int pose = 0)
        {
            var target = Spawn(g, definition);
            target.maxHp = target.hp = 1000000;
            if (pose != 0)
            {
                target.PlayOnce(pose);
                int frame = Array.FindIndex(target.Animation.actions[pose].steps, s => s.vulnerable && target.Animation.frames[s.frame].bodyRects.Length > 0);
                Require(frame >= 0, "No vulnerable test pose " + pose);
                target.Sample(pose, frame);
            }
            Set(Fighter(g, 900), "hold", hold);
            var role = g.Actors[-1]; role.ResetAction(); role.Face("r"); role.mapPosition = new Vector2(150, 250);
            Require(g.UseSkill(skill, 1, true), "UseSkill rejected " + skill);
            var steps = role.Animation.actions[role.actionIndex].steps;
            int attackFrame = Array.FindIndex(steps, s => s.attack);
            var rect = role.Animation.frames[steps[attackFrame].frame].attackRects[0];
            var body = target.CurrentFrame.bodyRects[0];
            float vx = steps.Take(attackFrame + 1).Sum(s => -s.vx), vy = steps.Take(attackFrame + 1).Sum(s => s.vy);
            target.mapPosition = role.mapPosition + new Vector2(vx - rect.x - rect.w / 2f + role.Animation.footWidth - body.x - body.w / 2f,
                vy + rect.y + rect.h / 2f - body.y - body.h / 2f);
            for (int i = 0; i <= attackFrame + 2 && target.hp == target.maxHp; i++) role.Tick(.08001f);
            Require(target.hp < target.maxHp, "Original attack rectangles missed skill " + skill);
            return target;
        }
        private static void Until(OriginalActor a, int action, int max = 256)
        {
            for (int i = 0; i < max && a.actionIndex != action; i++) a.Tick(.08001f);
            Require(a.actionIndex == action, "Expected action " + action + ", got " + a.actionIndex);
        }

        [MenuItem("古剑/验证受击与 Boss 自主行为")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            previousScale = Time.timeScale; previousRandom = UnityEngine.Random.state;
            UnityEngine.Random.InitState(19231);
            checks.Clear(); actions.Clear(); capturedHp.Clear();
            started = EditorApplication.timeSinceStartup;
            try
            {
                Stage(g);
                var target = Hit(g, 0, hold: 50);
                Require(target.actionIndex == 0 && (int)Get(Fighter(g, 900), "hold") == 40, "Armor failed to resist interruption");
                target = Hit(g, 0); Require(target.actionIndex == 2, "Normal hit reaction"); Until(target, 0);
                Require((int)Get(Fighter(g, 900), "hold") == 1, "Enemy hold was not restored");
                target = Hit(g, 2); Require(target.actionIndex == 4, "Launcher reaction");
                target = Hit(g, 0, pose: 4); Require(target.actionIndex == 5, "Airborne follow-up");
                target = Hit(g, 3, pose: 4); Require(target.actionIndex == 8, "Airborne heavy follow-up");
                target = Hit(g, 3); Require(target.actionIndex == 6, "Heavy hit reaction"); Until(target, 0);
                target = Hit(g, 4); Require(target.actionIndex == 7, "Break hit reaction");
                target.Stepped += a => actions.Add(a.actionIndex);
                Until(target, 0);
                Require(actions.Contains(9) && actions.Contains(10), "Break did not travel and rebound");
                checks.Add("Actual UseSkill/attack-box hits: armor resists, normal hurt recovers hold, launcher, airborne follow-ups, heavy hit, break -> moving knockback -> boundary rebound -> idle");

                target = Hit(g, 0, 232, 600, 14); Require(target.actionIndex == 15, "Boss guard reaction"); Until(target, 14);
                Require((int)Get(Fighter(g, 900), "hold") == OriginalAssets.Table<EnemyDefinition>("enemy").First(e => e.id == 232).hold && (int)Get(Fighter(g, 900), "defenseCounter") == 36, "Guard recovery values");
                target = Hit(g, 4, 232, 600, 14);
                Require(target.actionIndex == 16 && (int)Get(Fighter(g, 900), "hold") == 0, "Guard break failed");
                Until(target, 0);
                target = Hit(g, 0, 233); Until(target, 14);
                Require((int)Get(Fighter(g, 900), "defenseCounter") == 12, "Boss 233 defense duration"); Until(target, 0);
                target = Spawn(g, 232); PlaceNear(g, target); target.hp = target.maxHp / 4; target.PlayOnce(6); Until(target, 21);
                checks.Add("Boss guard hurt -> restored hold/guard duration, break -> action 16 -> recovery, Boss 233 12-tick guard and Boss 232 low-HP knockdown -> critical check");

                foreach (var pair in new[] { new[] {232,3600},new[] {233,1800},new[] {235,1800},new[] {236,1080},new[] {238,1440},new[] {239,720},new[] {241,3600},new[] {242,1800},new[] {244,1080},new[] {245,720} })
                {
                    target = Spawn(g, pair[0]);
                    var f = Fighter(g, 900); Require((bool)Get(f, "bossCriticalReady"), "Boss did not start ready");
                    Set(f, "bossCriticalReady", false); Set(f, "bossCriticalCounter", 0); Set(g, "aiElapsed", 0f);
                    CombatTick(g, pair[1] - 1); Require(!(bool)Get(f, "bossCriticalReady"), "Cooldown completed early " + pair[0]);
                    CombatTick(g); Require((bool)Get(f, "bossCriticalReady") && (int)Get(f, "bossCriticalCounter") == 0, "Cooldown boundary " + pair[0]);
                }
                checks.Add("All 10 Boss cooldowns start ready, remain unavailable through interval - 1, then rearm on the exact 80 ms tick (locked AI still accumulates cooldown)");

                target = Spawn(g, 238, false); PlaceNear(g, target); CombatTick(g);
                Require(target.actionIndex <= 1, "Full-HP Boss triggered");
                target.hp = target.maxHp / 4; target.locked = true; CombatTick(g, 4);
                Require(target.actionIndex <= 1, "Locked Boss triggered");
                target.locked = false; PlaceNear(g, target, 200); CombatTick(g);
                Require(target.actionIndex <= 1, "Distant Boss triggered");
                PlaceNear(g, target); Set(Fighter(g, 900), "bossCriticalReady", false); CombatTick(g);
                Require(target.actionIndex <= 1, "Cooling Boss triggered");
                Set(Fighter(g, 900), "bossCriticalReady", true);
                var loader = g.Machine.LoadScript;
                g.Machine.LoadScript = name => name == "validation-ai-wait" ? new[] { "x5E(20)" } : loader(name);
                g.Machine.Enqueue("validation-ai-wait"); g.Machine.Tick(); CombatTick(g);
                Require(target.actionIndex <= 1, "Boss initiated during script");
                g.Machine.Clear(); g.Machine.LoadScript = loader; Set(g, "waitUntil", 0f);
                actions.Clear(); target.Stepped += a => actions.Add(a.actionIndex);
                CombatTick(g); Require(target.actionIndex == 21, "Eligible Boss did not check");
                checks.Add("Boss 238 gates: full HP, AI lock, distance, cooldown and running script all block initiation; eligible near/low-HP Boss enters action 21 without x7E");
                Time.timeScale = 4; phase = 0;
                EditorApplication.update -= Advance; EditorApplication.update += Advance;
            }
            catch (Exception e) { Finish(false, e.ToString()); }
        }

        private static void StartCapture(GuJianGame g, bool protection)
        {
            var role = g.Actors[-1]; Set(Fighter(g, -1), "dead", false);
            role.hp = role.maxHp; role.ResetAction();
            g.Machine.Command(0x9C, new object[] { protection });
            var boss = Spawn(g, 242); PlaceNear(g, boss);
            capturedHp.Clear(); initialHp = role.hp; lastHit = 0; observedProjectile = null;
            g.Machine.Command(0x7E, new object[] { 900 });
        }
        private static void Advance()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            try
            {
                Require(EditorApplication.isPlaying && g, "Play mode ended");
                Require(g.LastError == null, g.LastError);
                Require(EditorApplication.timeSinceStartup - started < 100, "Timed out phase " + phase);
                if (g.DialogueVisible) { g.ContinueDialogue(); return; }
                if (phase == 0)
                {
                    OriginalActor activeBoss;
                    if (g.BossCriticalActive || (g.Actors.TryGetValue(900, out activeBoss) && activeBoss.actionIndex == 21)) return;
                    Require(actions.Contains(21) && actions.Contains(22) && actions.Contains(24), "Autonomous sequence missing actions");
                    Require(!g.Actors[-1].locked && (!g.Actors.TryGetValue(900, out activeBoss) || !(bool)Get(Fighter(g, 900), "bossCriticalReady")), "Boss finish did not unlock/cool down");
                    checks.Add("Real Unity Update: autonomous Boss 238 checks actual collision -> intro 22 -> attack 24 -> role unlock and cooldown");
                    var boss = Spawn(g, 235, false); PlaceNear(g, boss, 200);
                    actions.Clear(); boss.Stepped += a => actions.Add(a.actionIndex); phase = 1;
                }
                else if (phase == 1)
                {
                    if (!actions.Contains(26)) return;
                    Require(actions.Contains(25) && Mathf.Abs(g.Actors[900].mapPosition.x - g.Actors[-1].mapPosition.x) < 100, "Boss teleport failed");
                    checks.Add("Real Unity Update: distant Boss 235 charges six ticks, plays 25, teleports beside the role with map checks, then plays 26");
                    StartCapture(g, false); phase = 2;
                }
                else if (phase == 2 || phase == 3)
                {
                    var p = (OriginalActor)Get(g, "criticalProjectile");
                    if (p && p != observedProjectile)
                    {
                        observedProjectile = p;
                        p.Stepped += a =>
                        {
                            int hits = (int)Get(g, "criticalCaptureHits");
                            if (a.actionIndex == 25 && hits != lastHit)
                            {
                                lastHit = hits; capturedHp.Add(g.CurrentHp);
                            }
                        };
                    }
                    if (g.BossCriticalActive) return;
                    Require(capturedHp.Count == 11 && !g.Actors[-1].locked && !(bool)Get(g, "criticalCameraSaved") && !g.Actors[900].presentationHidden, "Capture hit count or cleanup mismatch: " + capturedHp.Count);
                    if (phase == 2)
                    {
                        for (int i = 0; i < 10; i++) Require(capturedHp[i] == initialHp - initialHp / 11 * (i + 1), "Incorrect equal damage hit " + i);
                        Require(g.CurrentHp == 0, "Eleventh hit did not consume remaining HP");
                        checks.Add("Real Unity Update: Boss 242 projectile 24 catches into 25; exactly ten initial-HP/11 hits, eleventh consumes all remaining HP, then projectile/Boss/camera/input restore");
                        StartCapture(g, true); phase = 3;
                    }
                    else
                    {
                        Require(g.CurrentHp == g.MaximumHp / 10, "Blood protection floor failed");
                        checks.Add("Real Unity Update: the same 11-hit capture respects x9C blood protection, ending at 10% HP and releasing the scene");
                        StartCapture(g, true); Stage(g); phase = 4;
                    }
                }
                else
                {
                    Require(!g.CriticalActive && !g.Actors[-1].locked && !UnityEngine.Object.FindObjectsOfType<OriginalActor>().Any(a => a.name.Contains("Boss绝杀投射物")), "Stage change left critical state/effect behind");
                    checks.Add("Changing stage during Boss critical cancels the sequence and removes temporary effects");
                    Finish(true, null);
                }
            }
            catch (Exception e) { Finish(false, "Phase " + phase + ": " + e); }
        }
        private static void Finish(bool passed, string error)
        {
            EditorApplication.update -= Advance; Time.timeScale = previousScale; UnityEngine.Random.state = previousRandom;
            var report = new ReconstructionValidation.Report { passed = passed, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/combat-behaviour.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_COMBAT_BEHAVIOUR_VALIDATION " + passed + " " + error);
        }
    }
}
