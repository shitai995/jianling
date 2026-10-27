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
    public static class MissionValidation
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static readonly List<string> checks = new List<string>();
        private static object Get(object target, string name) => target.GetType().GetField(name, Flags).GetValue(target);
        private static object Call(GuJianGame g, string name, params object[] args) => typeof(GuJianGame).GetMethod(name, Flags).Invoke(g, args);
        private static IList Tasks(GuJianGame g) => (IList)Get(g, "tasks");
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        private static void Pump(GuJianGame g, int choice = 0)
        {
            // Deterministic script fixture: retain dialogue/choice blocking, skip presentation delays.
            for (int i = 0; i < 3000; i++)
            {
                if (g.DialogueVisible) g.ContinueDialogue(choice);
                else if (!g.Machine.Running) return;
                else g.Machine.Tick();
            }
            throw new Exception("Script did not settle: " + g.Machine.CurrentScript + ":" + g.Machine.Instruction);
        }
        private static void Stage(GuJianGame g, string stage)
        {
            g.Machine.Values["stageID"] = 334;
            g.Machine.Enqueue(stage); Pump(g);
            Require(g.Map != null && g.LastError == null, "Stage failed: " + stage);
        }
        private static void Reset(GuJianGame g, int task = 1)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = 999;
            g.Machine.Values["taskID"] = task; g.Machine.Values["days"] = 1; Stage(g, "335");
        }
        private static void Mission(GuJianGame g, bool branch, params int[] rewards)
        {
            g.Machine.Command(0x5A, new object[] { branch ? "支线验证" : "主线验证", "任务说明", rewards.Cast<object>().ToArray(), branch });
        }
        private static long TotalExperience(GuJianGame g)
        {
            long total = g.experience;
            for (int i = 1; i < g.level; i++) total += (long)i * i * i * 25;
            return total;
        }
        private static void Interact(GuJianGame g, int id)
        {
            Require(g.Actors.ContainsKey(id) && !g.Actors[id].hidden, "Missing interactable " + id);
            g.Actors[-1].mapPosition = g.Actors[id].mapPosition + new Vector2(8, 0);
            Call(g, "Interact"); Pump(g, -1);
        }
        private static void RoundTrip(GuJianGame g)
        {
            g.Save(); g.Load(); Pump(g, -1);
        }
        [MenuItem("古剑/验证支线任务与奖励存档")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            string previousSave = g.saveFileName, error = null;
            var previousWaiting = g.Machine.IsWaiting;
            var random = UnityEngine.Random.state; float scale = Time.timeScale;
            checks.Clear(); Time.timeScale = 0; g.saveFileName = "validation-missions.json";
            g.Machine.IsWaiting = () => g.DialogueVisible;
            try
            {
                Reset(g); Mission(g, false, 123, 456, 0, 999); Mission(g, true, 789);
                Require(g.MissionReward(false, 0) == 123 && g.MissionReward(false, 1) == 456 && g.MissionReward(false, 2) == 0 && g.MissionReward(true, 0) == 789 && g.MissionReward(true, 1) == -1 && g.MissionReward(true, 2) == -1, "Reward normalization/main-branch independence");
                Require(g.money == 0 && TotalExperience(g) == 0 && g.Equipment.Count == 1, "Setting mission metadata granted rewards");
                RoundTrip(g);
                Require(g.MissionTitle == "主线验证" && g.BranchMissionTitle == "支线验证" && g.MissionReward(false, 2) == 0 && g.MissionReward(true, 0) == 789, "Mission metadata round-trip");
                string path = Path.Combine(Application.persistentDataPath, g.saveFileName);
                string legacy = System.Text.RegularExpressions.Regex.Replace(File.ReadAllText(path), "\"(mainRewards|branchRewards)\"\\s*:\\s*\\[[^\\]]*\\]\\s*,?", "");
                File.WriteAllText(path, legacy); g.Load(); Pump(g, -1);
                Require(Enumerable.Range(0, 3).All(i => g.MissionReward(false, i) == -1 && g.MissionReward(true, i) == -1) && g.money == 0 && TotalExperience(g) == 0, "Legacy v3 reward defaults");
                checks.Add("Reward arrays normalize to three slots, retain equipment ID 0, keep main/branch independent and grant nothing; Save/Load preserves metadata; existing v3 saves without reward fields restore -1 in all slots");

                int[] ids = { 1,2,3,4,5,6,7,8,9,10,11,12,13,101,102,103,104,105,106 };
                int[] exp = { 20000,10000,35000,50000,20000,100000,150000,30000,200000,50000,300000,400000,800000,50000,80000,500000,250000,500000,500000 };
                int[] money = { 400,1000,700,900,2000,1800,2400,3000,3000,5000,3800,4500,9000,2000,3000,6000,5000,6000,6000 };
                int[] weapons = { -1,-1,-1,-1,-1,4,6,-1,9,-1,-1,-1,-1,-1,-1,7,10,12,13 };
                int[] deliveryStages = {407,705,612,816}, deliveryActors = {60,72,45,79};
                for (int n = 0; n < ids.Length; n++)
                {
                    int id = ids[n]; Reset(g, id); g.Machine.Enqueue("taskdisc"); Pump(g);
                    Require(g.Machine.Get("taskID") == id && !string.IsNullOrWhiteSpace(g.BranchMissionTitle), "Accept task " + id);
                    Require(g.MissionReward(true, 0) == exp[n] && g.MissionReward(true, 1) == money[n], "Advertised rewards " + id);
                    Require(g.money == 0 && TotalExperience(g) == 0 && g.Equipment.Count == 1, "Premature acceptance reward " + id);
                    string mode;
                    if (new[] {2,5,8,10}.Contains(id))
                    {
                        int k = Array.IndexOf(new[] {2,5,8,10}, id);
                        Stage(g, deliveryStages[k].ToString());
                        Require(g.Actors[deliveryActors[k]].interactionScript == "taskover.s", "Merchant callback " + id);
                        Interact(g, deliveryActors[k]); mode = "original merchant interaction at " + deliveryStages[k];
                    }
                    else
                    {
                        Require(Tasks(g).Count == 1, "Missing kill task " + id);
                        object task = Tasks(g)[0]; var stages = (int[])Get(task, "stages"); var enemies = (int[])Get(task, "enemyIds"); var counts = (int[])Get(task, "counts");
                        string before = string.Join("|", g.MissionObjectiveLabels());
                        g.Machine.Values["stageID"] = -999; Call(g, "CompleteKillTasks", enemies[0]);
                        Require(before == string.Join("|", g.MissionObjectiveLabels()), "Wrong-stage kill counted " + id);
                        g.Machine.Values["stageID"] = stages[0]; Call(g, "CompleteKillTasks", -999);
                        Require(before == string.Join("|", g.MissionObjectiveLabels()), "Wrong enemy counted " + id);
                        if (id == 3) Require(enemies.SequenceEqual(new[] {210,213}) && counts.SequenceEqual(new[] {10,20}), "Original task 3 arrays changed");
                        if (counts[0] > 1)
                        {
                            Call(g, "CompleteKillTasks", enemies[0]); var remaining = (int[])counts.Clone();
                            g.Machine.Values["stageID"] = 334; RoundTrip(g);
                            task = Tasks(g)[0]; stages = (int[])Get(task, "stages"); enemies = (int[])Get(task, "enemyIds"); counts = (int[])Get(task, "counts");
                            Require(counts.SequenceEqual(remaining) && g.MissionReward(true, 0) == exp[n], "Remaining counts round-trip " + id);
                            g.Machine.Values["stageID"] = stages[0];
                        }
                        int kills = counts.Sum();
                        for (int k = 0; k < enemies.Length; k++)
                        {
                            int remaining = counts[k];
                            for (int j = 0; j < remaining; j++) Call(g, "CompleteKillTasks", enemies[k]);
                        }
                        Pump(g); Require(Tasks(g).Count == 0, "Completed kill task retained " + id);
                        mode = "production kill-counter fixture; " + kills + " remaining kills";
                    }
                    Require(g.Machine.Get("taskID") == id + 900 && g.BranchMissionInfo.Contains("已完成") && g.money == 0 && TotalExperience(g) == 0 && g.Equipment.Count == 1, "Completion state/premature reward " + id);
                    Stage(g, "335"); Interact(g, 89);
                    Require(g.Machine.Get("taskID") == -1 && g.money == money[n] && TotalExperience(g) == exp[n], "Claim XP/money " + id);
                    Require(g.Equipment.Count == (weapons[n] < 0 ? 1 : 2) && (weapons[n] < 0 || g.Equipment[1].id == weapons[n]), "Claim weapon " + id);
                    Require(string.IsNullOrWhiteSpace(g.BranchMissionTitle) && Tasks(g).Count == 0 && g.MissionObjectiveLabels().Length == 0, "Claim cleanup " + id);
                    long total = TotalExperience(g); int balance = g.money, bag = g.Equipment.Count;
                    Interact(g, 89);
                    Require(total == TotalExperience(g) && balance == g.money && bag == g.Equipment.Count && g.Machine.Get("taskID") == -1, "Repeated interaction granted twice " + id);
                    RoundTrip(g);
                    Require(g.money == balance && TotalExperience(g) == total && g.Equipment.Count == bag && g.Machine.Get("taskID") == -1 && string.IsNullOrWhiteSpace(g.BranchMissionTitle), "Post-claim save " + id);
                    checks.Add("Task " + id + ": accept -> " + mode + " -> completed/unpaid -> NPC claim " + exp[n] + " XP / " + money[n] + " silver / weapon " + weapons[n] + "; repeat interaction and Save/Load retain exactly one reward");
                }
                Reset(g, 1); g.Machine.Enqueue("taskdisc"); Pump(g);
                g.Machine.Enqueue("335n0"); Pump(g, 1);
                Require(g.Machine.Get("taskID") == 1 && Tasks(g).Count == 1, "Declining cancel removed task");
                g.Machine.Enqueue("335n0"); Pump(g);
                Require(g.Machine.Get("taskID") == -1 && Tasks(g).Count == 0 && string.IsNullOrWhiteSpace(g.BranchMissionTitle) && g.money == 0 && TotalExperience(g) == 0, "Cancel cleanup/rewards");
                checks.Add("Original NPC cancel choice: No retains the task; Yes clears branch metadata and all kill objectives without rewards");

                Reset(g, 6); g.Machine.Enqueue("taskdisc"); Pump(g);
                Mission(g, false, 20000, 1000, 2);
                g.Machine.Command(0x67, new object[0]);
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                g.Machine.IsWaiting = previousWaiting; g.saveFileName = previousSave; Time.timeScale = scale; UnityEngine.Random.state = random;
                Directory.CreateDirectory("../Reference/Validation");
                File.WriteAllText("../Reference/Validation/missions.json", JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
                Debug.Log("GUJIAN_MISSION_VALIDATION " + (error == null) + " " + error);
            }
        }
    }
}
