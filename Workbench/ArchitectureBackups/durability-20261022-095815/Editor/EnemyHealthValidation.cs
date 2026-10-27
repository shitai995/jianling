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
    public static class EnemyHealthValidation
    {
        private const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        private const BindingFlags S=BindingFlags.Static|BindingFlags.NonPublic;
        private static object Get(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);
        private static void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
        private static object Call(GuJianGame g,string n,params object[] a)=>typeof(GuJianGame).GetMethod(n,F).Invoke(g,a);
        private static void Require(bool yes,string why){if(!yes)throw new Exception(why);}
        private static SpriteRenderer Bar(OriginalActor a)=>(SpriteRenderer)Get(a.CombatFeedback,"healthBar");
        private static void Refresh(GuJianGame g)=>Call(g,"RefreshScenePresentation");
        private static void Stage(GuJianGame g)
        {
            typeof(ComboValidation).GetMethod("Stage",S).Invoke(null,new object[]{g,false});Time.timeScale=0;
            foreach(var a in g.Actors.Values)a.locked=true;
            Set(g,"lockInput",true);Set(g,"toastUntil",0f);Call(g,"ResetStageInfo");Call(g,"ResetCombatFeedback");
        }
        private static void Report(string name,List<string> checks,string error)
        {
            var r=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
            File.WriteAllText("../Reference/Validation/"+name+".json",JsonUtility.ToJson(r,true));Debug.Log("GUJIAN_"+name+" "+r.passed+" "+error);
        }
        [MenuItem("古剑/验证普通敌人受击血条")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first");var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var checks=new List<string>();string error=null;float scale=Time.timeScale;var random=UnityEngine.Random.state;
            try
            {
                var ui=Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                Require(ui.enemyHealthBars.Length==20,"Twenty saved widths");
                for(int i=0;i<20;i++)Require(ui.enemyHealthBars[i]&&AssetDatabase.Contains(ui.enemyHealthBars[i])&&ui.enemyHealthBars[i].rect==new Rect(i*21,0,21,5)&&ui.enemyHealthBars[i].pixelsPerUnit==1,"Saved bar width "+i);
                foreach(var e in OriginalAssets.Catalog.actors)if(e.prefab.CombatFeedback)Require(Bar(e.prefab)&&AssetDatabase.Contains(Bar(e.prefab)),"Saved health renderer "+e.id);
                checks.Add("All twenty integer widths are persistent 21x5 sprites; every saved fighter feedback prefab already contains its health Renderer");
                Stage(g);var t=g.Actors[900];t.hp=500;t.maxHp=1000;
                for(int action=0;action<t.Animation.actions.Length;action++)
                {if(t.Animation.actions[action].steps.Length==0)continue;t.Sample(action,0);Refresh(g);bool wanted=action==2||action>=4&&action<=13;Require(Bar(t).enabled==wanted,"Enemy.act state1 action "+action);}
                checks.Add("Every action in the real normal-enemy animation obeys source state1: only action2 and actions4..13 show the bar; idle, death, defense, attacks and cinematics hide it");
                t.Sample(2,0);t.maxHp=1900;
                int objects=UnityEngine.Object.FindObjectsOfType<GameObject>().Length,components=t.GetComponentsInChildren<Component>(true).Length,textures=Resources.FindObjectsOfTypeAll<Texture2D>().Length;
                for(int width=0;width<20;width++){t.hp=width*100;Refresh(g);Require(Bar(t).enabled&&Bar(t).sprite==ui.enemyHealthBars[width],"Integer ratio "+width);}
                t.maxHp=1000;t.hp=999;Refresh(g);Require(Bar(t).sprite==ui.enemyHealthBars[18],"999/1000 floor");
                t.hp=1;Refresh(g);Require(Bar(t).sprite==ui.enemyHealthBars[0],"Positive subpixel HP");
                t.hp=1000;Refresh(g);Require(Bar(t).sprite==ui.enemyHealthBars[19],"Full bar");
                t.maxHp=0;Refresh(g);Require(!Bar(t).enabled,"Zero maximum");t.maxHp=1000;
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length==objects&&t.GetComponentsInChildren<Component>(true).Length==components&&Resources.FindObjectsOfTypeAll<Texture2D>().Length==textures,"Bar creates runtime nodes/components/textures");
                checks.Add("Actual presentation selects all twenty widths and floors 999/1000 to18 and 1/1000 to0, guards a zero maximum, and creates no runtime node/component/texture");
                t.mapPosition=new Vector2(210,250);Refresh(g);var b=Bar(t);int left=(t.Animation.footWidth-20)/2,top=t.CurrentFrame.edgeRect.y-8;
                Require(b.transform.localPosition==new Vector3(left+10.5f,-top-2.5f,0)&&b.sortingOrder==2100,"Original bar placement/source draw order");
                var p=b.transform.position;t.mirror=true;Refresh(g);Require(b.transform.position==p&&!b.flipX&&!b.flipY,"Mirrored health bar");
                t.renderOffset=new Vector2(-112,0);Refresh(g);Require(Mathf.Abs(b.transform.position.x-p.x+112)<.001f,"Burial offset");t.renderOffset=Vector2.zero;
                Set(g,"hideUi",true);Refresh(g);Require(b.enabled,"HUD hide suppressed bar");Set(g,"hideUi",false);
                t.hidden=true;Refresh(g);Require(!b.enabled,"Hidden enemy bar");t.hidden=false;t.frozen=true;Refresh(g);Require(b.enabled,"Frozen pose lost bar");t.frozen=false;
                Set(g,"miniMapOpen",true);Refresh(g);Require(!b.enabled,"Minimap bar");Set(g,"miniMapOpen",false);
                Call(g,"ShowCombatMiss",t);Call(g,"ShowCombatNumber",t,100,0);Refresh(g);
                Require(b.enabled&&((SpriteRenderer)Get(t.CombatFeedback,"miss")).sortingOrder>b.sortingOrder&&((SpriteRenderer[])Get(t.CombatFeedback,"digits"))[0].sortingOrder>b.sortingOrder,"Face/bar/miss/number order");
                checks.Add("Bar stays unmirrored at source foot-centered x and edgeRect.y-8, follows actor offsets and frozen poses, remains under miss/numbers, and respects hidden actor/minimap while hide-UI retains it");
                int special=OriginalAssets.Table<SpriteDefinition>("sprite").First(d=>d.type==11).id;
                g.Machine.Command(0x30,new object[]{901,special,"l",210,250,2});var t11=g.Actors[901];t11.Sample(2,0);Refresh(g);Require(Bar(t11).enabled,"Type11 hurt bar");
                g.Machine.Command(0x30,new object[]{902,232,"l",210,250,2});g.Actors[902].Sample(2,0);Refresh(g);Require(!Bar(g.Actors[902]).enabled,"Boss type10 small bar");
                g.Actors[-1].Sample(2,0);Refresh(g);Require(!Bar(g.Actors[-1]).enabled,"Player small bar");
                checks.Add("Real type11 definition also shows its hurt bar; real Boss232 type10 and the player never show this small bar");
                Stage(g);t=g.Actors[900];int damage=(int)typeof(ComboValidation).GetMethod("Hit",S).Invoke(null,new object[]{g,0,0,0,true});Refresh(g);
                Require(damage>0&&t.actionIndex==2&&Bar(t).enabled,"Production hit did not reveal bar");
                for(int i=0;i<200&&t.actionIndex==2;i++)t.Tick(.080001f);Refresh(g);Require(t.actionIndex==0&&!Bar(t).enabled,"Natural hurt completion did not hide bar");
                checks.Add("Production attack rectangles deal real damage and enter hurt action2, reveal the bar, then the real action-finished callback returns to idle and hides it");
                Stage(g);t=g.Actors[900];damage=(int)typeof(ComboValidation).GetMethod("Hit",S).Invoke(null,new object[]{g,0,100000,0,true});Refresh(g);
                Require(damage>0&&t.actionIndex==0&&!Bar(t).enabled,"Damage with retained hold must not reveal hurt bar");
                checks.Add("A real damaging hit that leaves hold intact does not enter hurt state and does not show the small bar");
                g.Begin();g.Machine.Clear();Refresh(g);Require(!Bar(g.Actors[-1]).enabled,"New game retained bar");
                checks.Add("Starting a new game clears the previous enemy health display and uses the saved feedback prefab");
            }
            catch(Exception e){error=e.ToString();}
            finally{Time.timeScale=scale;UnityEngine.Random.state=random;Report("enemy-health",checks,error);}
        }
        [Serializable] public class CaptureInfo{public int width,height;public Rect logicalBounds;public int footWidth,edgeY;}
        private static GuJianGame liveGame;
        private static OriginalActor liveTarget;
        private static double liveStarted,liveUntil;
        private static float liveScale;
        private static int livePhase,liveFrame,liveFrames;
        private static List<string> liveChecks;
        public static bool LiveRunning=>liveGame;
        public static void RunLive()
        {
            Require(EditorApplication.isPlaying&&!LiveRunning,"Play first and await any previous live check");
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();liveScale=Time.timeScale;Stage(g);
            int damage=(int)typeof(ComboValidation).GetMethod("Hit",S).Invoke(null,new object[]{g,0,0,0,true});
            g.Actors[-1].ResetAction();var target=g.Actors[900];Refresh(g);
            Require(damage>0&&target.actionIndex==2&&Bar(target).enabled,"Live production hit fixture");
            liveGame=g;liveTarget=target;liveFrame=target.FrameIndex;liveFrames=0;livePhase=0;
            liveChecks=new List<string>{"A production attack deals real damage, enters hurt action2 and reveals the saved health Renderer before real background frames begin"};
            liveStarted=EditorApplication.timeSinceStartup;liveUntil=liveStarted+.4;Time.timeScale=.25f;Set(g,"menu",true);
            EditorApplication.update-=AdvanceLive;EditorApplication.update+=AdvanceLive;
        }
        private static void AdvanceLive()
        {
            try
            {
                Require(EditorApplication.isPlaying&&liveGame&&liveTarget,"Live fixture ended");liveFrames++;
                double now=EditorApplication.timeSinceStartup;Require(now-liveStarted<20,"Health live timeout");
                if(livePhase==0)
                {
                    if(now<liveUntil)return;
                    Require(liveTarget.actionIndex==2&&liveTarget.FrameIndex==liveFrame&&Bar(liveTarget).enabled,"Menu failed to hold hurt bar");
                    liveChecks.Add("Actual background Update preserves the hurt frame and visible health bar while the menu pauses actor animation");
                    Set(liveGame,"menu",false);liveTarget.frozen=true;livePhase=1;liveUntil=now+.4;
                }
                else if(livePhase==1)
                {
                    if(now<liveUntil)return;
                    Require(liveTarget.actionIndex==2&&liveTarget.FrameIndex==liveFrame&&Bar(liveTarget).enabled,"Frozen actor lost bar");
                    liveChecks.Add("Actual background frames retain the health bar and hurt pose while the enemy is frozen");
                    liveTarget.frozen=false;livePhase=2;
                }
                else if(liveTarget.actionIndex==0&&!Bar(liveTarget).enabled)
                {
                    liveChecks.Add("After unfreezing, actual background player frames complete the hurt action, return to idle and hide the health bar; editor callbacks="+liveFrames);
                    FinishLive(null);
                }
            }
            catch(Exception e){FinishLive(e.ToString());}
        }
        private static void FinishLive(string error)
        {
            EditorApplication.update-=AdvanceLive;
            if(liveGame)Set(liveGame,"menu",false);if(liveTarget)liveTarget.frozen=false;
            Time.timeScale=liveScale;Report("enemy-health-live",liveChecks,error);liveGame=null;liveTarget=null;
        }
        public static void PrepareVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Stage(g);var role=g.Actors[-1];role.mapPosition=new Vector2(210,250);role.hidden=true;
            foreach(var a in g.Actors.Values)if(a.actorId!=-1)a.hidden=true;
            var t=g.Actors[900];t.hidden=false;t.mapPosition=new Vector2(210,250);t.maxHp=1000;t.hp=500;t.Sample(2,0);Call(g,"UpdateCamera");Refresh(g);
        }
        private static void Save(string name)
        {
            var image=(Texture2D)typeof(HudFeedbackValidation).GetMethod("Render",S).Invoke(null,null);
            try{File.WriteAllBytes("../Reference/Validation/"+name+".png",image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
        }
        public static void CaptureVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var t=g.Actors[900];
            t.maxHp=0;Save("enemy-health-blank");t.maxHp=1000;
            foreach(int hp in new[]{0,1,52,53,500,999,1000}){t.hp=hp;Save("enemy-health-"+hp);}
            t.hp=500;t.mirror=true;Save("enemy-health-mirror");Set(g,"hideUi",true);Save("enemy-health-hideui");Set(g,"hideUi",false);
            WriteCaptureInfo(t,"enemy-health-capture");
        }
        public static void PrepareMapVisuals()
        {
            WorldPresentationValidation.PrepareMapVisuals();var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            g.Actors[-1].hidden=true;var t=g.Actors[900];t.hidden=false;t.layer=1;t.mapPosition=new Vector2(300,240);
            t.maxHp=1000;t.hp=500;t.Sample(2,0);Refresh(g);
        }
        public static void CaptureMapVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var t=g.Actors[900];
            t.maxHp=0;Save("enemy-health-map-covered-blank");t.maxHp=1000;Save("enemy-health-map-covered");
            t.layer=2;t.maxHp=0;Save("enemy-health-map-above-blank");t.maxHp=1000;Save("enemy-health-map-above");
            WriteCaptureInfo(t,"enemy-health-map-capture");
        }
        private static void WriteCaptureInfo(OriginalActor t,string name)
        {
            var image=(Texture2D)typeof(HudFeedbackValidation).GetMethod("Render",S).Invoke(null,null);
            try
            {
                var b=Bar(t).bounds;var cam=Camera.main.transform.position;
                var info=new CaptureInfo{width=image.width,height=image.height,logicalBounds=new Rect(b.min.x-(cam.x-266.5f),(cam.y+160)-b.max.y,b.size.x,b.size.y),footWidth=t.Animation.footWidth,edgeY=t.CurrentFrame.edgeRect.y};
                File.WriteAllText("../Reference/Validation/"+name+".json",JsonUtility.ToJson(info,true));
            }
            finally{UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}
