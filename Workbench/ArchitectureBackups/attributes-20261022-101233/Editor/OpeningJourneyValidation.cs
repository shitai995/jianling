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
    public static partial class OpeningJourneyValidation
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> checks = new List<string>();
        private static readonly HashSet<string> scripts = new HashSet<string>();
        private static readonly HashSet<int> events = new HashSet<int>();
        private static double started, nextAction;
        private static float previousScale;
        private static string previousSave;
        private static Action<string> previousTrace;
        private static int attacks, pages, previews, portals, interactions, savedMoney, savedLevel;
        private static bool restoring;
        private static bool cityJourney, yuanbaoJourney, cityChecked, openingChecked, sawNormalCritical, sawIntegratedCritical, defeated;
        private static int travels, tutorialPages;
        private static bool tutorialReturned;
        private static object Get(object o, string name) => o.GetType().GetField(name, Flags | BindingFlags.Public).GetValue(o);
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        [MenuItem("古剑/验证新档剑冢连续流程")]
        public static void Run()
        { Start(false); }
        [MenuItem("古剑/验证新档至清溪村连续流程")]
        public static void RunToQingxi()
        { Start(true); }
        [MenuItem("古剑/验证新档至元宝湾连续流程")]
        public static void RunToYuanbao()
        { Start(true, true); }
        [MenuItem("古剑/验证新档至结局连续主线")]
        public static void RunFullJourney() { Start(true, true, true); }
        private static void Start(bool throughCity, bool throughQingxi = false, bool throughEnding = false)
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            previousScale = Time.timeScale; previousSave = g.saveFileName; previousTrace = g.Machine.Trace;
            fullJourney = throughEnding; fullChecked = fullCheckpoint = fullRestoring = false; fullBossDeaths = shopTutorialSteps = 0; fullMilestones.Clear();
            cityJourney = throughCity; yuanbaoJourney = throughQingxi;
            cityChecked = openingChecked = sawNormalCritical = sawIntegratedCritical = defeated = false; travels = 0;
            tutorialPages = 0; tutorialReturned = false;
            g.saveFileName = fullJourney ? "validation-full-journey.json" : yuanbaoJourney ? "validation-yuanbao-journey.json" : cityJourney ? "validation-city-journey.json" : "validation-opening-journey.json";
            Time.timeScale = 12; checks.Clear(); scripts.Clear(); events.Clear();
            attacks = pages = previews = portals = interactions = 0; restoring = false;
            started = EditorApplication.timeSinceStartup; nextAction = 0;
            g.Begin();
            g.Machine.Trace = line => { int colon = line.IndexOf(':'); if (colon > 0) scripts.Add(line.Substring(0, colon)); };
            EditorApplication.update -= Advance; EditorApplication.update += Advance;
        }
        private static void TalkTo(GuJianGame g, int id)
        {
            var npc = g.Actors[id];
            Require(!npc.hidden && !string.IsNullOrEmpty(npc.interactionScript), "NPC unavailable: " + id);
            g.Actors[-1].mapPosition = npc.mapPosition + new Vector2(8, 0);
            typeof(GuJianGame).GetMethod("Interact", Flags).Invoke(g, null);
            interactions++; nextAction = EditorApplication.timeSinceStartup + .1;
        }
        private static void Portal(GuJianGame g, string destination)
        {
            var transitions = (Dictionary<Vector2Int, string>)Get(g, "transitions");
            var candidates = transitions.Where(p => p.Value == destination + ".s").ToArray();
            Require(candidates.Length > 0, "Missing registered exit " + destination);
            var cell = candidates[0].Key;
            g.Actors[-1].mapPosition = new Vector2(cell.x * g.Map.cellWidth + 1, cell.y * g.Map.cellHeight + 1);
            // Let the normal Update/HandleInput detect entry into the registered portal cell.
            portals++; nextAction = EditorApplication.timeSinceStartup + .2;
        }
        private static void Hit(GuJianGame g, OriginalActor target)
        {
            var role = g.Actors[-1];
            if (role.LogicalAction > 1 || role.locked || role.hidden) return;
            g.Machine.Command(0x9C, new object[] { true });
            target.hp = 1;
            role.mapPosition = target.mapPosition + new Vector2(-36, target.Animation.footHeight - role.Animation.footHeight);
            role.Face("r"); g.UseSkill(0, 1, true); attacks++;
        }
        private static void Advance()
        {
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            try
            {
                Require(EditorApplication.isPlaying && g, "Play mode ended");
                Require(g.LastError == null, g.LastError);
                Require(EditorApplication.timeSinceStartup - started < (fullJourney ? 1800 : cityJourney ? 420 : 240), "Journey timeout: event=" + g.Machine.Get("event") + " stage=" + g.Machine.Get("stageID") + " script=" + g.Machine.CurrentScript + ":" + g.Machine.Instruction);
                int storyEvent = g.Machine.Get("event");
                int stage; int.TryParse((string)Get(g, "currentStageScript"), out stage);
                events.Add(storyEvent);
                // Fixture locks AI but retains production animation, respawn, hit and death handling.
                foreach (var enemy in g.Actors.Values.Where(a => a.IsEnemy)) enemy.locked = true;
                if (g.WeaponPreviewVisible) { g.CloseWeaponPreview(); previews++; return; }
                if (stage == 403 && g.CriticalActive)
                {
                    if (g.Actors[-1].actionBank == 27) sawIntegratedCritical = true; else sawNormalCritical = true;
                }
                if (g.DialogueVisible)
                {
                    int choice = 0;
                    var choices = g.DialogueChoices;
                    if (cityJourney && choices != null)
                    {
                        int reinforce = Array.FindIndex(choices, s => s == "强化武器");
                        if (reinforce >= 0) choice = reinforce;
                        int learned = Array.FindIndex(choices, s => s == "是，学会了");
                        if (yuanbaoJourney && learned >= 0) choice = learned;
                    }
                    g.ContinueDialogue(choice); pages++; return;
                }
                if (cityJourney && g.ReinforcementVisible)
                {
                    if (g.TutorialKey == 4) g.ReinforceConfirmOrApply();
                    else if (g.TutorialKey == 6) g.CloseReinforce();
                    else if (!g.Machine.Running) g.CloseReinforce();
                    return;
                }
                if (g.TutorialKey >= 0)
                {
                    if (fullJourney && g.Machine.CurrentScript == "guide3")
                    {
                        if (g.TutorialKey == 0)
                        {
                            g.RespondTutorial(0); shopTutorialSteps++;
                            int expected = shopTutorialSteps == 1 ? 23 : 24 - shopTutorialSteps;
                            Require(g.ShopSelectedItemId == expected, "Shop selection did not wrap to actual medicine: " + g.ShopSelectedItemId);
                        }
                        else { Require(shopTutorialSteps == 8 && g.CloseMenu(), "Shop tutorial return failed"); }
                    }
                    else if (yuanbaoJourney && g.Machine.CurrentScript == "guide4")
                    {
                        if (g.TutorialKey == 3)
                        {
                            Require(!g.SelectMenuPage(4) && !g.CloseMenu(), "Tutorial accepts unrelated tab or early close");
                            int expected = new[] { 2, 1, 6 }[tutorialPages];
                            if (tutorialPages == 1) Require(g.SelectMenuPage(expected), "Tutorial tab click rejected");
                            else g.RespondTutorial(3);
                            Require((int)Get(g, "menuPage") == expected, "Tutorial did not switch actual menu page");
                            tutorialPages++;
                        }
                        else { Require(g.TutorialKey == 6 && tutorialPages == 3 && g.CloseMenu(), "Tutorial return rejected"); tutorialReturned = true; }
                    }
                    else g.RespondTutorial(g.TutorialKey);
                    return;
                }
                if (cityJourney && (bool)Get(g, "menu") && (int)Get(g, "menuPage") == 4)
                {
                    int destination = fullJourney && storyEvent >= 601 ? FullArea(storyEvent) : storyEvent == 300 ? 1 : storyEvent == 400 ? 2 : storyEvent == 500 ? 3 : storyEvent == 600 ? 4 : -1;
                    Require(destination >= 0 && g.TravelToArea(destination), "World-map selection unavailable: " + storyEvent);
                    travels++; return;
                }
                if (g.Machine.Running || g.CriticalActive || EditorApplication.timeSinceStartup < nextAction) return;
                if (fullJourney && fullChecked)
                {
                    if (AdvanceFull(g, stage, storyEvent)) return;
                }
                if (storyEvent == 300 && (!cityJourney || !openingChecked))
                {
                    if (!restoring)
                    {
                        Require(events.Contains(200) && events.Contains(202) && events.Contains(203) && events.Contains(204) && events.Contains(205) && events.Contains(207) && events.Contains(208), "Missing continuous story milestone");
                        Require(new[] {"200e1","201e1","202n0","203e1","204e1","204e3","fufei1","100e2","101e3","101e4"}.All(s => scripts.Contains(s)), "Missing original story callback");
                        Require(g.Actors.ContainsKey(-2) && g.MissionTitle == "前往无双城" && g.MissionReward(false, 0) == 20000, "Main mission/spirit missing");
                        checks.Add("One Begin/start -> events 200..208 -> 300 with original NPC interactions, registered portal triggers and production attack rectangles; no story-event writes or direct follow-up script queues");
                        checks.Add("Original 201 two-wave 14/20 kills, 203 two-wave 16/20 kills, three sword previews, sword-spirit sequence, 204/101 special-health callbacks and elder interruption all completed under real Update");
                        if (cityJourney) { openingChecked = true; return; }
                        savedMoney = g.money; savedLevel = g.level; g.Save(); g.Load(); restoring = true; return;
                    }
                    Require(g.money == savedMoney && g.level == savedLevel && g.Actors.ContainsKey(-2) && g.MissionTitle == "前往无双城" && g.MissionReward(false, 0) == 20000 && g.MissionReward(false, 1) == 0, "Journey final save/load mismatch");
                    checks.Add("End-of-journey Save/Load restores event 300, real earned silver/level, sword spirit, main mission and reward slots");
                    checks.Add("Pages=" + pages + "; previews=" + previews + "; interactions=" + interactions + "; portals=" + portals + "; fixture attack attempts=" + attacks + "; scripts=" + string.Join(",", scripts.OrderBy(s => s)));
                    Finish(null); return;
                }
                if (yuanbaoJourney && !fullChecked && cityChecked && AdvanceQingxi(g, stage, storyEvent)) return;
                if (cityJourney && openingChecked && !cityChecked && AdvanceCity(g, stage, storyEvent)) return;
                if (stage == 200 && storyEvent == 200 && g.Actors.ContainsKey(103)) { TalkTo(g, 103); return; }
                if (stage == 200 && storyEvent == 201) { Portal(g, "200_201"); return; }
                if (stage == 201 && storyEvent == 202) { Portal(g, "201_202"); return; }
                if (stage == 202 && storyEvent == 202) { TalkTo(g, 99); return; }
                if (stage == 202 && storyEvent == 203) { Portal(g, "202_203"); return; }
                if (stage == 203 && storyEvent == 204) { Portal(g, "203_204"); return; }
                if (stage == 204 && storyEvent == 204)
                {
                    var npc = new[] {97,98,100}.FirstOrDefault(id => g.Actors.ContainsKey(id) && !g.Actors[id].hidden && !string.IsNullOrEmpty(g.Actors[id].interactionScript));
                    if (npc != 0) TalkTo(g, npc);
                    return;
                }
                if (stage == 204 && storyEvent == 205) { Hit(g, g.Actors[114]); return; }
                if (stage == 101 && storyEvent == 207) { Hit(g, g.Actors[68]); return; }
                var tasks = (IList)Get(g, "tasks");
                if (tasks.Count == 0) return;
                var task = tasks[0]; var enemyIds = (int[])Get(task, "enemyIds"); var counts = (int[])Get(task, "counts");
                var remaining = enemyIds.Where((id, i) => counts[i] > 0).ToArray();
                var target = g.Actors.Values.FirstOrDefault(a => a.IsEnemy && !a.hidden && a.hp > 0 && remaining.Contains(a.definitionId));
                if (target) Hit(g, target);
            }
            catch (Exception error) { Finish(error.ToString()); }
        }
        private static bool AdvanceCity(GuJianGame g, int stage, int storyEvent)
        {
            if (storyEvent < 300) return false;
            if (stage == 101 && storyEvent == 300) { Portal(g, "101_1"); return true; }
            if (stage == 334 && storyEvent == 300)
            {
                if (g.Machine.Get("sub1") == 0) TalkTo(g, 41);
                else if (g.Machine.Get("sub2") == 0) TalkTo(g, 40);
                else Portal(g, "334_335");
                return true;
            }
            if (stage == 335 && storyEvent == 300) { TalkTo(g, 89); return true; }
            if (stage == 335 && storyEvent == 400) { Portal(g, "335_1"); return true; }
            if (stage == 400 && storyEvent == 401) { Portal(g, "400_401"); return true; }
            if (stage == 401 && storyEvent == 401) { Portal(g, "401_402"); return true; }
            if (stage == 402 && storyEvent == 401) { Portal(g, "402e1"); return true; }
            if (stage == 402 && storyEvent == 402) { Portal(g, "402_403"); return true; }
            if (stage == 403 && storyEvent == 403 && !defeated)
            {
                // Force the designed loss through production damage/death handling, retaining original callbacks.
                g.Machine.Command(0x9C, new object[] { false });
                var fighters = (IDictionary)Get(g, "fighters");
                typeof(GuJianGame).GetMethod("Damage", Flags).Invoke(g, new object[] { fighters[-1], g.Actors[-1].hp, false });
                defeated = true; return true;
            }
            if (stage == 334 && storyEvent == 405) { Portal(g, "334_335"); return true; }
            if (stage == 335 && storyEvent == 405) { TalkTo(g, 89); return true; }
            if (storyEvent == 500)
            {
                if (stage == 335)
                {
                    if (g.Machine.Get("taskID") == 914 || (g.Machine.Get("days") == -1 && g.Machine.Get("taskID") != 14)) TalkTo(g, 89);
                    else if (g.Machine.Get("taskID") == 14) Portal(g, "335_334");
                    else Portal(g, "335_1");
                }
                else if (stage == 334)
                {
                    if (g.Machine.Get("taskID") == 14) TalkTo(g, 41);
                    else Portal(g, "334_335");
                }
                return true;
            }
            if (stage == 500 && storyEvent == 501)
            {
                Require(defeated && sawNormalCritical && sawIntegratedCritical, "Missing designed loss or two original critical sequences");
                Require(new[] {"334n0","334n1","335n0","400e0","402e1","403e0","403e2","403e4","334e1","tasklist","taskdisc","guide1","taskover","500e0"}.All(s => scripts.Contains(s)), "Missing continuous city callback");
                Require(g.Machine.Get("taskID") == -1 && g.Machine.Get("days") == 1 && g.Equipment.Any(e => e.id == 2) && g.Equipment.Any(e => e.reinforcement > 0), "Mandatory reinforcement reward missing");
                if (!restoring)
                {
                    checks.Add("New game -> city merchants -> lord -> Zicaowu 400/401/402/403 through registered portals and world-map choices; original designed loss and normal/integrated critical choreography -> event405");
                    checks.Add("Lord main reward -> event500 -> tasklist/taskdisc grants Longyuan -> guide1 reinforcement -> lord taskover reward -> world map -> Qingxi event501, without story-event writes or direct follow-up queues");
                    if (yuanbaoJourney) { cityChecked = true; return true; }
                    savedMoney = g.money; savedLevel = g.level; g.Save(); g.Load(); restoring = true; return true;
                }
                Require(g.money == savedMoney && g.level == savedLevel && g.MissionTitle == "阻击" && g.MissionReward(false, 0) == 50000 && g.MissionReward(false, 1) == 5000, "City final save/load mismatch");
                var open = (bool[])Get(g, "openAreas");
                Require(open[0] && open[1] && open[2] && open[3] && open[10] && !open[9], "Visited areas lost or locked area opened");
                checks.Add("Final real Save/Load retains event501, earned money/level, reinforcement and required task reward, mission rewards and visited areas; unvisited Liejian remains closed");
                checks.Add("Pages=" + pages + "; previews=" + previews + "; interactions=" + interactions + "; portals=" + portals + "; world-map travels=" + travels + "; fixture attack attempts=" + attacks + "; scripts=" + string.Join(",", scripts.OrderBy(s => s)));
                Finish(null); return true;
            }
            return true;
        }
        private static bool AdvanceQingxi(GuJianGame g, int stage, int storyEvent)
        {
            if (storyEvent == 501)
            {
                if (stage == 500) { Portal(g, "500_501"); return true; }
                if (stage == 501) { Portal(g, "501_502"); return true; }
                if (stage == 502) { Portal(g, "502_503"); return true; }
                if (stage == 503) { Portal(g, "503_505"); return true; }
                // Stage 505 uses the shared production attack/task loop below.
                return stage != 505;
            }
            if (stage == 335 && storyEvent == 503) { TalkTo(g, 89); return true; }
            if (stage == 335 && storyEvent == 600) { Portal(g, "335_1"); return true; }
            if (stage == 600 && storyEvent == 601)
            {
                Require(new[] { "501", "guide2", "503", "guide4", "505e0", "505e1", "600e0" }.All(s => scripts.Contains(s)), "Missing Qingxi continuous callback");
                Require(events.Contains(503) && events.Contains(600) && g.Machine.Get("sub2") == 0, "Qingxi combat or tutorial milestone missing");
                Require(tutorialPages == 3 && tutorialReturned, "Spirit menu tutorial did not complete");
                Require(g.MissionTitle == "怒火" && g.MissionReward(false, 0) == 70000 && g.MissionReward(false, 1) == 5000, "Yuanbao mission/reward mismatch");
                if (!restoring)
                {
                    checks.Add("Qingxi registered exits 500/501/502/503/505 -> original critical and spirit tutorials -> 505 production attack/task death loop (10/6/2 enemies) -> original 505e1 returns to lord at event503");
                    checks.Add("Original guide4 runs while its menu stays open; right-key and actual tab actions visit equipment/backpack/spirit pages, unrelated tabs and early close reject, return completes tutorial and resumes normal travel");
                    checks.Add("Lord interaction completes Qingxi main reward and accepts Fury -> registered city exit -> world-map area4 -> original Yuanbao entry event601; no story-event writes or direct follow-up queues");
                    if (fullJourney) { fullChecked = true; return true; }
                    savedMoney = g.money; savedLevel = g.level; g.Save(); g.Load(); restoring = true; return true;
                }
                Require(g.money == savedMoney && g.level == savedLevel && g.Machine.Get("sub2") == 0 && g.Equipment.Any(e => e.id == 2) && g.Equipment.Any(e => e.reinforcement > 0), "Yuanbao final save/load mismatch");
                var open = (bool[])Get(g, "openAreas");
                Require(open[0] && open[1] && open[2] && open[3] && open[4] && open[10] && !open[9], "Yuanbao visited areas mismatch");
                checks.Add("Final real Save/Load retains event601, earned money/level, completed tutorials, reinforcement, Fury mission rewards and visited areas");
                checks.Add("Pages=" + pages + "; previews=" + previews + "; interactions=" + interactions + "; portals=" + portals + "; world-map travels=" + travels + "; fixture attack attempts=" + attacks + "; scripts=" + string.Join(",", scripts.OrderBy(s => s)));
                Finish(null); return true;
            }
            return true;
        }
        private static void Finish(string error)
        {
            EditorApplication.update -= Advance; Time.timeScale = previousScale;
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (g) { g.saveFileName = previousSave; if (g.Machine != null) g.Machine.Trace = previousTrace; }
            File.WriteAllText("../Reference/Validation/" + (fullJourney ? "full-journey.json" : yuanbaoJourney ? "yuanbao-journey.json" : cityJourney ? "city-journey.json" : "opening-journey.json"), JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            Debug.Log("GUJIAN_OPENING_JOURNEY " + (error == null) + " " + error);
        }
    }
}
