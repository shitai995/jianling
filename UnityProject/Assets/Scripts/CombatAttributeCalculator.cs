using System;
using System.Collections.Generic;

namespace GuJian
{
    // A value snapshot, not a second owner of actor or equipment state.
    public struct CombatAttributes
    {
        public int Hp, Durability, Attack, Defense, Hit, Dodge, Critical, Hold, Injury;
        public int SpiritValue(int index)
        {
            switch (index)
            {
                case 0: return Hp;
                case 1: return Durability;
                case 2: return Attack;
                case 3: return Defense;
                case 4: return Hit;
                case 5: return Dodge;
                case 6: return Critical;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    // Callers supply live database rows and state; calculation never mutates them.
    // HP application, refresh timing, death, visuals and skill effects stay with the host.
    public static class CombatAttributeCalculator
    {
        private static AttributeDefinition SpiritRow(IReadOnlyList<AttributeDefinition> rows, ref int level, out int lower)
        {
            level = Math.Max(1, Math.Min(level, 40));
            foreach (var row in rows)
            {
                var range = row.levels.Split(',');
                lower = int.Parse(range[0]);
                if (level >= lower && level <= int.Parse(range[1])) return row;
            }
            throw new InvalidOperationException("No sword-spirit growth row for level " + level);
        }
        private static string SpiritFormula(AttributeDefinition row, int index)
        {
            switch (index)
            {
                case 0: return row.hp;
                case 1: return row.durability;
                case 2: return row.attack;
                case 3: return row.defense;
                case 4: return row.hit;
                case 5: return row.dodge;
                case 6: return row.critical;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        public static int SpiritValue(IReadOnlyList<AttributeDefinition> rows, int level, int index, IReadOnlyList<int> bonuses)
        {
            var row = SpiritRow(rows, ref level, out int lower);
            return OriginalCombatMath.Growth(SpiritFormula(row, index), level - lower) + bonuses[index];
        }
        public static CombatAttributes Spirit(IReadOnlyList<AttributeDefinition> rows, int level, IReadOnlyList<int> bonuses)
        {
            var row = SpiritRow(rows, ref level, out int lower);
            int offset = level - lower;
            return new CombatAttributes {
                Hp = OriginalCombatMath.Growth(row.hp, offset) + bonuses[0],
                Durability = OriginalCombatMath.Growth(row.durability, offset) + bonuses[1],
                Attack = OriginalCombatMath.Growth(row.attack, offset) + bonuses[2],
                Defense = OriginalCombatMath.Growth(row.defense, offset) + bonuses[3],
                Hit = OriginalCombatMath.Growth(row.hit, offset) + bonuses[4],
                Dodge = OriginalCombatMath.Growth(row.dodge, offset) + bonuses[5],
                Critical = OriginalCombatMath.Growth(row.critical, offset) + bonuses[6]
            };
        }
        private static int ComboBonus(int attack, int hits, int breaks)
        {
            int bonus = (int)((long)attack * (hits / 10 + breaks * 5) / 100);
            return hits > 9 ? Math.Max(1, bonus) : bonus;
        }
        public static CombatAttributes Role(AttributeDefinition row, int level, CombatAttributes weapon,
            bool weaponActive, IReadOnlyList<int> bonuses, int hits, int breaks)
        {
            int enabled = weaponActive ? 1 : 0;
            int attack = OriginalCombatMath.Growth(row.attack, level - 1) + weapon.Attack + bonuses[1];
            return new CombatAttributes {
                Hp = OriginalCombatMath.Growth(row.hp, level - 1) + weapon.Hp * enabled + bonuses[0],
                Attack = attack + ComboBonus(attack, hits, breaks),
                Defense = OriginalCombatMath.Growth(row.defense, level - 1) + weapon.Defense * enabled + bonuses[2],
                Hit = OriginalCombatMath.Growth(row.hit, level - 1) + weapon.Hit * enabled + bonuses[3],
                Dodge = OriginalCombatMath.Growth(row.dodge, level - 1) + weapon.Dodge * enabled + bonuses[4],
                Critical = Math.Min(1000, OriginalCombatMath.Growth(row.critical, level - 1) + weapon.Critical * enabled + bonuses[5])
            };
        }
        // comboCounter excludes role prop AP; only the multiplier excludes spirit prop AP.
        public static int ComboBaseAttack(int roleAttack, int weaponAttack, int spiritAttackBonus) => roleAttack + weaponAttack - spiritAttackBonus;
        public static int ComboAttack(int roleAttack, int weaponAttack, int spiritAttackBonus, int hits, int breaks) =>
            roleAttack + weaponAttack + ComboBonus(ComboBaseAttack(roleAttack, weaponAttack, spiritAttackBonus), hits, breaks);

        public static CombatAttributes Enemy(IReadOnlyList<AttributeDefinition> rows, EnemyDefinition enemy, int type, int level)
        {
            int lv = Math.Max(10, Math.Min(level, 150));
            int band = lv < 100 ? lv / 10 - 1 : 8;
            int offset = lv < 100 ? lv % 10 : lv - 90;
            int group = type == 9 ? 0 : type == 11 ? 9 : 18;
            var row = rows[group + band];
            return new CombatAttributes {
                Hp = (int)((long)OriginalCombatMath.Growth(row.hp, offset) * enemy.hpCoefficient / 100),
                Attack = OriginalCombatMath.Growth(row.attack, offset) * enemy.attackCoefficient / 100,
                Defense = OriginalCombatMath.Growth(row.defense, offset),
                Hit = OriginalCombatMath.Growth(row.hit, offset) + enemy.hit,
                Dodge = OriginalCombatMath.Growth(row.dodge, offset) + enemy.dodge,
                Hold = enemy.hold, Injury = row.injuryCoefficient
            };
        }
    }
}
