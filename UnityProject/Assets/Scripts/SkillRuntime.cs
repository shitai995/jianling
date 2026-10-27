using System;

namespace GuJian
{
    public enum SkillCastFailure { None, InvalidSkill, Cooldown, Durability, ActionLocked }
    [Serializable]
    public sealed class SkillRuntimeSnapshot
    {
        public int skillCooldownTicks, breakCooldownTicks, energy, superArmorTicks, superArmorElapsed;
        public int recoveryTicks, recoveryAction, pendingSkill, pendingVersion, pendingAmount;
    }

    public sealed class SkillRuntime
    {
        private readonly WeaponDurability weapons;
        public SkillRuntime(WeaponDurability weapons) { this.weapons = weapons ?? throw new ArgumentNullException(nameof(weapons)); }
        public int SkillCooldownTicks { get; private set; }
        public int BreakCooldownTicks { get; private set; }
        public int Energy { get; private set; }
        public int SuperArmorTicks { get; private set; }
        public int SuperArmorElapsed { get; private set; }
        public int RecoveryTicks { get; private set; }
        public int RecoveryAction { get; private set; }
        public int PendingSkill { get; private set; } = -1;
        public int PendingVersion { get; private set; }
        public int PendingAmount { get; private set; }

        public SkillCastFailure TryPayCost(int behavior, int cost, bool scripted, bool infiniteDurability, bool cooldownDisabled,
            int cooldown, bool weaponAvailable, out int durabilityBefore)
        {
            durabilityBefore = weapons.Current;
            if (!weaponAvailable && !scripted) return SkillCastFailure.ActionLocked;
            if (!scripted && behavior == 4 && durabilityBefore <= 0 && !infiniteDurability) return SkillCastFailure.Durability;
            if (!scripted && behavior >= 5 && SkillCooldownTicks > 0) return SkillCastFailure.Cooldown;
            if (scripted || behavior < 5 || behavior > 21) return SkillCastFailure.None;
            if (behavior < 14 && !infiniteDurability && cost > 0)
            {
                if (durabilityBefore <= 0) return SkillCastFailure.Durability;
                weapons.Spend(cost);
            }
            SetSkillCooldown(cooldownDisabled ? 1 : cooldown);
            return SkillCastFailure.None;
        }

        public void SetSkillCooldown(int ticks) => SkillCooldownTicks = Math.Max(0, ticks);
        public void SetBreakCooldown(int ticks) => BreakCooldownTicks = Math.Max(0, ticks);
        public void SetEnergy(int value) => Energy = Math.Max(0, Math.Min(2000, value));
        public void SetRecovery(int ticks, int action) { RecoveryTicks = Math.Max(0, ticks); RecoveryAction = action; }
        public void SetSuperArmor(int ticks) => SuperArmorTicks = Math.Max(0, ticks);
        public void RestoreArmor(int ticks, int elapsed) { SetSuperArmor(ticks); SuperArmorElapsed = Math.Max(0, elapsed); }
        public void ActivateArmor(int amount) => SetSuperArmor(Math.Max(1, amount - SuperArmorElapsed + 1));
        public bool IsSupportCasting(int version, int action) => PendingSkill >= 14 && action == 21 && version == PendingVersion;
        public void PrepareSupport(SkillDefinition skill, int version, int maximumHp)
        {
            // The recovered support calculator always uses level 1.
            int amount = skill.BehaviorId < 17
                ? maximumHp * OriginalCombatMath.SkillValue(skill.damage, 1, true) / 100 + OriginalCombatMath.SkillValue(skill.amend, 1, false)
                : OriginalCombatMath.SkillValue(skill.amend, 1, false);
            BeginSupport(skill.BehaviorId, version, amount);
        }
        public void ResetSupport()
        {
            SkillCooldownTicks = SuperArmorTicks = SuperArmorElapsed = RecoveryTicks = RecoveryAction = 0;
            CancelSupport(); PendingVersion = PendingAmount = 0;
        }

        public void BeginSupport(int skill, int version, int amount)
        { PendingSkill = skill; PendingVersion = version; PendingAmount = amount; }

        public void CancelSupport() { PendingSkill = -1; }

        // Loading a stage starts a new actor action. Its unfinished cast cannot resume.
        public void CancelAction()
        {
            SetRecovery(0, 0);
            CancelSupport(); PendingVersion = PendingAmount = 0;
        }

        public bool TryCompleteSupport(int currentVersion, int action, out int skill, out int amount)
        {
            skill = PendingSkill;
            amount = PendingAmount;
            if (skill < 14 || action != 21 || currentVersion != PendingVersion) return false;
            CancelSupport();
            return true;
        }

        public void Tick(bool mayAdvanceCooldowns, bool mayAdvanceArmor, bool mayAdvanceRecovery)
        {
            if (mayAdvanceCooldowns)
            {
                if (SkillCooldownTicks > 0) SkillCooldownTicks--;
                if (BreakCooldownTicks > 0) BreakCooldownTicks--;
            }
            if (mayAdvanceArmor && SuperArmorTicks > 0)
            {
                SuperArmorTicks--;
                if (SuperArmorTicks == 0) SuperArmorElapsed = 0;
                else SuperArmorElapsed++;
            }
            if (mayAdvanceRecovery && RecoveryTicks > 0) RecoveryTicks--;
        }

        public void Reset()
        {
            SkillCooldownTicks = BreakCooldownTicks = Energy = SuperArmorTicks = SuperArmorElapsed = RecoveryTicks = RecoveryAction = 0;
            CancelSupport(); PendingVersion = PendingAmount = 0;
        }

        public SkillRuntimeSnapshot Capture() => new SkillRuntimeSnapshot {
            skillCooldownTicks = SkillCooldownTicks, breakCooldownTicks = BreakCooldownTicks, energy = Energy,
            superArmorTicks = SuperArmorTicks, superArmorElapsed = SuperArmorElapsed,
            recoveryTicks = RecoveryTicks, recoveryAction = RecoveryAction, pendingSkill = PendingSkill,
            pendingVersion = PendingVersion, pendingAmount = PendingAmount
        };

        public void Restore(SkillRuntimeSnapshot snapshot)
        {
            Reset();
            if (snapshot == null) return;
            SetSkillCooldown(snapshot.skillCooldownTicks); SetBreakCooldown(snapshot.breakCooldownTicks);
            SetEnergy(snapshot.energy); SetRecovery(snapshot.recoveryTicks, snapshot.recoveryAction);
            SetSuperArmor(snapshot.superArmorTicks); SuperArmorElapsed = Math.Max(0, snapshot.superArmorElapsed);
            if (snapshot.pendingSkill >= 14) BeginSupport(snapshot.pendingSkill, snapshot.pendingVersion, snapshot.pendingAmount);
        }
    }
}
