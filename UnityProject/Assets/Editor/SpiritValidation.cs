using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class SpiritValidation
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<string> checks = new List<string>();
        private static object Get(object o, string name) => ValidationState.Get(o, name);
        private static void Set(object o, string name, object value) => ValidationState.Set(o, name, value);
        private static object Call(GuJianGame g, string name, params object[] args) => typeof(GuJianGame).GetMethod(name, Private).Invoke(g, args);
        private static void Require(bool yes, string message) { if (!yes) throw new Exception(message); }
        private static EquipmentInventory Bag(GuJianGame g) => g.EquipmentBag;
        private static void Stage(GuJianGame g, int spiritLevel = 1)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999;
            g.Machine.Enqueue("200"); g.Machine.Tick();
            Require(g.Map != null && !g.Machine.Running && !g.DialogueVisible, "Stage load failed");
            foreach (var a in g.Actors.Values) a.locked = true;
            if (spiritLevel > 0) g.Machine.Command(0x64, new object[] { spiritLevel });
            g.Machine.Command(0x65, new object[] { true });
        }
        private static void SpiritTick(GuJianGame g, int count = 1)
        {
            for (int i = 0; i < count; i++) Call(g, "TickSpirit", .080001f);
        }
        private static EquipmentInstance Add(GuJianGame g, int id, int plus = 0)
        {
            var item = EquipmentInstance.Create(id, plus); Bag(g).Add(item); return item;
        }

        private static bool Feed(GuJianGame g, int index, bool inherit = false)
        {
            bool accepted = g.FeedSpirit(index, inherit);
            if (accepted) g.CompleteSpiritFeed();
            return accepted;
        }
        private static bool Confirm(GuJianGame g, bool inherit)
        {
            bool accepted = g.ConfirmSpiritFeed(inherit);
            if (accepted) g.CompleteSpiritFeed();
            return accepted;
        }
        [MenuItem("古剑/验证剑灵吞噬与培养")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            float scale = Time.timeScale; var random = UnityEngine.Random.state; string save = g.saveFileName;
            checks.Clear(); Time.timeScale = 0; string error = null;
            try
            {
                Stage(g, 0); Add(g, 3);
                Require(!Feed(g, 1) && !g.SelectSpiritFood(1), "Feeding without spirit accepted");
                g.Machine.Command(0x64, new object[] { 1 });
                int count = g.Equipment.Count;
                Require(!Feed(g, -1) && !Feed(g, count) && !Feed(g, g.EquippedIndex), "Invalid/equipped food accepted");
                Require(g.SelectSpiritFood(1) && g.SpiritFeedConfirming, "Confirmation missing");
                g.CancelSpiritFeed();
                Require(!g.SpiritFeedConfirming && !Confirm(g, true) && g.Equipment.Count == count && g.SpiritExperience == 0, "Cancel consumed weapon or awarded XP");
                checks.Add("Missing spirit, invalid indices and equipped weapon are rejected; selecting and canceling confirmation changes no equipment, experience or attributes");

                Stage(g); var food = Add(g, 3, 9); food.durability = 77; food.skillId = 5; food.skillLevel = 7;
                Set(g, "spiritDurability", 250);
                Require(g.SelectSpiritFood(1) && Confirm(g, false), "Feed with retained skill failed");
                Require(g.SpiritLevel == 7 && g.SpiritExperience == 2475, "Expected 13500 XP -> level 7 with 2475 remainder");
                Require(g.GetSpiritAttribute(0) == 3706 && g.GetSpiritAttribute(1) == 621 && g.GetSpiritAttribute(2) == 543 && g.GetSpiritAttribute(3) == 259,
                    "Reinforced food stats or +9 defense effect mismatch");
                Require(g.SpiritDurability == 304 && g.SpiritSkill == 13 && g.SpiritSkillLevel == 1 && g.Equipment.Count == 1,
                    "Sequential durability rounding, retained skill or instance consumption mismatch");
                checks.Add("Level-15 +9 精金 with 77 durability: exactly 13500 XP -> level 7 / 2475 XP; HP 3706, durability 621, attack 543, defense 259; current durability 304 preserves each integer growth ratio; skill retained");

                Stage(g, 40); food = Add(g, 3, 8); food.durability = 0;
                int defense = g.GetSpiritAttribute(3), maximum = g.GetSpiritAttribute(1);
                Require(Feed(g, 1), "Level-cap feeding failed");
                Require(g.GetSpiritAttribute(3) == defense && g.GetSpiritAttribute(1) == maximum && g.SpiritExperience == 0,
                    "Non-+9 effect applied, spent durability awarded or max-level XP grew");
                food = Add(g, 25, 9); food.durability = 0;
                int attack = g.GetSpiritAttribute(2), hit = g.GetSpiritAttribute(4), hp = g.GetSpiritAttribute(0);
                Require(Feed(g, 1), "Malformed source effect entry failed");
                Require(g.GetSpiritAttribute(0) == hp + 1160 && g.GetSpiritAttribute(2) == attack + 1160 && g.GetSpiritAttribute(4) == hit + 4,
                    "Original first-pair effect parsing not preserved");
                checks.Add("At level 40 weapons still add permanent stats without XP; +8 adds no +9 effect and zero durability adds none; original 真轩辕 effect entry retains the source reader's first-pair interpretation");

                Stage(g, 39); Set(g, "spiritExperience", 39 * 39 * 39 * 25 - 1); Add(g, 0);
                Require(Feed(g, 1) && g.SpiritLevel == 40 && g.SpiritExperience == 1600000, "Cap boundary did not fill level-40 XP");
                Add(g, 0); Feed(g, 1);
                Require(g.SpiritLevel == 40 && g.SpiritExperience == 1600000, "XP overflow at cap");
                checks.Add("A feed across level 39 -> 40 caps level at 40 and fills XP to the original 1600000 threshold; further feeding leaves XP unchanged");

                Stage(g); food = Add(g, 3); food.skillId = 5; food.skillLevel = 7;
                g.SelectSpiritFood(1); Require(Confirm(g, true) && g.SpiritSkill == 5 && g.SpiritSkillLevel == 7, "Skill inheritance failed");
                food = Add(g, 3); food.skillId = 5; food.skillLevel = 3;
                Require(Feed(g, 1, true) && g.SpiritSkillLevel == 7, "Same-skill downgrade accepted");
                food = Add(g, 3); food.skillId = 5; food.skillLevel = 8;
                Require(Feed(g, 1, true) && g.SpiritSkillLevel == 8, "Same-skill upgrade rejected");
                Set(g, "spiritSkill", -2); food = Add(g, 4); food.skillId = 8; food.skillLevel = 2;
                Require(Feed(g, 1) && g.SpiritSkill == 8 && g.SpiritSkillLevel == 2, "Empty spirit skill did not inherit automatically");
                checks.Add("Confirmation can retain or replace a skill; same skill upgrades only, lower skill cannot downgrade, and an empty spirit skill inherits automatically");

                Stage(g); var equipped = Add(g, 3); Call(g, "SelectEquipment", 1);
                Require(Feed(g, 0) && g.EquippedIndex == 0 && ReferenceEquals(g.Equipment[0], equipped), "Feeding an earlier bag slot changed equipped identity");
                food = Add(g, 4); var selected = food; g.SelectSpiritFood(1); Bag(g).TryRemove(selected); Add(g, 5);
                Require(!Confirm(g, true) && g.Equipment.Count == 2, "Stale confirmation consumed replacement instance");
                checks.Add("Feeding before the equipped slot preserves equipped instance identity; a stale confirmation cannot consume a replacement at the same bag index");

                Stage(g); Require(g.SpiritDurability == 0, "New spirit should start empty");
                SpiritTick(g, 10); Require(g.SpiritDurability == 0, "Recovery tick early");
                SpiritTick(g); Require(g.SpiritDurability == 5, "11th 80-ms tick must recover 1 percent");
                foreach (string flag in new[] { "menu", "reinforceOpen" })
                {
                    Set(g, flag, true); SpiritTick(g, 44); Set(g, flag, false);
                    Require(g.SpiritDurability == 5, "UI did not freeze recovery");
                }
                Set(g, "criticalStage", 24); SpiritTick(g, 44); Set(g, "criticalStage", 0);
                Require(g.SpiritDurability == 5, "Critical did not freeze recovery");
                Set(g, "spiritDurability", 499); SpiritTick(g, 11);
                Require(g.SpiritDurability == 500, "Recovery exceeded maximum");
                g.Machine.Command(0x7F, new object[0]);
                Require((bool)g.Machine.Command(0x91, new object[0]) && g.durability == 450 && g.Actors[-2].hidden, "Integration entry cost or visibility");
                SpiritTick(g, 10); Require(g.durability == 450, "Integrated drain early");
                SpiritTick(g); Require(g.durability == 445, "Integrated drain mismatch");
                g.Machine.Command(0x7C, new object[] { true }); SpiritTick(g, 33);
                Require(g.durability == 500, "Infinite durability must refill and prevent drain");
                g.Machine.Command(0x7C, new object[] { false }); g.durability = 3; SpiritTick(g, 11);
                Require(!(bool)g.Machine.Command(0x91, new object[0]) && g.SpiritDurability == 0 && g.durability == g.Equipment[g.EquippedIndex].MaximumDurability && !g.Actors[-2].hidden,
                    "Exhaustion did not split/restore weapon and spirit");
                checks.Add("Natural recovery and integrated drain use exact 11-tick / 1% cadence; UI and critical pause, recovery clamps, entry costs 10%, infinite durability prevents drain, and exhaustion splits and restores the ordinary weapon");

                Stage(g, 10); Set(g, "spiritDurability", 600);
                g.Machine.Command(0x7F, new object[0]);
                food = Add(g, 3, 9); food.skillId = 5; food.skillLevel = 7; food.durability = 77;
                g.SelectSpiritFood(1); Confirm(g, true);
                Require(g.SpiritLevel == 10 && g.SpiritExperience == 13500 && (int)Get(g, "equippedSkill") == 5, "Integrated food/skill not synchronized");
                int savedDurability = g.SpiritDurability;
                int[] savedAttributes = Enumerable.Range(0, 7).Select(g.GetSpiritAttribute).ToArray();
                g.saveFileName = "validation-spirit.json"; g.Save();
                Set(g, "spiritSkill", 13); Set(g, "spiritBonuses", new int[7]); g.durability = 1;
                g.Load(); g.Machine.Tick();
                Require(!g.Machine.Running && !g.DialogueVisible && g.SpiritLevel == 10 && g.SpiritExperience == 13500 && g.SpiritSkill == 5 && g.SpiritSkillLevel == 7 &&
                    g.SpiritDurability == savedDurability && Enumerable.Range(0, 7).Select(g.GetSpiritAttribute).SequenceEqual(savedAttributes), "Spirit save/load values mismatch");
                Require((bool)g.Machine.Command(0x91, new object[0]) && (int)Get(g, "equippedSkill") == 5 && g.Actors[-2].hidden, "Integrated save/load skill/visibility mismatch");
                var images = (string[])Get(g.Actors[-1], "images");
                Require(images[1] == "action/w_97" && images[4] == "action/" + g.Equipment[g.EquippedIndex].Data.image, "Loaded sword sheets must preserve spirit and ordinary equipment independently");
                checks.Add("Actual integrated Save/Load restores level, XP, all seven attributes, live durability, inherited skill/level, hidden spirit, active skill selection and spirit weapon appearance");

                g.Machine.Command(0x7F, new object[0]);
                Set(g, "menu", true); Set(g, "menuPage", 6); g.Actors[-1].locked = true;
                Add(g, 3, 9);
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                Time.timeScale = scale; UnityEngine.Random.state = random; g.saveFileName = save;
                Set(g, "criticalStage", 0);
                var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
                Directory.CreateDirectory("../Reference/Validation");
                File.WriteAllText("../Reference/Validation/spirit.json", JsonUtility.ToJson(report, true));
                Debug.Log("GUJIAN_SPIRIT_VALIDATION " + (error == null) + " " + error);
            }
        }
    }
}
