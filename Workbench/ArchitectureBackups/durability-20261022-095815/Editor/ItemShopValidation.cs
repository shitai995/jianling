using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ItemShopValidation
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<string> checks = new List<string>();
        private static object Get(object o, string n) => o.GetType().GetField(n, Private | BindingFlags.Public).GetValue(o);
        private static void Set(object o, string n, object v) => o.GetType().GetField(n, Private | BindingFlags.Public).SetValue(o, v);
        private static object Call(GuJianGame g, string n, params object[] args) => typeof(GuJianGame).GetMethod(n, Private).Invoke(g, args);
        private static void Require(bool yes, string why) { if (!yes) throw new Exception(why); }
        private static EquipmentInventory Bag(GuJianGame g) => g.EquipmentBag;
        private static void Give(GuJianGame g, int id, int n = 1) { g.Inventory.RemoveUpTo(id, int.MaxValue); g.Inventory.Add(id, n); }
        private static void Stage(GuJianGame g, bool spirit = true)
        {
            g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = -999;
            g.Machine.Enqueue("200"); g.Machine.Tick();
            Require(g.Map != null && !g.Machine.Running && !g.DialogueVisible, "Stage load failed");
            foreach (var a in g.Actors.Values) a.locked = true;
            if (spirit) g.Machine.Command(0x64, new object[] { 1 });
        }
        private static void Shop(GuJianGame g, int type = 1) { g.Machine.Command(0x63, new object[] { type }); }
        [MenuItem("古剑/验证道具与商店交易")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            string save = g.saveFileName, error = null; var random = UnityEngine.Random.state; float scale = Time.timeScale;
            Time.timeScale = 0; checks.Clear();
            try
            {
                Stage(g); var role = g.Actors[-1]; Give(g, 0); Give(g, 4);
                Require(!g.UseItem(0) && g.ItemCount(0) == 1 && !g.UseItem(4) && g.ItemCount(4) == 1, "Full HP/durability consumed medicine");
                role.hp = role.maxHp - 10; Require(g.UseItem(0) && role.hp == role.maxHp, "HP clamp");
                g.durability = 0; Require(g.UseItem(4) && g.durability == 50 && g.Equipment[0].durability == 50, "Ordinary broken weapon repair");
                Set(g, "spiritDurability", 500); g.Machine.Command(0x7F, new object[0]); g.durability = 0; Give(g, 4); Give(g, 10);
                Require(!g.UseItem(4) && g.ItemCount(4) == 1 && g.UseItem(10) && g.SpiritDurability == 500, "Integrated zero durability rule");
                Give(g, 7); g.durability = 450; Require(g.UseItem(7) && g.durability == 500, "Stone cap");
                checks.Add("Full HP/durability rejects without consumption; HP/stone recovery clamps; broken ordinary weapon accepts a stone, zero-durability integrated spirit needs a hammer and stays synchronized");

                Stage(g); Give(g, 8, 2); var equipped = g.Equipment[0]; equipped.skillId = -2;
                Require(!g.UseItem(8), "No-skill book accepted"); equipped.skillId = 5; equipped.skillLevel = 8;
                Require(g.UseItem(8) && equipped.skillLevel == 9 && !g.UseItem(8) && g.ItemCount(8) == 1, "Skill cap");
                Set(g, "spiritDurability", 500); g.Machine.Command(0x7F, new object[0]);
                Require(g.UseItem(8) && g.SpiritSkillLevel == 2 && equipped.skillLevel == 9, "Integrated book target");
                Give(g, 23, 64);
                for (int i = 0; i < 64; i++) Require(g.UseItem(23) && g.SpiritSkill >= 5 && g.SpiritSkill <= 21 && g.SpiritSkillLevel == 1 && (int)Get(g, "equippedSkill") == g.SpiritSkill, "Random spirit skill range/level/sync");
                g.Machine.Command(0x7F, new object[0]); Give(g, 23); Require(g.UseItem(23) && equipped.skillId >= 5 && equipped.skillId <= 21 && equipped.skillLevel == 1, "Random weapon skill");
                checks.Add("Skill books reject absent/max-level skills, target the active weapon or spirit, and random manuals yield only IDs 5..21 at level 1 with active skill synchronized");

                Stage(g, false); Give(g, 11, 2); Require(!g.UseItem(11) && g.ItemCount(11) == 2, "No-spirit XP consumed");
                g.Machine.Command(0x64, new object[] { 1 }); Require(g.UseItem(11) && g.ItemCount(11) == 0 && g.SpiritLevel == 4 && g.SpiritExperience == 1100, "XP stack result");
                g.Actors[-2].level = 40; Give(g, 12); Require(!g.UseItem(12) && g.ItemCount(12) == 1, "Max spirit XP consumed");
                Give(g, 14, 105); int money = g.money;
                Require(g.UseItem(14) && g.money == money + 99000 && g.ItemCount(14) == 6 && g.UseItem(14) && g.money == money + 105000, "99-stack gold conversion");
                Give(g, 15, 2); money = g.money; Require(g.UseItem(15) && g.money == money + 10000, "Large gold conversion");
                checks.Add("XP pills require a spirit below level 40 and consume one batch of up to 99; exact 2000 XP yields level 4 / 1100 remainder; both gold sizes convert whole batches with leftovers retained");

                Stage(g); role = g.Actors[-1]; Give(g, 16, 2); g.experience = 17; role.hp = 1;
                Require(g.UseItem(16) && g.level == 2 && role.hp == role.maxHp && g.experience == 17, "Level pill XP/heal");
                g.level = 999; Require(!g.UseItem(16) && g.ItemCount(16) == 1, "Level cap wasted medicine");
                g.level = 1; Call(g, "RefreshAttributes", role, true); int max = role.maxHp; role.hp /= 2; float ratio = (float)role.hp / max;
                for (int id = 17; id <= 22; id++) { Give(g, id); Require(g.UseItem(id), "Permanent medicine failed"); }
                Require(((int[])Get(g, "permanentBonuses")).SequenceEqual(new[] { 100, 40, 20, 3, 2, 5 }) && role.maxHp == max + 100 && role.hp == Mathf.RoundToInt(ratio * (max + 100)), "Permanent stats/HP ratio");
                checks.Add("Level medicine heals and preserves XP, refuses level 999 without consuming; six permanent medicines add exact source bonuses and retain HP ratio");

                Stage(g); role = g.Actors[-1]; Give(g, 9, 2); Require(!g.UseItem(9), "Living revival accepted");
                var fighters = (System.Collections.IDictionary)Get(g, "fighters"); var f = fighters[-1];
                Call(g, "Damage", f, role.maxHp + 1, false, -1);
                Require(role.hp == 0 && g.ItemCount(9) == 2 && (bool)Get(f, "dead"), "Revival happened before death action ended");
                for (int i = 0; i < 120 && role.hp == 0; i++) role.Tick(.080001f);
                Require(role.hp == role.maxHp && role.actionIndex == 0 && !(bool)Get(f, "dead") && g.ItemCount(9) == 1 && !g.DialogueVisible, "Automatic revive state");
                role.hp = 0; role.Act(3, true); Set(f, "dead", true);
                Require(g.UseItem(9) && role.hp == role.maxHp && !(bool)Get(f, "dead"), "Manual revive state");
                checks.Add("Living characters cannot consume revival; actual fatal Damage waits for the death animation, then auto-consumes one medicine and restores full HP, idle action and live combat flag; manual revival also restores state");

                Stage(g); role = g.Actors[-1]; var spirit = g.Actors[-2];
                Give(g, 10); g.durability = 0; Call(g, "TickAutoMedicine");
                Require(spirit.actionIndex == 3 && g.durability == 0 && g.ItemCount(10) == 1, "Hammer did not wait for spirit animation");
                spirit.Tick(.080001f); Require(g.ItemCount(10) == 1, "Medicine consumed early");
                for (int i = 0; i < 120 && g.ItemCount(10) > 0; i++) spirit.Tick(.080001f);
                Require(g.durability == 50 && g.ItemCount(10) == 0 && spirit.actionIndex == 0, "Spirit hammer end effect");
                role.maxHp = 1000; role.hp = 1; g.durability = 1; Give(g, 0); Give(g, 4);
                Call(g, "TickAutoMedicine"); Require((int)Get(g, "autoMedicine") == 0, "HP did not take priority over repair");
                for (int i = 0; i < 120 && g.ItemCount(0) > 0; i++) spirit.Tick(.080001f);
                Require(g.ItemCount(0) == 0 && g.ItemCount(4) == 1, "Priority consumed repair");
                role.hp = role.maxHp; Call(g, "TickAutoMedicine");
                for (int i = 0; i < 120 && g.ItemCount(4) > 0; i++) spirit.Tick(.080001f);
                Require(g.durability == 50 && g.ItemCount(4) == 0, "Automatic stone failed");
                Give(g, 3); role.hp = 1;
                foreach (string flag in new[] { "menu", "reinforceOpen", "integrated" })
                {
                    Set(g, flag, true); Call(g, "TickAutoMedicine"); Set(g, flag, false);
                    Require((int)Get(g, "autoMedicine") == -1 && g.ItemCount(3) == 1, "Paused/integrated auto-use");
                }
                spirit.locked = true; Call(g, "TickAutoMedicine"); spirit.locked = false;
                Require((int)Get(g, "autoMedicine") == -1, "Locked spirit auto-use");
                Give(g, 1); Give(g, 2); role.maxHp = 10000; role.hp = 5000;
                Call(g, "TickAutoMedicine"); Require((int)Get(g, "autoMedicine") == 3, "Largest HP medicine threshold");
                // Switching equipped instance during the pending action must not use medicine on a different target.
                Bag(g).Add(EquipmentInstance.Create(3)); Call(g, "SelectEquipment", 1);
                for (int i = 0; i < 120 && (int)Get(g, "autoMedicine") >= 0; i++) spirit.Tick(.080001f);
                Require(g.ItemCount(3) == 1, "Pending medicine moved to replacement weapon");
                g.SetAutoRecoverPercent(90); Require(g.AutoRecoverPercent == 90, "Threshold setting");
                checks.Add("Sword spirit selects automatic medicines below the configured threshold, prioritizes HP, repairs ordinary breakage with a hammer, applies only at action 3 completion, pauses with UI/locks/integration, and cancels safely when equipment changes");

                Stage(g); Shop(g); g.money = 1000;
                Require(!g.SelectItemTrade(14, false) && !g.SelectItemTrade(15, false), "Gold listed for purchase");
                Require(g.SelectItemTrade(0, false), "Select buy"); g.SetTradeQuantity(3); g.CancelTrade();
                Require(!g.ConfirmTrade() && g.money == 1000 && g.ItemCount(0) == 0, "Canceled buy mutated");
                g.SelectItemTrade(0, false); g.SetTradeQuantity(99); Require(g.TradeQuantity == 99, "Quantity cap");
                g.SetTradeQuantity(3); Require(g.TradePrice == 150 && g.ConfirmTrade() && g.money == 850 && g.ItemCount(0) == 3, "Bulk buy");
                g.SelectItemTrade(0, false); g.money = 50; Require(!g.ConfirmTrade() && g.money == 50 && g.ItemCount(0) == 3, "Strict original balance boundary");
                Shop(g, 0); Require(g.ItemPurchasePrice(0) == 100, "Shop0 markup");
                g.money = 1000; g.SelectItemTrade(0, false); Require(g.ConfirmTrade() && g.money == 900, "Shop0 actual deduction");
                Give(g, 0, 5); Require(g.SelectItemTrade(0, true), "Select sale"); g.SetTradeQuantity(3);
                Require(g.TradePrice == 30 && g.ConfirmTrade() && g.money == 930 && g.ItemCount(0) == 2, "Bulk sell source price");
                Give(g, 0, 205); Require(g.SelectItemTrade(0, true, 2), "Select third sale stack"); g.SetTradeQuantity(99);
                Require(g.TradeQuantity == 7 && g.TradePrice == 70 && g.ConfirmTrade() && g.ItemCount(0) == 198, "Sale exceeded selected stack");
                Require(g.SelectItemTrade(0, true, 1), "Select second sale stack"); g.SetTradeQuantity(99);
                Require(g.TradeQuantity == 99 && g.ConfirmTrade() && g.ItemCount(0) == 99, "Second stack sale mismatch");
                checks.Add("Purchase/sale confirmation and cancellation, quantities 1..99, exact deductions, selected-stack sale limits and deductions, fixed sale prices, shop0 double purchase prices, unavailable gold and original strict-greater balance boundary");

                Stage(g); Give(g, 14, 101);
                Call(g, "OpenItemConfirmation", 14, 0); Call(g, "CancelItemConfirmation");
                Require(g.ItemCount(14) == 101, "Canceled item confirmation mutated inventory");
                Call(g, "OpenItemConfirmation", 14, 0); Call(g, "ConfirmItemConfirmation");
                Require(g.ItemCount(14) == 2 && g.money == 99000, "Use confirmation did not consume the original 99-item stack");
                Call(g, "OpenItemConfirmation", 14, 0); Set(g, "itemConfirmChoice", 1); Call(g, "ConfirmItemConfirmation");
                Require(g.ItemCount(14) == 0 && !((bool)Get(g, "itemConfirming")), "Discard confirmation mismatch: count=" + g.ItemCount(14) + ", confirming=" + Get(g, "itemConfirming"));
                checks.Add("Item confirmation cancel preserves inventory; use consumes the original 99-item batch; discard removes the selected remainder stack");

                Stage(g, false); Shop(g); g.money = 500000; g.SelectItemTrade(11, false);
                Require(!g.ConfirmTrade() && g.money == 500000 && g.ItemCount(11) == 0, "No spirit shop charged");
                g.Machine.Command(0x64, new object[] { 1 }); g.SelectItemTrade(11, false); g.SetTradeQuantity(2);
                Require(g.ConfirmTrade() && g.money == 496000 && g.SpiritLevel == 4 && g.SpiritExperience == 1100 && g.ItemCount(11) == 0, "Instant XP purchase");
                g.Actors[-2].level = 40; Require(!g.SelectItemTrade(11, false), "Max spirit shop lists XP");
                g.SelectItemTrade(17, false); g.SetTradeQuantity(99); Require(g.TradeQuantity == 1 && g.TradePrice == 1000 && g.ConfirmTrade(), "Instant stat purchase");
                Require(g.ItemCount(17) == 0 && ((int[])Get(g, "permanentBonuses"))[0] == 100 && g.ItemPurchasePrice(17) == 2000, "Progressive price/stat");
                for (int i = 0; i < 20; i++) { g.money = 1000000; g.SelectItemTrade(17, false); Require(g.ConfirmTrade(), "Repeated medicine purchase"); }
                Require(g.ItemPurchasePrice(17) == 20000 && g.ItemTrading.CapturePurchaseCounts()[1] == 19, "Price cap");
                checks.Add("Shop XP immediately trains spirit without inventory residue; invalid spirit purchases do not charge; stat medicines immediately apply one unit and rise by 1000 to a capped coefficient of 19 / price 20000");

                Stage(g); g.level = 20; Shop(g, 2); g.money = 1000000;
                Require(!g.SelectWeaponTrade(g.EquippedIndex, true) && !g.SelectWeaponTrade(25, false), "Equipped sale/locked weapon purchase accepted");
                Require(g.SelectWeaponTrade(3, false) && g.ConfirmTrade(), "Weapon purchase failed");
                var first = g.Equipment[1]; first.reinforcement = 9; first.durability = 1;
                Bag(g).Add(EquipmentInstance.Create(3)); var second = g.Equipment[2];
                g.SelectWeaponTrade(1, true); g.CancelTrade(); Require(g.Equipment.Count == 3, "Sale cancel consumed");
                g.SelectWeaponTrade(1, true); Bag(g).TryRemove(first); Bag(g).Add(EquipmentInstance.Create(4));
                Require(!g.ConfirmTrade() && ReferenceEquals(g.Equipment[1], second), "Stale sale sold replacement");
                Call(g, "SelectEquipment", 1); var current = g.Equipment[1]; int before = g.money;
                var sell = g.Equipment[0]; sell.reinforcement = 9; sell.durability = 1; g.SelectWeaponTrade(0, true);
                Require(g.TradePrice == sell.Data.sell && g.ConfirmTrade() && g.money == before + sell.Data.sell && ReferenceEquals(g.Equipment[g.EquippedIndex], current), "Sale identity/index/price");
                Require(g.SelectWeaponTrade(1, true), "Pending-sale fixture must select an unequipped instance"); Call(g, "SelectEquipment", 1); Require(!g.ConfirmTrade(), "Pending sale allowed subsequently equipped weapon");
                checks.Add("Weapon purchase creates independent instances; sales reject equipped weapons, canceled/stale confirmations cannot consume replacements, source sale price ignores reinforcement/durability, and removing an earlier slot preserves equipped identity");

                Stage(g); Shop(g); g.money = 100000; g.SelectItemTrade(17, false); g.ConfirmTrade(); Give(g, 23, 3);
                g.SetAutoRecoverPercent(40); g.saveFileName = "validation-items-shop.json"; g.Save();
                g.Load(); g.Machine.Tick(); Shop(g);
                Require(g.ItemPurchasePrice(17) == 2000 && ((int[])Get(g, "permanentBonuses"))[0] == 100 && g.ItemCount(23) == 3 && g.money == 99000 && g.AutoRecoverPercent == 40, "Save/load price/stats/items");
                string path = Path.Combine(Application.persistentDataPath, g.saveFileName);
                string contents = File.ReadAllText(path);
                // Missing optional price fields in existing version-3 saves must restore zeros.
                string legacy = System.Text.RegularExpressions.Regex.Replace(contents, "\\\"specialItemPurchases\\\"\\s*:\\s*\\[[^\\]]*\\]\\s*,?", "");
                legacy = System.Text.RegularExpressions.Regex.Replace(legacy, "\\\"autoRecoverPercent\\\"\\s*:\\s*\\d+\\s*,?", "");
                File.WriteAllText(path, legacy); g.Load(); g.Machine.Tick(); Shop(g);
                Require(g.ItemPurchasePrice(17) == 1000 && g.AutoRecoverPercent == 70, "Legacy save optional-field fallback");
                checks.Add("Actual Save/Load round-trips price coefficients, auto-recovery setting, permanent stats, inventory and money; version-3 saves without the optional fields retain compatibility with zero price coefficients and default 70% automatic recovery");
                g.SelectItemTrade(0, false); g.SetTradeQuantity(3);
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                Time.timeScale = scale; UnityEngine.Random.state = random; g.saveFileName = save;
                var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
                Directory.CreateDirectory("../Reference/Validation");
                File.WriteAllText("../Reference/Validation/items-shop.json", JsonUtility.ToJson(report, true));
                Debug.Log("GUJIAN_ITEM_SHOP_VALIDATION " + (error == null) + " " + error);
            }
        }
    }
}
