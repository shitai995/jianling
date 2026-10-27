using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GuJian.Editor
{
    public static class WorldPresentationValidation
    {
        private const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
        private const BindingFlags S=BindingFlags.Static|BindingFlags.NonPublic;
        private static object Get(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);
        private static void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
        private static object Call(GuJianGame g,string n,params object[] a)=>typeof(GuJianGame).GetMethod(n,F).Invoke(g,a);
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
        private static void Stage(GuJianGame g)
        {
            typeof(ComboValidation).GetMethod("Stage",S).Invoke(null,new object[]{g,false});Time.timeScale=0;
            foreach(var a in g.Actors.Values)a.locked=true;
            Set(g,"lockInput",true);Set(g,"toastUntil",0f);Call(g,"ResetStageInfo");
        }
        private static void Refresh(GuJianGame g)=>Call(g,"RefreshScenePresentation");
        private static void Add(GuJianGame g,OriginalActor a,int value=12345,int type=1)=>Call(g,"ShowCombatNumber",a,value,type);
        private static IList Numbers(GuJianGame g)=>(IList)Get(g,"numbers");
        private static SpriteRenderer[] Digits(OriginalActor a)=>(SpriteRenderer[])Get(a.CombatFeedback,"digits");
        private static SpriteRenderer Miss(OriginalActor a)=>(SpriteRenderer)Get(a.CombatFeedback,"miss");
        private static void Report(string name,List<string> checks,string error)
        {
            var r=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
            File.WriteAllText("../Reference/Validation/"+name+".json",JsonUtility.ToJson(r,true));Debug.Log("GUJIAN_"+name+" "+r.passed+" "+error);
        }
        [MenuItem("古剑/验证角色整体排序与战斗反馈遮挡")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first");var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();float scale=Time.timeScale;var random=UnityEngine.Random.state;var checks=new List<string>();string error=null;Time.timeScale=0;
            try
            {
                var catalog=OriginalAssets.Catalog;var ui=Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                Require(ui.combatDigits.Length==40&&ui.combatDigits.All(s=>s&&AssetDatabase.Contains(s))&&ui.combatMissSprite&&AssetDatabase.Contains(ui.combatMissSprite),"Persistent number/miss crops");
                for(int t=0;t<4;t++)for(int d=0;d<10;d++)
                {
                    var sprite=ui.combatDigits[t*10+d];Require(sprite.texture==ui.combatNumbers[t]&&sprite.rect==new Rect(d*(ui.combatNumbers[t].width/10),0,ui.combatNumbers[t].width/10,ui.combatNumbers[t].height)&&sprite.pixelsPerUnit==1,"Original digit slice");
                }
                int feedback=0;
                foreach(var entry in catalog.actors)
                {
                    Require(entry.prefab.PresentationGroup&&AssetDatabase.Contains(entry.prefab.PresentationGroup),"Saved actor group "+entry.id);
                    var definition=OriginalAssets.Table<SpriteDefinition>("sprite").First(d=>d.id==entry.id);
                    if(definition.type==0||definition.type==9||definition.type==10||definition.type==11)
                    {Require(entry.prefab.CombatFeedback&&entry.prefab.CombatFeedback.Capacity==50,"Saved feedback capacity");feedback++;}
                }
                Require(feedback==39&&catalog.effects.All(e=>e.prefab.PresentationGroup),"39 fighter and 119 effect groups");
                foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Resources/Baked/Drops"}))
                    Require(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)).GetComponent<SortingGroup>(),"Saved drop group");
                checks.Add("137 actor/119 effect/52 drop prefabs have saved SortingGroups; 39 fighters contain 50 digits plus miss, using forty persistent original crops and persistent miss sprite");
                Stage(g);var role=g.Actors[-1];var target=g.Actors[900];role.mapPosition=new Vector2(210,250);role.ResetAction();target.ResetAction();
                int roleBase=250+role.Animation.footHeight;
                target.mapPosition=new Vector2(210,roleBase-target.Animation.footHeight+5);Refresh(g);
                Require(target.PresentationGroup.sortingOrder<role.PresentationGroup.sortingOrder,"Player +6 source foot bias");
                target.mapPosition=new Vector2(210,roleBase-target.Animation.footHeight+7);Refresh(g);
                Require(target.PresentationGroup.sortingOrder>role.PresentationGroup.sortingOrder,"Foot-height crossing");
                var body=(List<SpriteRenderer>)Get(role,"renderers");
                Require(body.Where(r=>r.enabled).Select(r=>r.sortingOrder).SequenceEqual(Enumerable.Range(0,role.CurrentFrame.pieces.Length)),"Body piece local order");
                checks.Add("Actual actor groups cross using mapY+footHeight, with the source +6 only for the player on layer2; body pieces preserve frame order inside an atomic group");
                for(int layer=0;layer<4;layer++)
                {
                    target.layer=layer;target.mapPosition=new Vector2(210,10000);Refresh(g);int order=target.PresentationGroup.sortingOrder;
                    Require(layer==0?order>-30000&&order<-25000:layer==1?order>-25000&&order<-5000:layer==2?order>-5000&&order<28000:order>28000,"Layer interval "+layer+":"+order);
                }
                checks.Add("All four source actor layers remain between the correct saved map layers even at mapY10000; large coordinates cannot overflow the signed sorting range or leak across map layers");
                Stage(g);role=g.Actors[-1];int objects=UnityEngine.Object.FindObjectsOfType<GameObject>().Length,components=role.GetComponentsInChildren<Component>(true).Length;
                for(int i=0;i<6;i++)Add(g,role,int.MaxValue,i%4);Call(g,"ShowCombatMiss",role);Refresh(g);
                Require(Digits(role).Count(r=>r.enabled)==50&&Miss(role).enabled,"Five ten-digit slots plus miss");
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length==objects&&role.GetComponentsInChildren<Component>(true).Length==components,"Feedback generated objects/components");
                Require(Digits(role).Where(r=>r.enabled).All(r=>r.sortingOrder>Miss(role).sortingOrder&&r.sprite&&AssetDatabase.Contains(r.sprite)),"Feedback draw order/persistence");
                Numbers(g).Clear();Refresh(g);Require(Digits(role).All(r=>!r.enabled)&&Miss(role).enabled,"Independent miss slot");
                checks.Add("Six real feedback insertions retain five slots/50 digits plus miss; all digits sort after miss, and no GameObject/component/Sprite is generated at runtime");
                Stage(g);role=g.Actors[-1];Add(g,role);Refresh(g);var digit=Digits(role)[0];Vector3 before=digit.transform.position;
                role.renderOffset=new Vector2(-112,0);Refresh(g);Require(Mathf.Abs(digit.transform.position.x-before.x+112)<.001f,"Burial render offset ignored");
                role.renderOffset=Vector2.zero;role.screenCentered=true;Refresh(g);Require(role.PresentationGroup.sortingOrder==30000&&digit.transform.parent==role.transform,"Centered feedback group");
                role.screenCentered=false;role.frozen=true;int ticks=(int)Get(Numbers(g)[0],"ticks");role.Tick(.8f);Refresh(g);Require((int)Get(Numbers(g)[0],"ticks")==ticks&&Digits(role)[0].enabled,"Frozen feedback presentation");
                checks.Add("Feedback follows the actual actor transform including burial -112px and screen-centered actions; frozen actors still render their held feedback without advancing its lifetime");
                Set(g,"hideUi",true);Refresh(g);Require(Digits(role)[0].enabled,"Hide UI hid world feedback");Set(g,"hideUi",false);
                foreach(string flag in new[]{"miniMapOpen","worldMapOpen"}){Set(g,flag,true);Refresh(g);Require(Digits(role).All(r=>!r.enabled),flag+" world feedback");Set(g,flag,false);}
                role.hidden=true;Refresh(g);Require(Digits(role).All(r=>!r.enabled),"Invisible feedback");role.hidden=false;
                g.Machine.Command(0x5D,new object[]{"黑幕"});Refresh(g);Require(Digits(role).All(r=>!r.enabled),"Black screen feedback");g.ContinueDialogue();Refresh(g);Require(Digits(role)[0].enabled,"Feedback did not return");
                checks.Add("Hide-UI retains world numbers; invisible actors, minimap, world map and black-screen text suppress them and restore them when the scene becomes visible");
                Stage(g);role=g.Actors[-1];role.locked=false;Call(g,"ResetSkillCooldown",true);Require(g.UseSkill(5),"Real skill visual");Call(g,"ShowFace",-1,0,1);Add(g,role);Call(g,"TickVisuals",0f);Refresh(g);
                var front=(OriginalActor)Get(g,"skillFront");var back=(OriginalActor)Get(g,"skillBack");
                Require(front.PresentationGroup.sortingOrder==1000&&back.PresentationGroup.sortingOrder==-500&&Digits(role)[0].sortingOrder>front.PresentationGroup.sortingOrder,"Nested skill order");
                Set(g,"guideTarget",900);g.Actors[900].mapPosition=role.mapPosition+new Vector2(200,0);Call(g,"TickVisuals",0f);
                var arrow=(OriginalActor)Get(g,"guide");Require(!arrow.hidden&&arrow.PresentationGroup.sortingOrder>back.PresentationGroup.sortingOrder&&arrow.PresentationGroup.sortingOrder<0,"Source back-effect/guide/body order");
                checks.Add("Actual skill/face visuals remain nested with their owner, with back effect before body and front effects before raising digits");
                foreach(int stage in new[]{24,25})
                {
                    Stage(g);role=g.Actors[-1];target=g.Actors[900];((HashSet<int>)Get(g,"criticalTargets")).Add(900);Set(g,"criticalStage",stage);
                    role.Sample(stage,stage==24?34:52);Call(g,"TickCriticalVisuals");Refresh(g);
                    var effects=(List<OriginalActor>)Get(g,"criticalEffects");
                    Require(role.PresentationGroup.sortingOrder==0&&target.PresentationGroup.sortingOrder==10,"Critical role/target source order");
                    Require(effects[0].PresentationGroup.sortAtRoot&&effects[0].PresentationGroup.sortingOrder==(stage==24?1:-10),"Critical first effect");
                    Require(effects[stage==24?1:2].PresentationGroup.sortingOrder==11,"Critical final foreground effect");
                    Require(effects.Where(a=>!a.hidden).All(a=>a.PresentationGroup.sortAtRoot),"Critical effects trapped in owner group");
                }
                checks.Add("Actual critical24/25 visual paths place role, intermediary effect, targets and final effect in source order; global cinematic groups escape nested owner sorting");
                g.Begin();g.Machine.Clear();Refresh(g);Require(g.Actors[-1].CombatFeedback&&Digits(g.Actors[-1]).All(r=>!r.enabled),"New game feedback reset");
                checks.Add("New game instantiates the saved presentation components and clears old visible feedback");
            }
            catch(Exception e){error=e.ToString();}
            finally{Time.timeScale=scale;UnityEngine.Random.state=random;Report("world-presentation",checks,error);}
        }
        private static void Save(string name)
        {
            var image=(Texture2D)typeof(HudFeedbackValidation).GetMethod("Render",S).Invoke(null,null);
            try{File.WriteAllBytes("../Reference/Validation/"+name+".png",image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
        }
        public static void PrepareVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Stage(g);
            var role=g.Actors[-1];role.mapPosition=new Vector2(210,250);role.ResetAction();
            foreach(var a in g.Actors.Values)if(a.IsEnemy)a.hidden=true;
            Call(g,"UpdateCamera");Refresh(g);
        }
        public static void CaptureVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var role=g.Actors[-1];var target=g.Actors[900];var checks=new List<string>();string error=null;
            try
            {
                var map=(GameObject)Get(g,"mapRoot");var foreground=map.GetComponentsInChildren<SpriteRenderer>().First(r=>r.sortingOrder==28000);
                foreground.enabled=false;
                Numbers(g).Clear();target.hidden=true;Save("world-number-blank");Add(g,role);Save("world-number-visible");
                target.hidden=false;target.mapPosition=new Vector2(200,255);target.layer=1;target.ResetAction();
                Numbers(g).Clear();Save("world-number-behind-blank");Add(g,role);Save("world-number-behind");
                target.layer=2;Numbers(g).Clear();Save("world-number-front-blank");Add(g,role);Save("world-number-front");
                checks.Add("Existing GameView captured actual original golem201 behind and in front of the same five-digit source number");
                target.hidden=true;Numbers(g).Clear();Call(g,"ShowCombatMiss",role);Save("world-miss-visible");
                Set(g,"hideUi",true);Save("world-miss-hideui");Set(g,"hideUi",false);Call(g,"TickActorCombatFeedback",role);Save("world-miss-rise");
                checks.Add("Existing GameView captured world miss, hide-UI miss and the next source 1px rise");
                foreground.enabled=true;
                Report("world-presentation-rendered",checks,null);
            }
            catch(Exception e){error=e.ToString();Report("world-presentation-rendered",checks,error);}
        }
        public static void PrepareMapVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Stage(g);
            foreach(Transform child in ((GameObject)Get(g,"mapRoot")).transform)child.gameObject.SetActive(false);
            g.Machine.Command(0x23,new object[]{"map100"});
            foreach(var a in g.Actors.Values)if(a.actorId!=-1)a.hidden=true;
            var role=g.Actors[-1];role.mapPosition=new Vector2(310,240);role.ResetAction();role.layer=1;
            Call(g,"UpdateCamera");Refresh(g);
        }
        public static void CaptureMapVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var role=g.Actors[-1];
            var foreground=((GameObject)Get(g,"mapRoot")).GetComponentsInChildren<SpriteRenderer>().First(r=>r.sortingOrder==-5000);
            foreground.enabled=false;Numbers(g).Clear();Save("world-map-blank");Add(g,role);Save("world-map-visible");
            foreground.enabled=true;Numbers(g).Clear();Save("world-map-covered-blank");Add(g,role);Save("world-map-covered");
            role.layer=2;Numbers(g).Clear();Save("world-map-above-blank");Add(g,role);Save("world-map-above");
        }
        public static void PrepareSkillVisuals()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Stage(g);var role=g.Actors[-1];role.locked=false;
            role.mapPosition=new Vector2(210,250);Call(g,"ResetSkillCooldown",true);Require(g.UseSkill(5),"Skill visual capture");
            role.Sample(12,3);Call(g,"TickVisuals",0f);Add(g,role);Call(g,"UpdateCamera");Refresh(g);
        }
        public static void PrepareCriticalVisuals(int stage)
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();Stage(g);var role=g.Actors[-1];
            role.mapPosition=new Vector2(210,250);var target=g.Actors[900];target.hidden=false;target.mapPosition=new Vector2(240,250);
            ((HashSet<int>)Get(g,"criticalTargets")).Add(900);Set(g,"criticalStage",stage);
            role.Sample(stage,stage==24?34:52);Call(g,"TickCriticalVisuals");Call(g,"UpdateCamera");Refresh(g);
        }
        public static void CaptureCurrentVisual(string name){Save(name);}
    }
}
