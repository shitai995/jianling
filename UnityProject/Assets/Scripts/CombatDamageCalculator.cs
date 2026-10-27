using System;
using UnityEngine;

namespace GuJian
{
    public readonly struct DamageRoll
    {
        public readonly int Amount;
        public readonly bool Critical;
        public DamageRoll(int amount, bool critical) { Amount = amount; Critical = critical; }
    }

    // Calculates numbers only. The host owns collision, combo updates, HP and effects.
    // Randomness is supplied so draw order and exclusive upper bounds can be verified.
    public static class CombatDamageCalculator
    {
        public static int RoleBaseDamage(int attack, int defense, int level, int enemyLevel, int injury, SkillDefinition skill, int skillLevel)
        {
            long raw = (long)attack * OriginalCombatMath.SkillValue(skill.damage, skillLevel, true) / 100
                + OriginalCombatMath.SkillValue(skill.amend, skillLevel, false) - defense;
            return (int)(raw * Mathf.Clamp(100 + (level - enemyLevel) * 5, 50, 150) * injury / 10000);
        }

        public static DamageRoll RoleHit(int attack, int defense, int level, int enemyLevel, int injury,
            SkillDefinition skill, int skillLevel, int criticalChance, int ordinaryMaximumDurability, Func<int, int, int> randomRange)
        {
            int damage = RoleBaseDamage(attack, defense, level, enemyLevel, injury, skill, skillLevel);
            // Break uses the ordinary sword maximum even during integration, without a variance draw.
            damage = skill.BehaviorId == 4 ? Mathf.Max(10, damage + ordinaryMaximumDurability * 3) : WithVariance(damage, 10, randomRange);
            bool critical = randomRange(0, 1000) < criticalChance;
            return new DamageRoll(critical ? damage * 2 : damage, critical);
        }

        public static int EnemyBaseDamage(int attack, int defense, int level, int targetLevel) =>
            (attack - defense) * Mathf.Clamp(100 + (level - targetLevel) * 5, 50, 150) / 100;

        public static int EnemyHit(int attack, int defense, int level, int targetLevel,
            EnemyDefinition enemy, int attackMode, Func<int, int, int> randomRange)
        {
            int damage = EnemyBaseDamage(attack, defense, level, targetLevel);
            if (attackMode >= 0) damage = damage * int.Parse(enemy.damageCoefficients.Split(',')[attackMode]) / 100;
            return WithVariance(damage, 1, randomRange);
        }

        public static int WithVariance(int value, int minimum, Func<int, int, int> randomRange)
        {
            int range = Mathf.Abs(value) / 10;
            return Mathf.Max(minimum, value + (range > 0 ? randomRange(-range, range) : 0));
        }

        public static int DodgeChance(int hit, int dodge, int attackLevel, int defenseLevel, bool role)
        {
            int chance = 100 - (hit - dodge + (attackLevel - defenseLevel) * 3);
            return role ? Mathf.Clamp(chance, 10, 90) : Mathf.Min(chance, 90);
        }
    }
}
