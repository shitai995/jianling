using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private int comboHits, comboBreaks;
        private int breakCooldownTicks { get => Skills.BreakCooldownTicks; set => Skills.SetBreakCooldown(value); }
        private bool breakHit;
        public int ComboHits => comboHits;
        public int ComboBreaks => comboBreaks;
        public int ComboAttackPercent => comboHits / 10 + comboBreaks * 5;
        public int BreakCooldownTicks => breakCooldownTicks;

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
            f.attack = CombatAttributeCalculator.ComboAttack(roleAttack, weaponAttack, integrated ? spiritBonuses[2] : 0, comboHits, comboBreaks);
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
            // A non-breaking hit does not call updateAttribute in the source.
            int remaining = Weapons.Spend(maximum / 3);
            if (remaining == 0 && !integrated)
            {
                DestroyOrdinarySword();
            }
        }
    }
}
