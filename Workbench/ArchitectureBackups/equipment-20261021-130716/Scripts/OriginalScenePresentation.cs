using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private readonly List<OriginalActor>[] sceneLayers = {
            new List<OriginalActor>(), new List<OriginalActor>(), new List<OriginalActor>(), new List<OriginalActor>() };
        private readonly List<OriginalActor> sceneActors = new List<OriginalActor>();
        private static readonly int[] SceneLayerOrder = { -28000, -20000, 0, 28010 };
        private void RefreshEnemyBackEffects()
        {
            // drawSprite(1) draws type11 from spriteOnLayer1 before all spriteOnLayer0 bodies.
            // Preserve that vector's previous draw order before the current layer2 sort.
            foreach (var actor in sceneActors)
                if (actor.EnemyBackEffect) actor.EnemyBackEffect.hidden = true;
            int order = -24000;
            foreach (var actor in sceneLayers[2])
            {
                if (actor.Definition.type != 11) continue;
                var effect = actor.EnemyBackEffect;
                if (!effect) throw new System.InvalidOperationException("Missing saved type11 back effect: " + actor.name);
                effect.hidden = AtTitle || miniMapOpen || worldMapOpen || storyScreen && DialogueVisible || actor.hidden || actor.presentationHidden || criticalStage == 24 || criticalStage == 25;
                effect.mapPosition = actor.mapPosition + new Vector2((actor.Animation.footWidth - effect.Animation.footWidth) >> 1, (actor.Animation.footHeight - effect.Animation.footHeight) >> 1);
                effect.mirror = false;
                // Original s_36 action0 has a single step, so drawFrameCycle always selects frame0.
                effect.Sample(0, 0);
                effect.SetPresentationOrder(order++, true);
            }
            foreach (var actor in sceneActors)
                if (actor.EnemyBackEffect && actor.layer != 2) actor.EnemyBackEffect.RefreshFrame();
        }

        private void LateUpdate() { RefreshScenePresentation(); }

        // Rendering must stay current during menu/break/frozen frames without ticking gameplay.
        private void RefreshScenePresentation()
        {
            sceneActors.Clear();
            foreach (var actor in actors.Values) if (actor) sceneActors.Add(actor);
            foreach (var projectile in projectiles) if (projectile.actor && !projectile.dead) sceneActors.Add(projectile.actor);
            foreach (var drop in groundDrops) if (drop && drop.WeaponActor && !drop.Expired) sceneActors.Add(drop.WeaponActor);
            foreach (var list in sceneLayers) list.RemoveAll(a => !a || !sceneActors.Contains(a) || Mathf.Clamp(a.layer, 0, 3) != System.Array.IndexOf(sceneLayers, list));
            foreach (var actor in sceneActors)
            {
                var list = sceneLayers[Mathf.Clamp(actor.layer, 0, 3)];
                if (!list.Contains(actor)) list.Add(actor);
                actor.RefreshFrame();
            }
            RefreshEnemyBackEffects();
            for (int layer = 0; layer < sceneLayers.Length; layer++)
            {
                var list = sceneLayers[layer];
                if (layer > 0) SortSourceLayer(list, layer);
                int special = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    var actor = list[i];
                    int type = actor.Definition.type;
                    int order = layer == 2 && (type == 4 || type == 5) ? -4500 + special++ : SceneLayerOrder[layer] + i * 10;
                    actor.SetPresentationOrder(actor.screenCentered ? 30000 : order);
                }
            }
            int icon = 0;
            foreach (var drop in groundDrops)
                if (drop && !drop.WeaponActor && !drop.Expired) drop.GetComponent<SortingGroup>().sortingOrder = -4000 + icon++;
            ApplyCriticalPresentationOrder();
            RefreshWorldCombatFeedback();
        }

        private static bool VisibleForSourceSort(OriginalActor actor) => actor && !actor.hidden;
        private static int SourceFootY(OriginalActor actor, int layer) => (int)actor.mapPosition.y + actor.Animation.footHeight +
            (layer == 2 && actor.Definition.type == 0 ? 6 : 0);
        private static void SortSourceLayer(List<OriginalActor> list, int layer)
        {
            // GameCanvas.drawSprite uses selection swaps and retains the resulting vector order.
            for (int i = 0; i < list.Count; i++)
            {
                var current = list[i]; int type = current.Definition.type;
                if (!VisibleForSourceSort(current) || layer == 2 && (type == 4 || type == 5)) continue;
                int min = i, y = SourceFootY(current, layer);
                for (int j = i + 1; j < list.Count; j++)
                {
                    var next = list[j]; int nextType = next.Definition.type;
                    if (!VisibleForSourceSort(next) || layer == 2 && (nextType == 3 || nextType == 12 || nextType == 4 || nextType == 5)) continue;
                    int nextY = SourceFootY(next, layer);
                    if (nextY < y) { y = nextY; min = j; }
                }
                if (min != i) { var previous = list[i]; list[i] = list[min]; list[min] = previous; }
            }
        }

        private void ApplyCriticalPresentationOrder()
        {
            if (!Role) return;
            if (criticalStage == 24 || criticalStage == 25)
            {
                Role.SetPresentationOrder(0); int last = 0;
                foreach (int id in criticalTargets) { var target = Actor(id); if (target) target.SetPresentationOrder(last += 10); }
                for (int i = 0; i < criticalEffects.Count; i++)
                {
                    var effect = criticalEffects[i]; if (!effect || effect.hidden) continue;
                    int order = criticalStage == 24 ? (i == 0 ? Role.FrameIndex < 6 ? -10 : 1 : last + 1) :
                        i == 0 ? -10 : i == 1 ? 1 : i == 3 ? 2 : last + 1;
                    effect.SetPresentationOrder(order, true);
                }
            }
            else if (bossCritical && (bossCritical.actionIndex == 22 || criticalProjectile))
            {
                Role.SetPresentationOrder(0); bossCritical.SetPresentationOrder(10);
                if (criticalProjectile) criticalProjectile.SetPresentationOrder(20, true);
                foreach (var effect in criticalEffects) if (effect && !effect.hidden) effect.SetPresentationOrder(-10, true);
                if (bossTitle && !bossTitle.hidden) bossTitle.SetPresentationOrder(criticalProjectile ? 21 : -10, true);
            }
        }

        private void RefreshWorldCombatFeedback()
        {
            foreach (var fighter in fighters.Values)
            {
                var actor = fighter.actor; if (!actor || !actor.CombatFeedback) continue;
                var feedback = actor.CombatFeedback; feedback.Clear();
                if (!originalUi || AtTitle || miniMapOpen || worldMapOpen || storyScreen && DialogueVisible || actor.hidden || actor.presentationHidden) continue;
                feedback.EnemyHealth(actor, originalUi);
                feedback.Miss(actor, originalUi, fighter.missTicks);
                int slot = 0;
                foreach (var number in numbers)
                    if (number.actor == actor) feedback.Number(actor, originalUi, slot++, number.value, number.type, number.ticks);
            }
        }
    }
}
