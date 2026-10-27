using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class DamageModuleValidation
    {
        private sealed class Rolls
        {
            private readonly Queue<Vector3Int> expected = new Queue<Vector3Int>();
            public Rolls(params Vector3Int[] draws) { foreach (var draw in draws) expected.Enqueue(draw); }
            public int Next(int min, int max)
            {
                Require(expected.Count > 0, "Unexpected random draw");
                var draw = expected.Dequeue(); Require(min == draw.x && max == draw.y, "Random interval/order changed: " + min + "," + max);
                Require(draw.z >= min && draw.z < max, "Fixture outside exclusive interval"); return draw.z;
            }
            public void Done() { Require(expected.Count == 0, "Missing random draw"); }
        }
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        [MenuItem("古剑/验证伤害计算模块")]
        public static void Run()
        {
            var checks = new List<string>(); string error = null;
            try
            {
                var skill = new SkillDefinition { id = 5, damage = "100", amend = "0" };
                Require(CombatDamageCalculator.RoleBaseDamage(100, 21, 1, 20, 125, skill, 1) == 49 &&
                    CombatDamageCalculator.RoleBaseDamage(100, 21, 20, 1, 125, skill, 1) == 148, "Level clamp/integer order");
                skill.damage = "200";
                Require(CombatDamageCalculator.RoleBaseDamage(1000000000, 0, 20, 1, 50, skill, 1) == 1500000000, "Role long intermediates");
                skill.damage = "50,5,9,5"; skill.amend = "0,500,9,500";
                Require(CombatDamageCalculator.RoleBaseDamage(200, 10, 1, 1, 100, skill, 1) == 600 &&
                    CombatDamageCalculator.RoleBaseDamage(200, 10, 1, 1, 100, skill, 9) == 5190, "Skill level9 formula");
                checks.Add("Fixed role base values cover integer order, 50/150 level limits, long intermediates and skill level1/9 expressions");
                skill.damage = "100"; skill.amend = "0";
                var rolls = new Rolls(new Vector3Int(-8, 8, -8), new Vector3Int(0, 1000, 49));
                var result = CombatDamageCalculator.RoleHit(101, 21, 1, 1, 100, skill, 1, 50, 999, rolls.Next); rolls.Done();
                Require(result.Amount == 144 && result.Critical, "Variance before critical");
                rolls = new Rolls(new Vector3Int(-8, 8, 7), new Vector3Int(0, 1000, 50));
                result = CombatDamageCalculator.RoleHit(101, 21, 1, 1, 100, skill, 1, 50, 0, rolls.Next); rolls.Done();
                Require(result.Amount == 87 && !result.Critical, "Critical strict threshold");
                checks.Add("Role hits request variance before critical, retain exclusive upper bounds, strict critical threshold and ignore ordinary durability for non-break skills");
                rolls = new Rolls(new Vector3Int(0, 1000, 0));
                result = CombatDamageCalculator.RoleHit(1, 0, 1, 1, 100, skill, 1, 1, 0, rolls.Next); rolls.Done();
                Require(result.Amount == 20 && result.Critical, "Minimum before doubling");
                rolls = new Rolls(new Vector3Int(-10, 10, -10), new Vector3Int(0, 1000, 0));
                result = CombatDamageCalculator.RoleHit(0, 100, 1, 1, 100, skill, 1, 0, 0, rolls.Next); rolls.Done();
                Require(result.Amount == 10 && !result.Critical, "Negative base variance/floor");
                checks.Add("Small base skips variance but still draws critical; negative base draws absolute variance; minimum10 precedes doubling");
                skill.id = 4; rolls = new Rolls(new Vector3Int(0, 1000, 999));
                result = CombatDamageCalculator.RoleHit(100, 20, 1, 1, 100, skill, 1, 1000, 7, rolls.Next); rolls.Done();
                Require(result.Amount == 202 && result.Critical, "Break ordinary maximum and critical");
                checks.Add("Break adds ordinary maximum durability times3, skips variance entirely and still rolls/doubles critical");
                var enemy = new EnemyDefinition { damageCoefficients = "125,50" };
                rolls = new Rolls(new Vector3Int(-10, 10, -10));
                Require(CombatDamageCalculator.EnemyHit(101, 20, 2, 1, enemy, 0, rolls.Next) == 96, "Enemy coefficient/rounding"); rolls.Done();
                rolls = new Rolls(new Vector3Int(-4, 4, 3));
                Require(CombatDamageCalculator.EnemyHit(10, 100, 1, 1, enemy, 1, rolls.Next) == 1, "Enemy negative floor"); rolls.Done();
                rolls = new Rolls(new Vector3Int(-8, 8, 7));
                Require(CombatDamageCalculator.EnemyHit(101, 20, 2, 1, null, -1, rolls.Next) == 92, "No attack mode"); rolls.Done();
                rolls = new Rolls(); Require(CombatDamageCalculator.WithVariance(9, 1, rolls.Next) == 9 && CombatDamageCalculator.WithVariance(-9, 1, rolls.Next) == 1, "Zero-range draw"); rolls.Done();
                Require(CombatDamageCalculator.EnemyBaseDamage(30000000, 0, 20, 1) == 2050327, "Existing enemy int arithmetic changed");
                checks.Add("Enemy damage retains level-then-mode rounding, no-mode lookup skip, negative floor1, exclusive variance and existing int intermediate semantics");
                Require(CombatDamageCalculator.DodgeChance(1000, 0, 1, 1, false) == -900 && CombatDamageCalculator.DodgeChance(1000, 0, 1, 1, true) == 10 &&
                    CombatDamageCalculator.DodgeChance(0, 1000, 1, 1, false) == 90 && CombatDamageCalculator.DodgeChance(0, 1000, 1, 1, true) == 90 &&
                    CombatDamageCalculator.DodgeChance(100, 0, 1, 10, false) == 27, "Dodge asymmetry");
                checks.Add("Dodge retains role10..90 versus enemy upper-only90 and three points per level");
            }
            catch (Exception e) { error = e.ToString(); }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/damage-modules.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_DAMAGE_MODULE " + report.passed + " " + error);
        }
    }
}
