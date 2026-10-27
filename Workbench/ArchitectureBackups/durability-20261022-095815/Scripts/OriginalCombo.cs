using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private int comboHits, comboBreaks, breakCooldownTicks;
        private bool breakHit;
        public int ComboHits => comboHits;
        public int ComboBreaks => comboBreaks;
        public int ComboAttackPercent => comboHits / 10 + comboBreaks * 5;
        public int BreakCooldownTicks => breakCooldownTicks;

        private int ComboBonus(int attack)
        {
            int bonus = (int)((long)attack * ComboAttackPercent / 100);
            return comboHits > 9 ? Mathf.Max(1, bonus) : bonus;
        }
        private int RefreshedComboAttack(int attack) => attack + ComboBonus(attack);
        private void RecordComboHit()
        {
            comboHits++;
            NotifyComboHud();
            UpdateComboAttack();
        }
        private void UpdateComboAttack()
        {
            if (!Role || !fighters.TryGetValue(-1, out var f)) return;
            var data = OriginalAssets.Table<AttributeDefinition>("role")[0];
            int roleAttack = OriginalCombatMath.Growth(data.attack, level - 1);
            int weaponAttack = integrated ? SpiritAttribute(2) : Equipped?.Attack ?? 0;
            // Role.comboCounter excludes role prop AP; integrated bonus uses the spirit's base AP.
            int bonusBase = roleAttack + weaponAttack - (integrated ? spiritBonuses[2] : 0);
            f.attack = roleAttack + weaponAttack + ComboBonus(bonusBase);
        }
        private void ResetComboStatistics()
        {
            comboHits = comboBreaks = 0;
            breakHit = false;
            UpdateComboAttack();
        }
        private void CompleteBreakHit()
        {
            if (!Role || Role.LogicalAction != 11 || !breakHit) return;
            breakHit = false;
            breakCooldownTicks = 3;
            if (comboHits > 1) comboBreaks++;
            NotifyComboHud();
            UpdateComboAttack();
            if (infiniteDurability) return;
            int maximum = integrated ? SpiritAttribute(1) : Equipped?.MaximumDurability ?? 0;
            int remaining = Mathf.Max(0, durability - maximum / 3);
            // A non-breaking hit does not call updateAttribute in the source.
            durability = remaining;
            if (integrated) spiritDurability = remaining;
            else if (Equipped != null) Equipped.durability = remaining;
            if (remaining == 0 && !integrated)
            {
                DestroyOrdinarySword();
            }
        }
    }
}
