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
    public static class CombatFeedbackValidation
    {
        private const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
        private const BindingFlags S=BindingFlags.Static|BindingFlags.NonPublic;
        private static object Get(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);
        private static void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
        private static object Call(GuJianGame g,string n,params object[] a)=>typeof(GuJianGame).GetMethod(n,F).Invoke(g,a);
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
        private static IList Numbers(GuJianGame g)=>(IList)Get(g,"numbers");
        private static object Fighter(GuJianGame g,int id)=>((IDictionary)Get(g,"fighters"))[id];
        private static void Stage(GuJianGame g)
        {
            typeof(ComboValidation).GetMethod("Stage",S).Invoke(null,new object[]{g,false});
            foreach(var a in g.Actors.Values)a.locked=true;
            Call(g,"ResetStageInfo");Call(g,"ResetCombatFeedback");
        }
        private static void Hit(GuJianGame g)
        {typeof(ComboValidation).GetMethod("Hit",S).Invoke(null,new object[]{g,0,100000,0,true});}
        private static void Tick(OriginalActor actor,int count){for(int i=0;i<count;i++)actor.Tick(.080001f);}
        private static void Add(GuJianGame g,OriginalActor actor,int value,int type)=>Call(g,"ShowCombatNumber",actor,value,type);
        private static int Int(object n,string field)=>(int)Get(n,field);
        private static int Count(GuJianGame g,OriginalActor actor)=>Numbers(g).Cast<object>().Count(n=>(OriginalActor)Get(n,"actor")==actor);
        private static void Report(string name,List<string> checks,string error)
        {
            var report=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
            File.WriteAllText("../Reference/Validation/"+name+".json",JsonUtility.ToJson(report,true));
            Debug.Log("GUJIAN_"+name+" "+report.passed+" "+error);
        }
        [MenuItem("古剑/验证伤害数字闪避与气血尾条")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first");var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var checks=new List<string>();string error=null;float scale=Time.timeScale;var random=UnityEngine.Random.state;Time.timeScale=0;
            try
            {
                var ui=Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                Require(ui&&ui.combatNumbers!=null&&ui.combatNumbers.Length==4&&ui.combatMiss&&ui.combatMiss.name=="miss"&&AssetDatabase.Contains(ui.combatMiss),"Saved combat atlases");
                for(int i=0;i<4;i++)Require(ui.combatNumbers[i]&&ui.combatNumbers[i].name=="u_"+(33+i)&&AssetDatabase.Contains(ui.combatNumbers[i]),"Saved atlas "+i);
                checks.Add("All four source atlases u33/u34/u35/u36 and miss are persistent Unity asset references");
                Stage(g);Hit(g);Require(Numbers(g).Count==1,"Production hit must create one number");
                var n=Numbers(g)[0];Require((OriginalActor)Get(n,"actor")==g.Actors[900]&&Int(n,"type")==0&&Int(n,"ticks")==10&&Int(n,"value")>0,"Normal damage feedback");
                Set(Fighter(g,-1),"critical",1000);Hit(g);Require(Int(Numbers(g)[0],"type")==1,"Critical damage feedback");
                Require((int)Get(g,"healthDecrement")==0,"Ordinary damage must not invent source white-tail triggers");
                checks.Add("Real original-frame attacks create ordinary/critical numeric atlas entries on the actual target; ordinary damage leaves source-specific HP tail unchanged");
                Stage(g);var role=g.Actors[-1];var target=g.Actors[900];int objects=UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
                for(int i=1;i<=6;i++)Add(g,role,i,0);Add(g,target,77,1);
                var values=Numbers(g).Cast<object>().Where(x=>(OriginalActor)Get(x,"actor")==role).Select(x=>Int(x,"value")).ToArray();
                Require(values.SequenceEqual(new[]{6,5,4,3,2})&&Count(g,target)==1,"Five slots per actor/newest first");
                Tick(target,1);Require(Int(Numbers(g)[0],"ticks")==9&&Int(Numbers(g)[1],"ticks")==10,"Owner tick independence");
                role.frozen=true;Tick(role,12);Require(Int(Numbers(g)[1],"ticks")==10,"Frozen actor advanced numbers");role.frozen=false;
                Tick(role,9);Require(Count(g,role)==5&&Int(Numbers(g)[1],"ticks")==1,"Final live number tick");Tick(role,1);Require(Count(g,role)==0&&Count(g,target)==1,"Ten ticks expiry");
                target.keepLastFrame=true;typeof(OriginalActor).GetProperty("Completed").SetValue(target,true,null);Tick(target,9);
                Require(Numbers(g).Count==0,"Completed held pose stopped base feedback cycles");
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length==objects,"Feedback generated fixed objects");
                checks.Add("Five slots are independent per actor, expire after ten actual 80ms actor cycles, freeze with the actor and continue on a completed held pose; no HUD objects generated");
                Stage(g);role=g.Actors[-1];target=g.Actors[900];Set(Fighter(g,-1),"hit",0);Set(Fighter(g,900),"dodge",100000);
                int seed=0;for(;seed<100;seed++){UnityEngine.Random.InitState(seed);if(UnityEngine.Random.Range(0,100)<90)break;}
                UnityEngine.Random.InitState(seed);int hp=target.hp;Hit(g);
                Require(target.hp==hp&&Numbers(g).Count==0&&(int)Get(Fighter(g,900),"missTicks")==10,"Production dodge feedback");
                Tick(target,9);Require((int)Get(Fighter(g,900),"missTicks")==1,"Final miss tick");Tick(target,1);Require((int)Get(Fighter(g,900),"missTicks")==0,"Miss did not end");
                checks.Add("Real production dodge creates the original miss image for ten target cycles without a damage entry or HP change");
                typeof(SupportSkillValidation).GetMethod("Stage",S).Invoke(null,new object[]{g});role=g.Actors[-1];role.maxHp=10000;role.hp=9999;
                Require(g.UseSkill(14),"Healing fixture");typeof(SupportSkillValidation).GetMethod("FinishCast",S).Invoke(null,new object[]{g});
                Require(role.hp==10000&&Int(Numbers(g)[0],"type")==2&&Int(Numbers(g)[0],"value")==1100,"Source full recovery amount must display despite one HP actual gain");
                typeof(SupportSkillValidation).GetMethod("Ready",S).Invoke(null,new object[]{g});Require(g.UseSkill(17),"Repair fixture");
                typeof(SupportSkillValidation).GetMethod("FinishCast",S).Invoke(null,new object[]{g});
                Require(Int(Numbers(g)[0],"type")==3&&Int(Numbers(g)[0],"value")==170,"Repair amount/type");
                checks.Add("Actual capped healing and repair casts display full original calculated 1100/170 amounts with HP/MP atlases, without plus-sign text");
                foreach(int id in new[]{232,235})
                {
                    var boss=(OriginalActor)typeof(BossFamilyValidation).GetMethod("Stage",S).Invoke(null,new object[]{g,id,false});
                    role=g.Actors[-1];Set(g,"bossCritical",boss);boss.hp=boss.maxHp/2;boss.Act(24,true);
                    int frame=Array.FindIndex(boss.Animation.actions[24].steps,s=>s.attack&&boss.Animation.frames[s.frame].attackRects.Length>0);
                    Require(frame>=0,"Boss attack frame");boss.Sample(24,frame);role.ResetAction();
                    typeof(BossFamilyValidation).GetMethod("Align",S).Invoke(null,new object[]{boss,role});
                    Call(g,"ResolveBossCriticalHit",boss);Require((int)Get(g,"healthDecrement")>0,"Source boss tail "+id);
                    if(id==235)Require(Numbers(g).Cast<object>().Any(x=>(OriginalActor)Get(x,"actor")==boss&&Int(x,"type")==2),"Boss drain healing digits");
                }
                checks.Add("Production half-HP and draining Boss critical hit paths trigger the source white tail; drain also creates a healing number on the Boss");
                Stage(g);role=g.Actors[-1];role.maxHp=1450;role.hp=725;Set(g,"healthDecrement",100);
                Call(g,"TickHealthDecrement",.040f);Require((int)Get(g,"healthDecrement")==100,"Tail partial tick");
                Call(g,"TickHealthDecrement",.04001f);Require((int)Get(g,"healthDecrement")==80,"Source integer velocity");
                Call(g,"TickHealthDecrement",.32001f);Require((int)Get(g,"healthDecrement")==0,"Tail expiry");
                role.maxHp=100;Set(g,"healthDecrement",30);Call(g,"TickHealthDecrement",.80001f);
                Require((int)Get(g,"healthDecrement")==30,"Original zero-velocity integer edge case changed");
                role.maxHp=10000;role.hp=1;g.Machine.Command(0x51,new object[]{0,1});Require(g.UseItem(0)&&(int)Get(g,"healthDecrement")==0,"Medicine tail clearing");
                checks.Add("White tail follows integer maxHP/145*2 decay with fractional accumulation and retains the original low-HP zero-velocity edge; actual HP medicine clears it");
                Add(g,role,55,0);Call(g,"ShowCombatMiss",role);Call(g,"ClearStage");
                Require(Numbers(g).Count==0&&(int)Get(Fighter(g,-1),"missTicks")==0,"Stage clear feedback");
                g.Begin();g.Machine.Clear();Require(Numbers(g).Count==0&&(int)Get(g,"healthDecrement")==0,"New game feedback");
                checks.Add("Scene changes clear original actor number/miss slots; new game clears the transient HP tail");
            }
            catch(Exception e){error=e.ToString();}
            finally{Time.timeScale=scale;UnityEngine.Random.state=random;Report("combat-feedback",checks,error);}
        }

        private static GuJianGame liveGame;
        private static List<string> liveChecks;
        private static int livePhase;
        private static double liveNext,liveDeadline;
        private static float liveScale;
        public static void RunLive()
        {
            Require(EditorApplication.isPlaying&&liveGame==null,"Play first; one live run at a time");
            liveGame=UnityEngine.Object.FindObjectOfType<GuJianGame>();liveScale=Time.timeScale;Time.timeScale=0;Stage(liveGame);
            var role=liveGame.Actors[-1];role.ResetAction();role.maxHp=1450;role.hp=725;Add(liveGame,role,4321,0);Set(liveGame,"healthDecrement",200);
            Call(liveGame,"ShowCombatMiss",role);Set(liveGame,"menu",true);livePhase=0;liveChecks=new List<string>();
            liveNext=EditorApplication.timeSinceStartup+.35;liveDeadline=EditorApplication.timeSinceStartup+7;Time.timeScale=1;EditorApplication.update+=LiveUpdate;
        }
        private static void LiveUpdate()
        {
            try
            {
                Require(EditorApplication.isPlaying&&liveGame,"Live editor stopped");double now=EditorApplication.timeSinceStartup;
                Require(now<liveDeadline,"Live feedback timeout");if(now<liveNext)return;
                var role=liveGame.Actors[-1];
                if(livePhase==0)
                {
                    Require(Count(liveGame,role)==1&&Int(Numbers(liveGame)[0],"ticks")==10&&(int)Get(Fighter(liveGame,-1),"missTicks")==10,"Menu advanced actor feedback");
                    Require((int)Get(liveGame,"healthDecrement")<200,"Menu paused global HP tail");
                    liveChecks.Add("Actual background menu Update freezes actor number/miss ticks while the global white HP tail continues decaying");
                    Set(liveGame,"menu",false);liveGame.Machine.Command(0x29,new object[]{"战斗反馈周期验证"});liveNext=now+.35;livePhase++;
                }
                else if(livePhase==1)
                {
                    Require(liveGame.DialogueVisible&&Count(liveGame,role)==1&&Int(Numbers(liveGame)[0],"ticks")==10,"Dialogue advanced actor feedback");
                    liveChecks.Add("Actual dialogue pauses the actor feedback lifetime across real Unity frames");
                    liveGame.ContinueDialogue();liveNext=now+.25;livePhase++;
                }
                else if(livePhase==2)
                {
                    Require(Count(liveGame,role)==1&&Int(Numbers(liveGame)[0],"ticks")<10,"Unpaused Update failed to advance number");
                    liveChecks.Add("Returning to gameplay advances actor numbers through real Update");livePhase++;
                }
                else if(Count(liveGame,role)==0)
                {
                    Require((int)Get(Fighter(liveGame,-1),"missTicks")==0&&liveGame.LastError==null,"Miss/runtime completion");
                    liveChecks.Add("Real background actor cycles expire both number and miss feedback naturally");FinishLive(null);
                }
            }
            catch(Exception e){FinishLive(e.ToString());}
        }
        private static void FinishLive(string error)
        {EditorApplication.update-=LiveUpdate;Time.timeScale=liveScale;Report("combat-feedback-live",liveChecks,error);liveGame=null;}

        public static void PrepareVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Time.timeScale=0;Stage(g);
            g.Actors[-1].mapPosition=new Vector2(210,250);g.Actors[-1].ResetAction();
            foreach(var actor in g.Actors.Values)if(actor.IsEnemy)actor.hidden=true;
            Set(g,"toastUntil",0f);Set(g,"energy",0);Call(g,"UpdateCamera");
        }
        public static void CaptureVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var role=g.Actors[-1];var checks=new List<string>();string error=null;
            try
            {
                Numbers(g).Clear();Save("combat-number-blank");
                for(int type=0;type<4;type++){Numbers(g).Clear();Add(g,role,12345,type);Save("combat-number-"+type);}
                Numbers(g).Clear();Call(g,"ShowCombatMiss",role);Save("combat-miss-10");Tick(role,9);Save("combat-miss-1");
                Set(Fighter(g,-1),"missTicks",0);Numbers(g).Clear();Add(g,role,12345,0);Tick(role,5);Save("combat-number-rise");
                Set(g,"hideUi",true);Save("combat-number-hideui");Set(g,"hideUi",false);
                Numbers(g).Clear();role.maxHp=1450;role.hp=725;Set(g,"healthDecrement",725);Save("combat-health-tail");
                Set(g,"healthDecrement",0);Save("combat-health-tail-expired");
                checks.Add("Actual existing GameView captured all four source numeric atlases, ten/one-tick miss, five-tick number rise and feedback with scene HUD hidden");
                checks.Add("Actual GameView captured half-health white tail and the same frame after tail removal");
            }
            catch(Exception e){error=e.ToString();}
            Report("combat-feedback-rendered",checks,error);
        }
        private static void Save(string name)
        {
            var image=(Texture2D)typeof(HudFeedbackValidation).GetMethod("Render",S).Invoke(null,null);
            try{File.WriteAllBytes("../Reference/Validation/"+name+".png",image.EncodeToPNG());}
            finally{UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}
