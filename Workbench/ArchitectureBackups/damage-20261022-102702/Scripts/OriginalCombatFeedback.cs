using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private int healthDecrement;
        private float healthDecrementElapsed;
        private int criticalHitDuration, criticalHitCounter;
        private float criticalHitElapsed;

        private void SetCriticalHitEffect(int duration)
        {
            // GameCanvas.setCriticalEffect does not restart the active counter.
            criticalHitDuration = duration;
            SetShake(4, UnityEngine.Random.Range(0, 1));
            PlayOriginalSound(1);
        }

        private void TickCriticalHitEffect(float delta)
        {
            criticalHitElapsed += delta;
            while (criticalHitElapsed >= .08f)
            {
                criticalHitElapsed -= .08f;
                if (criticalHitDuration <= 0) continue;
                if (criticalHitCounter < criticalHitDuration) criticalHitCounter++;
                else criticalHitDuration = criticalHitCounter = 0;
            }
        }

        private void DrawCriticalHitEffect()
        {
            if (criticalHitDuration <= 0 || !originalUi) return;
            var effect = originalUi.criticalHitOverlay;
            if (effect == null || effect.animation == null) return;
            OriginalGui.Animation(effect.animation, effect.sheets, 0, criticalHitCounter - 1,
                new Vector2((int)ViewWidth >> 1, (int)ViewHeight >> 1));
        }

        private void ShowCombatNumber(OriginalActor actor, int value, int type)
        {
            // FightSprite shifts five slots for this actor, newest first, even for nonpositive values.
            int existing = 0;
            for (int i = 0; i < numbers.Count; i++)
                if (numbers[i].actor == actor && ++existing >= 5) { numbers.RemoveAt(i); break; }
            numbers.Insert(0, new FloatingNumber { actor = actor, value = value, type = type });
        }

        private void ShowCombatMiss(OriginalActor actor)
        {
            if (fighters.TryGetValue(actor.actorId, out var fighter)) fighter.missTicks = 10;
        }

        private void TickActorCombatFeedback(OriginalActor actor)
        {
            for (int i = numbers.Count - 1; i >= 0; i--)
                if (numbers[i].actor == actor && --numbers[i].ticks <= 0) numbers.RemoveAt(i);
            if (fighters.TryGetValue(actor.actorId, out var fighter) && fighter.missTicks > 0) fighter.missTicks--;
        }

        private void ResetCombatFeedback()
        {
            numbers.Clear();
            healthDecrement = 0;
            healthDecrementElapsed = 0;
            criticalHitDuration = criticalHitCounter = 0;
            criticalHitElapsed = 0;
        }

        private void TickHealthDecrement(float delta)
        {
            // GameCanvas.cycle updates this globally, before scene/actor cycles, even in menus.
            healthDecrementElapsed += delta;
            while (healthDecrementElapsed >= .08f)
            {
                healthDecrementElapsed -= .08f;
                if (healthDecrement > 0 && Role && originalUi && originalUi.health)
                    healthDecrement = Mathf.Max(0, healthDecrement - (Role.maxHp / originalUi.health.width << 1));
            }
        }

        private void DrawHealthDecrement()
        {
            if (healthDecrement <= 0 || !Role || !originalUi || !originalUi.health) return;
            var ui = originalUi; int maxHp = Mathf.Max(1, Role.maxHp);
            int width = (int)((long)ui.health.width * Role.hp / maxHp);
            int end = (int)((long)ui.health.width * ((long)Role.hp + healthDecrement) / maxHp);
            var old = GUI.color; GUI.color = Color.white;
            GUI.DrawTexture(new Rect(ui.healthBounds.x + width * ui.healthBounds.width / ui.health.width,
                ui.healthBounds.y, (end - width) * ui.healthBounds.width / ui.health.width, ui.healthBounds.height), Texture2D.whiteTexture);
            GUI.color = old;
        }
    }
}
