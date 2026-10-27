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
    public static class SupportSkillValidation
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<string> checks = new List<string>();
        private static object Get(object o, string n) => ValidationState.Get(o, n);
        private static void Set(object o, string n, object v) => ValidationState.Set(o, n, v);
        private static object Call(GuJianGame g, string n, params object[] args) => typeof(GuJianGame).GetMethod(n, Private).Invoke(g, args);
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static void Stage(GuJianGame g)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999; g.Machine.Enqueue("200"); g.Machine.Tick();
            Require(g.Map != null && !g.Machine.Running && !g.DialogueVisible, "Stage fixture");
            foreach (var a in g.Actors.Values) a.locked = a.actorId != -1;
            g.Machine.Command(0x65, new object[] { true });
        }
        private static void FinishCast(GuJianGame g)
        {
            for (int i = 0; i < 150 && g.SupportSkillCasting; i++) g.Actors[-1].Tick(.080001f);
            Require(!g.SupportSkillCasting, "Cast did not finish");
        }
        private static void Ready(GuJianGame g) { g.Actors[-1].ResetAction(); Call(g, "ResetSkillCooldown", true); }
        private static void Timers(GuJianGame g, int count) { for (int i = 0; i < count; i++) Call(g, "TickRoleSkillTimers"); }
        private static object RoleFighter(GuJianGame g) => ((IDictionary)Get(g, "fighters"))[-1];
        [MenuItem("古剑/验证辅助技能与冷却")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first"); var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var random = UnityEngine.Random.state; string save = g.saveFileName, error = null; float scale = Time.timeScale;
            checks.Clear(); Time.timeScale = 0;
            try
            {
                foreach (int id in new[] { 14, 15, 16 })
                {
                    Stage(g); var role = g.Actors[-1]; role.maxHp = 10000; role.hp = 1; int durability = g.durability;
                    Require(g.UseSkill(id, 9) && g.SupportSkillCasting && role.hp == 1 && g.durability == durability && g.SkillCooldownTicks == 240, "Heal did not defer/cool down " + id);
                    FinishCast(g); int expected = id == 14 ? 1101 : id == 15 ? 1601 : 3201;
                    Require(role.hp == expected && g.durability == durability, "Original level-1 recovery formula " + id);
                    Ready(g); role.hp = role.maxHp - 1; Require(g.UseSkill(id, 1), "Capped heal cast"); FinishCast(g); Require(role.hp == role.maxHp, "Heal exceeded maximum");
                }
                checks.Add("Skills 14/15/16 consume no durability, begin 240-tick cooldown and heal only after action 21 ends; the recovered level-1 calculator yields 1100/1600/3200 at 10000 max HP even when equipped skill level is 9; healing clamps");

                foreach (int id in new[] { 17, 18, 19 })
                {
                    Stage(g); var item = EquipmentInstance.Create(25, 9); g.EquipmentBag.Add(item); Call(g, "SelectEquipment", 1); Ready(g);
                    g.durability = item.durability = 0;
                    Require(g.UseSkill(id, 9) && g.durability == 0, "Broken ordinary weapon repair could not start"); FinishCast(g);
                    int expected = id == 17 ? 170 : id == 18 ? 340 : 650;
                    Require(g.durability == Mathf.Min(item.MaximumDurability, expected) && item.durability == g.durability, "Ordinary repair amount/sync");
                    Ready(g); g.durability = item.MaximumDurability - 1; Require(g.UseSkill(id), "Repair cap cast"); FinishCast(g); Require(g.durability == item.MaximumDurability, "Repair cap");
                }
                Stage(g); g.Machine.Command(0x64, new object[] { 10 }); Set(g, "spiritDurability", g.GetSpiritAttribute(1)); g.Machine.Command(0x7F, new object[0]); Ready(g);
                int ordinary = g.Equipment[g.EquippedIndex].durability; g.durability = 0;
                Require(g.UseSkill(18, 9), "Integrated repair start"); FinishCast(g);
                Require(g.SpiritDurability == Mathf.Min(340, g.GetSpiritAttribute(1)) && g.Equipment[g.EquippedIndex].durability == ordinary, "Integrated repair target");
                checks.Add("Skills 17/18/19 repair 170/340/650 at cast completion, can repair zero durability when ready, clamp to the maximum and synchronize ordinary instances; integration repairs spirit only and preserves ordinary weapon durability");

                Stage(g); var r = g.Actors[-1]; r.hp = 1; Require(g.UseSkill(14), "Interrupt cast fixture");
                Call(g, "ApplyHitReaction", RoleFighter(g), 0, true);
                for (int i = 0; i < 60; i++) r.Tick(.080001f);
                Require(r.hp == 1 && !g.SupportSkillCasting && g.SkillCooldownTicks > 0, "Interrupted heal applied or cooldown refunded");
                Ready(g); Require(g.UseSkill(14), "Transition cast"); Call(g, "ClearStage"); Require(!g.SupportSkillCasting, "Stage clear retained pending effect");
                checks.Add("Actual forced hit reaction interrupts action 21 without healing or refunding cooldown; changing stage cancels pending support effects");

                Stage(g); Require(g.UseSkill(14), "Cooldown fixture"); g.Actors[-1].Tick(.080001f); Require(g.SkillCooldownTicks == 239, "80-ms timer step");
                foreach (string flag in new[] { "menu", "reinforceOpen" })
                { Set(g, flag, true); Timers(g, 300); Set(g, flag, false); Require(g.SkillCooldownTicks == 239, "UI advanced cooldown"); }
                Set(g, "dialogue", "pause"); Timers(g, 300); Set(g, "dialogue", null); Require(g.SkillCooldownTicks == 239, "Dialogue advanced cooldown");
                g.Actors[-1].frozen = true; Timers(g, 300); g.Actors[-1].frozen = false; Require(g.SkillCooldownTicks == 239, "Frozen actor advanced cooldown");
                g.Actors[-1].ResetAction(); Timers(g, 238); Require(g.SkillCooldownTicks == 1 && !g.UseSkill(14), "Cooldown boundary early");
                Timers(g, 1); Require(g.SkillCooldownTicks == 0 && g.UseSkill(14), "Cooldown boundary late");
                g.durability = 0; Timers(g, 20); Require(g.SkillCooldownTicks == 240, "Broken ordinary weapon advanced cooldown");
                Set(g, "integrated", true); Timers(g, 1); Set(g, "integrated", false); Require(g.SkillCooldownTicks == 239, "Integration zero-durability timer");
                checks.Add("Cooldown uses exact 80-ms ticks, stops for menu/reinforcement/dialogue/frozen actors and ordinary zero durability, advances during integration, and unlocks precisely at the final tick");

                Stage(g); Require(g.UseSkill(20, 9) && g.SuperArmorTicks == 0, "Armor applied before cast completion"); FinishCast(g);
                Require(g.SuperArmorTicks == 397, "Original armor duration boundary");
                var f = RoleFighter(g); Set(f, "hold", 0); int hp = g.CurrentHp;
                Call(g, "Damage", f, 10, false, -1); Call(g, "ApplyHitReaction", f, 0, false);
                Require(g.CurrentHp == hp - 10 && g.Actors[-1].actionIndex == 0, "Armor blocked HP damage or allowed ordinary stagger");
                Call(g, "ApplyHitReaction", f, 1, true); Require(g.Actors[-1].actionIndex == 5, "Guaranteed attack did not bypass armor"); g.Actors[-1].ResetAction();
                Timers(g, 396); Require(g.SuperArmorTicks == 1, "Armor expired early"); Timers(g, 1); Require(g.SuperArmorTicks == 0, "Armor expired late");
                Call(g, "ApplyHitReaction", f, 0, false); Require(g.Actors[-1].actionIndex == 2, "Expired armor still blocked stagger");
                Ready(g); Require(g.UseSkill(21, 9), "Long armor cast"); FinishCast(g); Require(g.SuperArmorTicks == 793, "Long armor duration");
                Set(g, "menu", true); Timers(g, 800); Set(g, "menu", false); Require(g.SuperArmorTicks == 793, "Menu drained armor");
                checks.Add("Skills 20/21 activate after casting for original 397/793 cycle boundaries; armor prevents ordinary stagger while HP damage still applies, guaranteed attacks bypass it, expiry restores reactions and menus pause duration");

                Stage(g); Require(g.UseSkill(20), "Armor refresh fixture"); FinishCast(g); Timers(g, 240); Ready(g);
                int elapsed = (int)Get(g, "superArmorElapsed"); Require(g.UseSkill(20), "Armor recast"); FinishCast(g);
                Require(g.SuperArmorTicks == Mathf.Max(1, 397 - (int)Get(g, "superArmorElapsed")) && (int)Get(g, "superArmorElapsed") > elapsed, "Recast reset original accumulated counter");
                checks.Add("Recasting armor retains the source's accumulated duration counter instead of silently refreshing it to a full duration");

                Stage(g); var equipped = g.Equipment[g.EquippedIndex]; equipped.skillId = 14; Set(g, "equippedSkill", 14); Call(g, "ResetSkillCooldown", false);
                Require(g.SkillCooldownTicks == 240, "Equipped support cooldown initialization");
                Set(g, "disableCooldown", true); Ready(g); Require(g.UseSkill(14) && g.SkillCooldownTicks == 1, "Offline cooldown option");
                Ready(g); g.durability = 10; Require(g.UseSkill(5, 1) && g.durability == 0 && g.SkillCooldownTicks == 1, "Attack skill low durability/cooldown regression");
                checks.Add("Selecting a skill initializes its original cooldown; the offline cooldown option uses one tick; attack skills retain low-durability casting and the common timer");

                Stage(g); Require(g.UseSkill(20), "Save armor fixture"); FinishCast(g); Timers(g, 40);
                int ticks = g.SuperArmorTicks, cooldown = g.SkillCooldownTicks, frame = (int)Get(g, "superArmorElapsed");
                g.saveFileName = "validation-support-skills.json"; g.Save(); g.Load(); g.Machine.Tick();
                Require(g.SuperArmorTicks == ticks && g.SkillCooldownTicks == cooldown && (int)Get(g, "superArmorElapsed") == frame, "Save/load cooldown/armor");
                string path = Path.Combine(Application.persistentDataPath, g.saveFileName), text = File.ReadAllText(path);
                foreach (string key in new[] { "skillCooldownTicks", "superArmorTicks", "superArmorElapsed" }) text = System.Text.RegularExpressions.Regex.Replace(text, "\\\"" + key + "\\\"\\s*:\\s*\\d+\\s*,?", "");
                File.WriteAllText(path, text); g.Load(); g.Machine.Tick(); Require(g.SuperArmorTicks == 0 && g.SkillCooldownTicks == 0, "Legacy timer fields");
                checks.Add("Real Save/Load preserves remaining cooldown, armor duration and visual counter; existing version-3 saves missing new timer fields still load with zero transient timers");

                Stage(g); Require(g.UseSkill(14), "Visual heal fixture"); FinishCast(g); Call(g, "DrawSupportSkillVisuals");
                var visual = (OriginalActor)Get(g, "supportRecoveryVisual");
                Require(visual && visual.actionIndex == 1 && visual.GetComponentsInChildren<SpriteRenderer>().Length >= 10, "Saved recovery prefab or action");
                Timers(g, 11); Call(g, "DrawSupportSkillVisuals"); Require(visual.hidden, "Recovery visual did not end");
                Ready(g); Require(g.UseSkill(20), "Visual armor fixture"); FinishCast(g); Call(g, "DrawSupportSkillVisuals");
                var armor = (OriginalActor)Get(g, "supportArmorVisual"); Require(armor && armor.actionIndex == 0 && !armor.hidden, "Armor visual missing");
                int sprites = Resources.FindObjectsOfTypeAll<Sprite>().Length, pieces = armor.transform.childCount;
                Timers(g, 5); Call(g, "DrawSupportSkillVisuals"); Require(Resources.FindObjectsOfTypeAll<Sprite>().Length == sprites && armor.transform.childCount == pieces, "Visual generated sprite/pieces at runtime");
                Set(g, "menu", false);
                checks.Add("Recovery/armor use original s_41 actions from a saved prefab with sufficient pieces, recovery ends after 11 ticks, and repeated sampling creates neither Sprite nor frame-piece nodes");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                Time.timeScale = scale; UnityEngine.Random.state = random; g.saveFileName = save;
                var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
                Directory.CreateDirectory("../Reference/Validation"); File.WriteAllText("../Reference/Validation/support-skills.json", JsonUtility.ToJson(report, true));
                Debug.Log("GUJIAN_SUPPORT_SKILL_VALIDATION " + (error == null) + " " + error);
            }
        }
    }
}
