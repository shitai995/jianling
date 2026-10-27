namespace GuJian
{
    // Compatibility rules live in one place; configured content uses explicit fields.
    public static class ItemRules
    {
        public static ItemEffectKind LegacyEffect(int id)
        {
            if (id >= 0 && id <= 3) return ItemEffectKind.HealHealth;
            if (id >= 4 && id <= 7) return ItemEffectKind.RestoreDurability;
            switch (id)
            {
                case 8: return ItemEffectKind.UpgradeWeaponSkill;
                case 9: return ItemEffectKind.Revive;
                case 10: return ItemEffectKind.FullyRepair;
                case 11: case 12: case 13: return ItemEffectKind.SwordSpiritExperience;
                case 14: case 15: return ItemEffectKind.Money;
                case 16: return ItemEffectKind.PlayerLevel;
                case 23: return ItemEffectKind.RandomWeaponSkill;
                default: return id >= 17 && id <= 22 ? ItemEffectKind.AttributeBonus : ItemEffectKind.None;
            }
        }
    }
}
