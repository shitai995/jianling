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
    public static class AttackSkillValidation
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<string> checks = new List<string>();
        private static object Get(object o, string n) => o.GetType().GetField(n, Private | BindingFlags.Public).GetValue(o);
        private static void Set(object o, string n, object v) => o.GetType().GetField(n, Private | BindingFlags.Public).SetValue(o, v);
        private static object Call(GuJianGame g, string n, params object[] args) => typeof(GuJianGame).GetMethod(n, Private).Invoke(g, args);
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static object Fighter(GuJianGame g, int id) => ((IDictionary)Get(g, "fighters"))[id];
        private static void Stage(GuJianGame g, bool integrated = false)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999; g.Machine.Enqueue("200"); g.Machine.Tick();
            Require(g.Map != null && !g.Machine.Running && !g.DialogueVisible, "Stage fixture");
            foreach (var a in g.Actors.Values) if (a.IsEnemy) { a.hidden = true; a.locked = true; }
            g.Machine.Command(0x65, new object[] { true });
            var item = EquipmentInstance.Create(25, 9); ((List<EquipmentInstance>)Get(g, "equipment")).Add(item); Call(g, "SelectEquipment", 1);
            if (integrated)
            {
                g.Machine.Command(0x64, new object[] { 40 }); Set(g, "spiritDurability", g.GetSpiritAttribute(1));
                g.Machine.Command(0x7F, new object[0]);
            }
            g.Actors[-1].ResetAction(); Call(g, "ResetSkillCooldown", true);
            g.Machine.Command(0x30, new object[] { 900, 201, "l", 200, 250, 2 });
            var target = g.Actors[900]; target.locked = true; target.maxHp = target.hp = 10000000;
            Set(Fighter(g, 900), "hold", 100000); Set(Fighter(g, 900), "defense", 0); Set(Fighter(g, 900), "injury", 100);
        }
        private static int[] AttackFrames(int skill, bool integrated)
        {
            switch (skill)
            {
                case 5: return new[] { 3, 6 };
                case 6: return new[] { 5, integrated ? 9 : 8 };
                case 7: return new[] { 3, 6, 8, 10 };
                case 8: return new[] { 4, 6, 8, 10 };
                case 9: return new[] { 3, 6, 10, 13 };
                case 10: return new[] { 5, 8, 11, 13, 16, 18, 20 };
                case 11: return new[] { 5, 8, 10, 12, 14, 16 };
                case 12: return new[] { 5, 8, 11, 14 };
                default: return new[] { 6, 11, 15, 20, 24, 27, 29, 34, 39, 44, 49, 54, 59, 65 };
            }
        }
        private static int RawDamage(int skill, int level, int attack)
        {
            // Independent values from the recovered table, with independently calculated AP, equal levels, defense=0.
            int coefficient, amend;
            switch (skill)
            {
                case 5: case 8: coefficient = level == 1 ? 55 : 100; amend = 500; break;
                case 6: case 7: coefficient = 10; amend = level == 1 ? 500 : 5000; break;
                case 9: case 11: coefficient = 20; amend = level == 1 ? 1500 : 7000; break;
                case 10: coefficient = level == 1 ? 60 : 150; amend = 1000; break;
                case 12: coefficient = level == 1 ? 60 : 160; amend = 2000; break;
                default: coefficient = 40; amend = level == 1 ? 1500 : 7000; break;
            }
            return attack * coefficient / 100 + amend;
        }
        private static void Ready(GuJianGame g) { g.Actors[-1].ResetAction(); Call(g, "ResetSkillCooldown", true); }
        [MenuItem("古剑/验证主角攻击技能与连段")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>(); float scale = Time.timeScale;
            var random = UnityEngine.Random.state; string error = null, save = g.saveFileName; checks.Clear(); Time.timeScale = 0;
            try
            {
                foreach (int skill in Enumerable.Range(5, 9))
                {
                    foreach (bool integrated in new[] { false, true }) foreach (bool mirror in new[] { false, true }) foreach (int level in new[] { 1, 9 })
                    {
                        Stage(g, integrated); var role = g.Actors[-1]; var target = g.Actors[900];
                        role.Face(mirror ? "r" : "l"); role.mapPosition = new Vector2(mirror ? 160 : 400, 250);
                        int before = g.durability, ordinary = g.Equipment[g.EquippedIndex].durability;
                        Require(g.UseSkill(skill, level), "Skill start " + skill);
                        Require(role.actionIndex == skill + 7 + (integrated ? 27 : 0), "Wrong skill animation bank");
                        int cost = skill <= 6 ? 5 + 5 * level : skill <= 8 ? 10 + 5 * level : skill <= 12 ? 40 + 10 * level : 50 + 20 * level;
                        Require(g.durability == before - cost, "Skill cost " + skill);
                        Require(integrated ? g.SpiritDurability == g.durability && g.Equipment[g.EquippedIndex].durability == ordinary : g.Equipment[g.EquippedIndex].durability == g.durability, "Cost target synchronization");
                        Require(g.SkillCooldownTicks == (skill <= 8 ? 48 : skill <= 12 ? 72 : 120), "Skill cooldown " + skill);
                        var source = Fighter(g, -1); Set(source, "hit", 100000); Set(source, "critical", 0);
                        target.level = role.level;
                        var steps = role.Animation.actions[role.actionIndex].steps;
                        int hits = 0; var frames = new List<int>();
                        var ap = OriginalAssets.Table<AttributeDefinition>("role")[0].attack.Split(',').Select(int.Parse).ToArray();
                        int baseAttack = ap[0] + (ap.Length > 1 ? ap[1] * (role.level - 1) : 0) + (integrated ? g.GetSpiritAttribute(2) : g.Equipment[g.EquippedIndex].Attack);
                        Vector2 start = role.mapPosition;
                        for (int i = 0; i < steps.Length; i++)
                        {
                            target.ResetAction(); target.Face("l");
                            var step = role.CurrentStep;
                            if (step.attack)
                            {
                                var attack = OriginalCombatMath.Rect(role, role.CurrentFrame.attackRects[0]);
                                var body = target.CurrentFrame.bodyRects[0];
                                target.mapPosition = new Vector2(attack.center.x + step.vx * (mirror ? -1 : 1) - body.x - body.w / 2f,
                                    attack.center.y + step.vy - body.y - body.h / 2f);
                            }
                            int hp = target.hp; role.Tick(.080001f);
                            int damage = hp - target.hp;
                            if (damage > 0)
                            {
                                hits++; frames.Add(i);
                                int bonus = baseAttack * (hits / 10) / 100;
                                int raw = RawDamage(skill, level, baseAttack + (hits > 9 ? Mathf.Max(1, bonus) : bonus));
                                Require(damage >= raw - raw / 10 && damage < raw + raw / 10, "Damage formula " + skill + ": " + damage + " / " + raw);
                            }
                        }
                        Require(frames.SequenceEqual(AttackFrames(skill, integrated)), "Attack frame sequence " + skill + ": " + string.Join(",", frames));
                        Require(role.LogicalAction == 0 && role.actionIndex == (integrated ? 27 : 0), "Skill did not return to bank idle");
                        Require(Mathf.Abs(role.mapPosition.x - start.x - steps.Sum(s => s.vx) * (mirror ? -1 : 1)) < .01f, "Original skill movement");
                        Require(g.ActiveProjectileCount == 0, "Unspecified role projectiles were created");
                    }
                    checks.Add("Skill " + skill + ": actual UseSkill and 80-ms attack rectangles match recovered hit frames, level-1/9 formula ranges, movement, cooldown and synchronized durability for both directions and both animation banks (8 variants)");
                }

                foreach (bool integrated in new[] { false, true })
                {
                    Stage(g, integrated); var r = g.Actors[-1]; int before = g.durability;
                    g.Attack(); Require(r.LogicalAction == 7, "Initial basic attack");
                    int version = r.ActionVersion; g.Attack(); Require(r.ActionVersion == version, "Repeated attack restarted its action");
                    // Initial action has only three steps; even the first step is within its queue window.
                    var sequence = new List<int> { r.LogicalAction };
                    for (int chain = 0; chain < 4; chain++)
                    {
                        int action = r.LogicalAction, length = r.Animation.actions[r.actionIndex].steps.Length;
                        while (r.FrameIndex < length - 3) r.Tick(.080001f);
                        g.Attack();
                        for (int i = 0; i < length + 1 && r.LogicalAction == action; i++) r.Tick(.080001f);
                        sequence.Add(r.LogicalAction);
                    }
                    Require(sequence.SequenceEqual(new[] { 7, 8, 9, 10, 7 }), "Queued four-hit loop: " + string.Join(",", sequence));
                    Set(g, "queuedAttack", false);
                    while (r.LogicalAction > 1) r.Tick(.080001f);
                    g.Attack(); Require(r.LogicalAction == 8, "Immediate idle continuation");
                    while (r.LogicalAction > 1) r.Tick(.080001f);
                    r.Tick(.24001f); g.Attack(); Require(r.LogicalAction == 7, "Three-tick idle reset");
                    Require(g.durability == before, "Basic swings consumed non-source durability");
                    Ready(g); g.UseSkill(1); g.Attack(); Require(!(bool)Get(g, "queuedAttack"), "Early input queued outside final three steps");
                    r.Tick(.16001f); g.Attack(); Require((bool)Get(g, "queuedAttack"), "Final-three-step input not queued");
                    Call(g, "ApplyHitReaction", Fighter(g, -1), 0, true);
                    for (int i = 0; i < 40; i++) r.Tick(.080001f);
                    Require(r.LogicalAction == 0 && !(bool)Get(g, "queuedAttack"), "Interrupted queue survived hurt");
                }
                checks.Add("Ordinary/integrated basic chains accept only final-three-step input, continue 7/8/9/10/7, keep the immediate idle continuation, reset after three game ticks, preserve durability and discard hurt-interrupted input");

                foreach (int skill in Enumerable.Range(5, 9))
                {
                    Stage(g); Require(g.UseSkill(skill), "Visual cast"); var r = g.Actors[-1];
                    r.Sample(skill + 7, 4); Call(g, "TickVisuals", 0f);
                    var front = (OriginalActor)Get(g, "skillFront");
                    if (skill == 13) Require(!front, "Ghost dance retained an unrelated overlay");
                    else
                    {
                        Require(front && !front.hidden && front.actionIndex == 0 && front.FrameIndex == 4 % front.Animation.actions[0].steps.Length, "Front effect frame " + skill);
                        var back = (OriginalActor)Get(g, "skillBack"); bool expected = skill == 5 || skill == 7 || skill == 9;
                        Require(!expected || (back && !back.hidden && back.actionIndex == 1), "Back effect " + skill);
                        Require(!back || expected || back.hidden, "Unexpected back effect " + skill);
                        Require(front.GetComponentsInChildren<SpriteRenderer>(true).All(s => !s.sprite || AssetDatabase.Contains(s.sprite)), "Runtime-created Sprite");
                        r.presentationHidden = true; Call(g, "TickVisuals", 0f); Require(front.hidden, "Presentation-hidden role leaked its skill overlay"); r.presentationHidden = false;
                        r.ResetAction(); Call(g, "TickVisuals", 0f); Require(front.hidden, "Effect lingered after skill");
                    }
                }
                checks.Add("Skills 5-12 sample original saved foreground/background actions and persistent Sprites; overlay layers, frame sampling and hiding match the role; skill 13 has its effect within the role animation");

                Stage(g, true); var role2 = g.Actors[-1]; Call(g, "ApplyHitReaction", Fighter(g, -1), 1, true);
                Require(role2.actionIndex == 32, "Integrated hurt bank");
                for (int i = 0; i < 60; i++) role2.Tick(.080001f);
                Require(role2.actionIndex == 27, "Integrated hurt recovery bank");
                g.Machine.Command(0x7F, new object[0]); for (int i = 0; i < 60; i++) role2.Tick(.080001f);
                Require(role2.actionBank == 0 && role2.actionIndex == 0, "Unintegration bank");
                checks.Add("Integrated hurt uses the second bank and returns to action 27; ending integration restores ordinary animation idle");

                Stage(g, true); g.saveFileName = "validation-attack-skills.json"; g.Save(); g.Load(); g.Machine.Tick();
                Require(g.Actors[-1].actionBank == 27 && g.Actors[-1].actionIndex == 27, "Integrated save/load idle bank");
                Call(g, "ResetSkillCooldown", true); Require(g.UseSkill(14), "Integrated support after load");
                Require(g.Actors[-1].actionIndex == 48 && g.SupportSkillCasting, "Integrated support bank");
                for (int i = 0; i < 30; i++) g.Actors[-1].Tick(.080001f);
                Require(g.Actors[-1].actionIndex == 27 && !g.SupportSkillCasting, "Integrated support completion");
                var player = g.Actors[-1]; ((IDictionary<int, int>)Get(g, "inventory"))[9] = 1;
                Call(g, "Damage", Fighter(g, -1), player.hp + 1, false, -1);
                Require(player.actionIndex == 30, "Integrated death bank");
                for (int i = 0; i < 60 && player.hp == 0; i++) player.Tick(.080001f);
                Require(player.hp == player.maxHp && player.actionIndex == 27 && g.ItemCount(9) == 0, "Integrated automatic revive");
                Call(g, "MoveActor", player, "r", 6f, 12f);
                Require(player.actionIndex == 28, "Integrated scripted movement bank");
                ((IList)Get(g, "movements")).Clear(); player.ResetAction();
                checks.Add("Real integrated Save/Load restores idle 27, support casting uses 48, death/automatic revival uses 30/27, and scripted walking uses 28");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                Time.timeScale = scale; UnityEngine.Random.state = random; g.saveFileName = save;
                var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
                Directory.CreateDirectory("../Reference/Validation"); File.WriteAllText("../Reference/Validation/attack-skills.json", JsonUtility.ToJson(report, true));
                Debug.Log("GUJIAN_ATTACK_SKILL_VALIDATION " + report.passed + " " + error);
            }
        }
    }
}
