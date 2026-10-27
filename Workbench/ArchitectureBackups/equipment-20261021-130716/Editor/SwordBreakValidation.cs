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
    public static class SwordBreakValidation
    {
        private const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
        private const BindingFlags S=BindingFlags.Static|BindingFlags.NonPublic;
        private static object Get(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);
        private static void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
        private static object Call(GuJianGame g,string n,params object[] a)=>typeof(GuJianGame).GetMethod(n,F).Invoke(g,a);
        private static object Fighter(GuJianGame g,int id)=>((IDictionary)Get(g,"fighters"))[id];
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
        private static void Stage(GuJianGame g,bool integrated=false)
        {
            typeof(ComboValidation).GetMethod("Stage",S).Invoke(null,new object[]{g,integrated});
            foreach(var a in g.Actors.Values)a.locked=true;
            Set(g,"lockInput",true); Call(g,"ResetStageInfo");
        }
        private static void Durability(GuJianGame g,int value)
        {
            g.durability=value;
            if(((bool)Get(g,"integrated")))Call(g,"SetSpiritDurability",value);else g.Equipment[g.EquippedIndex].durability=value;
        }
        private static float Cycle(GuJianGame g,float delta=.08f)=>(float)Call(g,"TickSwordBreak",delta);
        private static void Break(GuJianGame g)=>Call(g,"DestroyOrdinarySword");
        private static void Report(string name,List<string> checks,string error)
        {
            var r=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
            File.WriteAllText("../Reference/Validation/"+name+".json",JsonUtility.ToJson(r,true));Debug.Log("GUJIAN_"+name+" "+r.passed+" "+error);
        }
        private static int Seed(bool dodge)
        {
            for(int seed=0;seed<1000;seed++){UnityEngine.Random.InitState(seed);int value=UnityEngine.Random.Range(0,100);if(dodge?value<90:value>=10)return seed;}
            throw new Exception("No deterministic seed");
        }
        private static OriginalActor EnemyHit(GuJianGame g,int id,bool integrated,bool right,bool low,bool dodge=false)
        {
            var boss=(OriginalActor)typeof(BossFamilyValidation).GetMethod("Stage",S).Invoke(null,new object[]{g,id,integrated});
            var role=g.Actors[-1];
            if(integrated){((int[])Get(g,"spiritBonuses"))[1]+=1000;Call(g,"SetSpiritDurability",g.GetSpiritAttribute(1));}
            int maximum=integrated?g.GetSpiritAttribute(1)-((int[])Get(g,"spiritBonuses"))[1]:g.Equipment[g.EquippedIndex].MaximumDurability;
            int before=low?1:g.durability,loss=maximum*30/100;Durability(g,before);
            Set(g,"infiniteDurability",true); // Enemy destruction bypasses the player's consumption option.
            Set(Fighter(g,900),"attackMode",0);boss.Face(right?"r":"l");boss.Act(17,true);
            int frame=Array.FindIndex(boss.Animation.actions[17].steps,s=>s.attack&&boss.Animation.frames[s.frame].attackRects.Length>0);
            Require(frame>=0,"Amakusa attack frame");boss.Sample(17,frame);role.ResetAction();
            typeof(BossFamilyValidation).GetMethod("Align",S).Invoke(null,new object[]{boss,role});
            if(dodge){Set(Fighter(g,900),"hit",0);Set(Fighter(g,-1),"dodge",100000);}
            int hp=role.hp;UnityEngine.Random.InitState(Seed(dodge));boss.Tick(.080001f);
            int expected=dodge?before:Mathf.Max(0,before-loss);
            Require(g.durability==expected,"Amakusa durability "+id+":"+integrated+":"+right+":"+low+":"+dodge+" "+g.durability+"/"+expected);
            Require(dodge?role.hp==hp:role.hp<hp,"Real Amakusa hit/dodge");
            Require(g.SwordBreakVisible==(!dodge&&!integrated&&expected==0),"Amakusa HUD flag");
            return boss;
        }
        [MenuItem("古剑/验证破剑动画暂停与敌方耐久攻击")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first");var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var checks=new List<string>();string error=null;float scale=Time.timeScale;var random=UnityEngine.Random.state;string save=g.saveFileName;Time.timeScale=0;
            try
            {
                var ui=Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                Require(ui&&ui.scene.actions[2].steps.Select(s=>s.frame).SequenceEqual(new[]{3,4,5,6}),"Source four saved steps");
                Require(ui.sceneSheets.All(t=>t&&AssetDatabase.Contains(t)),"Persistent source sheets");
                checks.Add("Saved source scene action2 contains frames3/4/5/6 and all referenced sheets are persistent assets");
                Stage(g);int objectCount=UnityEngine.Object.FindObjectsOfType<GameObject>().Length;Break(g);Require(g.SwordBreakCounter==0&&g.SwordBreakVisible,"Initial break phase");
                Require(Cycle(g,.04f)==0&&g.SwordBreakCounter==0,"Fractional break tick");
                Require(Cycle(g,.04f)==0&&g.SwordBreakCounter==1,"First break frame");
                for(int i=2;i<=4;i++)Require(Cycle(g)==0&&g.SwordBreakCounter==i&&g.SwordBreakVisible,"Break frame "+i);
                Require(Cycle(g)==0&&!g.SwordBreakVisible,"Fifth source cycle must clear and still pause");
                Require(Mathf.Abs(Cycle(g)-.08f)<.00001f,"Resume next cycle");
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length==objectCount,"Break effect generated fixed objects");
                Break(g);Require(Mathf.Abs(Cycle(g,.72f)-.32f)<.00001f&&!g.SwordBreakVisible,"Long frame may only advance the unpaused remainder");
                checks.Add("Actual break state advances four 80ms frames, pauses the fifth clearing cycle, accumulates fractional frames and filters 400ms from a long frame before resuming");
                foreach(int mode in Enumerable.Range(0,10))
                {
                    Stage(g);Break(g);
                    switch(mode)
                    {
                        case 0: Set(g,"menu",true);break;
                        case 1: g.Machine.Command(0x29,new object[]{"消息"});break;
                        case 2: g.Machine.Command(0x2D,new object[]{"选择",new object[]{"继续"}});break;
                        case 3: g.Machine.Command(0x5D,new object[]{"黑幕"});break;
                        case 4: Set(g,"cameraWait",true);break;
                        case 5: Call(g,"MoveActor",g.Actors[-1],"r",1f,100f);break;
                        case 6: Set(g,"waitUntil",Time.time+10);break;
                        case 7: Set(g,"miniMapOpen",true);break;
                        case 8: Set(g,"reinforceOpen",true);break;
                        case 9: g.Machine.Command(0x27,new object[]{"角色","对白","",false});break;
                    }
                    float allowed=Cycle(g);Require(g.SwordBreakCounter==1&&Mathf.Abs(allowed-(mode==9?0:.08f))<.00001f,"Source state pause mapping "+mode);
                }
                checks.Add("Menu/message/choice/black-screen/camera-wait/scripted-movement/time-wait/minimap/reinforcement advance the break effect without scene12 suspension; character dialogue retains it");
                foreach(bool integrated in new[]{false,true})foreach(int remaining in new[]{1,10})
                {
                    Stage(g,integrated);g.Actors[-1].locked=false;Durability(g,remaining);Call(g,"ResetSkillCooldown",true);
                    Set(Fighter(g,-1),"defense",777777);int before=777777;Require(g.UseSkill(5),"Exhausting skill cast");
                    Require(g.durability==0&&g.SwordBreakVisible==!integrated&&g.BreakCooldownTicks==3&&g.SkillCooldownTicks==48,"Exhausting skill state");
                    int after=(int)Get(Fighter(g,-1),"defense");
                    Require(integrated||remaining!=1||after==before,"Insufficient source branch unexpectedly refreshed attributes");
                    Require(integrated||remaining!=10||after<before,"Exact-zero source branch did not refresh attributes");
                }
                Stage(g);Durability(g,1);Call(g,"ResetSkillCooldown",true);Set(g,"infiniteDurability",true);g.Actors[-1].locked=false;
                Require(g.UseSkill(5)&&g.durability==1&&!g.SwordBreakVisible,"Infinite skill consumption");
                checks.Add("Real skills accept exact and insufficient positive MP, distinguish ordinary/spirit break effects, retain source attribute-refresh asymmetry and respect infinite consumption");
                foreach(bool integrated in new[]{false,true})
                {
                    Stage(g,integrated);Durability(g,1);
                    typeof(ComboValidation).GetMethod("Hit",S).Invoke(null,new object[]{g,4,0,0,true});
                    Require(g.durability==0&&g.SwordBreakVisible==!integrated,"Actual break hit exhaustion");
                    Stage(g,integrated);Call(g,"BeginCriticalKill",true,true);Call(g,"SetCriticalStage",25);
                    var role=g.Actors[-1];for(int i=0;i<=98;i++)role.Tick(.080001f);
                    Require(g.durability==0&&g.SwordBreakVisible==!integrated,"Actual burial frame98 exhaustion");
                    Stage(g,integrated);Set(g,"infiniteDurability",true);int before=g.durability;Call(g,"BeginCriticalKill",true,true);Call(g,"SetCriticalStage",25);
                    role=g.Actors[-1];for(int i=0;i<=98;i++)role.Tick(.080001f);
                    Require(g.durability==before&&!g.SwordBreakVisible,"Infinite burial consumption");
                }
                checks.Add("Real original attack-frame break hits and burial step98 exhaust the correct MP bank; spirit depletion has no ordinary effect and infinite burial preserves MP");
                Stage(g);g.Machine.Command(0x7B,new object[0]);Cycle(g);g.Machine.Command(0x7B,new object[0]);
                Require(g.SwordBreakCounter==1&&g.Equipment[g.EquippedIndex].durability==0,"Repeated already-broken script restarted effect");
                Stage(g,true);g.Machine.Command(0x7B,new object[0]);Require(g.SpiritDurability==0&&g.durability==0&&!g.SwordBreakVisible,"Integrated script break");
                Stage(g);g.Machine.Command(0x64,new object[]{40});g.Actors[-1].hp=1;
                g.Inventory.RemoveUpTo(0,int.MaxValue);g.Inventory.Add(0,1);Call(g,"TickAutoMedicine");var spirit=g.Actors[-2];
                Require(spirit.actionIndex==3&&(int)Get(g,"autoMedicine")==0,"Spirit medicine fixture");g.Machine.Command(0x7B,new object[0]);
                Require(spirit.actionIndex==0&&(int)Get(g,"autoMedicine")==-1&&g.ItemCount(0)==1,"Break failed to cancel queued medicine");
                for(int i=0;i<10;i++)spirit.Tick(.080001f);Require(g.ItemCount(0)==1,"Canceled medicine was consumed later");
                checks.Add("Script x7B synchronizes both MP banks, avoids restarting an already-broken ordinary sword and cancels the spirit medicine action without consuming its queued item");
                foreach(int id in new[]{243,244,245})foreach(bool integrated in new[]{false,true})foreach(bool right in new[]{false,true})foreach(bool low in new[]{false,true})
                    EnemyHit(g,id,integrated,right,low);
                foreach(int id in new[]{243,244,245})foreach(bool integrated in new[]{false,true})EnemyHit(g,id,integrated,false,false,true);
                checks.Add("30 real Amakusa243/244/245 original-frame cases cover both directions/MP banks/high and low MP/dodge: base spirit MP excludes +1000 reinforcement; forced loss bypasses player infinite-consumption option");
                foreach(int id in new[]{238,239})foreach(bool integrated in new[]{false,true})
                {
                    var boss=(OriginalActor)typeof(BossFamilyValidation).GetMethod("Stage",S).Invoke(null,new object[]{g,id,integrated});
                    int ordinary=g.Equipment[g.EquippedIndex].durability;var role=g.Actors[-1];Set(g,"bossCritical",boss);boss.Act(24,true);
                    int frame=Array.FindIndex(boss.Animation.actions[24].steps,s=>s.attack&&boss.Animation.frames[s.frame].attackRects.Length>0);
                    boss.Sample(24,frame);role.ResetAction();typeof(BossFamilyValidation).GetMethod("Align",S).Invoke(null,new object[]{boss,role});Call(g,"ResolveBossCriticalHit",boss);
                    Require(integrated?!((bool)Get(g,"integrated"))&&g.SpiritDurability==0&&g.Equipment[g.EquippedIndex].durability==ordinary&&!g.SwordBreakVisible:g.durability==0&&g.SwordBreakVisible,"Boss forced sword destruction");
                }
                checks.Add("Production Boss238/239 critical attacks trigger ordinary destruction or separate an exhausted spirit while preserving the ordinary sword");
                Stage(g,true);var items=(List<EquipmentInstance>)Get(g,"equipment");items[0].durability=0;items[1].durability-=19;
                int spiritBefore=g.SpiritDurability;int expectedCost=(items[0].MaximumDurability+19)/10+1000;
                Require((int)g.Machine.Command(0x89,new object[0])==expectedCost,"Original repair quote");g.money=expectedCost-1;g.Machine.Command(0x87,new object[0]);
                Require(items[0].durability==0&&g.money==expectedCost-1,"Insufficient repair mutated bag");g.ContinueDialogue();g.money=expectedCost;g.Machine.Command(0x87,new object[0]);
                Require(g.money==0&&items.All(e=>e.durability==e.MaximumDurability)&&g.SpiritDurability==spiritBefore,"Repair all ordinary swords");g.ContinueDialogue();
                Stage(g);Durability(g,0);g.money=10000;g.Machine.Command(0x89,new object[0]);g.Machine.Command(0x87,new object[0]);
                Require(g.durability==g.Equipment[g.EquippedIndex].MaximumDurability&&!g.SwordBreakVisible,"Ordinary repair alias");
                checks.Add("Original x89 repair quote is ordinary missing MP/10 plus 500 per damaged sword; x87 checks money, repairs the whole bag and preserves spirit MP with either active bank");
                Stage(g);g.Machine.Command(0x7B,new object[0]);g.saveFileName="validation-sword-break.json";g.Save();g.Load();g.Machine.Clear();
                Require(g.durability==0&&!g.SwordBreakVisible&&g.SwordBreakCounter==0,"Load retained transient break animation");
                checks.Add("Production save/load preserves broken durability but resets transient break animation");
            }
            catch(Exception e){error=e.ToString();}
            finally{g.saveFileName=save;Time.timeScale=scale;UnityEngine.Random.state=random;Report("sword-break",checks,error);}
        }

        private static GuJianGame live;
        private static List<string> liveChecks;
        private static int livePhase,heldFrame;
        private static double liveNext,liveDeadline;
        private static float liveScale;
        private static Vector2 heldCamera;
        private static Func<string,string[]> liveLoader;
        public static void RunLive()
        {
            Require(EditorApplication.isPlaying&&live==null,"Play first; one live run");live=UnityEngine.Object.FindObjectOfType<GuJianGame>();liveScale=Time.timeScale;Time.timeScale=0;Stage(live);
            var role=live.Actors[-1];role.ResetAction();role.Act(7,true);heldFrame=role.FrameIndex;
            role.maxHp=1450;role.hp=725;Set(live,"healthDecrement",100);Call(live,"ShowStageInfo");
            Set(live,"timerScript","200");Set(live,"timerSeconds",100f);Call(live,"MoveCameraRelative",100,0,1,false);heldCamera=live.CameraPosition;
            liveLoader=live.Machine.LoadScript;
            live.Machine.LoadScript=name=>name=="validation-sword-break-vm"?new[]{"x7D()","return"}:liveLoader(name);
            Set(live,"energy",0);live.Machine.Enqueue("validation-sword-break-vm");Break(live);liveChecks=new List<string>();livePhase=0;
            liveNext=EditorApplication.timeSinceStartup+.16;liveDeadline=EditorApplication.timeSinceStartup+15;Time.timeScale=.25f;EditorApplication.update+=LiveUpdate;
        }
        private static void LiveUpdate()
        {
            try
            {
                Require(EditorApplication.isPlaying&&live,"Background editor stopped");double now=EditorApplication.timeSinceStartup;
                Require(now<liveDeadline,"Background break timeout");if(now<liveNext)return;
                if(livePhase==0)
                {
                    if(live.SwordBreakCounter==0)return;
                    Require(live.SwordBreakVisible&&live.SwordBreakCounter<=3,"Live break phase "+live.SwordBreakCounter);
                    Require(live.Actors[-1].FrameIndex==heldFrame&&(int)Get(live,"healthDecrement")==100&&(float)Get(live,"timerSeconds")==100f,"Live scene counters advanced while broken");
                    Require(!live.Machine.Running&&live.Energy==1000&&live.CameraPosition==heldCamera,"Live VM must continue independently while relative camera stays paused");
                    liveChecks.Add("Actual minimized background Update advances break frames while suspending actor frame, HP tail, timer and relative camera; the production VM executes an unblocked queued script independently");
                    live.Machine.Clear();liveNext=now+.55;livePhase++;
                }
                else if(livePhase==1)
                {
                    if(live.SwordBreakVisible||(int)Get(live,"healthDecrement")==100)return;
                    Require(!live.SwordBreakVisible&&(int)Get(live,"healthDecrement")<100&&(float)Get(live,"timerSeconds")<100f&&live.CameraPosition!=heldCamera,"Live scene did not resume");
                    liveChecks.Add("Actual background Update resumes timer, HP-tail and camera after the fifth cycle without external unpause");
                    live.Machine.LoadScript=liveLoader;Stage(live);Set(live,"menu",true);Set(live,"healthDecrement",100);live.Actors[-1].maxHp=1450;Break(live);
                    liveNext=now+.2;livePhase++;
                }
                else
                {
                    if(live.SwordBreakCounter==0)return;
                    Require(live.SwordBreakVisible&&live.SwordBreakCounter>0&&(int)Get(live,"healthDecrement")<100,"Menu incorrectly suspended global tail");
                    liveChecks.Add("Actual menu Update continues global HP-tail ticks while the ordinary break animation advances");FinishLive(null);
                }
            }
            catch(Exception e){FinishLive(e.ToString());}
        }
        private static void FinishLive(string error)
        {
            EditorApplication.update-=LiveUpdate;Time.timeScale=liveScale;Report("sword-break-live",liveChecks,error);
            if(live){if(liveLoader!=null)live.Machine.LoadScript=liveLoader;live.Begin();live.Machine.Clear();typeof(GuJianGame).GetProperty("AtTitle").SetValue(live,true,null);}live=null;
        }
        public static void PrepareVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Time.timeScale=0;Stage(g);foreach(var a in g.Actors.Values)if(a.IsEnemy){a.hidden=true;a.RefreshFrame();}
            Set(g,"toastUntil",0f);Call(g,"UpdateCamera");Break(g);
        }
        public static void CaptureVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var checks=new List<string>();string error=null;
            try
            {
                // Execute-code can run with another panel's Screen dimensions. Use the
                // existing GameView render target, without changing any window or view size.
                var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                var view=Resources.FindObjectsOfTypeAll(viewType)[0];
                var size=(Vector2)viewType.GetProperty("targetSize",F).GetValue(view,null);
                float aspect=533f/320f,screenAspect=size.x/size.y;
                Camera.main.rect=screenAspect>aspect?new Rect((1-aspect/screenAspect)/2,0,aspect/screenAspect,1):
                    new Rect(0,(1-screenAspect/aspect)/2,1,screenAspect/aspect);
                foreach(int counter in new[]{0,1,2,3,4,5})
                {
                    Set(g,"swordBreakVisible",counter<5);Set(g,"swordBreakCounter",Mathf.Min(4,counter));
                    var texture=(Texture2D)typeof(HudFeedbackValidation).GetMethod("Render",S).Invoke(null,null);
                    File.WriteAllBytes("../Reference/Validation/sword-break-"+counter+".png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
                }
                checks.Add("Existing GameView rendered initial/source frames3/4/5/6 and settled broken HUD from saved source sheets without focusing or resizing the editor");
                Report("sword-break-rendered",checks,null);
            }
            catch(Exception e){error=e.ToString();Report("sword-break-rendered",checks,error);}
        }
    }
}
