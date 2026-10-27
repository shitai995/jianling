using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    internal static class ValidationState
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public static object Get(object target, string name)
        {
            var field = target.GetType().GetField(name, Flags);
            return field != null ? field.GetValue(target) : target.GetType().GetProperty(name, Flags).GetValue(target);
        }
        public static void Set(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, Flags);
            if (field != null) field.SetValue(target, value);
            else target.GetType().GetProperty(name, Flags).SetValue(target, value);
        }
    }

    public static class DurabilityModuleValidation
    {
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Report(string name, List<string> checks, string error)
        {
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/" + name + ".json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_" + name + " " + report.passed + " " + error);
        }
        [MenuItem("古剑/验证武器耐久模块")]
        public static void Run()
        {
            var checks = new List<string>(); string error = null;
            var random = UnityEngine.Random.state;
            try
            {
                var bag = new EquipmentInventory(); var first = EquipmentInstance.Create(0);
                bag.Reset(first); var second = EquipmentInstance.Create(0); bag.Add(second);
                var state = new WeaponDurability(bag);
                state.Current = 17; Require(first.durability == 17, "Active write is detached");
                first.durability = 13; Require(state.Current == 13, "Instance write is detached");
                bag.TrySelect(1); state.Current = 29; bag.TrySelect(0);
                Require(state.Current == 13 && second.durability == 29, "Switch leaked wear");
                checks.Add("Current durability reads/writes the selected instance directly; same-ID swords and selection changes retain independent wear");

                state.SetSpirit(50, 500); Require(!state.TryToggle(500, false) && !state.Integrated && state.Spirit == 50 && first.durability == 13, "Strict integration threshold");
                state.SetSpirit(51, 500); Require(state.TryToggle(500, false) && state.Current == 1, "Integration fee");
                state.Current = 37; bag.TrySelect(1);
                Require(state.Spirit == 37 && first.durability == 13 && second.durability == 29, "Integrated wear changed ordinary sword");
                Require(state.TryToggle(500, false) && state.Current == 29 && state.Spirit == 37, "Release restored wrong sword");
                checks.Add("Integration requires strictly more than 10%, charges once, isolates spirit wear and releases to the currently selected ordinary instance");

                state.SetSpirit(0, 500); Require(state.TryToggle(500, true) && state.Current == 0, "Infinite integration at zero");
                state.RefillCurrent(500); Require(state.Current == 500 && second.durability == 29, "Integrated refill target");
                state.SetCurrent(900, 500); Require(state.Current == 500, "Spirit upper clamp");
                state.Spend(600); Require(state.Spirit == 0 && second.durability == 29, "Spirit floor");
                state.TryToggle(500, true); state.SetCurrent(-5, 500); Require(second.durability == 0, "Ordinary lower clamp");
                state.SetCurrent(1000, 500); Require(second.durability == second.MaximumDurability, "Ordinary upper clamp");
                checks.Add("Infinite mode permits zero-durability integration; refill, clamping and consumption affect only the active durability source");

                bag.TrySelect(0); state.Restore(123, true); state.BreakOrdinary();
                Require(first.durability == 0 && state.Current == 123 && second.durability == second.MaximumDurability, "Breaking ordinary during integration");
                state.Restore(77, false); state.Current = 11;
                var snapshot = bag.Capture(); bag.Restore(snapshot, 0);
                Require(state.Current == 11 && !ReferenceEquals(bag.Selected, first), "Restored bag retains obsolete object");
                checks.Add("Ordinary destruction preserves integrated spirit; replacing equipment from a save immediately changes the active source without stale references");

                bag[0].durability = 9; bag[1].durability = 7;
                int expected = (bag[0].MaximumDurability - 9 + bag[1].MaximumDurability - 7) / 10 + 1000;
                state.Restore(81, true); Require(state.QuoteRepair() == expected, "Repair quote totals");
                int money = expected - 1;
                Require(!state.TryRepair(ref money) && money == expected - 1 && bag[0].durability == 9 && bag[1].durability == 7 && state.Spirit == 81, "Failed repair mutated state");
                bag[0].durability = 1; money = expected;
                Require(state.TryRepair(ref money) && money == 0 && bag[0].durability == bag[0].MaximumDurability && bag[1].durability == bag[1].MaximumDurability && state.Spirit == 81, "Repair changed quote or spirit");
                checks.Add("Repair totals missing durability across all ordinary swords, rejects insufficient money atomically, accepts exact balance and charges the stored quote even after wear changes; spirit stays unchanged");

                state.Reset(); bag[0].durability = 0; money = 0;
                Require(!state.Integrated && state.Spirit == 0 && state.RepairCost == 0 && state.TryRepair(ref money) && money == 0 && bag[0].durability == bag[0].MaximumDurability, "Original quote-free x87/reset semantics");
                Require(state.QuoteRepair() == 0, "Full equipment repair quote");
                checks.Add("Reset clears mode, spirit and quote; original x87 without x89 still repairs at the initial zero quote, and full swords quote zero");
            }
            catch (Exception e) { error = e.ToString(); }
            finally { UnityEngine.Random.state = random; }
            Report("durability-modules", checks, error);
        }
        [MenuItem("古剑/验证耐久合体存档接入")]
        public static void RunIntegration()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var random = UnityEngine.Random.state; string save = game.saveFileName, error = null;
            float scale = Time.timeScale; var checks = new List<string>();
            try
            {
                Time.timeScale = 0; game.saveFileName = "validation-durability-integration.json";
                foreach (bool integrated in new[] { false, true })
                {
                    game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
                    game.Machine.Enqueue("200"); game.Machine.Tick();
                    foreach (var actor in game.Actors.Values) actor.locked = true;
                    game.Machine.Command(0x64, new object[] { 12 });
                    game.Machine.Command(0x53, new object[] { 3, 4, false });
                    game.Machine.Command(0x55, new object[] { 3 });
                    game.durability = 37; game.Weapons.SetSpirit(600, game.GetSpiritAttribute(1));
                    if (integrated) { game.Machine.Command(0x7F, new object[0]); game.durability = 211; }
                    int spirit = game.SpiritDurability;
                    game.Save();
                    game.durability = 0; game.Weapons.SetSpirit(0, game.GetSpiritAttribute(1));
                    game.Load(); game.Machine.Tick();
                    Require(!game.Machine.Running && game.LastError == null && game.Equipment[game.EquippedIndex].durability == 37 &&
                        game.durability == (integrated ? 211 : 37) && game.SpiritDurability == spirit && game.Weapons.Integrated == integrated, "Save-load durability/mode/ordinary instance");
                    Require(game.Actors[-1].actionBank == (integrated ? 27 : 0) && game.Actors[-2].hidden == integrated, "Save-load presentation mode");
                    if (integrated) { game.Machine.Command(0x7F, new object[0]); Require(game.durability == 37 && game.SpiritDurability == 211, "Post-load release lost ordinary durability"); }
                    checks.Add((integrated ? "Integrated" : "Ordinary") + " production Save/Load preserves selected sword durability37, spirit durability, mode/action bank/visibility and release mapping without cross-writing the saved ordinary sword");
                }
                game.Weapons.SetSpirit(600, game.GetSpiritAttribute(1)); game.Machine.Command(0x7F, new object[0]);
                game.durability = 123; game.Equipment[0].durability = 5;
                int quote = Convert.ToInt32(game.Machine.Command(0x89, new object[0])); game.money = quote;
                game.Machine.Command(0x87, new object[0]);
                Require(game.money == 0 && game.durability == 123 && game.SpiritDurability == 123, "VM repair altered spirit or charged wrong fee");
                foreach (var item in game.Equipment) Require(item.durability == item.MaximumDurability, "VM repair missed ordinary instance");
                checks.Add("Production x89/x87 repairs all ordinary instances during integration, charges exact quoted money and retains active spirit durability123");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.saveFileName = save; game.Begin(); game.Machine.Clear(); game.Machine.Command(0x69, new object[0]);
                Time.timeScale = scale; UnityEngine.Random.state = random;
            }
            Report("durability-module-integration", checks, error);
        }
    }
}
