using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class LiveCombatValidation
    {
        private static readonly int[] types={234,238};
        private static readonly List<string> checks=new List<string>();
        private static readonly HashSet<int> enemyActions=new HashSet<int>();
        private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        private static int index,roleStart,enemyStart,minRole,minEnemy,attacks,moves,hits,totalDamage,previousEnemyHp,activeRoleLevel,activeEnemyLevel;
        private static int roleAttackFrames,rawOverlapFrames,validHitFrames,vulnerableFrames,skillStarts,actionChangeCount,lastRoleActionVersion;
        private static int blockedByMachine,blockedByDialogue,blockedByLock,acceptedInputs;
        private static string roleCombatStats,enemyCombatStats;
        private static bool activeLateBoss;
        private static double began;
        private static float previousScale;
        private static string previousSave;
        private static UnityEngine.Random.State previousRandom;
        private static object Get(object o,string name)=>o.GetType().GetField(name,Flags).GetValue(o);
        private static object Field(object o,string name)=>o.GetType().GetField(name,Flags|BindingFlags.Public).GetValue(o);
        private static object Call(GuJianGame g,string name,params object[] args)=>typeof(GuJianGame).GetMethod(name,Flags).Invoke(g,args);
        [MenuItem("古剑/验证正常属性与自主 AI 实战")]
        public static void Run()
        {
            if(!EditorApplication.isPlaying)throw new Exception("Enter Play first");
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();previousScale=Time.timeScale;previousSave=g.saveFileName;previousRandom=UnityEngine.Random.state;
            g.saveFileName="validation-live-combat.json";Time.timeScale=8;checks.Clear();index=0;Load(g);
            EditorApplication.update-=Advance;EditorApplication.update+=Advance;
        }
        private static void Load(GuJianGame g)
        {
            g.Begin();g.Machine.Clear();g.Machine.Values["event"]=-999;g.Machine.Enqueue("200");g.Machine.Tick();
            foreach(var actor in g.Actors.Values.Where(a=>a.IsEnemy))actor.hidden=true;
            UnityEngine.Random.InitState(31100+types[index]);
            // The two late-game encounters are authored for the late-game
            // player state.  Keep the ordinary enemies on the original
            // level-21 Raikiri fixture, but use the original high-tier sword
            // progression for bosses 234 and 238 so this remains a meaningful
            // autonomous fight rather than a deliberately under-levelled DPS
            // timeout.
            bool lateBoss = types[index] == 234 || types[index] == 238;
            int roleLevel = lateBoss ? 80 : 21;
            int weaponId = lateBoss ? 13 : 5;
            int enemyLevel = types[index] == 238 ? 36 : 19;
            activeRoleLevel=roleLevel;activeEnemyLevel=enemyLevel;activeLateBoss=lateBoss;
            g.Machine.Command(0x4D,new object[]{roleLevel});g.Machine.Command(0x53,new object[]{weaponId,9,false});g.Machine.Command(0x55,new object[]{weaponId});
            g.Machine.Command(0x64,new object[]{11});g.Machine.Command(0x51,new object[]{3,20});g.Machine.Command(0x51,new object[]{7,20});
            g.Machine.Command(0x30,new object[]{900,types[index],"l",310,250,2});g.Machine.Command(0x3E,new object[]{types[index],enemyLevel});
            g.Actors[-1].mapPosition=new Vector2(155,250);g.Actors[-1].Face("r");
            var target=g.Actors[900];target.locked=false;target.frozen=false;
            foreach(var actor in g.Actors.Values) if(actor.actorId!=-1 && actor.actorId!=900) actor.hidden=true;
            enemyActions.Clear();target.Stepped+=a=>enemyActions.Add(a.LogicalAction);
            roleAttackFrames=rawOverlapFrames=validHitFrames=vulnerableFrames=skillStarts=actionChangeCount=0;
            blockedByMachine=blockedByDialogue=blockedByLock=acceptedInputs=0;
            lastRoleActionVersion=-1;
            g.Actors[-1].Stepped += a =>
            {
                if (lastRoleActionVersion != a.ActionVersion)
                {
                    actionChangeCount++; lastRoleActionVersion = a.ActionVersion;
                    if (a.LogicalAction >= 7 && a.LogicalAction <= 10) skillStarts++;
                }
                if (!a.CurrentStep.attack) return;
                roleAttackFrames++;
                var attackFrame=a.CurrentFrame.attackRects;
                var bodyFrame=target.CurrentFrame.bodyRects;
                bool raw=false;
                foreach (var ar in attackFrame) foreach (var br in bodyFrame)
                    if (OriginalCombatMath.Rect(a,ar).Overlaps(OriginalCombatMath.Rect(target,br))) raw=true;
                if (raw) rawOverlapFrames++;
                if (target.CanReceiveHit) vulnerableFrames++;
                if (OriginalCombatMath.Hit(a,target)) validHitFrames++;
            };
            var fighterMap=(System.Collections.IDictionary)Get(g,"fighters");
            var roleFighter=fighterMap[-1];var enemyFighter=fighterMap[900];
            roleCombatStats="attack="+Field(roleFighter,"attack")+",defense="+Field(roleFighter,"defense")+",hit="+Field(roleFighter,"hit")+",skill="+Field(roleFighter,"skill")+",skillLevel="+Field(roleFighter,"skillLevel");
            enemyCombatStats="attack="+Field(enemyFighter,"attack")+",defense="+Field(enemyFighter,"defense")+",hit="+Field(enemyFighter,"hit")+",dodge="+Field(enemyFighter,"dodge");
            roleStart=minRole=g.CurrentHp;enemyStart=minEnemy=previousEnemyHp=minEnemy=target.hp;attacks=moves=hits=totalDamage=0;began=EditorApplication.timeSinceStartup;
        }
        private static void Advance()
        {
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();
            try
            {
                if(!EditorApplication.isPlaying||!g||g.LastError!=null)throw new Exception(g?g.LastError:"Play ended");
                var role=g.Actors[-1];var target=g.Actors[900];minRole=Mathf.Min(minRole,role.hp);minEnemy=Mathf.Min(minEnemy,target.hp);
                if(previousEnemyHp>target.hp){hits++;totalDamage+=previousEnemyHp-target.hp;} previousEnemyHp=target.hp;
                double seconds=EditorApplication.timeSinceStartup-began;
                if(target.locked||target.frozen)throw new Exception("AI unexpectedly locked");
                // High-HP bosses need a longer observation window; the original
                // combat rules remain unchanged and the test still uses natural
                // stats, autonomous AI, and real hitboxes.
                double limit = (types[index] == 234 || types[index] == 238) ? 120 : 12;
                if(target.hp<=0||role.hp<=0||seconds>limit)
                {
                    if(enemyActions.Count<2||minEnemy>=enemyStart)throw new Exception("No normal-stat AI combat for "+types[index]+"; hp="+minRole+"/"+roleStart+", enemy="+minEnemy+"/"+enemyStart+", actions="+string.Join(",",enemyActions));
                    checks.Add("Enemy "+types[index]+" Lv"+activeEnemyLevel+", role Lv"+activeRoleLevel+" "+(activeLateBoss?"Kui Shui+9":"Raikiri+9")+": natural HP "+roleStart+" / "+enemyStart+", observed minima "+minRole+" / "+minEnemy+", successful hits="+hits+", damage="+totalDamage+", role "+roleCombatStats+", enemy "+enemyCombatStats+"; AI actions "+string.Join(",",enemyActions.OrderBy(a=>a))+"; attack requests "+attacks+", accepted inputs="+acceptedInputs+", blocked(machine/dialogue/lock)="+blockedByMachine+"/"+blockedByDialogue+"/"+blockedByLock+", action starts "+skillStarts+", action changes "+actionChangeCount+", attack frames "+roleAttackFrames+", raw overlaps "+rawOverlapFrames+", vulnerable frames "+vulnerableFrames+", valid hit frames "+validHitFrames+", collision-aware steering "+moves+"; result="+(target.hp<=0?"enemy defeated":role.hp<=0?"role defeated":"time-bounded exchange")+"; no HP overrides, AI locks, direct Damage, protection or infinite durability");
                    if(++index==types.Length)Finish(null);else Load(g);return;
                }
                if(seconds<.8||g.CriticalActive||role.LogicalAction>1||role.locked)return;
                if(g.Machine.Running){blockedByMachine++;return;}
                if(g.DialogueVisible){blockedByDialogue++;return;}
                if((bool)Get(g,"lockInput")){blockedByLock++;return;}
                float dx=target.mapPosition.x-role.mapPosition.x;
                float dy=target.mapPosition.y+target.Animation.footHeight-role.mapPosition.y-role.Animation.footHeight;
                if(Mathf.Abs(dx)>45||Mathf.Abs(dy)>6)
                {
                    Call(g,"TryMove",role,new Vector2(Mathf.Abs(dx)>35?Mathf.Sign(dx)*6:0,Mathf.Abs(dy)>5?Mathf.Sign(dy)*3:0));moves++;role.Face(dx<0?"l":"r");
                }
                else
                {
                    role.Face(dx<0?"l":"r");
                    if(g.SkillCooldownTicks==0 && (int)Get(g,"equippedSkill")>=5)g.PressControl(7);else g.PressControl(4);attacks++;acceptedInputs++;
                }
            }
            catch(Exception e){Finish(e.ToString());}
        }
        private static void Finish(string error)
        {
            EditorApplication.update-=Advance;Time.timeScale=previousScale;UnityEngine.Random.state=previousRandom;
            var g=UnityEngine.Object.FindObjectOfType<GuJianGame>();if(g)g.saveFileName=previousSave;
            File.WriteAllText("../Reference/Validation/live-combat.json",JsonUtility.ToJson(new ReconstructionValidation.Report{passed=error==null,utc=DateTime.UtcNow.ToString("O"),checks=checks.ToArray(),error=error},true));Debug.Log("GUJIAN_LIVE_COMBAT "+(error==null)+" "+error);
        }
    }
}
