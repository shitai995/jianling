using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private sealed class Projectile
        {
            public OriginalActor actor;
            public Fighter owner;
            public Vector2 velocity;
            public int damage, holdDamage;
            public bool persistent, canDodge, dead;
        }

        private readonly List<Projectile> projectiles = new List<Projectile>();
        public int ActiveProjectileCount => projectiles.Count;

        // Bullet's velocity is independent of the animation's per-frame movement.
        private static Vector2 ProjectileVelocity(int direction, int speed)
        {
            switch (direction)
            {
                case 0: return new Vector2(-speed, 0);
                case 1: return new Vector2(speed, 0);
                case 2: return new Vector2(0, -speed);
                case 3: return new Vector2(0, speed);
                case 11: return new Vector2((-speed) >> 1, -speed);
                case 12: return new Vector2(speed >> 1, -speed);
                case 21: return new Vector2((-speed) >> 1, speed);
                case 22: return new Vector2(speed >> 1, speed);
                case 31: return new Vector2(-speed, (-speed) >> 1);
                case 32: return new Vector2(-speed, speed >> 1);
                case 41: return new Vector2(speed, (-speed) >> 1);
                case 42: return new Vector2(speed, speed >> 1);
                default: return Vector2.zero;
            }
        }

        private int EnemyDamage(Fighter source, Fighter target) => CombatDamageCalculator.EnemyHit(
            source.attack, target.defense, source.actor.level, target.actor.level,
            source.enemy, source.attackMode, UnityEngine.Random.Range);

        private int EnemyHoldDamage(Fighter source) => source.attackMode >= 0 ? int.Parse(source.enemy.holdDamage.Split(',')[source.attackMode]) : 100;

        private void EmitEnemyProjectile(Fighter source)
        {
            var a = source.actor;
            if (!a.IsEnemy || !Role || Role.hidden || Role.hp <= 0) return;
            int id = a.BehaviorId, frame = a.FrameIndex;
            bool caster = id == 240 || id == 241 || id == 242;
            if (caster && frame == 9 && (a.actionIndex == 18 || a.actionIndex == 19))
                AddProjectile(source, a.actionIndex == 18 ? 27 : 28, Role.mapPosition, 0, 0, true, true, true);
            else if (a.actionIndex == 19 && (frame == 4 || frame == 10) &&
                (id == 210 || id == 211 || id == 212 || id == 219 || id == 221))
                AddProjectile(source, 21, a.mapPosition, a.mirror ? 1 : 0, 30, false, false, true);
            else if (a.actionIndex == 19 && frame == 9 && (id == 238 || id == 239))
                a.mapPosition = Role.mapPosition + new Vector2((Role.Animation.footWidth - a.Animation.footWidth) >> 1,
                    (Role.Animation.footHeight - a.Animation.footHeight) >> 1);
        }

        private void AddProjectile(Fighter source, int action, Vector2 position, int direction, int speed,
            bool persistent, bool disappearOnEnd, bool canDodge)
        {
            var definition = source.actor.Definition;
            var entry = Array.Find(OriginalAssets.Catalog.effects, e => e.animation == definition.animation);
            if (entry == null || !entry.prefab) throw new InvalidOperationException("Missing projectile prefab " + definition.animation);
            var visual = Instantiate(entry.prefab, actorRoot.transform, false);
            visual.Initialize(new SpriteDefinition { id = -1001, type = 15, name = "飞行物", animation = definition.animation, images = definition.images },
                -1001, position, 2);
            visual.mirror = false; // Bullet.draw uses the unmirrored frame in both travel directions.
            visual.returnToIdle = false;
            visual.PlayOnce(action);
            visual.keepLastFrame = disappearOnEnd;
            var p = new Projectile
            {
                actor = visual, owner = source, velocity = ProjectileVelocity(direction, speed),
                damage = EnemyDamage(source, fighters[-1]), holdDamage = EnemyHoldDamage(source),
                persistent = persistent, canDodge = canDodge,
            };
            visual.Stepped += _ => StepProjectile(p);
            if (disappearOnEnd) visual.ActionFinished += _ => p.dead = true;
            projectiles.Add(p);
        }

        private void TickProjectiles(float delta)
        {
            if (menu || reinforceOpen || DialogueVisible || criticalStage != 0 || LastError != null) return;
            foreach (var p in projectiles.ToArray())
            {
                if (!p.actor || !p.owner.actor) p.dead = true;
                if (!p.dead) p.actor.Tick(delta);
                if (!p.dead) continue;
                if (p.actor) Destroy(p.actor.gameObject);
                projectiles.Remove(p);
            }
        }

        private void StepProjectile(Projectile p)
        {
            if (p.dead || !p.actor) return;
            HitProjectile(p);
            if (p.dead) return;
            // Original moveSpriteByStep checks intermediate positions in 20 x 10 pixel steps.
            var remaining = p.velocity;
            while (!p.dead && remaining.sqrMagnitude > .0001f)
            {
                float fraction = Mathf.Min(1, 20 / Mathf.Max(20, Mathf.Abs(remaining.x)), 10 / Mathf.Max(10, Mathf.Abs(remaining.y)));
                var movement = remaining * fraction;
                p.actor.mapPosition += movement;
                remaining -= movement;
                var a = p.actor;
                if (Map == null || a.mapPosition.x <= 0 || a.mapPosition.y + a.CurrentFrame.edgeRect.y <= 0 ||
                    a.mapPosition.x + a.Animation.footWidth >= Map.Width || a.mapPosition.y + a.Animation.footHeight >= Map.Height)
                { p.dead = true; break; }
                // Bullets inherit isCheckMapBlock=false, so terrain collision does not stop them.
                HitProjectile(p);
            }
            var edge = OriginalCombatMath.Rect(p.actor, p.actor.CurrentFrame.edgeRect);
            if (!edge.Overlaps(new Rect(cameraPoint.x - ViewWidth / 2, cameraPoint.y - ViewHeight / 2, ViewWidth, ViewHeight))) p.dead = true;
        }

        private void HitProjectile(Projectile p)
        {
            if (!Role || Role.hidden || !fighters.TryGetValue(-1, out var target) || target.dead || !OriginalCombatMath.Hit(p.actor, Role)) return;
            if (p.canDodge && UnityEngine.Random.Range(0, 100) <
                CombatDamageCalculator.DodgeChance(p.owner.hit, target.dodge, p.owner.actor.level, Role.level, true))
                ShowCombatMiss(Role);
            else
            {
                target.hold = Mathf.Max(0, target.hold - p.holdDamage);
                // Bullet.iCriticalPoint is never initialized from its owner and stays zero.
                Damage(target, p.damage, false);
                ApplyHitReaction(target, p.actor.CurrentStep.attackType, !p.canDodge);
            }
            if (!p.persistent) p.dead = true; // A dodged one-shot projectile is consumed too.
        }

        private void ClearProjectiles(OriginalActor owner = null)
        {
            foreach (var p in projectiles.ToArray())
            {
                if (owner && p.owner.actor != owner) continue;
                if (p.actor) Destroy(p.actor.gameObject);
                projectiles.Remove(p);
            }
        }
    }
}
