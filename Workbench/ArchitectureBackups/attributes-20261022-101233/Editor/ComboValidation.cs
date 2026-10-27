using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ComboValidation
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
        private static readonly List<string> checks = new List<string>();
        private static object Get(object o, string n) => ValidationState.Get(o, n);
        private static void Set(object o, string n, object v) => ValidationState.Set(o, n, v);
        private static object Call(GuJianGame g, string n, params object[] a) => typeof(GuJianGame).GetMethod(n, Flags).Invoke(g, a);
        private static object Fighter(GuJianGame g, int id) => ((IDictionary)Get(g, "fighters"))[id];
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static int AP(GuJianGame g) => (int)Get(Fighter(g, -1), "attack");
        private static int BaseAP(GuJianGame g)
        {
            var p = OriginalAssets.Table<AttributeDefinition>("role")[0].attack.Split(',').Select(int.Parse).ToArray();
            return p[0] + (p.Length > 1 ? p[1] * (g.Actors[-1].level - 1) : 0);
        }
        private static void Stage(GuJianGame g, bool integrate = false)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999; g.Machine.Enqueue("200"); g.Machine.Tick();
            Require(g.Map != null && !g.Machine.Running && !g.DialogueVisible, "Stage fixture");
            foreach (var a in g.Actors.Values) if (a.IsEnemy) a.hidden = a.locked = true;
            g.Machine.Command(0x65, new object[] { true });
            g.EquipmentBag.Add(EquipmentInstance.Create(25, 9)); Call(g, "SelectEquipment", 1);
            if (integrate)
            {
                g.Machine.Command(0x64, new object[] { 40 }); Set(g, "spiritDurability", g.GetSpiritAttribute(1)); g.Machine.Command(0x7F, new object[0]);
            }
            g.Actors[-1].ResetAction();
            Set(Fighter(g, -1), "hit", 100000); Set(Fighter(g, -1), "critical", 0);
            Spawn(g, 900);
        }
        private static OriginalActor Spawn(GuJianGame g, int id, int definition = 201)
        {
            g.Machine.Command(0x30, new object[] { id, definition, "l", 300, 250, 2 });
            var a = g.Actors[id]; a.locked = true; a.maxHp = a.hp = 10000000; a.level = g.Actors[-1].level;
            Set(Fighter(g, id), "defense", 0); Set(Fighter(g, id), "hold", 100000); Set(Fighter(g, id), "injury", 100); return a;
        }
        private static void Pose(OriginalActor target, int pose)
        {
            target.ResetAction();
            if (pose == 0) return;
            target.PlayOnce(pose);
            int frame = Array.FindIndex(target.Animation.actions[pose].steps, s => s.vulnerable && target.Animation.frames[s.frame].bodyRects.Length > 0);
            Require(frame >= 0, "Vulnerable pose " + pose); target.Sample(pose, frame);
        }
        private static void Align(OriginalActor r, OriginalActor t)
        {
            var step = r.CurrentStep; var attack = OriginalCombatMath.Rect(r, r.CurrentFrame.attackRects[0]);
            var body = OriginalCombatMath.Rect(t, t.CurrentFrame.bodyRects[0]);
            t.mapPosition += attack.center - body.center + new Vector2(step.vx * (r.mirror ? -1 : 1), step.vy);
        }
        private static int Hit(GuJianGame g, int skill = 0, int hold = 100000, int pose = 0, bool scripted = true)
        {
            var r = g.Actors[-1]; var t = g.Actors[900]; Pose(t, pose); Set(Fighter(g, 900), "hold", hold);
            r.ResetAction(); r.Face("r"); r.mapPosition = new Vector2(180, 250);
            Require(g.UseSkill(skill, 1, scripted), "Cast " + skill);
            int frame = Array.FindIndex(r.Animation.actions[r.actionIndex].steps, s => s.attack);
            for (int i = 0; i < frame; i++) r.Tick(.080001f);
            Align(r, t); int before = t.hp; r.Tick(.080001f); return before - t.hp;
        }
        [MenuItem("古剑/验证连击与崩击结算")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play first"); var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            float scale = Time.timeScale; var random = UnityEngine.Random.state; string save = g.saveFileName, error = null; checks.Clear(); Time.timeScale = 0;
            try
            {
                foreach (bool integrated in new[] { false, true })
                {
                    Stage(g, integrated); int baseAP = BaseAP(g) + (integrated ? g.GetSpiritAttribute(2) : g.Equipment[g.EquippedIndex].Attack);
                    for (int i = 1; i <= 20; i++)
                    {
                        int damage = Hit(g); int bonus = baseAP * (i / 10) / 100; if (i > 9) bonus = Mathf.Max(1, bonus);
                        Require(g.ComboHits == i && AP(g) == baseAP + bonus && damage >= 10, "AP boundary " + integrated + ":" + i);
                        // Basic attack 0: damage 70%, amend 0, zero defense and equal levels.
                        int raw = (baseAP + bonus) * 70 / 100;
                        Require(damage >= raw - raw / 10 && damage < raw + raw / 10, "Damage before increment " + i + ":" + damage + "/" + raw);
                    }
                    Require(g.ComboAttackPercent == 2 && g.Energy == 100, "Counter/energy");
                }
                checks.Add("20 actual original-frame hits in each animation bank: count and damage include the current hit's 10/20-hit AP boundary; each hit adds 5 energy");

                Stage(g); var role = g.Actors[-1]; var target = g.Actors[900];
                for (int i = 0; i < 10; i++) Hit(g);
                var props = (int[])Get(g, "permanentBonuses"); props[1] = 1000; Call(g, "RefreshAttributes", role, false);
                int apBase = BaseAP(g) + g.Equipment[g.EquippedIndex].Attack;
                Require(AP(g) == apBase + 1000 + (apBase + 1000) / 100, "Attribute refresh prop bonus");
                Hit(g); Require(AP(g) == apBase + apBase / 100, "Hit formula excludes role prop AP");
                Call(g, "ApplyHitReaction", Fighter(g, -1), 0, true); Require(g.ComboHits == 0 && AP(g) == apBase, "ComboOver formula");
                checks.Add("Preserves recovered formula differences: attribute refresh includes permanent role AP; comboCounter and comboOver exclude it");

                Stage(g, true); ((int[])Get(g, "spiritBonuses"))[2] = 1000;
                for (int i = 0; i < 10; i++) Hit(g);
                int total = BaseAP(g) + g.GetSpiritAttribute(2);
                Require(AP(g) == total + Mathf.Max(1, (total - 1000) / 100), "Spirit base/final AP difference");
                checks.Add("Integrated combo multiplier uses spirit base AP while final attack retains swallowed permanent spirit AP");

                Stage(g); Hit(g); Set(Fighter(g, -1), "hold", 100);
                Call(g, "Damage", Fighter(g, -1), 1, false, -1); Call(g, "ApplyHitReaction", Fighter(g, -1), 0, false);
                Require(g.ComboHits == 1, "Hold lost combo"); Set(Fighter(g, -1), "hold", 0); Set(g, "superArmorTicks", 20);
                Call(g, "ApplyHitReaction", Fighter(g, -1), 0, false); Require(g.ComboHits == 1, "Armor lost combo");
                Call(g, "ApplyHitReaction", Fighter(g, -1), 1, true); Require(g.ComboHits == 0 && g.ComboBreaks == 0, "Forced hurt retained combo");
                checks.Add("HP damage resisted by hold/armor retains combo; actual forced hurt resets hit/break counters and AP");

                Stage(g); role = g.Actors[-1]; target = g.Actors[900];
                Set(Fighter(g, -1), "hit", -100000); int dur = g.durability;
                UnityEngine.Random.InitState(2026); Hit(g, 4, 0); Require(g.ComboHits == 0 && g.durability == dur, "Dodged break counted");
                role.ResetAction(); target.hidden = true; g.UseSkill(4, 1, true); for (int i = 0; i < 30; i++) role.Tick(.080001f);
                Require(g.ComboHits == 0 && g.durability == dur, "Empty break counted");
                checks.Add("Dodge and empty swings add neither combo nor durability loss");

                foreach (bool integrated in new[] { false, true })
                {
                    Stage(g, integrated); dur = g.durability; int max = integrated ? g.GetSpiritAttribute(1) : g.Equipment[g.EquippedIndex].MaximumDurability;
                    int ordinary = g.Equipment[g.EquippedIndex].durability;
                    int baseAttack = BaseAP(g) + (integrated ? g.GetSpiritAttribute(2) : g.Equipment[g.EquippedIndex].Attack);
                    int damage = Hit(g, 4, 0);
                    // Break table is independently checked below; fixed term always uses ordinary +9 maximum durability.
                    int expected = baseAttack * 2 + g.Equipment[g.EquippedIndex].MaximumDurability * 3;
                    Require(damage == expected, "Break fixed damage " + damage + "/" + expected);
                    Require(g.ComboHits == 1 && g.ComboBreaks == 0 && g.durability == dur - max / 3 && g.BreakCooldownTicks == 3, "First break settlement");
                    Require(integrated ? g.SpiritDurability == g.durability && g.Equipment[g.EquippedIndex].durability == ordinary : g.Equipment[g.EquippedIndex].durability == g.durability, "Break instance sync");
                    g.Actors[-1].ResetAction(); for (int i = 0; i < 30; i++) g.Actors[-1].Tick(.080001f);
                    Require(g.durability == dur - max / 3, "Repeated break debit");
                    Hit(g, 0); Hit(g, 4, 0);
                    Require(g.ComboHits == 3 && g.ComboBreaks == 1 && g.ComboAttackPercent == 5 && g.durability == dur - 2 * (max / 3), "Chained break bonus");
                }
                checks.Add("Ordinary/integrated break: strengthened ordinary maximum x3 fixed damage, active maximum/3 debit once, correct instance, first-hit zero break bonus and chained break +5%");

                foreach (int pose in new[] { 0, 4, 14 })
                {
                    Stage(g);
                    if (pose == 14) { g.Machine.Command(0x31, new object[] { 900 }); Spawn(g, 900, 232); }
                    dur = g.durability; Hit(g, 0); Hit(g, 4, pose == 14 ? 100000 : 0, pose);
                    Require(g.ComboBreaks == 1 && g.durability < dur, "Reaction break pose " + pose);
                }
                Stage(g); dur = g.durability; Hit(g, 0); Hit(g, 4);
                Require(g.ComboHits == 2 && g.ComboBreaks == 0 && g.durability == dur, "High hold false break");
                checks.Add("Breaks launchable/airborne targets and guarded targets even with hold; high hold without guard prevents the break bonus and durability loss");

                Stage(g); Set(g, "infiniteDurability", true); dur = g.durability; Hit(g); Hit(g, 4, 0);
                Require(g.ComboBreaks == 1 && g.durability == dur, "Infinite durability lost break bonus");
                Stage(g); var item = g.Equipment[g.EquippedIndex]; Set(g, "equippedSkill", 14); g.durability = item.durability = 1;
                Hit(g); Hit(g, 4, 0); Require(g.durability == 0 && item.durability == 0 && g.SkillCooldownTicks == 240, "Destroyed sword cooldown");
                g.Actors[-1].ResetAction(); Require(!g.UseSkill(4), "Broken ordinary weapon allowed break");
                Set(g, "infiniteDurability", true); Require(g.UseSkill(4), "Infinite broken weapon break rejected");
                checks.Add("Infinite durability still grants break bonus; low durability clamps to zero, synchronizes instance and resets equipped skill cooldown; durability gate honors infinite option");

                foreach (bool lastBreaks in new[] { false, true })
                {
                    Stage(g); Spawn(g, 901); role = g.Actors[-1]; role.Face("r"); role.mapPosition = new Vector2(180, 250); dur = g.durability;
                    Set(Fighter(g, 900), "hold", lastBreaks ? 100000 : 0); Set(Fighter(g, 901), "hold", lastBreaks ? 0 : 100000);
                    Require(g.UseSkill(4, 1, true), "Multiple break cast");
                    int frame = Array.FindIndex(role.Animation.actions[role.actionIndex].steps, s => s.attack);
                    for (int i = 0; i < frame; i++) role.Tick(.080001f);
                    Align(role, g.Actors[900]); Align(role, g.Actors[901]); role.Tick(.080001f);
                    Require(g.ComboHits == 2 && g.ComboBreaks == (lastBreaks ? 1 : 0), "Last successful target flag");
                    Require(g.durability == dur - (lastBreaks ? g.Equipment[g.EquippedIndex].MaximumDurability / 3 : 0), "Multiple target single debit");
                }
                checks.Add("One attack frame counts each target; the last successful target overwrites the original break flag; at most one maximum/3 debit per frame");

                Stage(g); Hit(g); role = g.Actors[-1]; Set(g, "menu", true);
                for (int i = 0; i < 100; i++) Call(g, "TickRoleSkillTimers"); Set(g, "menu", false); Require(g.ComboHits == 1, "Menu lost combo");
                Call(g, "ClearStage"); Require(g.ComboHits == 1, "Stage transition reset combo");
                g.saveFileName = "validation-combo.json"; Stage(g); Hit(g); g.Save(); g.Load(); g.Machine.Tick();
                Require(g.ComboHits == 0 && g.ComboBreaks == 0, "Transient combo persisted");
                checks.Add("Menu and stage clear retain source combo; real Save/Load starts fresh counters and uses a separate validation save");

                Stage(g); role = g.Actors[-1]; target = g.Actors[900];
                // Drive the actual critical pipeline from its targeting action, including full attack sequences.
                g.Machine.Command(0x80, new object[] { true }); int criticalHits = 0;
                for (int i = 0; i < 300 && g.CriticalActive; i++)
                {
                    int stage = g.CriticalStage;
                    if (stage == 22 && role.CurrentFrame.attackRects.Length > 0) { target.ResetAction(); Align(role, target); }
                    int before = target.hp; bool attack = role.CurrentStep.attack;
                    role.Tick(.080001f); if (target.hp < before) criticalHits++;
                }
                Require(!g.CriticalActive && criticalHits > 10 && g.ComboHits == criticalHits && g.ComboAttackPercent >= 1, "Critical hit combo " + g.ComboHits + "/" + criticalHits);
                checks.Add("Full actual critical-kill targeting/actions 22/24/25 finish and count each damage frame before its damage calculation, including 10-hit AP growth");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                Time.timeScale = scale; UnityEngine.Random.state = random; g.saveFileName = save;
                var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
                Directory.CreateDirectory("../Reference/Validation"); File.WriteAllText("../Reference/Validation/combo-break.json", JsonUtility.ToJson(report, true));
                Debug.Log("GUJIAN_COMBO_VALIDATION " + report.passed + " " + error);
            }
        }
    }
}
