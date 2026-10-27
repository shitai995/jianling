using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private int skillCooldownTicks, superArmorTicks, superArmorElapsed, supportRecoveryTicks, supportRecoveryAction;
        private int pendingSupportSkill = -1, pendingSupportVersion, pendingSupportAmount;
        private OriginalActor supportRecoveryVisual, supportArmorVisual;
        public int SkillCooldownTicks => skillCooldownTicks;
        public int SuperArmorTicks => superArmorTicks;
        public bool SupportSkillCasting => pendingSupportSkill >= 14 && Role && Role.LogicalAction == 21 && Role.ActionVersion == pendingSupportVersion;

        private void ResetSkillCooldown(bool ready = false)
        {
            var data = OriginalAssets.Table<SkillDefinition>("skill").FirstOrDefault(s => s.id == equippedSkill);
            skillCooldownTicks = ready || data == null ? 0 : disableCooldown ? 1 : data.cooldown;
        }
        private void CancelSupportCast() { pendingSupportSkill = -1; }
        private void ResetSupportSkills()
        {
            CancelSupportCast(); skillCooldownTicks = superArmorTicks = superArmorElapsed = supportRecoveryTicks = 0;
            supportRecoveryVisual = supportArmorVisual = null;
        }
        private void TickRoleSkillTimers()
        {
            if (!Role || Role.frozen || menu || reinforceOpen || DialogueVisible || criticalStage != 0) return;
            if (skillCooldownTicks > 0 && (integrated || durability > 0)) skillCooldownTicks--;
            if (breakCooldownTicks > 0 && (integrated || durability > 0)) breakCooldownTicks--;
            if (superArmorTicks > 0) { superArmorTicks--; if (superArmorTicks == 0) superArmorElapsed = 0; else superArmorElapsed++; }
            if (supportRecoveryTicks > 0) supportRecoveryTicks--;
            if (pendingSupportSkill >= 0 && !SupportSkillCasting) CancelSupportCast();
        }
        private void PrepareSupportSkill(SkillDefinition skill)
        {
            pendingSupportSkill = skill.id; pendingSupportVersion = Role.ActionVersion;
            // Role.getRecoverPoint constructs Skill(data), whose level is always 1 in the recovered version.
            pendingSupportAmount = skill.id < 17 ? Role.maxHp * OriginalCombatMath.SkillValue(skill.damage, 1, true) / 100 + OriginalCombatMath.SkillValue(skill.amend, 1, false)
                : OriginalCombatMath.SkillValue(skill.amend, 1, false);
        }
        private void CompleteSupportSkill(OriginalActor actor)
        {
            if (actor != Role || !SupportSkillCasting || actor.hp <= 0) return;
            int skill = pendingSupportSkill, amount = pendingSupportAmount;
            CancelSupportCast();
            if (skill <= 16)
            {
                Role.hp = Mathf.Min(Role.maxHp, Role.hp + amount);
                ShowCombatNumber(Role, amount, 2);
                supportRecoveryAction = 1; supportRecoveryTicks = 11;
            }
            else if (skill <= 19)
            {
                SetCurrentDurability(durability + amount);
                ShowCombatNumber(Role, amount, 3);
                supportRecoveryAction = 2; supportRecoveryTicks = 11;
            }
            else
            {
                // Original counter clears on the cycle after reaching duration (396 -> 397 cycles).
                superArmorTicks = Mathf.Max(1, amount - superArmorElapsed + 1);
            }
        }
        private void DrawSupportSkillVisuals()
        {
            if (!Role) return;
            UpdateSupportVisual(ref supportRecoveryVisual, supportRecoveryTicks > 0, supportRecoveryAction, 11 - supportRecoveryTicks);
            UpdateSupportVisual(ref supportArmorVisual, superArmorTicks > 0, 0, superArmorElapsed);
        }
        private void UpdateSupportVisual(ref OriginalActor visual, bool visible, int action, int frame)
        {
            if (visible && !visual) visual = CreateVisual("辅助技能效果", "s_41", new[] { "action/s_41", "action/h_04" }, Role);
            if (!visual) return;
            visual.hidden = !visible || Role.hidden || Role.presentationHidden || AtTitle;
            visual.mapPosition = Role.mapPosition; visual.mirror = Role.mirror;
            // Armor loops continuously; recovery samples its original one-shot animation.
            visual.Sample(action, Mathf.Max(0, frame));
            SortVisual(visual, 1200);
        }
    }
}
