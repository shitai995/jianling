using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private int criticalStage;
        private bool criticalWait, criticalEnd, criticalScripted, criticalDurabilityConsumed;
        private readonly HashSet<int> criticalTargets = new HashSet<int>();
        private OriginalActor bossCritical, criticalProjectile, bossTitle;
        private int criticalCaptureHits, criticalCaptureDamage;
        private bool criticalCameraSaved, criticalPreviousFixedCamera;
        private Vector2 criticalPreviousCamera;
        private readonly List<OriginalActor> criticalEffects = new List<OriginalActor>();
        public bool CriticalActive => criticalStage != 0 || bossCritical;
        public int CriticalStage => criticalStage;
        public bool CriticalWaiting => criticalWait;
        public bool BossCriticalActive => bossCritical;
        private static readonly string[] CriticalImages = { "s_17", "s_18", "s_19", "s_20", "s_23", "s_24", "s_26", "o_00", "s_25", "s_27", "s_28", "s_242" };

        public bool TryCriticalKill()
        {
            if (!Role || !Role.WeaponVisible || Role.locked || Role.hp <= 0 || CriticalActive || Role.LogicalAction > 1) return false;
            if (energy != 2000) { Toast("怒气未满"); return false; }
            if (durability <= 0 && !infiniteDurability) { Toast("武器耐久不足"); return false; }
            if (!maximumPower) energy = 0;
            BeginCriticalKill(true, false);
            return true;
        }
        private void BeginCriticalKill(bool finish, bool scripted = true)
        {
            if (!Role) throw new InvalidOperationException("绝杀缺少主角");
            ResetCriticalSequences();
            criticalScripted = scripted; criticalEnd = finish;
            // A complete sequence owns the VM until hit or miss finishes. A preview runs beside its script.
            criticalWait = finish; criticalDurabilityConsumed = false;
            SetCriticalStage(22);
        }
        private void SetCriticalStage(int stage)
        {
            criticalStage = stage;
            Role.PlayOnce(stage + (integrated ? 27 : 0));
        }
        private bool ShouldTickDuringCritical(OriginalActor actor)
        {
            if (criticalStage != 24 && criticalStage != 25) return true;
            return actor == Role || (criticalTargets.Contains(actor.actorId) && (criticalStage == 25 || Role.FrameIndex >= 34));
        }
        private static bool CriticalOverlap(OriginalActor source, OriginalActor target)
        {
            foreach (var a in source.CurrentFrame.attackRects)
                foreach (var b in target.CurrentFrame.bodyRects)
                    if (OriginalCombatMath.Rect(source, a).Overlaps(OriginalCombatMath.Rect(target, b))) return true;
            return false;
        }
        private void OnCriticalActionFinished(OriginalActor actor)
        {
            if (actor == Role && criticalStage != 0)
            {
                if (criticalStage == 22)
                {
                    foreach (var f in fighters.Values)
                        if (f.actor && f.actor.IsEnemy && !f.dead && !f.actor.hidden && f.actor.CurrentStep.vulnerable && CriticalOverlap(Role, f.actor)) criticalTargets.Add(f.actor.actorId);
                    SetCriticalStage(criticalTargets.Count > 0 ? 24 : 23);
                }
                else if (criticalStage == 24)
                {
                    foreach (int id in criticalTargets)
                    {
                        var target = Actor(id); if (!target) continue;
                        target.mapPosition = new Vector2(ClampCriticalX(Role.mapPosition.x + (Role.mirror ? 56 : -56) - target.Animation.footWidth / 2f, target), target.mapPosition.y);
                    }
                    SetCriticalStage(25);
                }
                else EndRoleCritical();
                return;
            }
            if (criticalTargets.Contains(actor.actorId) && actor.hp > 0 && criticalStage != 0)
            {
                if (actor.actionIndex == 11) actor.PlayOnce(12);
                else if (actor.actionIndex == 12)
                {
                    actor.mapPosition = new Vector2(ClampCriticalX(Role.mirror ? Role.mapPosition.x + 176 : Role.mapPosition.x + 64 - actor.Animation.footWidth, actor), actor.mapPosition.y);
                    actor.PlayOnce(13);
                }
                else if (actor.actionIndex == 13) actor.ResetAction();
            }
            if (actor != bossCritical) return;
            if (actor.actionIndex == 22)
            {
                Role.locked = false;
                if (actor.definitionId == 232 || actor.definitionId == 233) SetCriticalHitEffect(5);
                if (actor.definitionId == 235 || actor.definitionId == 236)
                {
                    PlaceBossBesideRole(actor);
                }
                if (actor.definitionId == 241 || actor.definitionId == 242)
                {
                    // This Boss emits action 24 as a stationary projectile at the target's feet.
                    criticalProjectile = CreateVisual("Boss绝杀投射物", actor.Definition.animation, actor.Definition.images, actor);
                    criticalProjectile.mapPosition = Role.mapPosition + new Vector2((Role.Animation.footWidth - actor.Animation.footWidth) / 2f, Role.Animation.footHeight - actor.Animation.footHeight);
                    // Bullet.draw always uses unmirrored frames; the effect is centered on its target.
                    criticalProjectile.mirror = false;
                    criticalProjectile.PlayOnce(24);
                    criticalProjectile.Stepped += OnCriticalProjectileStep;
                    criticalProjectile.ActionFinished += _ => EndBossCritical();
                    actor.ResetAction();
                }
                else actor.PlayOnce(24);
            }
            else if (actor.actionIndex == 24) EndBossCritical();
        }
        private float ClampCriticalX(float x, OriginalActor actor)
        {
            float right = cameraPoint.x + ViewWidth / 2 - actor.Animation.footWidth - 1;
            return Mathf.Clamp(x, 1, Mathf.Max(1, right));
        }
        private void EndRoleCritical()
        {
            foreach (int id in criticalTargets)
            {
                var a = Actor(id); if (a && a.hp > 0 && a.actionIndex >= 11 && a.actionIndex <= 13) a.ResetAction();
            }
            criticalStage = 0; criticalWait = false; criticalTargets.Clear();
            if (Role) Role.ResetAction();
        }
        private bool ProcessCriticalStep(OriginalActor actor)
        {
            if (actor == Role && criticalStage != 0)
            {
                if (criticalStage == 24 && criticalScripted && !criticalEnd && actor.FrameIndex == 33)
                {
                    actor.HoldFrame(); criticalStage = 0; criticalWait = false; criticalTargets.Clear();
                    return true;
                }
                if (criticalStage == 25 && actor.FrameIndex == 98 && !criticalDurabilityConsumed)
                {
                    criticalDurabilityConsumed = true;
                    if (!infiniteDurability)
                    {
                        if (integrated) { SetSpiritDurability(0); breakCooldownTicks = 3; }
                        else DestroyOrdinarySword();
                    }
                }
                if ((criticalStage == 24 || criticalStage == 25) && actor.CurrentStep.attack)
                {
                    var source = fighters[-1];
                    var skill = OriginalAssets.Table<SkillDefinition>("skill").First(s => s.id == (criticalStage == 24 ? 25 : integrated ? 23 : 22));
                    foreach (int id in criticalTargets.ToArray())
                    {
                        if (!fighters.TryGetValue(id, out var target) || !target.actor || target.dead) continue;
                        if (criticalStage == 24 && actor.FrameIndex == 34)
                        {
                            target.actor.Face(actor.mapPosition.x > target.actor.mapPosition.x ? "r" : "l");
                            target.actor.PlayOnce(11);
                        }
                        RecordComboHit();
                        int damage = OriginalCombatMath.RoleDamage(source.attack, target.defense, level, target.actor.level,
                            target.actor.actionIndex == 14 ? 1 : target.injury, skill, 1);
                        damage = Mathf.Max(10, damage + RandomVariance(damage));
                        bool crit = UnityEngine.Random.Range(0, 1000) < source.critical;
                        Damage(target, crit ? damage * 2 : damage, crit, crit ? 1 : 2);
                    }
                }
                return true;
            }
            if (criticalTargets.Contains(actor.actorId) && criticalStage != 0) return true;
            if (actor == bossCritical)
            {
                if (actor.actionIndex == 24) ResolveBossCriticalHit(actor);
                return true;
            }
            return criticalStage == 24 || criticalStage == 25;
        }
        private void BeginBossCritical(int id)
        {
            var boss = Actor(id);
            if (!boss || boss.Definition.type != 10 || boss.Animation.actions.Length <= 24) throw new InvalidOperationException("Boss绝杀目标无效: " + id);
            ResetCriticalSequences(); bossCritical = boss;
            Role.locked = true;
            boss.PlayOnce(22);
        }
        private void ResolveBossCriticalHit(OriginalActor source)
        {
            if (!Role || Role.hp <= 0 || !source.CurrentStep.attack || !Role.CanReceiveHit || !CriticalOverlap(source, Role)) return;
            if ((source.definitionId == 235 || source.definitionId == 236) && source.FrameIndex == 26)
                SetCriticalHitEffect(5);
            var f = fighters[bossCritical.actorId]; var role = fighters[-1];
            int damage = Mathf.Max(1, (f.attack - role.defense) * Mathf.Clamp(100 + (bossCritical.level - level) * 5, 50, 150) / 100);
            if (bossCritical.definitionId == 232 || bossCritical.definitionId == 233) damage = Role.maxHp / 2;
            else
            {
                if (f.attackMode >= 0)
                {
                    var coefficients = f.enemy.damageCoefficients.Split(',').Select(int.Parse).ToArray();
                    damage = damage * coefficients[f.attackMode] / 100;
                }
                else if (bossCritical.definitionId == 244 || bossCritical.definitionId == 245) damage = damage * 60 / 100;
                damage = Mathf.Max(1, damage + RandomVariance(damage));
                if (bossCritical.definitionId == 235 || bossCritical.definitionId == 236)
                {
                    damage = Mathf.Max(1, damage * 20 / 100);
                    bossCritical.hp = Mathf.Min(bossCritical.maxHp, bossCritical.hp + Mathf.Max(1, (int)((long)bossCritical.maxHp * damage / Role.maxHp)));
                    ShowCombatNumber(bossCritical, Mathf.Max(1, (int)((long)bossCritical.maxHp * damage / Role.maxHp)), 2);
                    healthDecrement = damage;
                }
                if (bossCritical.definitionId == 238 || bossCritical.definitionId == 239)
                {
                    if (integrated) { ToggleIntegrate(); spiritDurability = 0; }
                    else DestroyOrdinarySword();
                }
            }
            role.hold = Mathf.Max(0, role.hold - (f.attackMode >= 0 ? int.Parse(f.enemy.holdDamage.Split(',')[f.attackMode]) : 100));
            Role.Face(bossCritical.mapPosition.x > Role.mapPosition.x ? "r" : "l");
            bool critical = UnityEngine.Random.Range(0, 1000) < f.critical;
            bool halfHp = bossCritical.definitionId == 232 || bossCritical.definitionId == 233;
            if (halfHp) healthDecrement = damage;
            Damage(role, halfHp ? damage : critical ? damage * 2 : damage,
                halfHp || critical, -1);
            if (Role.hp > 0 && (bossCritical.definitionId == 232 || bossCritical.definitionId == 233)) { ResetAttackChain(); ResetComboStatistics(); Role.PlayOnce(5 + Role.actionBank); }
            else ApplyHitReaction(role, source.CurrentStep.attackType, true);
        }
        private void OnCriticalProjectileStep(OriginalActor projectile)
        {
            if (!bossCritical || !Role || Role.hp <= 0) return;
            // Bullet.isContinueMoveAfterHitTarget: 24 catches; 25 inflicts ten equal hits, then all remaining HP.
            if (projectile.actionIndex == 24)
            {
                if (!OriginalCombatMath.Hit(projectile, Role)) return;
                criticalCaptureDamage = Role.hp / 11;
                ResetAttackChain(); ResetComboStatistics();
                criticalCaptureHits = 0;
                criticalPreviousFixedCamera = fixedCamera; criticalPreviousCamera = cameraPoint; criticalCameraSaved = true;
                fixedCamera = true;
                cameraPoint.x = projectile.mapPosition.x + projectile.Animation.footWidth / 2f;
                Role.mapPosition = projectile.mapPosition + new Vector2((projectile.Animation.footWidth - Role.Animation.footWidth) / 2f,
                    (projectile.Animation.footHeight - Role.Animation.footHeight) / 2f);
                Role.PlayOnce(6 + Role.actionBank);
                projectile.PlayOnce(25);
            }
            else if (projectile.actionIndex == 25 && projectile.CurrentStep.attack && Role.CanReceiveHit)
            {
                int damage = criticalCaptureHits < 10 ? criticalCaptureDamage : Role.hp;
                criticalCaptureHits++;
                healthDecrement = damage;
                Damage(fighters[-1], damage, criticalCaptureHits > 10 && damage > criticalCaptureDamage, -1);
                if (Role.hp > 0) Role.PlayOnce(6 + Role.actionBank);
                if (criticalCaptureHits >= 11) RestoreCriticalCamera();
            }
        }
        private void TickCriticalProjectile(float delta)
        {
            if (criticalProjectile && !menu && !DialogueVisible) criticalProjectile.Tick(delta);
        }
        private void EndBossCritical()
        {
            var boss = bossCritical; bossCritical = null;
            if (boss)
            {
                boss.ResetAction(); boss.presentationHidden = false;
                if (fighters.TryGetValue(boss.actorId, out var f)) { f.bossCriticalReady = false; f.bossCriticalCounter = 0; }
            }
            if (boss && Role) Role.locked = false;
            if (criticalProjectile) Destroy(criticalProjectile.gameObject);
            criticalProjectile = null;
            RestoreCriticalCamera();
        }
        private void RestoreCriticalCamera()
        {
            if (!criticalCameraSaved) return;
            fixedCamera = criticalPreviousFixedCamera; cameraPoint = criticalPreviousCamera;
            criticalCameraSaved = false;
        }
        private void ResetCriticalSequences()
        {
            criticalStage = 0; criticalWait = false; criticalTargets.Clear();
            EndBossCritical();
            if (Role) { Role.suppressMirror = false; Role.renderOffset = Vector2.zero; }
            foreach (var a in actors.Values) if (a) a.presentationHidden = false;
            if (mapRoot) mapRoot.SetActive(true);
            foreach (var effect in criticalEffects) if (effect) Destroy(effect.gameObject);
            criticalEffects.Clear();
            if (bossTitle) Destroy(bossTitle.gameObject); bossTitle = null;
        }

        private void DrawCriticalEffect(int slot, int action, int frame, Vector2 position, int order)
        {
            while (criticalEffects.Count <= slot)
                criticalEffects.Add(CreateVisual("葬剑特效", "z_finalslash", CriticalImages.Select(s => "action/" + s).ToArray(), Role));
            var effect = criticalEffects[slot];
            effect.hidden = false; effect.mapPosition = position; effect.mirror = false;
            effect.Sample(action, frame); SortVisual(effect, order);
        }
        private void TickCriticalVisuals()
        {
            if (!Role) return;
            bool cinematic = criticalStage == 24 || criticalStage == 25;
            bool bossIntro = bossCritical && bossCritical.actionIndex == 22;
            bool projectile = criticalProjectile;
            if (mapRoot) mapRoot.SetActive(!cinematic && !bossIntro && !projectile);
            foreach (var a in actors.Values)
            {
                a.presentationHidden = (cinematic && a != Role && !criticalTargets.Contains(a.actorId)) ||
                    ((bossIntro || projectile) && a != Role && a != bossCritical) ||
                    (projectile && criticalProjectile.actionIndex == 25 && criticalCaptureHits < 11 && a == bossCritical);
                a.RefreshFrame();
            }
            foreach (var effect in criticalEffects) if (effect) { effect.hidden = true; effect.RefreshFrame(); }
            if (bossTitle) { bossTitle.hidden = true; bossTitle.RefreshFrame(); }
            Role.suppressMirror = criticalStage == 25;
            Role.renderOffset = criticalStage == 25 && !Role.mirror ? new Vector2(-112, 0) : Vector2.zero;
            Role.RefreshFrame();
            if (cinematic)
            {
                int frame = Role.FrameIndex;
                var pos = Role.mapPosition + new Vector2(Role.mirror ? 56 : -56, 0);
                int order = Mathf.RoundToInt(Role.mapPosition.y) * 20;
                if (criticalStage == 24)
                {
                    if (frame < 6) DrawCriticalEffect(0, 0, frame, pos + new Vector2(0, -ViewHeight / 2), order - 100);
                    else
                    {
                        DrawCriticalEffect(0, integrated ? 2 : 1, frame - 6, pos + new Vector2(0, -ViewHeight / 2), order + 100);
                        DrawCriticalEffect(1, Role.mirror ? 3 : 4, frame - 6, pos, order + 200);
                    }
                }
                else
                {
                    DrawCriticalEffect(0, 5, frame, pos, order - 100);
                    DrawCriticalEffect(1, 6, frame, pos, order + 100);
                    DrawCriticalEffect(2, integrated ? 8 : 7, frame, pos, order + 200);
                    if (frame > 50 && frame < 55) DrawCriticalEffect(3, 10, frame - 51, pos + new Vector2(120, 0), order + 300);
                }
            }
            else if (bossIntro)
            {
                int frame = bossCritical.FrameIndex;
                var center = cameraPoint - new Vector2(bossCritical.Animation.footWidth / 2f, bossCritical.Animation.footHeight / 2f);
                if (frame < 6) DrawCriticalEffect(0, 0, frame, cameraPoint, 29000);
                else
                {
                    if (!bossTitle || bossTitle.Animation != bossCritical.Animation)
                    {
                        if (bossTitle) Destroy(bossTitle.gameObject);
                        bossTitle = CreateVisual("Boss绝杀名称", bossCritical.Definition.animation, bossCritical.Definition.images, bossCritical);
                    }
                    bossTitle.hidden = false; bossTitle.mapPosition = center; bossTitle.Sample(23, frame); SortVisual(bossTitle, 29000);
                }
            }
            else if (projectile && bossTitle && bossTitle.Animation.actions.Length > 26)
            {
                bossTitle.hidden = false; bossTitle.mapPosition = criticalProjectile.mapPosition;
                bossTitle.Sample(26, criticalProjectile.FrameIndex);
                SortVisual(bossTitle, Mathf.RoundToInt(criticalProjectile.mapPosition.y) * 20 + 100);
            }
        }
    }
}
