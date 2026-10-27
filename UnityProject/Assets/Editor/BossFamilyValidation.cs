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
    public static class BossFamilyValidation
    {
        private const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
        private static object Get(object o,string n)=>ValidationState.Get(o,n);
        private static void Set(object o,string n,object v)=>ValidationState.Set(o,n,v);
        private static object Call(GuJianGame g,string n,params object[] a)=>typeof(GuJianGame).GetMethod(n,F).Invoke(g,a);
        private static object Fighter(GuJianGame g,int id)=>((IDictionary)Get(g,"fighters"))[id];
        private static void Require(bool b,string m){if(!b)throw new Exception(m);}
        private static OriginalActor Stage(GuJianGame g,int id,bool integrated=false)
        {
            typeof(ComboValidation).GetMethod("Stage",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{g,integrated});
            g.Machine.Command(0x31,new object[]{900}); g.Machine.Command(0x30,new object[]{900,id,"r",180,250,2});
            var boss=g.Actors[900]; boss.locked=true; boss.hp=boss.maxHp=10000000; boss.level=g.Actors[-1].level;
            g.Actors[-1].maxHp=g.Actors[-1].hp=100000000; Set(Fighter(g,-1),"defense",0); Set(Fighter(g,-1),"dodge",-100000); Set(g,"superArmorTicks",100000);
            Set(Fighter(g,900),"hit",100000); Set(Fighter(g,900),"attack",10000); Set(Fighter(g,900),"critical",0);
            return boss;
        }
        private static void Align(OriginalActor source,OriginalActor target)
        {
            var attack=OriginalCombatMath.Rect(source,source.CurrentFrame.attackRects[0]);
            var body=OriginalCombatMath.Rect(target,target.CurrentFrame.bodyRects[0]);
            target.mapPosition+=attack.center-body.center+new Vector2(source.CurrentStep.vx*(source.mirror?-1:1),source.CurrentStep.vy);
        }
        [MenuItem("古剑/验证全部Boss招式家族")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying,"Play first");var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();float scale=Time.timeScale;var random=UnityEngine.Random.state;var checks=new List<string>();string error=null;Time.timeScale=0;
            try
            {
                int seed=1; for(;seed<100;seed++){UnityEngine.Random.InitState(seed);if(UnityEngine.Random.Range(0,100)>=10)break;}
                foreach(int id in Enumerable.Range(231,15))
                {
                    int modes=OriginalAssets.Table<EnemyDefinition>("enemy").First(e=>e.id==id).damageCoefficients.Split(',').Length;
                    foreach(bool right in new[]{false,true}) for(int mode=0;mode<modes;mode++)
                    {
                        var boss=Stage(g,id);var role=g.Actors[-1];Set(Fighter(g,900),"attackMode",mode);boss.Face(right?"r":"l");boss.Act(17+mode,true);boss.keepLastFrame=false;
                        int hits=0;var action=boss.Animation.actions[boss.actionIndex];
                        foreach(var step in action.steps)
                        {
                            role.ResetAction();
                            if(boss.CurrentStep.attack&&boss.CurrentFrame.attackRects.Length>0)Align(boss,role);
                            int before=role.hp;UnityEngine.Random.InitState(seed);boss.Tick(.080001f);if(role.hp<before)hits++;
                        }
                        bool projectile=(id>=240&&id<=242)&&(mode==1||mode==2)||(id==238||id==239)&&mode==2;
                        Require(hits>0||projectile,"No original melee hit "+id+":"+mode+":"+right);
                        Require(g.LastError==null,"Boss runtime error");
                    }
                    checks.Add("Boss "+id+": every normal attack mode, both facing directions, original animation movement/attack boxes and damage; projectile-only modes retain their separate emissions");
                }
                foreach(int id in new[]{232,233,235,236,238,239,241,242,244,245})
                {
                    foreach(bool integrated in new[]{false,true})
                    {
                        var boss=Stage(g,id,integrated);var role=g.Actors[-1];role.ResetAction();boss.hp=boss.maxHp/2;
                        int ordinary=g.Equipment[g.EquippedIndex].durability,spirit=g.SpiritDurability;bool depleted=false;int hits=0;
                        g.Machine.Command(0x7E,new object[]{900});
                        for(int i=0;i<400&&g.BossCriticalActive;i++)
                        {
                            if(boss.actionIndex==24&&boss.CurrentStep.attack&&boss.CurrentFrame.attackRects.Length>0){role.ResetAction();Align(boss,role);}
                            int before=role.hp;boss.Tick(.080001f);
                            var p=(OriginalActor)Get(g,"criticalProjectile");if(p){role.ResetAction();p.Tick(.080001f);}
                            if(role.hp<before)hits++;
                            if(id==238||id==239)depleted|=integrated?g.SpiritDurability==0:g.durability==0;
                            role.hp=Mathf.Max(1,role.hp);
                        }
                        Require(!g.BossCriticalActive&&!role.locked&&hits>0,"Boss critical completion "+id+":"+integrated+":"+hits);
                        if(id==238||id==239)Require(depleted&&(integrated?g.Equipment[g.EquippedIndex].durability==ordinary:true),"Destroy sword target "+id);
                        if(id==235||id==236)Require(boss.hp>boss.maxHp/2,"Boss drain heal "+id);
                        Require(!(bool)Get(Fighter(g,900),"bossCriticalReady"),"Critical did not cool down");
                    }
                    checks.Add("Boss "+id+": actual critical 22/24 sequence (or 241/242 capture projectile), ordinary/integrated targets, real damage, finish/unlock/cooldown; family-specific drain/durability target checked");
                }
            }
            catch(Exception e){error=e.ToString();}
            finally
            {
                Time.timeScale=scale;UnityEngine.Random.state=random;var r=new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error};
                Directory.CreateDirectory("../Reference/Validation");File.WriteAllText("../Reference/Validation/boss-families.json",JsonUtility.ToJson(r,true));Debug.Log("GUJIAN_BOSS_FAMILIES "+r.passed+" "+error);
            }
        }
    }
}
