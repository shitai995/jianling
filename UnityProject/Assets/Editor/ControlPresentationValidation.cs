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
    public static class ControlPresentationValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static object Get(object o, string n) => o.GetType().GetField(n,F).GetValue(o);
        private static void Set(object o, string n, object v) => o.GetType().GetField(n,F).SetValue(o,v);
        private static object Call(GuJianGame g, string n, params object[] a) => typeof(GuJianGame).GetMethod(n,F).Invoke(g,a);
        private static void Require(bool b, string m) { if (!b) throw new Exception(m); }
        private static void Stage(GuJianGame g, bool integrated = false)
        {
            typeof(ComboValidation).GetMethod("Stage",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{g,integrated});
        }
        private static void Tick(GuJianGame g, int count) { for(int i=0;i<count;i++) Call(g,"TickCameraEffects",.080001f); }
        [MenuItem("古剑/验证后撤镜头触屏与背包")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first"); var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            float scale=Time.timeScale; var random=UnityEngine.Random.state; string error=null,save=g.saveFileName; var checks=new List<string>(); Time.timeScale=0;
            try
            {
                foreach(bool integrated in new[]{false,true}) foreach(bool right in new[]{false,true}) foreach(int action in new[]{2,7,8,9,10})
                {
                    Stage(g,integrated); var r=g.Actors[-1]; r.Face(right?"r":"l"); r.mapPosition=new Vector2(400,250);
                    r.Act(action+r.actionBank,true); Set(g,"queuedAttack",true); Set(g,"comboHits",12);
                    Require(!g.TryBackstep(right?1:-1),"Forward canceled"); Require(g.TryBackstep(right?-1:1),"Backstep rejected");
                    Require(r.LogicalAction==4 && !(bool)Get(g,"queuedAttack") && g.ComboHits==12,"Backstep did not discard input/preserve combo");
                    var steps=r.Animation.actions[r.actionIndex].steps; Vector2 start=r.mapPosition;
                    for(int i=0;i<steps.Length;i++) r.Tick(.080001f);
                    Require(r.actionIndex==(integrated?27:0),"Backstep idle bank");
                    Require(Mathf.Abs(r.mapPosition.x-start.x-steps.Sum(s=>s.vx)*(right?-1:1))<.01f,"Backstep source movement");
                }
                checks.Add("20 actual retreat variants: hurt/basic attacks, both directions/banks, original action-4 movement, forward rejection, queued-input cancellation, combo retention and return to correct idle");
                Stage(g); var role=g.Actors[-1];
                foreach(int action in new[]{0,5,6,11,12,20,21,26}) { role.Act(action,true); Require(!g.TryBackstep(-1),"Invalid cancel "+action); }
                role.Face("r"); role.Act(7,true); role.locked=true; Require(!g.TryBackstep(-1),"Locked retreat"); role.locked=false;
                checks.Add("Idle/heavy hurt/skills/break/integration and locked actors reject retreat cancellation");
                Stage(g); Set(g,"cameraPoint",new Vector2(300,160));
                g.Machine.Command(0x42,new object[]{560,360,8,true}); Vector2 initial=g.CameraPosition;
                Require(g.CameraMoving && (bool)Get(g,"cameraWait") && g.CameraPosition==initial,"Camera teleported");
                Vector2 target=(Vector2)Get(g,"cameraTarget");
                Tick(g,1); Require(g.CameraPosition==new Vector2(Mathf.MoveTowards(initial.x,target.x,8),Mathf.MoveTowards(initial.y,target.y,8)),"Camera step"); Tick(g,100);
                Require(!g.CameraMoving && !(bool)Get(g,"cameraWait"),"Camera wait did not end");
                g.Machine.Command(0x41,new object[]{8,false}); Tick(g,100); Require(g.CameraPosition==initial,"Camera saved return");
                checks.Add("x42 advances axes by exact step per 80 ms, waits without teleporting and x41 returns to saved position");
                Set(g,"cameraPoint",new Vector2(280,160)); g.Machine.Command(0x40,new object[]{80,0,12,true}); Tick(g,5);
                Require(g.CameraPosition.x==340 && g.CameraMoving && !(bool)Get(g,"cameraWait"),"Relative nonblocking motion"); Tick(g,1);
                Require(g.CameraPosition.x==352 && !g.CameraMoving,"Relative integer duration");
                g.Machine.Command(0x45,new object[]{8,1}); Tick(g,1); Require(g.CameraShakeOffset==new Vector2(0,2),"Shake original phase");
                Set(g,"menu",true); var shake=g.CameraShakeOffset; Tick(g,50); Require(g.CameraShakeOffset==shake,"Menu advanced shake"); Set(g,"menu",false);
                g.Machine.Command(0x46,new object[0]); Require(g.CameraShakeOffset==Vector2.zero,"Stop shake");
                g.Machine.Command(0x45,new object[]{-1,0}); Tick(g,40); Require((int)Get(g,"shakeTicks")>100,"Infinite shake ended"); Call(g,"ClearStage"); Require(g.CameraShakeOffset==Vector2.zero&&!g.CameraMoving,"Stage camera cleanup");
                checks.Add("x40 retains source integer-duration nonblocking motion; x45 shake phases/infinite mode, menu pause, x46 stop and stage cleanup pass");
                Stage(g); g.Machine.Command(0x6C,new object[]{new object[]{0,1,2,3},0});
                g.RespondTutorial(9); Require(g.TutorialKey==0,"Wrong checklist input released script");
                g.RespondTutorial(2); g.RespondTutorial(0); g.RespondTutorial(3); Require(g.TutorialKey==1,"Checklist lost remaining key"); g.RespondTutorial(1); Require(g.TutorialKey==-1,"Checklist never ended");
                g.Machine.Command(0x6A,new object[]{8}); g.RespondTutorial(7); Require((bool)g.Machine.Command(0x6B,new object[0]),"Wrong key query");
                g.Machine.Command(0x6A,new object[]{8}); g.PressControl(8); Require(!(bool)g.Machine.Command(0x6B,new object[0])&&g.TutorialKey==-1,"Touch tutorial response");
                checks.Add("x6C consumes all requested keys in any order, ignores unrelated keys, keeps waiting, then releases; x6A/x6B single-key correctness and touch responses pass");
                Stage(g); var layout=(OriginalTouchLayout)Get(g,"touchLayout"); Require(layout&&AssetDatabase.Contains(layout)&&layout.controls.Length==11,"Saved touch layout missing");
                int count=UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
                Call(g,"ReadTouchPoint",layout.controls.First(c=>c.code==2).bounds.center); Call(g,"ReadTouchPoint",layout.controls.First(c=>c.code==4).bounds.center);
                var held=(HashSet<int>)Get(g,"touchHeld"); Require(held.Contains(2)&&held.Contains(4),"Multitouch overlap");
                foreach(var actor in g.Actors.Values) actor.interactionScript=null;
                Set(g,"lockInput",false); g.PressControl(4); Require(g.Actors[-1].LogicalAction==7,"Touch attack");
                Set(g,"menu",true); Require(!g.TouchControlsVisible,"Touch on menu"); Set(g,"menu",false); Set(g,"dialogue","test"); Require(!g.TouchControlsVisible,"Touch on dialogue"); Set(g,"dialogue",null);
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length==count,"Controls created nodes");
                checks.Add("11 saved editable touch zones support simultaneous direction/attack, call real attack, hide for menu/dialogue, and generate no GameObjects");
                foreach(string field in new[]{"swingAudio","hitAudio","criticalAudio"})
                {
                    var audio=(AudioSource)Get(g,field); Require(audio&&audio.clip&&AssetDatabase.Contains(audio.clip)&&!audio.playOnAwake&&!audio.loop,"Saved audio source "+field);
                }
                Call(g,"PlayOriginalSound",2); Require(((AudioSource)Get(g,"hitAudio")).isPlaying,"Original hit sound did not play");
                checks.Add("Three persistent AudioSources reference original miss/critical/hit clips; actual hit playback starts without creating audio components");
                var swing=(AudioSource)Get(g,"swingAudio"); swing.Stop(); Stage(g);
                Require(g.UseSkill(0),"Basic attack rejected by audio timing validation");
                Require(!swing.isPlaying,"Swing sound played before the original first action tick");
                g.Actors[-1].Tick(.080001f);
                Require(swing.isPlaying,"Swing sound did not start on the original first action tick");
                checks.Add("Basic-attack swing sound starts on action 7-10 frame 0 during the first 80 ms actor tick, not immediately when input is accepted");
                Stage(g); g.Machine.Command(0x51,new object[]{14,200});
                Require(g.ItemStackCount(14)==3&&g.ItemStackQuantity(14,0)==99&&g.ItemStackQuantity(14,2)==2,"Stack partition");
                int money=g.money; Require(g.UseItemStack(14,2)&&g.money>money&&g.ItemCount(14)==198,"Partial stack use");
                Require(!g.UseItemStack(14,2)&&!g.UseItemStack(14,-1),"Invalid stack consumed");
                g.saveFileName="validation-control-stack.json"; g.Save(); g.Load(); g.Machine.Tick(); Require(g.ItemCount(14)==198&&g.ItemStackCount(14)==2,"Stack save restore");
                checks.Add("Inventory 200 partitions as 99/99/2; selecting final stack consumes exactly 2, invalid stack indices fail and real save/load restores total and stack views");
            }
            catch(Exception e){error=e.ToString();}
            finally
            {
                Time.timeScale=scale; UnityEngine.Random.state=random; g.saveFileName=save;
                var report=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
                Directory.CreateDirectory("../Reference/Validation"); File.WriteAllText("../Reference/Validation/control-presentation.json",JsonUtility.ToJson(report,true)); Debug.Log("GUJIAN_CONTROL_VALIDATION "+report.passed+" "+error);
            }
        }
    }
}
