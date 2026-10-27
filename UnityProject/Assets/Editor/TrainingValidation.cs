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
    public static class TrainingValidation
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static object Get(object o, string name) => o.GetType().GetField(name, Flags | BindingFlags.Public).GetValue(o);
        private static void Set(object o, string name, object value) => o.GetType().GetField(name, Flags | BindingFlags.Public).SetValue(o, value);
        private static object Call(GuJianGame g, string name, params object[] args) => typeof(GuJianGame).GetMethod(name, Flags).Invoke(g, args);
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Pump(GuJianGame g, params int[] choices)
        {
            var queue = new Queue<int>(choices);
            for (int i = 0; i < 10000; i++)
            {
                Require(g.LastError == null, g.LastError);
                if (g.DialogueVisible)
                {
                    var options = (string[])Get(g, "options");
                    int choice = options == null ? 0 : queue.Count > 0 ? queue.Dequeue() : -1;
                    g.ContinueDialogue(choice);
                }
                else if (g.Machine.Running) g.Machine.Tick();
                else { Require(queue.Count == 0, "Unused choices " + queue.Count); return; }
            }
            throw new Exception("Training script did not settle: " + g.Machine.CurrentScript + ":" + g.Machine.Instruction);
        }
        private static void Reset(GuJianGame g, int level = 21)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = 999; g.Machine.Values["days"] = 1;
            g.Machine.Command(0x4D, new object[] { level });
            g.Machine.Enqueue("101"); Pump(g);
        }
        private static void TalkToElder(GuJianGame g, params int[] choices)
        {
            Require(g.Actors.ContainsKey(61) && !g.Actors[61].hidden, "Elder missing");
            g.Actors[-1].mapPosition = g.Actors[61].mapPosition + new Vector2(8, 0);
            Call(g, "Interact"); Pump(g, choices);
        }
        private static void Portal(GuJianGame g, string name, params int[] choices)
        {
            var exits = (Dictionary<Vector2Int, string>)Get(g, "transitions");
            var matches = exits.Where(x => x.Value == name + ".s").ToArray();
            Require(matches.Length > 0, "Unregistered exit " + name);
            var cell = matches[0].Key;
            g.Actors[-1].mapPosition = new Vector2(cell.x * g.Map.cellWidth + 1, cell.y * g.Map.cellHeight + 1);
            g.Actors[-1].locked = false; g.Actors[-1].ResetAction();
            Set(g, "lastCell", new Vector2Int(-1,-1));
            Call(g, "HandleInput"); Pump(g, choices);
        }
        private static void Kill(GuJianGame g, OriginalActor actor)
        {
            foreach (var enemy in g.Actors.Values.Where(a => a.IsEnemy)) enemy.locked = true;
            var fighters = (IDictionary)Get(g, "fighters");
            Call(g, "Damage", fighters[actor.actorId], actor.hp, false, -1);
            Pump(g);
        }
        private static GuJianGame liveGame;
        private static OriginalActor liveTarget;
        private static object liveFighter;
        private static double liveStarted;
        private static float liveScale, liveDeathAt;
        private static int liveRounds, liveFrame, liveExperience;
        private static bool liveHidden;
        private static readonly List<string> liveChecks = new List<string>();
        public static bool LiveRunning => liveGame != null;
        [MenuItem("古剑/验证擂台实际刷新与倒计时")]
        public static void RunLive()
        {
            Require(EditorApplication.isPlaying && !EditorApplication.isPaused, "Enter unpaused Play first");
            Require(!LiveRunning,"Live training validation already running");
            liveGame=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            liveScale=Time.timeScale;var initializing=liveGame;var waiting=liveGame.Machine.IsWaiting;
            try
            {
                liveGame.Machine.IsWaiting=()=>liveGame.DialogueVisible;
                Reset(liveGame);liveGame.money=20000;TalkToElder(liveGame,0,1);
            }
            catch { liveGame=null; throw; }
            finally { initializing.Machine.IsWaiting=waiting; }
            foreach(var actor in liveGame.Actors.Values)actor.locked=true;
            liveGame.Machine.Command(0x65,new object[]{true});
            liveGame.Machine.Command(0x52,new object[]{false});
            liveTarget=liveGame.Actors[75];
            liveFighter=((IDictionary)Get(liveGame,"fighters"))[75];
            liveRounds=0;liveChecks.Clear();liveStarted=EditorApplication.timeSinceStartup;
            Time.timeScale=4;StartLiveDeath();
            EditorApplication.update-=AdvanceLive;EditorApplication.update+=AdvanceLive;
        }
        private static void StartLiveDeath()
        {
            liveHidden=false;liveFrame=Time.frameCount;liveDeathAt=Time.time;liveExperience=liveGame.experience;
            Call(liveGame,"Damage",liveFighter,liveTarget.hp,false);
        }
        private static void AdvanceLive()
        {
            try
            {
                Require(EditorApplication.isPlaying && liveGame && liveTarget,"Live training was interrupted");
                Require(liveGame.LastError==null,liveGame.LastError);
                Require(EditorApplication.timeSinceStartup-liveStarted<20,"Arena live respawn timeout");
                Require(liveGame.Machine.Get("stageID")==100 && (bool)Get(liveGame,"experienceRoom"),"Arena ended before live check");
                if(liveTarget.hidden)liveHidden=true;
                if((bool)Get(liveFighter,"dead"))return;
                Require(liveHidden && liveTarget.hp==liveTarget.maxHp && !liveTarget.hidden,"Death animation/hide/restore sequence missing");
                Require(Vector2.Distance(liveTarget.mapPosition,(Vector2)Get(liveFighter,"born"))<.01f,"Respawn did not restore original position");
                Require(Time.frameCount>liveFrame && Time.time-liveDeathAt>=.48f,"Respawn did not advance through real Update/time");
                Require((int)Get(liveFighter,"refreshRemaining")==0,"Infinite training drove finite refresh counter below zero");
                Require(liveGame.experience>liveExperience,"Production death did not award experience");
                liveChecks.Add("Round "+(++liveRounds)+": real Update completes death -> hidden -> full-HP respawn at original position; finite refresh count stays zero; experience awarded; frames="+(Time.frameCount-liveFrame)+" elapsed="+(Time.time-liveDeathAt));
                if(liveRounds<3){StartLiveDeath();return;}
                Require(liveGame.RemainingTimeText!=null && (float)Get(liveGame,"timerSeconds")<300 && (float)Get(liveGame,"timerSeconds")>0,"Real Update did not advance arena countdown");
                liveChecks.Add("Original 300-second arena countdown advances during real Update while infinite respawn remains active after three deaths");
                FinishLive(null);
            }
            catch(Exception e){FinishLive(e.ToString());}
        }
        private static void FinishLive(string error)
        {
            EditorApplication.update-=AdvanceLive;Time.timeScale=liveScale;
            File.WriteAllText("../Reference/Validation/training-live.json",JsonUtility.ToJson(new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=liveChecks.ToArray(),error=error},true));
            liveGame=null;liveTarget=null;liveFighter=null;
            Debug.Log("GUJIAN_TRAINING_LIVE "+(error==null)+" "+error);
        }
        [MenuItem("古剑/验证擂台与剑冢五连战")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var waiting = g.Machine.IsWaiting; var trace = g.Machine.Trace;
            string save = g.saveFileName; float scale = Time.timeScale;
            var random = UnityEngine.Random.state;
            var checks = new List<string>(); var scripts = new HashSet<string>();
            var report = new ReconstructionValidation.Report { utc = DateTime.UtcNow.ToString("O") };
            try
            {
                Time.timeScale = 0; g.saveFileName = "validation-training.json";
                // Script fixture skips only presentation waits. Choices, registered exits, payment,
                // production death handling and callbacks remain active; this is not a balance run.
                g.Machine.IsWaiting = () => g.DialogueVisible;
                g.Machine.Trace = line => scripts.Add(line.Split(':')[0]);
                Reset(g); g.money = 220000;
                TalkToElder(g, -1);
                Require(g.money == 220000 && g.Machine.Get("stageID") == 101 && g.Machine.Get("sub3") == 0, "Cancel changed training state");
                TalkToElder(g, 0, 0);
                Require(g.money == 220000 && g.Machine.Get("stageID") == 101, "Arena explanation charged or entered");
                TalkToElder(g, 1, 0);
                Require(g.money == 220000 && g.Machine.Get("stageID") == 101, "Challenge explanation charged or entered");
                checks.Add("Original elder interaction reaches training; cancel and both explanations retain balance, stage and progress");

                TalkToElder(g, 0, 1);
                Require(g.money == 200000 && g.Machine.Get("stageID") == 100 && g.Machine.Get("sub3") == 0 && (bool)Get(g,"experienceRoom"), "Arena entry/payment/flag mismatch");
                var targets = g.Actors.Values.Where(a => a.IsEnemy).ToArray();
                Require(targets.Length == 8 && targets.All(a => a.definitionId == 201 && a.level == 21), "Arena original eight opponents or level scaling mismatch");
                Require(g.RemainingTimeText == "剩余时间 5:00", "Arena timer display missing");
                foreach (var a in targets) a.locked = true;
                g.Actors[-1].locked = true;
                g.Machine.Command(0x84,new object[0]); Call(g,"TickCombat",10f);
                Require(g.RemainingTimeText == "剩余时间 5:00", "Paused training timer advanced");
                g.Machine.Command(0x85,new object[0]); Call(g,"TickCombat",299.5f);
                Require(g.RemainingTimeText == "剩余时间 0:01" && g.Machine.Get("stageID") == 100, "Arena expired early / timer formatting mismatch");
                Call(g,"TickCombat",.51f); Pump(g);
                Require(g.Machine.Get("stageID") == 101 && !(bool)Get(g,"experienceRoom") && g.RemainingTimeText == null && !g.Actors.Values.Any(a=>a.IsEnemy), "Arena timeout did not return and clear training modifiers");
                checks.Add("Original arena charges 20000 once, creates eight level-scaled enemies, displays 5:00, pauses/resumes and runs the 300-second timeout callback to village; stage load clears double-XP/fast-respawn mode");

                Reset(g,1); g.money=120000; TalkToElder(g,1,1);
                Require(g.money==20000 && g.Machine.Get("stageID")==200 && g.Machine.Get("sub3")==2000, "Challenge entry/payment mismatch");
                g.Machine.Command(0x52,new object[]{false});
                int initialBag=g.Equipment.Count;
                int[] types={233,236,239,242,245};
                for(int round=0;round<5;round++)
                {
                    Require(g.Machine.Get("sub3")==2000+round, "Wrong checkpoint round " + round);
                    if(round==2)
                    {
                        int balance=g.money;g.Save();g.Load();Pump(g);
                        Require(g.Machine.Get("stageID")==200 && g.Machine.Get("sub3")==2002 && g.money==balance, "Challenge checkpoint Save/Load mismatch");
                        g.Machine.Command(0x52,new object[]{false});
                    }
                    Portal(g,"200_201");
                    var boss=g.Actors.Values.Single(a=>a.IsEnemy);
                    Require(boss.definitionId==types[round] && boss.level==46+round && boss.deathScript=="exproom.s" && g.Machine.Get("sub3")==2001+round, "Original boss order/minimum levels/callback mismatch round "+round);
                    Kill(g,boss);
                    Require(g.Machine.Get("stageID")==200 && g.Machine.Get("sub3")==2001+round && !(bool)Get(g,"experienceRoom"), "Boss death did not return to checkpoint");
                }
                Require(g.Equipment.Count==initialBag+1 && g.Machine.Get("sub3")==2005, "Final challenge must grant one weapon");
                var reward=g.Equipment[g.Equipment.Count-1];
                Require(new[]{28,26,20,19,18,16,15}.Contains(reward.id) || (reward.id>=5 && reward.id<15 && reward.reinforcement==9), "Reward outside original random branches");
                Require(!((Dictionary<Vector2Int,string>)Get(g,"transitions")).Values.Contains("200_201.s"), "Finished challenge still allows sixth boss");
                checks.Add("One challenge payment -> five registered boss entries 233/236/239/242/245 at levels 46..50 -> production death callbacks -> one source-allowed random weapon; actual Save/Load after round two retains progress; completed route blocks a sixth boss");
                Portal(g,"exproom",0);
                Require(g.Machine.Get("stageID")==200 && g.Machine.Get("sub3")==2005, "Declining exit lost completed challenge");
                Portal(g,"exproom",1);
                Require(g.Machine.Get("stageID")==101 && g.Machine.Get("sub3")==0 && g.Equipment.Count==initialBag+1, "Challenge exit lost reward/progress cleanup");
                checks.Add("Original left exit No retains completed checkpoint; Yes returns to village and clears challenge mode without losing reward");

                Reset(g,55);g.money=100000;TalkToElder(g,1,1);Portal(g,"200_201");
                var high=g.Actors.Values.Single(a=>a.IsEnemy);
                Require(high.definitionId==233 && high.level==51, "High-level challenge should use player level minus four");
                Kill(g,high);Portal(g,"exproom",1);
                Require(g.Machine.Get("stageID")==101 && g.Machine.Get("sub3")==0 && g.money>=0, "Early exit or exact-fee balance failed");
                checks.Add("Level-55 entry charges exact 100000 balance and scales first boss to 51; early abandonment after one victory returns to normal stage and clears progress");
                Require(new[]{"100n0","training","100","101","200","200_201","201","exproom"}.All(scripts.Contains), "Original training callback coverage incomplete");
                report.passed=true;
            }
            catch(Exception e){report.error=e.ToString();}
            finally
            {
                g.Machine.IsWaiting=waiting;g.Machine.Trace=trace;g.saveFileName=save;
                Time.timeScale=scale;UnityEngine.Random.state=random;
            }
            report.checks=checks.ToArray();
            File.WriteAllText("../Reference/Validation/training.json",JsonUtility.ToJson(report,true));
            Debug.Log("GUJIAN_TRAINING_VALIDATION "+report.passed+" "+report.error);
        }
    }
}
