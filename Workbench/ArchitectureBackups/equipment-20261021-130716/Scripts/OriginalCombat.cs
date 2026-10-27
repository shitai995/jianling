using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    // Integer arithmetic follows Role.getDamagePoint / Enemy.initData in the recovered source.
    public static class OriginalCombatMath
    {
        public static int Growth(string formula, int offset)
        {
            var parts = formula.Split(',');
            return int.Parse(parts[0]) + (parts.Length > 1 ? int.Parse(parts[1]) * offset : 0);
        }
        public static int SkillValue(string formula, int level, bool damage)
        {
            var p = formula.Split(',').Select(int.Parse).ToArray();
            if (p.Length == 1) return p[0];
            int value = p[0] + level * p[1];
            if (p.Length == 4) value += damage ? (level >= 9 ? p[3] : 0) : level / p[2] * p[3];
            return value;
        }
        public static int RoleDamage(int attack, int defense, int level, int enemyLevel, int injury, SkillDefinition skill, int skillLevel)
        {
            long raw = (long)attack * SkillValue(skill.damage, skillLevel, true) / 100 + SkillValue(skill.amend, skillLevel, false) - defense;
            return (int)(raw * Mathf.Clamp(100 + (level - enemyLevel) * 5, 50, 150) * injury / 10000);
        }
        public static int DodgeChance(int hit, int dodge, int attackLevel, int defenseLevel, bool role)
        {
            int chance = 100 - (hit - dodge + (attackLevel - defenseLevel) * 3);
            return role ? Mathf.Clamp(chance, 10, 90) : Mathf.Min(chance, 90);
        }
        public static bool Hit(OriginalActor source, OriginalActor target)
        {
            if (!source.CurrentStep.attack || !target.CanReceiveHit) return false;
            foreach (var a in source.CurrentFrame.attackRects)
                foreach (var b in target.CurrentFrame.bodyRects)
                    if (Rect(source, a).Overlaps(Rect(target, b))) return true;
            return false;
        }
        public static Rect Rect(OriginalActor actor, OriginalRect r)
        {
            float x = actor.mirror ? -r.x - r.w + actor.Animation.footWidth : r.x;
            return new Rect(actor.mapPosition.x + x, actor.mapPosition.y + r.y, r.w, r.h);
        }
    }

    public sealed partial class GuJianGame
    {
        private sealed class Fighter
        {
            public OriginalActor actor;
            public EnemyDefinition enemy;
            public int attack, defense, hit, dodge, critical, injury = 100, hold;
            public int skill, skillLevel = 1, attackMode = -1;
            public int[] cooldowns;
            public int defenseCounter, bossCriticalCounter, teleportCounter, knockbackSpeed;
            public bool canDefend = true, bossCriticalReady = true;
            public bool dead, specialTriggered;
            public int refreshRemaining = 1;
            public float refreshAt;
            public Vector2 born;
            public int missTicks;
        }
        private sealed class FloatingNumber { public OriginalActor actor; public int value, type, ticks = 10; }
        private readonly Dictionary<int, Fighter> fighters = new Dictionary<int, Fighter>();
        private readonly List<FloatingNumber> numbers = new List<FloatingNumber>();
        private int comboIndex, comboIdleTicks = 3, queuedAttackVersion, energy, spiritExperience;
        private bool queuedAttack;
        private bool dropItems = true, infiniteDurability, maximumPower, bloodProtection, disableCooldown, integrated, experienceRoom;
        private int equippedSkill = -2, weaponLevel = 1;
        private float timerSeconds;
        private float combatPauseRemaining;
        private bool timerPaused;
        private string timerScript;
        public string RemainingTimeText
        {
            get
            {
                if (timerScript == null) return null;
                int seconds = Mathf.CeilToInt(Mathf.Max(0, timerSeconds));
                return "剩余时间 " + seconds / 60 + ":" + (seconds % 60).ToString("00");
            }
        }
        public int CurrentHp => Role ? Role.hp : 0;
        public int MaximumHp => Role ? Role.maxHp : 0;
        public int Energy => energy;
        public int EnemiesAlive => fighters.Values.Count(f => f.actor && f.actor.IsEnemy && !f.dead && !f.actor.hidden);

        private void RegisterFighter(OriginalActor actor)
        {
            if (actor.Definition.type != 0 && !actor.IsEnemy) return;
            actor.returnToIdle = true;
            var f = new Fighter { actor = actor, born = actor.mapPosition };
            if (actor.IsEnemy)
            {
                f.enemy = OriginalAssets.Table<EnemyDefinition>("enemy").First(e => e.id == actor.definitionId);
                f.cooldowns = new int[f.enemy.attackIntervals.Split(',').Length];
                actor.level = 10;
            }
            fighters[actor.actorId] = f;
            actor.Stepped += OnFighterStep;
            actor.Cycled += TickActorCombatFeedback;
            actor.ActionFinished += OnFighterActionFinished;
            RefreshAttributes(actor, true);
        }
        private void RefreshAttributes(OriginalActor actor, bool heal)
        {
            if (!fighters.TryGetValue(actor.actorId, out var f)) return;
            float ratio = actor.maxHp > 0 ? (float)actor.hp / actor.maxHp : 1;
            if (f.enemy == null)
            {
                actor.level = level;
                var a = OriginalAssets.Table<AttributeDefinition>("role")[0];
                var instance = Equipped;
                var e = instance?.Data;
                int bonus = durability > 0 && e != null ? 1 : 0;
                actor.maxHp = OriginalCombatMath.Growth(a.hp, level - 1) + (integrated ? SpiritAttribute(0) : instance?.Hp ?? 0) * bonus + permanentBonuses[0];
                f.attack = RefreshedComboAttack(OriginalCombatMath.Growth(a.attack, level - 1) + (integrated ? SpiritAttribute(2) : instance?.Attack ?? 0) + permanentBonuses[1]);
                f.defense = OriginalCombatMath.Growth(a.defense, level - 1) + (integrated ? SpiritAttribute(3) : instance?.Defense ?? 0) * bonus + permanentBonuses[2];
                f.hit = OriginalCombatMath.Growth(a.hit, level - 1) + (integrated ? SpiritAttribute(4) : e?.hit ?? 0) * bonus + permanentBonuses[3];
                f.dodge = OriginalCombatMath.Growth(a.dodge, level - 1) + (integrated ? SpiritAttribute(5) : e?.dodge ?? 0) * bonus + permanentBonuses[4];
                f.critical = Mathf.Min(1000, OriginalCombatMath.Growth(a.critical, level - 1) + (integrated ? SpiritAttribute(6) : e?.critical ?? 0) * bonus + permanentBonuses[5]);
                if (heal) f.hold = integrated ? 99 : 49;
            }
            else
            {
                int lv = Mathf.Clamp(actor.level, 10, 150);
                int band = lv < 100 ? lv / 10 - 1 : 8;
                int offset = lv < 100 ? lv % 10 : lv - 90;
                int group = actor.Definition.type == 9 ? 0 : actor.Definition.type == 11 ? 9 : 18;
                var a = OriginalAssets.Table<AttributeDefinition>("enemybase")[group + band];
                actor.maxHp = (int)((long)OriginalCombatMath.Growth(a.hp, offset) * f.enemy.hpCoefficient / 100);
                f.attack = OriginalCombatMath.Growth(a.attack, offset) * f.enemy.attackCoefficient / 100;
                f.defense = OriginalCombatMath.Growth(a.defense, offset);
                f.hit = OriginalCombatMath.Growth(a.hit, offset) + f.enemy.hit;
                f.dodge = OriginalCombatMath.Growth(a.dodge, offset) + f.enemy.dodge;
                f.hold = f.enemy.hold;
                f.injury = a.injuryCoefficient;
                f.dead = false;
            }
            actor.hp = Mathf.Clamp(heal ? actor.maxHp : Mathf.RoundToInt(ratio * actor.maxHp), 0, actor.maxHp);
            if (heal && actor.hp > 0 && f.dead) { f.dead = false; actor.ResetAction(); }
        }
        public void Attack()
        {
            if (!Role || !Role.WeaponVisible || Role.locked || Role.hp <= 0 || CriticalActive) return;
            int action = Role.LogicalAction;
            if (action <= 1)
            {
                comboIndex = comboIdleTicks < 3 && comboIndex < 3 ? comboIndex + 1 : 0;
                UseSkill(comboIndex);
            }
            else if (action < 11 && Role.FrameIndex >= Role.Animation.actions[Role.actionIndex].steps.Length - 3)
            { queuedAttack = true; queuedAttackVersion = Role.ActionVersion; }
        }
        private void TickAttackChain()
        {
            if (queuedAttack && queuedAttackVersion != Role.ActionVersion) queuedAttack = false;
            if (comboIdleTicks < 6) comboIdleTicks++;
            if (comboIdleTicks >= 3) comboIndex = 0;
        }
        private void ResetAttackChain()
        { queuedAttack = false; comboIndex = 0; comboIdleTicks = 3; }
        private bool ContinueAttackChain(OriginalActor actor)
        {
            int action = actor.LogicalAction;
            if (actor != Role || action < 7 || action > 10) return false;
            bool next = queuedAttack && queuedAttackVersion == actor.ActionVersion;
            queuedAttack = false;
            if (!next) { comboIdleTicks = 0; return false; }
            comboIndex = (action - 6) % 4;
            UseSkill(comboIndex, 1, true);
            return true;
        }
        public bool UseSkill(int skillId, int skillLevel = 1, bool scripted = false)
        {
            if (!Role || Role.hp <= 0 || (!scripted && (!Role.WeaponVisible || Role.locked || CriticalActive || Role.LogicalAction > 1))) return false;
            // GameCanvas.breakSkill checks durability, but does not gate on the recorded break timer.
            if (!scripted && skillId == 4 && durability <= 0 && !infiniteDurability) return false;
            if (!scripted && skillId >= 5 && skillCooldownTicks > 0)
            { Toast("技能冷却中：" + (skillCooldownTicks * .08f).ToString("0.0") + " 秒"); return false; }
            var skill = OriginalAssets.Table<SkillDefinition>("skill").FirstOrDefault(s => s.id == skillId);
            if (skill == null) return false;
            var f = fighters[-1]; f.skill = skillId; f.skillLevel = skillLevel;
            if (!scripted && skillId >= 5 && skillId <= 21)
            {
                int cost = skillId >= 14 || infiniteDurability ? 0 : OriginalCombatMath.Growth(skill.cost, skillLevel);
                if (durability <= 0 && cost > 0) { Toast("武器耐久不足"); return false; }
                if (cost > 0)
                {
                    int before = durability;
                    if (integrated) SetSpiritDurability(Mathf.Max(0, before - cost));
                    else
                    {
                        durability = Mathf.Max(0, before - cost); Equipped.durability = durability;
                        if (before == cost) RefreshAttributes(Role, false);
                    }
                }
                if (cost > 0 && durability == 0)
                { if (integrated) breakCooldownTicks = 3; else DestroyOrdinarySword(); }
                skillCooldownTicks = disableCooldown ? 1 : skill.cooldown;
            }
            CancelSupportCast();
            queuedAttack = false;
            if (skillId >= 5) LoadSkillVisual(skillId);
            Role.Act((skillId < 14 ? skillId + 7 : 21) + Role.actionBank, true);
            Role.keepLastFrame = false;
            if (skillId >= 14 && skillId <= 21) PrepareSupportSkill(skill);
            if (skillId < 4) comboIndex = skillId;
            return true;
        }
        private void OnFighterStep(OriginalActor actor)
        {
            if(actor==Role) TickRoleHudFeedback();
            if (!fighters.TryGetValue(actor.actorId, out var f) || f.dead || actor.hidden || actor.frozen) return;
            if (actor == Role) { TickRoleSkillTimers(); TickAttackChain(); }
            if (actor == Role && actor.LogicalAction >= 7 && actor.LogicalAction <= 10 && actor.FrameIndex == 0)
                PlayOriginalSound(0);
            if (ProcessCriticalStep(actor)) return;
            if (actor.IsEnemy && actor.actionIndex == 21) return; // Boss check is geometry only, never a normal attack.
            if (actor.IsEnemy && actor.actionIndex == 9)
            {
                TickKnockback(f);
                return;
            }
            EmitEnemyProjectile(f);
            if (actor.LogicalAction > 1 && !actor.ScriptControlled && Map != null)
            {
                var step = actor.CurrentStep;
                TryMove(actor, new Vector2(step.vx * (actor.mirror ? -1 : 1), step.vy));
            }
            if (!actor.CurrentStep.attack) return;
            foreach (var target in fighters.Values.ToArray())
            {
                if (!target.actor || target == f || target.dead || target.actor.hidden || actor.IsEnemy == target.actor.IsEnemy) continue;
                if (!OriginalCombatMath.Hit(actor, target.actor)) continue;
                if (UnityEngine.Random.Range(0, 100) < OriginalCombatMath.DodgeChance(f.hit, target.dodge, actor.level, target.actor.level, !target.actor.IsEnemy))
                {
                    ShowCombatMiss(target.actor); continue;
                }
                int damage;
                bool critical = false;
                if (f.enemy == null)
                {
                    RecordComboHit();
                    var skill = OriginalAssets.Table<SkillDefinition>("skill").First(s => s.id == f.skill);
                    damage = OriginalCombatMath.RoleDamage(f.attack, target.defense, actor.level, target.actor.level,
                        target.actor.actionIndex == 14 && f.skill != 4 ? 1 : target.injury, skill, f.skillLevel);
                    if (f.skill == 4 && !integrated) combatPauseRemaining = .08f;
                    if (f.skill == 4) damage += (Equipped?.MaximumDurability ?? 0) * 3;
                    else damage += RandomVariance(damage);
                    damage = Mathf.Max(10, damage);
                    critical = UnityEngine.Random.Range(0, 1000) < f.critical;
                    if (critical) damage *= 2;
                    target.hold = Mathf.Max(0, target.hold - skill.holdDamage);
                    GainHitEnergy();
                }
                else
                {
                    damage = EnemyDamage(f, target);
                    target.hold = Mathf.Max(0, target.hold - EnemyHoldDamage(f));
                    ApplyEnemySwordDamage(actor, target.actor);
                }
                target.actor.Face(actor.mapPosition.x + actor.Animation.footWidth / 2f > target.actor.mapPosition.x + target.actor.Animation.footWidth / 2f ? "r" : "l");
                int soundIndex = critical ? 1 : -1;
                Damage(target, damage, critical, soundIndex);
                if (f.enemy == null && target.dead && target.actor.Definition.type == 10) continue;
                // Role.processAttack overwrites this flag per successful target, except a dying Boss.
                if (f.enemy == null)
                    breakHit = actor.CurrentStep.attackType == 2 && (target.hold <= 0 || (!target.dead && target.actor.actionIndex == 14));
                ApplyHitReaction(target, actor.CurrentStep.attackType);
                if (f.enemy == null && target.hold <= 0 && (actor.CurrentStep.attackType == 1 || actor.CurrentStep.attackType == 2))
                    SetShake(4, actor.CurrentStep.attackType == 1 ? 1 : 0);
                // Role.processAttack plays the normal hit after reaction processing,
                // but returns before it when an ordinary attack kills a type-10 Boss.
                if (f.enemy == null && !critical)
                    PlayOriginalSound(2);
            }
            // The original consumes the flag as it loads the next frame, after all targets' damage.
            if (actor == Role) CompleteBreakHit();
        }
        private static int RandomVariance(int value)
        {
            int range = Mathf.Abs(value) / 10;
            return range > 0 ? UnityEngine.Random.Range(-range, range) : 0;
        }
        private void Damage(Fighter target, int damage, bool critical, int soundIndex = -1)
        {
            target.actor.hp = Mathf.Max(0, target.actor.hp - damage);
            target.actor.MarkHit();
            ShowCombatNumber(target.actor, damage, critical ? 1 : 0);
            if (!target.specialTriggered && !string.IsNullOrEmpty(target.actor.specialScript) && target.actor.hp <= target.actor.maxHp / 3)
            {
                target.specialTriggered = true;
                target.actor.hp = Mathf.Max(1, target.actor.hp);
                Machine.Enqueue(target.actor.specialScript);
                PlayDamageSound(soundIndex);
                return;
            }
            if (!target.actor.IsEnemy && bloodProtection) target.actor.hp = Mathf.Max(target.actor.maxHp / 10, target.actor.hp);
            if (target.actor.hp > 0)
            {
                PlayDamageSound(soundIndex);
                return;
            }
            target.dead = true;
            target.actor.Act(3 + target.actor.actionBank, true);
            if (target.enemy != null)
            {
                int enemyLevel = target.actor.level;
                if (dropItems) AwardDrops(target.actor, target.enemy, enemyLevel);
                AddExperience(EnemyExperience(target.enemy, enemyLevel));
                CompleteKillTasks(target.actor.definitionId);
            }
            if (!string.IsNullOrEmpty(target.actor.deathScript)) Machine.Enqueue(target.actor.deathScript);
            PlayDamageSound(soundIndex);
            // Normal player death resolves after action 3; scripted deaths retain their callback.
        }
        private void PlayDamageSound(int soundIndex)
        {
            if (soundIndex == 1) SetCriticalHitEffect(5);
            else if (soundIndex >= 0) PlayOriginalSound(soundIndex);
        }
        private void TryMove(OriginalActor actor, Vector2 step)
        {
            if (Map == null) return;
            var p = actor.mapPosition;
            if (Map.CanPass(p.x + step.x + actor.Animation.footWidth / 2f, p.y + actor.Animation.footHeight)) p.x += step.x;
            if (Map.CanPass(p.x + actor.Animation.footWidth / 2f, p.y + step.y + actor.Animation.footHeight)) p.y += step.y;
            bool moved = actor.mapPosition != p;
            actor.mapPosition = p;
            if (actor == Role && moved) PickUpTouchingWeapons();
        }
        private float aiElapsed;
        private void TickCombat(float delta)
        {
            numbers.RemoveAll(n => !n.actor);
            if (timerScript != null && !timerPaused && !menu && !DialogueVisible)
            {
                timerSeconds -= delta;
                if (timerSeconds <= 0) { Machine.Enqueue(timerScript); timerScript = null; }
            }
            aiElapsed += delta;
            if (menu || reinforceOpen || DialogueVisible || LastError != null || !Role || Role.hp <= 0) { aiElapsed = 0; return; }
            if (aiElapsed < .08f) return;
            int steps = Mathf.Min(8, (int)(aiElapsed / .08f)); aiElapsed = 0;
            for (int tick = 0; tick < steps; tick++)
            foreach (var f in fighters.Values.ToArray())
            {
                var a = f.actor;
                if (!a || f.enemy == null) continue;
                if (f.dead)
                {
                    if (a.Definition.type != 10 && (f.refreshRemaining > 0 || experienceRoom))
                    {
                        if (f.refreshAt == 0 && a.Completed) { a.hidden = true; f.refreshAt = Time.time + (experienceRoom ? .48f : 4.8f); }
                        if (f.refreshAt > 0 && Time.time >= f.refreshAt)
                        {
                            if (f.refreshRemaining > 0) f.refreshRemaining--;
                            f.refreshAt = 0; f.dead = false;
                            a.hidden = false; a.hp = a.maxHp; a.mapPosition = f.born; RestoreEnemyHold(f); a.ResetAction();
                        }
                    }
                    continue;
                }
                if (a.hidden || a.frozen) continue;
                TickEnemyCounters(f);
                if (CriticalActive || Role.hidden) continue;
                float dx = Role.mapPosition.x + Role.Animation.footWidth / 2f - a.mapPosition.x - a.Animation.footWidth / 2f;
                float dy = Role.mapPosition.y + Role.Animation.footHeight - a.mapPosition.y - a.Animation.footHeight;
                if ((a.definitionId == 235 || a.definitionId == 236) && Mathf.Abs(dx) <= f.enemy.vision)
                    f.teleportCounter = Mathf.Abs(dx) >= 80 ? f.teleportCounter + 1 : 0;
                if (a.locked || a.ScriptControlled || Machine.Running || a.actionIndex > 1) continue;
                if (Mathf.Abs(dx) > f.enemy.vision) { a.Act(0); continue; }
                if (TryBossDecision(f, dx, dy)) continue;
                var ranges = f.enemy.attackRanges.Split(';').Select(r => r.Split(',').Select(int.Parse).ToArray()).ToArray();
                bool attacking = false;
                for (int mode = 0; mode < ranges.Length; mode++)
                {
                    var r = ranges[mode];
                    if (f.cooldowns[mode] > 0 || Mathf.Abs(dx) > r[0] || Mathf.Abs(dy) > (dy < 0 ? r[1] : r[2]) || UnityEngine.Random.Range(0, 100) >= r[3]) continue;
                    a.Face(dx > 0 ? "r" : "l"); a.Act(17 + mode);
                    f.attackMode = mode; f.cooldowns[mode] = int.Parse(f.enemy.attackIntervals.Split(',')[mode]);
                    attacking = true; break;
                }
                if (attacking) continue;
                int speed = Mathf.Max(1, Mathf.Abs(a.Animation.actions[1].steps[0].vx));
                if (Mathf.Abs(dy) > speed * 2f / 3)
                {
                    a.Face(dy > 0 ? "d" : "u"); a.Act(1); TryMove(a, new Vector2(0, Mathf.Sign(dy) * speed * 2f / 3));
                }
                else if (Mathf.Abs(dx) > ranges[0][0])
                {
                    a.Face(dx > 0 ? "r" : "l"); a.Act(1); TryMove(a, new Vector2(Mathf.Sign(dx) * speed, 0));
                }
                else { a.Face(dx > 0 ? "r" : "l"); a.Act(0); }
            }
            if (!CriticalActive && !Machine.Running && Actor(-2) && !integrated && !Actor(-2).ScriptControlled)
            {
                var spirit = Actor(-2);
                var p = Role.mapPosition + new Vector2(Role.mirror ? -28 : 28, -18);
                spirit.mapPosition = Vector2.MoveTowards(spirit.mapPosition, p, 60 * delta);
                spirit.Face(Role.direction);
            }
        }
        private void AddExperience(int value)
        {
            experience += Mathf.Max(0, value) * Mathf.Clamp(smsExperienceMultiplier, 1, 4);
            bool changed = false;
            while (level < 999 && experience >= (long)level * level * level * 25)
            {
                experience -= (int)Math.Min(int.MaxValue, (long)level * level * level * 25);
                level++; changed = true;
            }
            if (changed) { RefreshAttributes(Role, true); Toast("等级提升为 " + level); }
        }
        private void CompleteKillTasks(int enemyId)
        {
            foreach (var task in tasks.ToArray())
            {
                if (!task.stages.Contains(Machine.Get("stageID"))) continue;
                for (int i = 0; i < task.enemyIds.Length; i++)
                    if (task.enemyIds[i] == enemyId && task.counts[i] > 0) task.counts[i]--;
                if (task.counts.Any(c => c > 0)) continue;
                tasks.Remove(task);
                Machine.Enqueue(task.script);
            }
        }
        private int EnemyExperience(EnemyDefinition enemy, int enemyLevel)
        {
            int value = Mathf.Min(enemyLevel * enemyLevel * 5 + 10, enemyLevel * 100) * enemy.experience / 100;
            if (experienceRoom) value *= 2;
            value = value * Mathf.Clamp(100 + (enemyLevel - level) * 10, 10, 200) / 100;
            int variance = value * 5 / 100;
            return value + UnityEngine.Random.Range(-variance, variance + 1);
        }
        private int EnemyDropMoney(EnemyDefinition enemy, int enemyLevel)
        {
            int value = Mathf.Min(100, enemyLevel + 20) * enemy.money / 100;
            int variance = value * 5 / 100;
            // Tool.getNextRnd returns -1 for an empty interval; money's upper bound is exclusive.
            int offset = variance > 0 ? UnityEngine.Random.Range(-variance, variance) : -1;
            return level > enemyLevel + 9 ? 10 : value + offset;
        }
    }
}
