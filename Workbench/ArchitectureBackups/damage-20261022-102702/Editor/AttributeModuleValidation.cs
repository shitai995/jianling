using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class AttributeModuleValidation
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Report(string name, List<string> checks, string error)
        {
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/" + name + ".json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_" + name + " " + report.passed + " " + error);
        }
        private static void Equal(int[] actual, int[] expected, string message)
        {
            Require(actual.Length == expected.Length, message + " length");
            for (int i = 0; i < actual.Length; i++) Require(actual[i] == expected[i], message + " [" + i + "]: " + actual[i] + " != " + expected[i]);
        }
        private static int[] RoleValues(CombatAttributes v) => new[] { v.Hp, v.Attack, v.Defense, v.Hit, v.Dodge, v.Critical };
        private static int[] EnemyValues(CombatAttributes v) => new[] { v.Hp, v.Attack, v.Defense, v.Hit, v.Dodge, v.Hold, v.Injury };
        [MenuItem("古剑/验证属性计算模块")]
        public static void Run()
        {
            var checks = new List<string>(); string error = null;
            try
            {
                var row = new AttributeDefinition { hp = "100,10", attack = "20,3", defense = "5,2", hit = "60,1", dodge = "4,1", critical = "995,1" };
                var weapon = new CombatAttributes { Hp = 50, Attack = 9, Defense = 7, Hit = 8, Dodge = 3, Critical = 30 };
                var bonuses = new[] { 11, 5, 2, 3, 4, 5 };
                Equal(RoleValues(CombatAttributeCalculator.Role(row, 3, weapon, true, bonuses, 10, 1)), new[] { 181, 42, 18, 73, 13, 1000 }, "Active role");
                Equal(RoleValues(CombatAttributeCalculator.Role(row, 3, weapon, false, bonuses, 10, 1)), new[] { 131, 42, 11, 65, 10, 1000 }, "Inactive role");
                row.critical = "-10"; Require(CombatAttributeCalculator.Role(row, 1, default, false, bonuses, 0, 0).Critical == -5, "Critical lower bound changed");
                checks.Add("Role fixture covers growth, permanent bonuses, combo refresh, broken-weapon AP retention and critical upper-only cap");
                Require(CombatAttributeCalculator.ComboAttack(26, 1009, 1000, 10, 1) == 1037 && CombatAttributeCalculator.ComboBaseAttack(26, 1009, 1000) == 35, "Spirit combo AP");
                Require(CombatAttributeCalculator.ComboAttack(1, 0, 0, 10, 0) == 2 && CombatAttributeCalculator.ComboAttack(1, 0, 0, 9, 1) == 1 && CombatAttributeCalculator.ComboAttack(0, 0, 0, 10, 0) == 1, "Combo minimum");
                Require(CombatAttributeCalculator.ComboAttack(1000000000, 0, 0, 20, 0) == 1020000000, "Combo intermediate overflow");
                checks.Add("Combo fixture covers spirit bonus exclusion from multiplier only, 9/10-hit minimum and long multiplication");
                var rows = new[] { new AttributeDefinition { levels = "1,10", hp = "100", durability = "100", attack = "1", defense = "1", hit = "1", dodge = "1", critical = "1" },
                    new AttributeDefinition { levels = "11,40", hp = "200,20", durability = "500,30", attack = "30,3", defense = "20,2", hit = "40,4", dodge = "6,1", critical = "7,2" } };
                var spiritBonus = new[] { 1, 2, 3, 4, 5, 6, 7 };
                var spirit = CombatAttributeCalculator.Spirit(rows, 12, spiritBonus);
                int[] expected = { 221, 532, 36, 26, 49, 13, 16 };
                for (int i = 0; i < 7; i++) Require(spirit.SpiritValue(i) == expected[i] && CombatAttributeCalculator.SpiritValue(rows, 12, i, spiritBonus) == expected[i], "Spirit field " + i);
                Require(CombatAttributeCalculator.Spirit(rows, 100, spiritBonus).Hp == 781 && CombatAttributeCalculator.Spirit(rows, 0, spiritBonus).Hp == 101, "Spirit clamp");
                spirit.Hp = -1; Require(rows[1].hp == "200,20" && spiritBonus[0] == 1, "Input mutation");
                rows[1].hp = "300,20"; Require(CombatAttributeCalculator.Spirit(rows, 12, spiritBonus).Hp == 321, "Stale spirit row");
                checks.Add("Spirit fixtures cover all seven fields, level bands/clamps, independent snapshots and immediate live-row edits");
                var enemyRows = new AttributeDefinition[27];
                for (int i = 0; i < 27; i++) enemyRows[i] = new AttributeDefinition { hp = ((i + 1) * 100) + ",10", attack = (i + 1) + ",3", defense = "50,2", hit = "20,1", dodge = "5,1", injuryCoefficient = i + 10 };
                var enemy = new EnemyDefinition { hpCoefficient = 125, attackCoefficient = 150, hit = 7, dodge = 9, hold = 88 };
                Equal(EnemyValues(CombatAttributeCalculator.Enemy(enemyRows, enemy, 11, 99)), new[] { 2362, 67, 68, 36, 23, 88, 27 }, "Enemy fixture");
                Require(CombatAttributeCalculator.Enemy(enemyRows, enemy, 9, 0).Hp == 125 && CombatAttributeCalculator.Enemy(enemyRows, enemy, 10, 100).Hp == 3500 && CombatAttributeCalculator.Enemy(enemyRows, enemy, 10, 200).Hp == 4125, "Enemy bands/clamps");
                enemyRows[26].hp = "2000000000"; enemy.hpCoefficient = 2;
                Require(CombatAttributeCalculator.Enemy(enemyRows, enemy, 10, 150).Hp == 40000000, "Enemy HP intermediate overflow");
                checks.Add("Enemy fixtures cover three family offsets, 99/100 growth, level clamps, coefficients, hold/injury and long HP multiplication");
            }
            catch (Exception e) { error = e.ToString(); }
            Report("attribute-modules", checks, error);
        }
        private static object Call(GuJianGame g, string name, params object[] args) => typeof(GuJianGame).GetMethod(name, Flags).Invoke(g, args);
        private static object Fighter(GuJianGame g, int id) => ((IDictionary)ValidationState.Get(g, "fighters"))[id];
        private static int[] Actual(OriginalActor actor, object f, bool enemy) => enemy
            ? new[] { actor.maxHp, (int)ValidationState.Get(f, "attack"), (int)ValidationState.Get(f, "defense"), (int)ValidationState.Get(f, "hit"), (int)ValidationState.Get(f, "dodge"), (int)ValidationState.Get(f, "hold"), (int)ValidationState.Get(f, "injury") }
            : new[] { actor.maxHp, (int)ValidationState.Get(f, "attack"), (int)ValidationState.Get(f, "defense"), (int)ValidationState.Get(f, "hit"), (int)ValidationState.Get(f, "dodge"), (int)ValidationState.Get(f, "critical") };
        [MenuItem("古剑/验证属性计算战斗接入")]
        public static void RunIntegration()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var random = UnityEngine.Random.state; string save = g.saveFileName, error = null;
            float scale = Time.timeScale; var checks = new List<string>();
            try
            {
                Time.timeScale = 0; g.saveFileName = "validation-attribute-integration.json";
                g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999; g.Machine.Enqueue("200"); g.Machine.Tick();
                foreach (var actor in g.Actors.Values) actor.locked = true;
                g.level = 3; ValidationState.Set(g, "permanentBonuses", new[] { 11, 5, 2, 3, 4, 5 });
                ValidationState.Set(g, "comboHits", 10); ValidationState.Set(g, "comboBreaks", 1);
                var role = g.Actors[-1]; var f = Fighter(g, -1);
                role.maxHp = 100; role.hp = 33; ValidationState.Set(f, "hold", 17);
                Call(g, "RefreshAttributes", role, false);
                Equal(Actual(role, f, false), new[] { 2161, 291, 137, 114, 58, 32 }, "Production ordinary role");
                Require(role.hp == 713 && (int)ValidationState.Get(f, "hold") == 17, "Non-heal ratio/hold");
                ValidationState.Set(f, "dead", true); Call(g, "RefreshAttributes", role, true);
                Require(role.hp == 2161 && (int)ValidationState.Get(f, "hold") == 49 && !(bool)ValidationState.Get(f, "dead"), "Role heal/death");
                checks.Add("Production ordinary role matches fixed golden values; non-heal preserves rounded HP ratio and hold, heal fills HP/reset death and sets hold49");
                g.durability = 0; Call(g, "RefreshAttributes", role, false);
                Equal(Actual(role, f, false), new[] { 2111, 291, 122, 107, 56, 27 }, "Production broken sword");
                checks.Add("Production zero-durability ordinary sword removes gated equipment attributes but retains AP");
                g.Machine.Command(0x64, new object[] { 12 });
                ValidationState.Set(g, "spiritBonuses", new[] { 1, 2, 1000, 4, 5, 6, 7 });
                int[] spirit = { 5601, 772, 1940, 464, 27, 14, 63 };
                for (int i = 0; i < 7; i++) Require(g.GetSpiritAttribute(i) == spirit[i], "Production spirit " + i);
                g.Weapons.SetSpirit(772, 772); g.Machine.Command(0x7F, new object[0]);
                Call(g, "RefreshAttributes", role, true);
                Equal(Actual(role, f, false), new[] { 7712, 2316, 586, 134, 70, 90 }, "Production integrated role");
                Require((int)ValidationState.Get(f, "hold") == 99, "Integrated hold");
                Call(g, "UpdateComboAttack"); Require((int)ValidationState.Get(f, "attack") == 2250, "Production combo event AP");
                checks.Add("Production spirit seven-field snapshot/integration matches golden values; refresh AP2316 and combo-event AP2250 preserve distinct original bonus rules");
                var growth = OriginalAssets.Table<AttributeDefinition>("ego")[1]; string hp = growth.hp;
                try { growth.hp = "12345"; Require(g.GetSpiritAttribute(0) == 12346, "Live production growth edit"); }
                finally { growth.hp = hp; }
                Require(g.GetSpiritAttribute(0) == 5601, "Growth edit restoration");
                checks.Add("Production growth table edits apply immediately without cached formulas; in-memory fixture restores the original row");
                int[] ids = { 201, 203, 231 }, levels = { 19, 20, 99, 100, 150 };
                int[][][] expected = {
                    new[] { new[] {13980,1110,190,112,6,1,120}, new[] {15000,1350,200,115,15,1,100}, new[] {338400,12300,990,610,691,1,100}, new[] {342000,12450,1000,625,710,1,100}, new[] {522000,19950,1500,1375,1660,1,100} },
                    new[] { new[] {20680,897,228,182,66,49,110}, new[] {22000,1080,240,185,75,49,100}, new[] {292710,9000,1188,680,751,49,90}, new[] {295900,9100,1200,695,770,49,90}, new[] {455400,14100,1800,1445,1720,49,90} },
                    new[] { new[] {165000,1302,456,205,94,99,90}, new[] {200000,1380,480,210,105,99,90}, new[] {2410000,11570,2376,707,801,99,50}, new[] {2500000,11800,2400,720,820,99,50}, new[] {7000000,23300,3600,1370,1770,99,50} }
                };
                for (int i = 0; i < ids.Length; i++)
                {
                    int id = 900 + i; g.Machine.Command(0x30, new object[] { id, ids[i], "l", 300, 250, 2 });
                    var enemy = g.Actors[id]; var ef = Fighter(g, id); enemy.locked = true;
                    for (int j = 0; j < levels.Length; j++)
                    {
                        enemy.level = levels[j]; ValidationState.Set(ef, "dead", true); Call(g, "RefreshAttributes", enemy, true);
                        Equal(Actual(enemy, ef, true), expected[i][j], "Enemy " + ids[i] + " level " + levels[j]);
                        Require(enemy.hp == enemy.maxHp && !(bool)ValidationState.Get(ef, "dead"), "Enemy heal/death");
                    }
                    checks.Add("Production enemy " + ids[i] + " golden HP/AP/DP/hit/dodge/hold/injury at levels19/20/99/100/150; refresh heals/resets death");
                }
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                g.saveFileName = save; g.Begin(); g.Machine.Clear(); g.Machine.Command(0x69, new object[0]);
                Time.timeScale = scale; UnityEngine.Random.state = random;
            }
            Report("attribute-module-integration", checks, error);
        }
    }
}
