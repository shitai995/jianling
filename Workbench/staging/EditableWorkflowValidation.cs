using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class EditableWorkflowValidation
    {
        private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        private static object Get(object o,string n)=>o.GetType().GetField(n,Flags|BindingFlags.Public).GetValue(o);
        private static void Set(object o,string n,object v)=>o.GetType().GetField(n,Flags|BindingFlags.Public).SetValue(o,v);
        private static object Call(GuJianGame g,string n,params object[] args)=>typeof(GuJianGame).GetMethod(n,Flags).Invoke(g,args);
        private static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        [MenuItem("古剑/验证编辑碰撞、计时存档与菜单资源")]
        public static void Run()
        {
            if(!EditorApplication.isPlaying)throw new Exception("Enter Play first");
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();float scale=Time.timeScale;string save=g.saveFileName;
            var report=new ReconstructionValidation.Report{utc=DateTime.UtcNow.ToString("O")};var checks=new List<string>();
            OriginalAssetCatalog catalog=null;int index=0,cell=0;bool blocked=false,touched=false;
            try
            {
                Time.timeScale=0;g.saveFileName="validation-editable-workflow.json";
                g.Begin();g.Machine.Clear();g.Machine.Values["event"]=-999;g.Machine.Enqueue("200");g.Machine.Tick();
                foreach(var actor in g.Actors.Values)actor.locked=true;
                catalog=OriginalAssets.Catalog;string id=catalog.stages.First(s=>s.script=="200").prefab.mapId;
                index=Array.FindIndex(catalog.maps,m=>m.id==id);var map=catalog.maps[index].data;int x=3,y=5;cell=x*map.rows+y;blocked=map.cells[cell].blocked;touched=true;
                MapCollisionEditor.SetCell(catalog,index,x,y,!blocked);AssetDatabase.SaveAssetIfDirty(catalog);
                Require(g.Map.CanPass(x*map.cellWidth+1,y*map.cellHeight+1)==blocked,"Saved collision edit not used by active production map");
                var imported=JsonUtility.FromJson<OriginalMap>(JsonUtility.ToJson(map));imported.cells[cell].blocked=blocked;
                MapCollisionEditor.ImportCollision(catalog,index,JsonUtility.ToJson(imported));
                Require(map.cells[cell].blocked==blocked,"Collision import failed");
                string before=JsonUtility.ToJson(map);imported.columns++;
                bool rejected=false;try{MapCollisionEditor.ImportCollision(catalog,index,JsonUtility.ToJson(imported));}catch(InvalidDataException){rejected=true;}
                Require(rejected&&JsonUtility.ToJson(map)==before,"Mismatched collision import changed data");
                checks.Add("Saved catalogue collision paint is read by the active production map; matching JSON imports collision only, dimension mismatch rejects without mutation; original cell restored");
                g.Machine.Command(0x64,new object[]{4});Set(g,"spiritElapsed",.037f);Set(g,"spiritDurabilityCounter",7);
                g.Machine.Command(0x83,new object[]{5,"guide4.s"});Call(g,"TickCombat",.25f);g.Machine.Command(0x84,new object[0]);
                g.Save();g.Load();g.Machine.Tick();
                Require((string)Get(g,"timerScript")=="guide4.s"&&Mathf.Abs((float)Get(g,"timerSeconds")-4.75f)<.0001f&&(bool)Get(g,"timerPaused"),"Paused timer save/load mismatch");
                Require((int)Get(g,"spiritDurabilityCounter")==7&&Mathf.Abs((float)Get(g,"spiritElapsed")-.037f)<.0001f,"Spirit cadence save/load mismatch");
                Call(g,"TickCombat",2f);Require(Mathf.Abs((float)Get(g,"timerSeconds")-4.75f)<.0001f,"Paused timer advanced");
                g.Machine.Command(0x85,new object[0]);Set(g,"menu",true);Call(g,"TickCombat",2f);Set(g,"menu",false);
                Require(Mathf.Abs((float)Get(g,"timerSeconds")-4.75f)<.0001f,"Menu timer advanced");
                Call(g,"TickCombat",4.5f);Require((string)Get(g,"timerScript")!=""&&Mathf.Abs((float)Get(g,"timerSeconds")-.25f)<.0001f,"Resumed timer timing");
                g.Save();g.Load();g.Machine.Tick();Require(!(bool)Get(g,"timerPaused")&&Mathf.Abs((float)Get(g,"timerSeconds")-.25f)<.0001f,"Active timer restore");
                Call(g,"TickCombat",.26f);g.Machine.Tick();
                Require(Get(g,"timerScript")==null&&g.Machine.CurrentScript=="guide4"&&g.DialogueVisible,"Timer expiry did not enqueue original callback exactly once");
                checks.Add("Paused and running x83 timers survive real Save/Load with fractional seconds; x84/x85 and menu pause retain time; expiry enqueues the original script once; sword-spirit 80ms accumulator and 11-tick cadence persist");
                g.Begin();g.Machine.Clear();g.Machine.Values["event"]=-999;g.Machine.Enqueue("200");g.Machine.Tick();g.Machine.Command(0x64,new object[]{1});
                var bag=(List<EquipmentInstance>)Get(g,"equipment");bag.Add(EquipmentInstance.Create(3,9));bag.Add(EquipmentInstance.Create(4));
                Require(g.SelectSpiritFood(1)&&g.ConfirmSpiritFeed(true),"Feed failed");int count=g.Equipment.Count,experience=g.SpiritExperience;
                Require(g.SpiritFeedAnimating&&!g.SelectSpiritFood(1),"Feed presentation did not block a new selection");
                Call(g,"TickSpiritFeedAnimation",.64f);Require(g.SpiritFeedAnimating&&Mathf.Abs(g.SpiritFeedProgress-.5f)<.001f,"Absorb midpoint");
                Call(g,"TickSpiritFeedAnimation",.65f);Require(!g.SpiritFeedAnimating&&g.SpiritFeedAnimationMessage.Contains("吸收完成")&&g.Equipment.Count==count&&g.SpiritExperience==experience,"Presentation repeated feed reward");
                checks.Add("Sword-spirit absorb runs a 1.28s presentation with actual source face/experience assets, blocks selection while playing, settles once, and preserves numerical results");
                var ui=(OriginalUiLayout)Get(g,"originalUi");Require(ui&&AssetDatabase.Contains(ui)&&ui.menu.actions.Length==35&&ui.menuSheets.Length==8,"Native menu atlas missing");
                foreach(var frame in ui.menu.frames)foreach(var piece in frame.pieces.Where(p=>p.image>=0))Require(piece.image<8&&ui.menuSheets[piece.image]&&piece.rect<ui.menu.sheets[piece.image].rects.Length,"Atlas piece reference invalid");
                foreach(int page in new[]{7,2,1,6,0,3,4}){Set(g,"menu",true);Require(g.SelectMenuPage(page),"Menu page refused "+page);Require((int)Get(g,"menuPage")==page,"Wrong menu page");}
                Require(ui.worldMap&&ui.spiritExperience,"Native map/experience assets absent");
                checks.Add("All 35 native menu actions reference saved source textures; role, equipment, 99-stack bag, sword-spirit, missions, system and world map share real page actions; no generated sprites/UI objects/textures");
                g.CloseMenu();report.passed=true;
            }
            catch(Exception e){report.error=e.ToString();}
            finally
            {
                if(touched&&catalog){catalog.maps[index].data.cells[cell].blocked=blocked;EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssetIfDirty(catalog);}
                Time.timeScale=scale;g.saveFileName=save;report.checks=checks.ToArray();File.WriteAllText("../Reference/Validation/editable-workflow.json",JsonUtility.ToJson(report,true));Debug.Log("GUJIAN_EDITABLE_WORKFLOW "+report.passed+" "+report.error);
            }
        }
    }
}
