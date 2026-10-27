using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        // Legacy private names remain views for editor diagnostics; SkillRuntime owns the only state.
        private int skillCooldownTicks { get => Skills.SkillCooldownTicks; set => Skills.SetSkillCooldown(value); }
        private int superArmorTicks { get => Skills.SuperArmorTicks; set => Skills.SetSuperArmor(value); }
        private int superArmorElapsed { get => Skills.SuperArmorElapsed; set => Skills.RestoreArmor(Skills.SuperArmorTicks, value); }
        private int supportRecoveryTicks => Skills.RecoveryTicks;
        private int supportRecoveryAction => Skills.RecoveryAction;
        private OriginalActor supportRecoveryVisual, supportArmorVisual;
        public int SkillCooldownTicks => skillCooldownTicks;
        public int SuperArmorTicks => superArmorTicks;
        public bool SupportSkillCasting => Role && Skills.IsSupportCasting(Role.ActionVersion, Role.LogicalAction);

        private void ResetSkillCooldown(bool ready = false)
        {
            var data = OriginalAssets.Table<SkillDefinition>("skill").FirstOrDefault(s => s.id == equippedSkill);
            skillCooldownTicks = ready || data == null ? 0 : disableCooldown ? 1 : data.cooldown;
        }
        private void CancelSupportCast() { Skills.CancelSupport(); }
        private void ResetSupportSkills()
        {
            Skills.ResetSupport();
            supportRecoveryVisual = supportArmorVisual = null;
        }
        private void TickRoleSkillTimers()
        {
            if (!Role || Role.frozen || menu || reinforceOpen || DialogueVisible || criticalStage != 0) return;
            Skills.Tick(integrated || durability > 0, true, true);
            if (!SupportSkillCasting) CancelSupportCast();
        }
        private void PrepareSupportSkill(SkillDefinition skill)
        {
            Skills.PrepareSupport(skill, Role.ActionVersion, Role.maxHp);
        }
        private void CompleteSupportSkill(OriginalActor actor)
        {
            if (actor != Role || !SupportSkillCasting || actor.hp <= 0) return;
            if (!Skills.TryCompleteSupport(actor.ActionVersion, actor.LogicalAction, out int skill, out int amount)) return;
            if (skill <= 16)
            {
                Role.Health.Heal(amount);
                ShowCombatNumber(Role, amount, 2);
                Skills.SetRecovery(11, 1);
            }
            else if (skill <= 19)
            {
                SetCurrentDurability(durability + amount);
                ShowCombatNumber(Role, amount, 3);
                Skills.SetRecovery(11, 2);
            }
            else
            {
                // Original counter clears on the cycle after reaching duration (396 -> 397 cycles).
                Skills.ActivateArmor(amount);
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
