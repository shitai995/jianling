using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private bool swordBreakVisible, characterDialogue;
        private int swordBreakCounter;
        private float swordBreakElapsed;
        public bool SwordBreakVisible => swordBreakVisible;
        public int SwordBreakCounter => swordBreakCounter;

        private void ResetSwordBreak()
        { swordBreakVisible = false; swordBreakCounter = 0; swordBreakElapsed = 0; }

        // GameCanvas.destroySword deliberately does not call Role.updateAttribute.
        private void DestroyOrdinarySword()
        {
            if (Equipped == null) return;
            Weapons.BreakOrdinary();
            swordBreakVisible = true; swordBreakCounter = 0; swordBreakElapsed = 0;
            ResetSkillCooldown(); breakCooldownTicks = 3;
            var spirit = Actor(-2);
            if (spirit && spirit.actionIndex == 3) { CancelAutoMedicine(); spirit.ResetAction(); }
        }

        private void ScriptDestroySword()
        {
            if (integrated) SetSpiritDurability(0);
            else if (durability > 0) DestroyOrdinarySword();
        }

        private bool SwordBreakPausesScene => !menu && !reinforceOpen && !miniMapOpen &&
            (!DialogueVisible || characterDialogue) && !cameraWait && movements.Count == 0 && Time.time >= waitUntil;

        // Consume all five source cycles, including the cycle that clears the flag.
        // Returning only the remainder prevents a long Unity frame from skipping the pause.
        private float TickSwordBreak(float delta)
        {
            if (!swordBreakVisible || delta <= 0) return delta;
            bool pause = SwordBreakPausesScene;
            float remaining = delta;
            while (swordBreakVisible && remaining > 0)
            {
                float consumed = Mathf.Min(remaining, .08f - swordBreakElapsed);
                remaining -= consumed; swordBreakElapsed += consumed;
                if (swordBreakElapsed < .08f) break;
                swordBreakElapsed = 0;
                if (swordBreakCounter < 4) swordBreakCounter++;
                else swordBreakVisible = false;
            }
            return pause ? remaining : delta;
        }

        private void ApplyEnemySwordDamage(OriginalActor source, OriginalActor target)
        {
            if (target != Role || source.actionIndex != 17 || source.definitionId < 243 || source.definitionId > 245) return;
            // Amakusa uses base spirit MP, excluding swallowed reinforcement bonuses.
            int maximum = integrated ? SpiritAttribute(1) - spiritBonuses[1] : Equipped.MaximumDurability;
            int loss = maximum * 30 / 100;
            if (integrated) SetSpiritDurability(Mathf.Max(0, durability - loss));
            else if (durability > loss) Weapons.Spend(loss);
            else DestroyOrdinarySword();
        }

        private int CostOfRepair() => Weapons.QuoteRepair();
        private void RepairOrdinarySwords()
        {
            if (!Weapons.TryRepair(ref money)) { Talk("", "银两不足"); return; }
            Talk("", "修理成功");
        }
    }
}
