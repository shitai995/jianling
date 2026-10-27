using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static partial class OpeningJourneyValidation
    {
        private static bool fullJourney, fullChecked, fullCheckpoint, fullRestoring;
        private static int fullBossDeaths, shopTutorialSteps;
        private static readonly HashSet<int> fullMilestones = new HashSet<int>();
        private static Dictionary<int, string[]> journeyEdges;
        private static readonly Dictionary<string, int> edgeDestinations = new Dictionary<string, int>();
        private static int FullArea(int e)
        {
            if (e == 700) return 5;
            if (e == 800 || e == 802 || e == 835) return 6;
            if (e == 825) return 4;
            if (e == 828) return 3;
            if (e == 831) return 5;
            if (e == 900) return 7;
            if (e == 1000 || e == 1054 || e == 1061) return 8;
            if (e == 1002 || e == 1068) return 1;
            if (e == 1100) return 9;
            if (e == 1181) return 12;
            if (e == 1285) return 11;
            return -1;
        }
        private static int EdgeDestination(string edge)
        {
            int value;
            if (edgeDestinations.TryGetValue(edge, out value)) return value;
            string path = "Assets/Resources/Original/script/" + edge + ".txt";
            if (!File.Exists(path)) return -1;
            var match = Regex.Match(File.ReadAllText(path), "x20\\(\"(\\d+)\\.s\"");
            value = match.Success ? int.Parse(match.Groups[1].Value) : -1;
            edgeDestinations[edge] = value; return value;
        }
        private static void Navigate(GuJianGame g, int stage, int target)
        {
            if (stage == target) return;
            if (journeyEdges == null)
            {
                journeyEdges = new Dictionary<int, string[]>();
                foreach (string path in Directory.GetFiles("Assets/Resources/Original/script", "*.txt"))
                {
                    int id; if (!int.TryParse(Path.GetFileNameWithoutExtension(path), out id)) continue;
                    journeyEdges[id] = Regex.Matches(File.ReadAllText(path), "x24\\([^\\n]*?\"([^\"]+)\\.s\"").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToArray();
                }
            }
            var actual = ((Dictionary<Vector2Int, string>)Get(g, "transitions")).Values.Select(x => Path.GetFileNameWithoutExtension(x)).Distinct();
            string best = null; int distance = int.MaxValue;
            foreach (string first in actual)
            {
                int start = EdgeDestination(first); if (start < 0) continue;
                var queue = new Queue<int>(); var depths = new Dictionary<int, int>(); queue.Enqueue(start); depths[start] = 1;
                while (queue.Count > 0)
                {
                    int at = queue.Dequeue();
                    if (at == target) { if (depths[at] < distance) { best = first; distance = depths[at]; } break; }
                    string[] edges; if (!journeyEdges.TryGetValue(at, out edges)) continue;
                    foreach (string edge in edges)
                    {
                        int to = EdgeDestination(edge);
                        if (to < 0 || depths.ContainsKey(to)) continue;
                        depths[to] = depths[at] + 1; queue.Enqueue(to);
                    }
                }
            }
            Require(best != null, "No registered route " + stage + " -> " + target);
            Portal(g, best);
        }
        private static bool AdvanceFull(GuJianGame g, int stage, int e)
        {
            if (g.AtTitle)
            {
                Require(e == 1300 && scripts.Contains("endding"), "Unexpected title before original ending");
                Require(new[] { 601,603,700,701,702,800,801,802,804,805,825,827,828,830,831,834,835,836,837,900,1000,1001,1002,1051,1052,1054,1055,1061,1062,1067,1068,1069,1071,1073,1100,1101,1105,1180,1181,1182,1183,1184,1185,1282,1285,1289,1300 }.All(events.Contains), "Missing full-journey event milestone: " + string.Join(",", events.OrderBy(x=>x)));
                Require(shopTutorialSteps == 8 && fullCheckpoint, "Shop tutorial/checkpoint missing");
                checks.Add("One new game through Yuanbao, Chaoyin, Qixia, three Ghost missions, Moye duel/escape, Giant Whale, docks, city evacuation/reconquest, manor crisis, Liejian five bosses, Whale counterattack, Tiancao and original ending/title; no direct story-event writes or follow-up queues");
                checks.Add("Original guide3 shop selected 23 -> 22..16 with eight actual up actions and actual return; subsequent city gate opened from original sub1 callback");
                checks.Add("Checkpoint Save/Load inside the same continuous journey retains mission, earned money/level, then continues to the original ending");
                checks.Add("Pages=" + pages + "; previews=" + previews + "; NPC interactions=" + interactions + "; registered portals=" + portals + "; area choices=" + travels + "; attack attempts=" + attacks + "; fixture boss deaths=" + fullBossDeaths + "; events=" + string.Join(",", events.OrderBy(x=>x)) + "; scripts=" + string.Join(",",scripts.OrderBy(x=>x)));
                Finish(null); return true;
            }
            if (new[] {700,800,825,835,900,1054,1071,1100,1180,1285}.Contains(e) && fullMilestones.Add(e))
                Debug.Log("GUJIAN_FULL_JOURNEY_MILESTONE event=" + e + " stage=" + stage);
            if (e == 1000 && stage == 1000 && !fullCheckpoint)
            {
                savedMoney = g.money; savedLevel = g.level; g.Save(); g.Load(); fullCheckpoint = fullRestoring = true; return true;
            }
            if (fullRestoring)
            {
                Require(g.money == savedMoney && g.level == savedLevel && g.MissionTitle == "顺风码头剿匪", "Full journey checkpoint restore mismatch: " + g.MissionTitle);
                fullRestoring = false;
            }
            // Task fixture retains real animation, respawns, attack rectangles and production kill counters.
            var tasks = (IList)Get(g, "tasks");
            foreach (var task in tasks)
            {
                if (!((int[])Get(task, "stages")).Contains(stage)) continue;
                var ids = (int[])Get(task, "enemyIds"); var counts = (int[])Get(task, "counts");
                var remaining = ids.Where((id,i)=>counts[i]>0).ToArray();
                var enemy = g.Actors.Values.FirstOrDefault(a=>a.IsEnemy && !a.hidden && a.hp>0 && remaining.Contains(a.definitionId));
                if (enemy) { Hit(g,enemy); return true; }
                if (remaining.Length > 0) return true;
            }
            // Boss fixture uses the production damage/death/special-health dispatch, never queues story callbacks.
            var boss = g.Actors.Values.FirstOrDefault(a => a.IsEnemy && !a.hidden && a.hp > 0 && (!string.IsNullOrEmpty(a.deathScript) || !string.IsNullOrEmpty(a.specialScript)));
            if (boss)
            {
                var fighters = (IDictionary)Get(g,"fighters");
                typeof(GuJianGame).GetMethod("Damage",Flags).Invoke(g,new object[]{fighters[boss.actorId],boss.hp,false});
                fullBossDeaths++; nextAction = EditorApplication.timeSinceStartup + .2; return true;
            }
            if (stage == 335 && new[]{603,702,801,805,827,830,834,837,1002}.Contains(e)) { TalkTo(g,89); return true; }
            if (stage == 900 && new[]{900,1001,1052,1067}.Contains(e)) { TalkTo(g,59); return true; }
            if (stage == 101 && e == 1071 && g.Machine.Get("sub1") == 0) { TalkTo(g,89); return true; }
            if (stage == 100 && new[]{1071,1073,1180,1282}.Contains(e)) { TalkTo(g,61); return true; }
            if (e == 800 && g.Machine.Get("sub1") == 0)
            {
                if (stage == 334) TalkTo(g,40); else Navigate(g,stage,334);
                return true;
            }
            int area = FullArea(e);
            if (area >= 0 && new[]{335,900,100,101,334}.Contains(stage))
            {
                if (stage == 100) Navigate(g,stage,101);
                else if (stage == 334) Navigate(g,stage,335);
                else Portal(g,stage+"_1");
                return true;
            }
            int target = e == 601 ? 607 : e == 602 ? 608 : e == 700 ? 705 : e == 701 ? 700 :
                e == 800 ? 807 : e == 802 ? 809 : e == 804 ? 408 : e == 825 || e == 826 ? 611 :
                e == 828 || e == 829 ? 512 : e == 831 || e == 832 ? 713 : e == 835 ? 817 : e == 836 ? 800 :
                e == 1000 ? 1008 : e == 1002 ? 335 : e == 1051 ? 310 : e == 1054 ? 1015 : e == 1055 ? 1000 :
                e == 1062 ? 1015 : e == 1069 ? 335 : e == 1071 ? 100 : e >= 1101 && e <= 1105 ? 1124 :
                e == 1180 || e == 1282 ? 100 : e == 1182 ? 907 : e == 1183 ? 914 : e == 1184 || e == 1185 ? 919 :
                e >= 1286 && e <= 1289 ? 335 : -1;
            if (e == 836 && stage == 800) { Portal(g,"800_1"); return true; }
                        if (target >= 0) { Navigate(g,stage,target); return true; }
            return true;
        }
    }
}
