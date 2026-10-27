using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        // Enemy.processBoss intervals are 80 ms ticks. All supported Bosses start with the skill ready.
        private static int BossCriticalInterval(int id)
        {
            switch (id)
            {
                case 232: case 241: return 3600;
                case 233: case 235: case 242: return 1800;
                case 236: case 244: return 1080;
                case 238: return 1440;
                case 239: case 245: return 720;
                default: return 0;
            }
        }

        private void TickEnemyCounters(Fighter f)
        {
            var a = f.actor;
            if (a.actionIndex <= 1)
                for (int c = 0; c < f.cooldowns.Length; c++) if (f.cooldowns[c] > 0) f.cooldowns[c]--;
            int interval = BossCriticalInterval(a.definitionId);
            if (a.Definition.type != 10 || interval == 0 || criticalStage != 0) return;
            if (!f.bossCriticalReady && ++f.bossCriticalCounter >= interval)
            {
                f.bossCriticalReady = true;
                f.bossCriticalCounter = 0;
            }
            if (f.canDefend && f.hold < 100 && !a.ScriptControlled && a != bossCritical)
            {
                f.canDefend = false;
                bool stronger = a.definitionId == 233 || a.definitionId == 236 || a.definitionId == 239 || a.definitionId == 242 || a.definitionId == 245;
                if (Random.Range(0, 100) < (stronger ? 40 : 20))
                {
                    f.defenseCounter = stronger ? 5 : 10;
                    a.PlayOnce(14);
                }
            }
        }

        private bool TryBossDecision(Fighter f, float dx, float dy)
        {
            var a = f.actor;
            if (a.Definition.type != 10) return false;
            if ((a.definitionId == 235 || a.definitionId == 236) && f.teleportCounter >= 6)
            {
                f.teleportCounter = 0;
                a.PlayOnce(25);
                return true;
            }
            if (!f.bossCriticalReady || Mathf.Abs(dx) > 64 || Mathf.Abs(dy) > 5) return false;
            int threshold;
            switch (a.definitionId)
            {
                case 235: case 238: case 239: case 242: threshold = 50; break;
                case 236: threshold = 70; break;
                case 241: threshold = 20; break;
                case 244: case 245: threshold = 101; break;
                default: return false; // 232/233 check after a knockdown, rather than from idle.
            }
            if ((long)a.hp * 100 >= (long)a.maxHp * threshold) return false;
            StartBossCheck(f);
            return true;
        }

        private void StartBossCheck(Fighter f)
        {
            f.attackMode = -1;
            f.actor.Face(Role.mapPosition.x + Role.Animation.footWidth / 2f > f.actor.mapPosition.x + f.actor.Animation.footWidth / 2f ? "r" : "l");
            f.actor.PlayOnce(21);
        }

        private void ApplyHitReaction(Fighter target, int attackType, bool guaranteed = false)
        {
            var a = target.actor;
            if (target.dead || a.hp <= 0 || a.ScriptControlled || criticalTargets.Contains(a.actorId)) return;
            if (!a.IsEnemy)
            {
                if (!guaranteed && (target.hold > 0 || superArmorTicks > 0)) return;
                if (attackType < 0 || attackType > 2) return;
                ResetAttackChain();
                ResetComboStatistics();
                a.PlayOnce(a.actionBank + (attackType == 0 ? 2 : attackType == 1 ? 5 : 6));
                return;
            }
            bool airborne = a.actionIndex == 4 || a.actionIndex == 5;
            int next = -1;
            if (target.hold <= 0)
            {
                switch (attackType)
                {
                    case 0: next = airborne ? 5 : 2; break;
                    case 1: next = airborne ? 8 : 6; break;
                    case 2: next = airborne ? 8 : a.actionIndex == 14 ? 16 : 7; break;
                    case 3: next = airborne ? 5 : 4; break;
                }
            }
            else if (a.actionIndex == 14)
            {
                next = attackType == 2 ? 16 : 15;
                if (attackType == 2) target.hold = 0;
            }
            if (next >= 0) a.PlayOnce(next);
        }

        private void RestoreEnemyHold(Fighter f)
        {
            f.hold = f.enemy.hold;
            f.canDefend = true;
        }

        private void OnFighterActionFinished(OriginalActor a)
        {
            int version = a.ActionVersion;
            CompleteSupportSkill(a);
            OnCriticalActionFinished(a);
            if (a.ActionVersion == version) OnCombatActionFinished(a);
        }

        private void OnCombatActionFinished(OriginalActor a)
        {
            if (!fighters.TryGetValue(a.actorId, out var f) || f.dead || a.ScriptControlled ||
                (a == Role && criticalStage != 0) || criticalTargets.Contains(a.actorId) || a == bossCritical) return;
            if (!a.IsEnemy)
            {
                if (ContinueAttackChain(a)) return;
                if (a.LogicalAction == 2 || a.LogicalAction == 5 || a.LogicalAction == 6)
                {
                    f.hold = integrated ? 99 : 49;
                    a.ResetAction();
                }
                return;
            }
            switch (a.actionIndex)
            {
                case 2:
                    RestoreEnemyHold(f);
                    if (a.Definition.type == 10)
                    {
                        f.defenseCounter = a.definitionId % 3 == 0 ? 60 : a.definitionId % 3 == 1 ? 24 : 12;
                        a.PlayOnce(14);
                    }
                    else a.ResetAction();
                    break;
                case 4: case 5: case 6: case 8: case 10:
                    RestoreEnemyHold(f);
                    if (a.Definition.type == 10)
                    {
                        int threshold = a.definitionId == 232 ? 30 : a.definitionId == 233 ? 50 : 0;
                        if (f.bossCriticalReady && threshold > 0 && (long)a.hp * 100 < (long)a.maxHp * threshold) StartBossCheck(f);
                        else a.PlayOnce(20);
                    }
                    else if (a.definitionId == 210 || a.definitionId == 211 || a.definitionId == 212 || a.definitionId == 219)
                    {
                        f.attackMode = 2;
                        a.PlayOnce(19);
                    }
                    else a.ResetAction();
                    break;
                case 7:
                    f.knockbackSpeed = a.mirror ? -30 : 30;
                    a.PlayOnce(9);
                    break;
                case 9: a.ContinueLastStep(); break;
                case 13: RestoreEnemyHold(f); a.ResetAction(); break;
                case 14:
                    if (f.defenseCounter-- > 0) a.ContinueLastStep();
                    else a.PlayOnce(20);
                    break;
                case 15:
                    f.hold = f.enemy.hold;
                    f.defenseCounter = a.definitionId % 3 == 2 ? 12 : 36;
                    a.PlayOnce(14);
                    break;
                case 21:
                    if (!CriticalActive && Role && Role.hp > 0 && !Role.hidden && Role.CanReceiveHit && CriticalOverlap(a, Role)) BeginBossCritical(a.actorId);
                    else a.ResetAction();
                    break;
                case 25:
                    if (a.definitionId == 235 || a.definitionId == 236) { PlaceBossBesideRole(a); a.PlayOnce(26); }
                    else a.ResetAction();
                    break;
                default:
                    if (a.actionIndex > 1) a.ResetAction();
                    break;
            }
        }

        private void TickKnockback(Fighter f)
        {
            var a = f.actor;
            if (Map == null) { a.PlayOnce(10); return; }
            int remaining = Mathf.Abs(f.knockbackSpeed);
            int sign = f.knockbackSpeed < 0 ? -1 : 1;
            while (remaining > 0)
            {
                int step = Mathf.Min(11, remaining);
                float x = a.mapPosition.x + step * sign;
                if (!CanPlaceEnemy(a, x) || x < cameraPoint.x - ViewWidth / 2 || x + a.Animation.footWidth >= cameraPoint.x + ViewWidth / 2)
                {
                    a.PlayOnce(10);
                    return;
                }
                a.mapPosition = new Vector2(x, a.mapPosition.y);
                remaining -= step;
            }
            f.knockbackSpeed += sign * 10;
        }

        private bool CanPlaceEnemy(OriginalActor a, float x)
        {
            return Map != null && x > 0 && x + a.Animation.footWidth < Map.Width &&
                Map.CanPass(x + a.Animation.footWidth / 2f, a.mapPosition.y + a.Animation.footHeight);
        }

        private void PlaceBossBesideRole(OriginalActor boss)
        {
            float center = Role.mapPosition.x + (Role.Animation.footWidth - boss.Animation.footWidth) / 2f;
            float preferred = center + (Role.mirror ? -64 : 64);
            float alternate = center + (Role.mirror ? 64 : -64);
            boss.mapPosition = new Vector2(CanPlaceEnemy(boss, preferred) ? preferred : CanPlaceEnemy(boss, alternate) ? alternate : Role.mapPosition.x, boss.mapPosition.y);
            boss.Face(Role.mapPosition.x + Role.Animation.footWidth / 2f > boss.mapPosition.x + boss.Animation.footWidth / 2f ? "r" : "l");
        }
    }
}
