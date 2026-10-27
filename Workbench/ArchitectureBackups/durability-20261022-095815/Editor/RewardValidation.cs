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
    public static class RewardValidation
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static void Set(object o, string key, object value) => o.GetType().GetField(key, Flags | BindingFlags.Public).SetValue(o, value);
        private static object Call(GuJianGame g, string name, params object[] args) => typeof(GuJianGame).GetMethod(name, Flags).Invoke(g, args);
        private static void Require(bool ok, string message) { if (!ok) throw new Exception(message); }
        private static HashSet<int> Samples(GuJianGame g, string method, EnemyDefinition enemy, int level)
        {
            var values = new HashSet<int>();
            for (int seed = 0; seed < 2048; seed++)
            {
                UnityEngine.Random.InitState(seed);
                values.Add((int)Call(g, method, enemy, level));
            }
            return values;
        }
        private static void Range(HashSet<int> values, int min, int max, string name)
        {
            Require(values.Min() == min && values.Max() == max && values.All(x => x >= min && x <= max), name);
        }
        [MenuItem("古剑/验证击杀经验与掉落数值")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var random = UnityEngine.Random.state; var scale = Time.timeScale;
            var checks = new List<string>();
            var report = new ReconstructionValidation.Report { utc = DateTime.UtcNow.ToString("O") };
            try
            {
                Time.timeScale = 0;
                g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999;
                g.Machine.Enqueue("200"); g.Machine.Tick();
                Require(!g.Machine.Running && !g.DialogueVisible, "Fixture stage did not settle");
                var enemy = new EnemyDefinition { experience = 100, money = 100, equipmentDrops = "", itemDrops = "" };
                g.level = 1; Set(g, "experienceRoom", false);
                Range(Samples(g,"EnemyExperience",enemy,1),15,15,"Level-one XP has no variance");
                g.level = 2;
                Range(Samples(g,"EnemyExperience",enemy,1),13,13,"Normal lower-level integer truncation");
                Set(g,"experienceRoom",true);
                var training = Samples(g,"EnemyExperience",enemy,1);
                Require(training.SetEquals(new[]{26,27,28}),"Training must double before level scaling, then include both variance endpoints");
                checks.Add("Source arithmetic: level-1 XP 15, player-level-2 XP 13; training produces 26/27/28 around 27 rather than doubling truncated 13");

                g.level=1;Set(g,"experienceRoom",false);
                Range(Samples(g,"EnemyExperience",enemy,11),1169,1291,"Upper level coefficient cap or inclusive XP bound");
                g.level=40;
                Range(Samples(g,"EnemyExperience",enemy,1),1,1,"Lower level coefficient cap");
                Set(g,"experienceRoom",true);
                Range(Samples(g,"EnemyExperience",enemy,1),3,3,"Training low-level clamp");
                g.level=100;Set(g,"experienceRoom",false);
                Range(Samples(g,"EnemyExperience",enemy,100),9500,10500,"Base XP cap");
                checks.Add("XP base cap, 10%/200% level coefficient clamps and inclusive +/-5% endpoints match source examples across low and high levels");

                g.level=1;
                Range(Samples(g,"EnemyDropMoney",enemy,1),20,21,"Money upper variance bound must be exclusive");
                enemy.money=50;
                Range(Samples(g,"EnemyDropMoney",enemy,1),9,9,"Original empty random interval returns -1");
                enemy.money=100;g.level=100;
                Range(Samples(g,"EnemyDropMoney",enemy,100),95,104,"Money base cap");
                checks.Add("Money variance uses exclusive upper endpoint; tiny reward preserves Tool.getNextRnd(empty)=-1; capped base 100 yields 95..104");

                g.level=10;Set(g,"experienceRoom",false);
                var normal=Samples(g,"EnemyDropMoney",enemy,1);
                Set(g,"experienceRoom",true);
                Require(normal.SetEquals(Samples(g,"EnemyDropMoney",enemy,1)),"Training must change drop chance, not coin amount");
                Range(normal,20,21,"Level difference nine must not suppress money");
                g.level=11;
                Range(Samples(g,"EnemyDropMoney",enemy,1),10,10,"Level difference ten must force ten coins");
                checks.Add("Training retains each coin amount; level gap nine keeps normal coins and gap ten forces ten coins");

                Set(g,"experienceRoom",false);g.level=20;g.experience=0;g.money=0;
                g.Machine.Command(0x30,new object[]{900,201,"l",250,250,2});
                var actor=g.Actors[900];actor.locked=true;actor.level=21;actor.hp=1;
                actor.deathScript=null;actor.specialScript=null;
                var fighters=(IDictionary)typeof(GuJianGame).GetField("fighters",Flags).GetValue(g);
                Set(fighters[900],"enemy",enemy);Set(g,"dropItems",true);
                UnityEngine.Random.InitState(701);
                Call(g,"AwardDrops",actor,enemy,21);int expectedMoney=g.GroundDrops.Where(d=>d.Kind==OriginalGroundDrop.DropKind.Money).Sum(d=>d.Value);
                int expectedXp=(int)Call(g,"EnemyExperience",enemy,21);
                int next=UnityEngine.Random.Range(0,1000000);
                Call(g,"ClearDrops");g.money=0;UnityEngine.Random.InitState(701);
                Call(g,"Damage",fighters[900],1,false,-1);
                Require(g.experience==expectedXp && g.money==0 && actor.hp==0 && g.GroundDrops.Where(d=>d.Kind==OriginalGroundDrop.DropKind.Money).Sum(d=>d.Value)==expectedMoney,"Production death reward/order mismatch");
                Require(UnityEngine.Random.Range(0,1000000)==next,"Death must consume drop RNG before experience RNG");
                checks.Add("Production Damage/death grants sampled XP, creates uncollected coin payloads without changing balance, and consumes drop/scatter RNG before experience RNG");

                enemy.equipmentDrops="100,5";enemy.itemDrops="100,0";
                g.level=10;int equipment=g.Equipment.Count;int count=g.ItemCount(0);
                Call(g,"ClearDrops");Call(g,"AwardDrops",actor,enemy,1);
                Require(g.Equipment.Count==equipment && g.ItemCount(0)==count && g.GroundDrops.Count(d=>d.Kind==OriginalGroundDrop.DropKind.Weapon)==1 && g.GroundDrops.Count(d=>d.Kind==OriginalGroundDrop.DropKind.Item)==1,"Gap-nine guaranteed ground payloads missing or granted too early");
                g.level=11;equipment=g.Equipment.Count;count=g.ItemCount(0);
                Call(g,"ClearDrops");Call(g,"AwardDrops",actor,enemy,1);
                Require(g.Equipment.Count==equipment && g.ItemCount(0)==count && g.GroundDrops.All(d=>d.Kind==OriginalGroundDrop.DropKind.Money),"Gap-ten equipment/item suppression missing");
                checks.Add("Ground equipment/item payloads are generated at a nine-level gap and suppressed at ten; uncollected rewards do not enter the bag");
                report.passed=true;
            }
            catch(Exception e) { report.error=e.ToString();Debug.LogException(e); }
            finally { Time.timeScale=scale;UnityEngine.Random.state=random; }
            report.checks=checks.ToArray();
            File.WriteAllText("../Reference/Validation/rewards.json",JsonUtility.ToJson(report,true));
            if(!report.passed) throw new Exception(report.error);
            Debug.Log("GUJIAN_REWARD_VALIDATION True");
        }
    }
}
