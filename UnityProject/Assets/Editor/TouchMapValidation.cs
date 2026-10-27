using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class TouchMapValidation
    {
        private const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
        private static object Get(object o,string n)=>o.GetType().GetField(n,F).GetValue(o);
        private static void Set(object o,string n,object v)=>o.GetType().GetField(n,F).SetValue(o,v);
        private static object Call(GuJianGame g,string n,params object[] args)=>typeof(GuJianGame).GetMethod(n,F).Invoke(g,args);
        private static void Require(bool yes,string why) { if(!yes) throw new Exception(why); }
        private static void Stage(GuJianGame g)
        {
            typeof(ComboValidation).GetMethod("Stage",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{g,false});
            Set(g,"lockInput",false);
        }
        [MenuItem("古剑/验证触屏资源与区域小地图")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first"); var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var checks=new List<string>(); string error=null; float scale=Time.timeScale; var random=UnityEngine.Random.state; Time.timeScale=0;
            try
            {
                var layout=Resources.Load<OriginalTouchLayout>("OriginalTouchLayout");
                Require(layout && AssetDatabase.Contains(layout) && layout.presentationVersion==1,"Saved layout");
                Require(layout.direction.frames.Length==5 && layout.direction.actions.Length==0,"Source frame-only direction data");
                Require(layout.directionSheets.Length==2 && layout.directionSheets.All(t=>t&&AssetDatabase.Contains(t)),"Persistent direction sheets");
                Require(layout.criticalReady.actions.Length>0 && layout.criticalReadySheets.All(t=>t&&AssetDatabase.Contains(t)),"Critical ready animation");
                Require(layout.back&&layout.backPressed&&AssetDatabase.Contains(layout.back)&&AssetDatabase.Contains(layout.backPressed),"Back key");
                foreach(var control in layout.controls.Where(c=>c.code>=4 && c.code!=9))
                    Require(control.normal&&control.pressed&&AssetDatabase.Contains(control.normal)&&AssetDatabase.Contains(control.pressed),"Saved button "+control.code);
                checks.Add("Persistent five-frame direction data (zero actions is valid), both sheets, all normal/pressed buttons, back and critical-ready animation");
                var attack=layout.controls.First(c=>c.code==4);
                Require(attack.bounds==new Rect(462,254,70,60),"Attack hit zone");
                Require(GuJianGame.TouchImageBounds(attack,attack.normal)==new Rect(459,246,77,77),"Normal centre/integer anchor");
                Require(GuJianGame.TouchImageBounds(attack,attack.pressed)==new Rect(455,242,85,85),"Pressed must grow without shrinking to hit zone");
                Require(layout.controls.First(c=>c.code==12).bounds==new Rect(482,1,50,50)&&layout.directionBounds==new Rect(-1,197,124,124),"Source map/direction position");
                checks.Add("Original touch hit zones and independent native-size images: attack 77px normal / 85px pressed share (497,284), integer half-size anchor");
                var custom=ScriptableObject.CreateInstance<OriginalTouchLayout>();
                try
                {
                    custom.direction=new OriginalAnimation{frames=new OriginalFrame[0]};
                    custom.controls=new[]{new OriginalTouchLayout.Control{code=4,label="custom",bounds=new Rect(400,210,55,49),normal=layout.back},
                        new OriginalTouchLayout.Control{code=11,label="custom talk",bounds=new Rect(129,273,38,32),normal=layout.back}};
                    custom.directionBounds=new Rect(4,5,140,130); custom.opacity=.8f;
                    ControlAssetSetup.PrepareLayout(custom);
                    Require(custom.direction.frames.Length==5 && custom.directionSheets.All(t=>t),"Repair empty arrays");
                    Require(custom.controls[0].normal==layout.back && custom.controls[0].bounds==new Rect(400,210,55,49)&&custom.opacity==.8f&&custom.directionBounds==new Rect(4,5,140,130),"Custom edits overwritten");
                    Require(custom.controls.Any(c=>c.code==11&&c.label=="custom talk"&&c.normal==layout.back),"Customized legacy control removed");
                    string before=EditorJsonUtility.ToJson(custom); ControlAssetSetup.PrepareLayout(custom);
                    Require(EditorJsonUtility.ToJson(custom)==before,"Not idempotent");
                    var heldSheet=custom.directionSheets[0]; custom.directionSheets[1]=null; custom.controls[0].pressed=null; ControlAssetSetup.PrepareLayout(custom);
                    Require(custom.directionSheets[0]==heldSheet&&custom.directionSheets[1]&&custom.controls[0].pressed,"Individual missing refs not repaired");
                }
                finally { UnityEngine.Object.DestroyImmediate(custom); }
                checks.Add("Editor setup repairs empty direction and individual missing refs, preserves custom coordinates/textures/opacity and is idempotent");
                var map=Resources.Load<OriginalMiniMapLayout>("OriginalMiniMapLayout");
                var source=JsonUtility.FromJson<MiniMapData>(File.ReadAllText("../Reference/minimap.json"));
                Require(map&&map.areas.Length==13&&map.areas.Sum(a=>a.rooms.Length)==174,"Map coverage");
                Require(JsonUtility.ToJson(new MiniMapData{areas=map.areas})==JsonUtility.ToJson(source),"Baked map differs from source export");
                Require(map.current&&map.mission&&map.save&&AssetDatabase.Contains(map.current)&&AssetDatabase.Contains(map.mission)&&AssetDatabase.Contains(map.save),"Map icons");
                Require(map.areas[6].rooms.Any(r=>r.exits.Contains(2))&&map.areas[12].rooms.Any(r=>r.exits.Contains(-1)),"Split/external exits lost");
                checks.Add("Editable map asset matches all 13 recovered areas / 174 source rooms, split/external exits and original three persistent marker textures");
                Stage(g); int count=UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
                for(int area=0;area<13;area++)
                {
                    Set(g,"areaIndex",area); Set(g,"currentStageScript",map.areas[area].rooms[0].stage.ToString());
                    g.PressControl(12); Require(g.MiniMapVisible&&(bool)Get(g,"menu")&&g.TouchControlsVisible,"Open area "+area);
                    var p=g.Actors[-1].mapPosition; int action=g.Actors[-1].actionIndex;
                    g.PressControl(4); g.PressControl(7); g.PressControl(8); g.PressControl(9);
                    Require(g.Actors[-1].mapPosition==p&&g.Actors[-1].actionIndex==action,"Map accepted combat");
                    Call(g,"Update"); Require(g.MiniMapVisible&&g.Actors[-1].mapPosition==p,"Map update moved role");
                    g.PressControl(12); Require(!g.MiniMapVisible&&!(bool)Get(g,"menu"),"Return area "+area);
                }
                Require(UnityEngine.Object.FindObjectsOfType<GameObject>().Length==count,"Map created fixed nodes");
                checks.Add("All 13 areas open/return through production touch entry; map pauses gameplay and rejects combat, real Update works, no generated GameObjects");
                Stage(g); Set(g,"dialogue","waiting"); Require(!g.OpenMiniMap(),"Dialogue opened map"); Set(g,"dialogue",null);
                Set(g,"lockInput",true); Require(!g.OpenMiniMap(),"Locked opened map"); Set(g,"lockInput",false);
                Require(g.OpenMiniMap(),"Map before reset"); Stage(g); Require(!g.MiniMapVisible&&!(bool)Get(g,"menu"),"New stage retained map");
                checks.Add("Dialogue/locked state rejects map; new game and stage cleanup close it without leaving menu pause");
                Stage(g); var role=g.Actors[-1]; role.mapPosition=new Vector2(160,200);
                int npcDefinition=OriginalAssets.Table<SpriteDefinition>("sprite").First(d=>d.type==6).id;
                g.Machine.Command(0x30,new object[]{777,npcDefinition,"l",170,200,2});
                var npc=g.Actors[777]; npc.interactionScript="335n0";
                g.PressControl(4); Require(g.Machine.Running&&role.LogicalAction==0,"Primary key failed NPC interaction");
                Stage(g); foreach(var actor in g.Actors.Values) actor.interactionScript=null;
                g.PressControl(4); Require(g.Actors[-1].LogicalAction==7,"Primary key failed attack without NPC");
                checks.Add("Primary touch key triggers real nearby NPC callback before attacking; without interactable NPC it executes the production basic attack");
            }
            catch(Exception e) {error=e.ToString();}
            finally
            {
                g.CloseMiniMap(); Time.timeScale=scale; UnityEngine.Random.state=random;
                var report=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
                File.WriteAllText("../Reference/Validation/touch-map.json",JsonUtility.ToJson(report,true)); Debug.Log("GUJIAN_TOUCH_MAP_VALIDATION "+report.passed+" "+error);
            }
        }
    }
}
