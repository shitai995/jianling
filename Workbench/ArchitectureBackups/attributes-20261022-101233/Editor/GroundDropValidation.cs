using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class GroundDropValidation
    {
        private const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        private static object Call(GuJianGame g,string name,params object[] args)=>typeof(GuJianGame).GetMethod(name,Flags).Invoke(g,args);
        private static void Set(object o,string name,object value)=>o.GetType().GetField(name,Flags).SetValue(o,value);
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
        private static void Stage(GuJianGame g)
        {
            g.Begin();g.Machine.Clear();g.Machine.Values["event"]=-999;
            g.Machine.Enqueue("200");g.Machine.Tick();
            Require(!g.Machine.Running && !g.DialogueVisible && g.Map!=null,"Fixture stage failed");
            g.Machine.Command(0x30,new object[]{900,201,"l",300,250,2});
            foreach(var a in g.Actors.Values)a.locked=true;
        }
        private static void NoCoinSeed()
        {
            for(int seed=0;seed<1000;seed++)
            {
                UnityEngine.Random.InitState(seed);
                if(UnityEngine.Random.Range(0,100)<40)continue;
                UnityEngine.Random.InitState(seed);return;
            }
            throw new Exception("No suitable test seed");
        }
        private static void Spawn(GuJianGame g,string items="100,0",string equipment="")
        {
            NoCoinSeed();
            Call(g,"AwardDrops",g.Actors[900],new EnemyDefinition{money=100,experience=100,itemDrops=items,equipmentDrops=equipment},1);
        }
        private static void Steps(GuJianGame g,int count){for(int i=0;i<count;i++)Call(g,"TickDrops",.080001f);}
        private static void Write(string name,List<string> checks,string error)
        {
            File.WriteAllText("../Reference/Validation/"+name+".json",JsonUtility.ToJson(new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error},true));
        }
        [MenuItem("古剑/验证落地拾取与掉落资产")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Enter Play first");
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();var random=UnityEngine.Random.state;float scale=Time.timeScale;
            string save=g.saveFileName;var checks=new List<string>();string error=null;
            try
            {
                Time.timeScale=0;Stage(g);g.level=1;
                var assets=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Resources/Baked/Drops"});
                int expected=1+OriginalAssets.Table<ItemDefinition>("item").Length+OriginalAssets.Table<EquipmentDefinition>("equip").Select(e=>e.image.Split(',')[0]).Distinct().Count();
                Require(assets.Length==expected,"Missing saved drop prefabs");
                foreach(var guid in assets)
                {
                    var prefab=AssetDatabase.LoadAssetAtPath<OriginalGroundDrop>(AssetDatabase.GUIDToAssetPath(guid));
                    Require(prefab && prefab.GetComponentsInChildren<SpriteRenderer>(true).Where(r=>r.enabled).All(r=>r.sprite && AssetDatabase.Contains(r.sprite)),"Unbaked visible sprite or missing drop component: "+AssetDatabase.GUIDToAssetPath(guid));
                }
                checks.Add(expected+" saved drop prefabs with serialized components and persistent Sprite assets; no runtime sprite/child-node creation");

                int count=g.ItemCount(0);Spawn(g);var drop=g.GroundDrops.Single();var origin=drop.Position;
                Set(drop,"direction",2);g.Actors[-1].mapPosition=origin;
                Require(g.ItemCount(0)==count,"Loot entered inventory at spawn");
                Steps(g,5);Require(drop.Position==origin+new Vector2(0,-30) && drop.RemainingTicks==295,"Original five rising steps mismatch");
                Steps(g,7);Require(drop.Position==origin && drop.Landed,"Original falling/landing steps mismatch");
                Steps(g,29);Require(drop.RemainingTicks==259 && g.GroundDrops.Count==1 && g.ItemCount(0)==count,"Pickup started before original <260 gate");
                Steps(g,1);Require(g.GroundDrops.Count==0 && g.ItemCount(0)==count+1,"First magnet cycle did not collect");
                Call(g,"CollectDrop",drop);Require(g.ItemCount(0)==count+1,"Duplicate pickup awarded twice");
                checks.Add("Item follows -10/-8/-6/-4/-2 rise, matching descent/landing, 41 cycles without pickup, then magnet collects once on cycle 42");

                Spawn(g);drop=g.GroundDrops.Single();g.Actors[-1].mapPosition=new Vector2(100000,100000);count=g.ItemCount(0);
                Set(g,"menu",true);Steps(g,50);Require(drop.RemainingTicks==300,"Menu advanced loot timer");Set(g,"menu",false);
                Set(g,"dialogue","暂停");Steps(g,50);Require(drop.RemainingTicks==300,"Dialogue advanced loot timer");Set(g,"dialogue",null);
                Steps(g,300);Require(g.GroundDrops.Count==1 && drop.RemainingTicks==0,"Loot expired before its terminal cycle");
                Steps(g,1);Require(g.GroundDrops.Count==0 && g.ItemCount(0)==count,"Expiry granted reward or failed to remove loot");
                checks.Add("Menus/dialogue pause loot; unreachable item expires on cycle 301 without granting its payload");

                Spawn(g,"","100,5");drop=g.GroundDrops.Single();int bag=g.Equipment.Count;
                Require(drop.Kind==OriginalGroundDrop.DropKind.Weapon && drop.Equipment.id==5,"Wrong weapon payload");
                Require(drop.WeaponActor.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.sprite && r.sprite.texture.name=="w_12"),"Single weapon must display dropped equipment texture");
                g.Actors[-1].mapPosition=new Vector2(50,50);Steps(g,301);
                Require(g.GroundDrops.Count==1 && drop.Landed && drop.WeaponActor.actionIndex==1 && g.Equipment.Count==bag,"Weapon must loop landed action without magnet/expiry");
                g.Actors[-1].mapPosition=drop.Position;Call(g,"TryMove",g.Actors[-1],new Vector2(1,0));
                Require(g.GroundDrops.Count==0 && g.Equipment.Count==bag+1 && g.Equipment.Last().id==5,"Role movement did not collect original weapon payload");
                checks.Add("Single weapon displays its own sheet, plays original 18-frame landing into action 1, persists beyond 300 cycles, and enters the bag only after role movement touches its feet");

                Spawn(g,"100,0","100,5");drop=g.GroundDrops.Single(d=>d.Kind==OriginalGroundDrop.DropKind.Weapon);
                Require(drop.Equipment.id==5 && drop.WeaponActor.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.sprite && r.sprite.texture.name=="w_01"),"Original multi-drop role-weapon visual/payload distinction lost");
                Call(g,"ClearDrops");
                Spawn(g,string.Join(";",Enumerable.Range(0,12).Select(i=>"100,"+i).ToArray()));
                Require(g.GroundDrops.Count==9 && g.GroundDrops.Select(d=>d.Value).OrderBy(x=>x).SequenceEqual(Enumerable.Range(3,9)),"Original nine-slot reverse emission cap mismatch");
                Require(g.GroundDrops.Select(d=>d.Position).Distinct().Count()==9,"Scatter repeated a grid slot");
                checks.Add("Multi-drop weapon retains original role-sheet visual with actual reward payload; scatter uses unique 3x3 slots and original last-nine emission cap");

                Call(g,"ClearDrops");g.Actors[900].mapPosition=Vector2.zero;Spawn(g,"100,0;100,1");
                Require(g.GroundDrops.All(d=>d.Position.x>=15 && d.Position.x<=g.Map.Width-15 && d.Position.y>=15 && d.Position.y<=g.Map.Height-15),"Multi-drop boundary clamp failed");
                count=g.ItemCount(0);g.saveFileName="validation-ground-drops.json";g.Save();g.Load();g.Machine.Tick();
                Require(g.GroundDrops.Count==0 && g.ItemCount(0)==count,"Save/Load retained or granted uncollected loot");
                checks.Add("Multi-drop positions clamp inside map margins; real Save/Load clears uncollected objects without granting them");
            }
            catch(Exception e){error=e.ToString();Debug.LogException(e);}
            finally{Time.timeScale=scale;UnityEngine.Random.state=random;g.saveFileName=save;}
            Write("ground-drops",checks,error);if(error!=null)throw new Exception(error);
        }
        private static GuJianGame live;
        private static OriginalGroundDrop target;
        private static double started;
        private static int firstFrame,initialItems;
        private static Vector2 initialPosition;
        private static float previousScale;
        private static UnityEngine.Random.State previousRandom;
        private static bool sawRise,sawLanding;
        public static bool LiveRunning=>live!=null;
        [MenuItem("古剑/验证掉落实时落地与拾取")]
        public static void RunLive()
        {
            Require(EditorApplication.isPlaying && !EditorApplication.isPaused && !LiveRunning,"Enter unpaused Play and finish previous live check");
            live=UnityEngine.Object.FindObjectOfType<GuJianGame>();previousScale=Time.timeScale;previousRandom=UnityEngine.Random.state;
            Stage(live);live.level=1;
            var source=live.Actors[900];source.hp=1;source.level=1;source.deathScript=source.specialScript=null;
            var fighters=(System.Collections.IDictionary)typeof(GuJianGame).GetField("fighters",Flags).GetValue(live);
            fighters[900].GetType().GetField("enemy",Flags|BindingFlags.Public).SetValue(fighters[900],
                new EnemyDefinition{money=100,experience=100,itemDrops="100,0",equipmentDrops=""});
            NoCoinSeed();Call(live,"Damage",fighters[900],1,false);target=live.GroundDrops.Single();
            initialPosition=target.Position;initialItems=live.ItemCount(0);firstFrame=Time.frameCount;
            live.Actors[-1].mapPosition=initialPosition+new Vector2(180,0);
            // Keep the rise observable even when a minimized editor delivers a long first frame.
            started=EditorApplication.timeSinceStartup;sawRise=sawLanding=false;Time.timeScale=.5f;
            EditorApplication.update-=Advance;EditorApplication.update+=Advance;
        }
        private static void Advance()
        {
            string error=null;var checks=new List<string>();
            try
            {
                Require(EditorApplication.isPlaying && live && live.LastError==null,"Live test interrupted/runtime error");
                Require(EditorApplication.timeSinceStartup-started<30,"Live pickup timed out");
                if(target && live.GroundDrops.Contains(target))
                {
                    if(target.Position.y<initialPosition.y)sawRise=true;
                    if(target.Landed){sawLanding=true;live.Actors[-1].mapPosition=target.Position;}
                    return;
                }
                Require(Time.frameCount>firstFrame && sawRise && sawLanding && live.ItemCount(0)==initialItems+1,"Real Update failed bounce/landing/single pickup: rise="+sawRise+" landing="+sawLanding+" items="+live.ItemCount(0)+" initial="+initialItems);
                checks.Add("Production enemy death creates an uncollected saved item prefab; actual Unity Update advances its rise and landing into automatic pickup once; frames="+(Time.frameCount-firstFrame));
            }
            catch(Exception e){error=e.ToString();}
            EditorApplication.update-=Advance;Time.timeScale=previousScale;UnityEngine.Random.state=previousRandom;
            Write("ground-drops-live",checks,error);live=null;target=null;
            Debug.Log("GUJIAN_GROUND_DROPS_LIVE "+(error==null)+" "+error);
        }
    }
}
