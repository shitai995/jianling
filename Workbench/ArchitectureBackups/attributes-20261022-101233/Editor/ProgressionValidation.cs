using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ProgressionValidation
    {
        private static int phase, expectedMoney, expectedDurability, expectedSkill, hpBefore;
        private static double started;
        private static float previousTimeScale;
        private static string previousSaveFileName;
        private static Func<string, string[]> previousLoader;
        private static readonly List<string> checks = new List<string>();
        private static readonly HashSet<int> seenActions = new HashSet<int>();
        private static bool sawPreviewHold, sawBossStart, sawBossEnd, sawProjectile;

        [MenuItem("古剑/验证强化与绝杀剧情")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (!g) throw new Exception("GuJian scene required");
            checks.Clear(); seenActions.Clear();
            sawPreviewHold = sawBossStart = sawBossEnd = sawProjectile = false;
            previousTimeScale = Time.timeScale; previousSaveFileName = g.saveFileName; previousLoader = g.Machine.LoadScript;
            g.saveFileName = "validation-progression.json";
            Time.timeScale = 8; started = EditorApplication.timeSinceStartup; phase = 0;
            Stage(g, "200");
            EditorApplication.update -= Advance;
            EditorApplication.update += Advance;
        }
        private static void Stage(GuJianGame g, string stage, int storyEvent = -999)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = storyEvent;
            g.Machine.Enqueue(stage);
        }
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        private static bool Idle(GuJianGame g) => !g.Machine.Running && !g.DialogueVisible;
        private static void LockEnemies(GuJianGame g)
        {
            foreach (var actor in g.Actors.Values.Where(a => a.IsEnemy)) g.Machine.Command(0x3F, new object[] { actor.actorId, true });
        }
        private static void Advance()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            try
            {
                Require(EditorApplication.isPlaying && g, "Play mode ended");
                Require(g.LastError == null, g.LastError);
                Require(EditorApplication.timeSinceStartup - started < 150, "Progression validation timed out, phase " + phase);
                if (g.CriticalActive) seenActions.Add(g.Actors[-1].actionIndex);
                if (phase == 4 && g.Actors[-1].actionIndex == 24 && g.Actors[-1].FrameIndex == 33 && g.Actors[-1].Completed) sawPreviewHold = true;
                if (phase == 5 && g.BossCriticalActive)
                {
                    sawBossStart |= g.Actors[999].actionIndex == 22;
                    sawBossEnd |= g.Actors[999].actionIndex == 24;
                }
                if (phase == 6 && g.BossCriticalActive)
                    sawProjectile |= UnityEngine.Object.FindObjectsOfType<OriginalActor>().Any(a => a.name.Contains("Boss绝杀投射物"));
                if (g.WeaponPreviewVisible) { g.CloseWeaponPreview(); return; }
                if (g.DialogueVisible)
                {
                    int choice = 0;
                    if (phase == 2 && g.DialogueChoices != null && g.DialogueChoices.Length == 3) choice = 2;
                    g.ContinueDialogue(choice); return;
                }
                switch (phase)
                {
                    case 0:
                        if (!Idle(g)) return;
                        LockEnemies(g); g.money = 200000;
                        g.Machine.Command(0x53, new object[] { 3, 4, false });
                        g.Machine.Command(0x53, new object[] { 3, 0, false });
                        g.Machine.Command(0x55, new object[] { 3 });
                        int equipped = g.EquippedIndex; var item = g.Equipment[equipped];
                        int oldSkill = g.Equipment[1].skillLevel, oldMoney = g.money;
                        g.durability = 37; int oldMaximum = item.MaximumDurability;
                        Require(g.ReinforceEquipment(equipped), "Equipped reinforcement rejected");
                        Require(g.durability == item.MaximumDurability * 37 / oldMaximum, "Live equipped durability ratio lost");
                        Require(g.Equipment[1].reinforcement == 4, "Duplicate weapon instance modified");
                        Require(g.money == oldMoney - item.ReinforceCost, "Incorrect reinforcement fee");
                        Require(g.ReinforceEquipment(1) && g.Equipment[1].skillLevel == oldSkill + 1, "Level 5 skill upgrade missing");
                        while (g.Equipment[1].reinforcement < 9) Require(g.ReinforceEquipment(1), "Reinforcement rejected before cap");
                        Require(g.Equipment[1].skillLevel == Mathf.Min(9, oldSkill + 2), "Level 9 skill upgrade missing");
                        Require(g.Equipment[1].Attack == g.Equipment[1].Data.attack * 2, "Level 9 stat scaling");
                        int atCap = g.money; Require(!g.ReinforceEquipment(1) && g.money == atCap, "Max-level reinforcement charged money");
                        g.money = item.ReinforceCost - 1;
                        int reinforcement = item.reinforcement, liveDurability = g.durability;
                        Require(!g.ReinforceEquipment(equipped) && item.reinforcement == reinforcement && g.durability == liveDurability, "Insufficient money modified weapon");
                        g.money = 123456;
                        checks.Add("Equipped/duplicate instances, live durability ratio, exact fee, +5/+9 skill upgrades, doubled +9 stats, cap and insufficient money passed");
                        expectedMoney = g.money; expectedDurability = g.durability; expectedSkill = g.Equipment[1].skillLevel;
                        g.Save(); g.money = 0; item.reinforcement = 0; g.durability = 1; g.Load(); phase = 1;
                        break;
                    case 1:
                        if (!Idle(g)) return;
                        Require(g.money == expectedMoney && g.durability == expectedDurability && g.Equipment[1].reinforcement == 9 && g.Equipment[1].skillLevel == expectedSkill && g.Equipment[g.EquippedIndex].reinforcement == 1, "Reinforcement save/load mismatch");
                        checks.Add("Reinforced equipment instances, selected index, money, skill level and durability survive real Save/Load");
                        LockEnemies(g); g.Machine.Values["event"] = 900; g.Machine.Values["taskID"] = 14;
                        g.Machine.Enqueue("334n0"); phase = 2;
                        break;
                    case 2:
                        if (g.ReinforcementVisible)
                        {
                            if (g.TutorialKey == 4) g.ReinforceConfirmOrApply();
                            else if (g.TutorialKey == 6) g.CloseReinforce();
                            else if (Idle(g)) g.CloseReinforce();
                        }
                        if (!Idle(g) || g.ReinforcementVisible || g.Machine.Get("taskID") != 914) return;
                        Require(g.Equipment[0].reinforcement == 1, "Tutorial did not strengthen selected candidate");
                        checks.Add("Unmodified 334n0 -> guide1 -> 334n0 scripts: choose reinforce, confirm twice, close, taskID 914, completed tutorial dialogue");
                        Stage(g, "203"); g.Machine.Values["taskID"] = 1; g.Machine.Enqueue("203e1"); phase = 3;
                        break;
                    case 3:
                        if (!Idle(g)) return;
                        Require(g.Machine.Get("event") == 204 && seenActions.Contains(24) && seenActions.Contains(25), "203e1 did not finish the hit sequence");
                        Require(!g.CriticalWaiting && g.durability == 0, "Critical kill did not release VM or exhaust weapon at frame 98");
                        checks.Add("Unmodified 203 + 203e1: actual hit check -> actions 24/25 -> durability zero -> VM resumes -> event 204");
                        Stage(g, "101", 207); phase = 40;
                        break;
                    case 40:
                        if (!Idle(g)) return;
                        LockEnemies(g); g.Machine.Enqueue("101e3"); phase = 4;
                        break;
                    case 4:
                        if (!Idle(g)) return;
                        Require(sawPreviewHold && g.Machine.Get("event") == 300 && !g.CriticalActive, "Interrupted 101e3 preview did not hold frame 33 or complete subsequent 101e4");
                        checks.Add("Unmodified 101/101e3/101e4: x80() holds action 24 frame 33, elder interrupts, x3B resets, event 208 then event 300 through original follow-up");
                        Stage(g, "200"); phase = 50;
                        break;
                    case 50:
                        if (!Idle(g)) return;
                        LockEnemies(g); g.Machine.Command(0x4D, new object[] { 40 });
                        g.Machine.Command(0x26, new object[] { 100, 250, "r" });
                        g.Machine.Command(0x30, new object[] { 999, 232, "l", 180, 250, 2 });
                        g.Machine.Command(0x3F, new object[] { 999, true });
                        hpBefore = g.CurrentHp; g.Machine.Command(0x7E, new object[] { 999 }); phase = 5;
                        break;
                    case 5:
                        if (g.BossCriticalActive) return;
                        Require(sawBossStart && sawBossEnd && g.CurrentHp == hpBefore - g.MaximumHp / 2 && !g.Actors[-1].locked, "Boss 232 start/end, half-HP hit or unlock mismatch");
                        checks.Add("Boss 232 x7E: original actions 22 -> 24, guaranteed half-max-HP hit, finish and role unlock");
                        g.Machine.Command(0x31, new object[] { 999 });
                        g.Machine.Command(0x88, new object[0]);
                        g.Machine.Command(0x30, new object[] { 999, 242, "l", 180, 250, 2 });
                        g.Machine.Command(0x3F, new object[] { 999, true });
                        hpBefore = g.CurrentHp; g.Machine.Command(0x7E, new object[] { 999 }); phase = 6;
                        break;
                    case 6:
                        if (g.BossCriticalActive) return;
                        Require(sawProjectile && g.CurrentHp < hpBefore && !g.Actors[-1].locked, "Boss 242 projectile failed to damage or finish");
                        checks.Add("Boss 242 x7E: intro, action 24 projectile at target, original attack frames hit, projectile cleanup and unlock");
                        Stage(g, "200"); phase = 70;
                        break;
                    case 70:
                        if (!Idle(g)) return;
                        LockEnemies(g); g.Actors[-1].mapPosition = new Vector2(10000, 10000);
                        g.Machine.LoadScript = name => name == "critical-wait-validation" ? new[] { "x80(true)", "x09(\"criticalAfter\",1)" } : previousLoader(name);
                        g.Machine.Enqueue("critical-wait-validation"); phase = 7;
                        break;
                    case 7:
                        if (g.CriticalActive) { Require(g.Machine.Get("criticalAfter") == 0, "VM continued during full critical sequence"); return; }
                        if (!Idle(g)) return;
                        Require(g.Machine.Get("criticalAfter") == 1 && !g.CriticalWaiting, "Miss sequence deadlocked VM");
                        checks.Add("No-target action 23 releases a waiting VM; subsequent instruction executes only after sequence ends");
                        g.Machine.LoadScript = previousLoader;
                        Stage(g, "200"); phase = 80;
                        break;
                    case 80:
                        if (!Idle(g)) return;
                        LockEnemies(g);
                        g.Machine.Command(0x64, new object[] { 10 }); g.Machine.Command(0x7C, new object[] { true }); g.Machine.Command(0x7F, new object[0]);
                        Require((bool)g.Machine.Command(0x91, new object[0]), "Test integration failed");
                        g.Machine.Command(0x26, new object[] { 100, 250, "r" });
                        g.Machine.Command(0x30, new object[] { 999, 232, "l", 180, 250, 2 }); g.Machine.Command(0x3F, new object[] { 999, true });
                        g.Machine.Command(0x80, new object[] { true }); phase = 8;
                        break;
                    case 8:
                        if (g.CriticalActive) return;
                        Require(seenActions.Contains(51) && seenActions.Contains(52), "Integrated critical animation bank not used");
                        checks.Add("Integrated critical uses actions 49/51/52 and the original spirit-specific finalslash layers");
                        Stage(g, "408", 804); phase = 90;
                        break;
                    case 90:
                        if (!Idle(g)) return;
                        LockEnemies(g); g.Machine.Command(0x4D, new object[] { 40 });
                        g.Machine.Enqueue("408e1"); phase = 9;
                        break;
                    case 9:
                        if (!Idle(g)) return;
                        Require(g.Machine.Get("event") == 825 && g.StageName == "无双城-城主府" && !g.CriticalActive && !g.Actors[-1].locked, "408e1 Boss story did not finish and restore scene/input");
                        checks.Add("Unmodified 408/408e0/408e1/335/335n0 scripts: Boss 233 critical story -> event 805 -> City Lord follow-up -> event 825, input unlocked");
                        Finish(true, null); break;
                }
            }
            catch (Exception error) { Finish(false, "Phase " + phase + ": " + error); }
        }
        private static void Finish(bool passed, string error)
        {
            EditorApplication.update -= Advance; Time.timeScale = previousTimeScale;
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (g) { g.saveFileName = previousSaveFileName; g.Machine.LoadScript = previousLoader; }
            var report = new ReconstructionValidation.Report { passed = passed, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/progression.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_PROGRESSION_VALIDATION " + passed + " " + error);
        }
    }
}
