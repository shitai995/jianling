using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class EquipmentModuleValidation
    {
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Reject(Action action)
        {
            try { action(); } catch (ArgumentException) { return; }
            throw new Exception("Invalid equipment operation was accepted");
        }
        private static void Report(string name, List<string> checks, string error)
        {
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/" + name + ".json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_" + name + " " + report.passed + " " + error);
        }
        [MenuItem("古剑/验证装备实例模块")]
        public static void Run()
        {
            var checks = new List<string>(); string error = null;
            var random = UnityEngine.Random.state;
            try
            {
                var bag = new EquipmentInventory(); var first = EquipmentInstance.Create(0);
                bag.Reset(first); var sameId = EquipmentInstance.Create(0); bag.Add(sameId);
                Require(bag.Count == 2 && !ReferenceEquals(first, sameId) && bag.SelectedIndex == 0 && ReferenceEquals(bag.Selected, first), "Duplicate definition identity");
                Require(!bag.TrySelect(-1) && !bag.TrySelect(2) && ReferenceEquals(bag.Selected, first), "Invalid selection changed active object");
                Reject(() => bag.Add(first)); Reject(() => bag.Add(null));
                Require(bag.Count == 2 && !((object)bag is IList<EquipmentInstance>), "Bag membership exposed mutable list");
                checks.Add("Same-ID swords remain independent; invalid slots and duplicate/null objects cannot change membership or selection; bag exposes no writable list");

                var outsider = EquipmentInstance.Create(0);
                Require(bag.SetDurability(first, 17) && first.durability == 17 && !bag.SetDurability(outsider, 9) && outsider.durability == outsider.MaximumDurability,
                    "Durability writes accepted a foreign instance");
                Require(bag.SetSkill(first, 5, 3) && first.skillId == 5 && first.skillLevel == 3 && !bag.SetSkill(outsider, 6, 1),
                    "Skill writes accepted a foreign instance");
                Require(bag.RefillDurability(first) && first.durability == first.MaximumDurability && !bag.RefillDurability(outsider),
                    "Single-instance refill escaped inventory ownership");
                bag.SetDurability(first, 10); bag.SetDurability(sameId, 12); bag.RefillAllDurability();
                Require(first.durability == first.MaximumDurability && sameId.durability == sameId.MaximumDurability,
                    "Full refill missed an owned instance");
                checks.Add("Durability, refill and skill mutations accept only owned equipment; single and full refills target the intended instances");

                Require(bag.TrySelect(1) && !bag.TryRemove(sameId) && bag.TryRemove(first), "Equipped removal protection");
                Require(bag.SelectedIndex == 0 && ReferenceEquals(bag.Selected, sameId) && !bag.TryRemove(first), "Earlier-slot removal changed identity or replayed");
                var replacement = EquipmentInstance.Create(0); bag.Add(replacement);
                Require(!bag.TryRemove(first) && ReferenceEquals(bag[1], replacement), "Stale object consumed replacement");
                checks.Add("Sales/feed removal rejects the equipped object; removing earlier slots preserves selected identity; stale/repeated removals cannot consume a replacement");

                bag.Reset(EquipmentInstance.Create(0)); var weapon = EquipmentInstance.Create(3); bag.Add(weapon); bag.TrySelect(1);
                var fallback = bag[0]; int created = 0;
                Require(bag.RemoveFirst(3, () => { created++; return EquipmentInstance.Create(0); }) && ReferenceEquals(bag.Selected, fallback) && created == 0 && bag.Count == 1, "Existing fallback not reused");
                Require(!bag.RemoveFirst(999, () => { created++; return EquipmentInstance.Create(0); }) && created == 0, "Missing script removal created sword");
                Require(bag.RemoveFirst(0, () => { created++; return EquipmentInstance.Create(0); }) && created == 1 && bag.Count == 1 && bag.Selected.id == 0 && !ReferenceEquals(bag.Selected, fallback), "Active starter removal fallback");
                checks.Add("Script removal reuses the original first ID-0 fallback; missing IDs do nothing; removing the active starter creates exactly one replacement");

                bag.Reset(EquipmentInstance.Create(0)); bag.Add(EquipmentInstance.Create(0)); first = bag[0]; sameId = bag[1];
                Require(bag.RemoveFirst(0, () => EquipmentInstance.Create(0)) && bag.Count == 2 && ReferenceEquals(bag[0], sameId) && !ReferenceEquals(bag.Selected, sameId) && bag.SelectedIndex == 1, "Original first-ID fallback semantics");
                var selected = bag.Selected;
                Require(bag.RemoveFirst(0, () => { throw new Exception("Unexpected fallback creation"); }) && ReferenceEquals(bag.Selected, selected) && bag.SelectedIndex == 0, "Removing unequipped starter changed identity");
                checks.Add("First-ID removal retains original duplicate-starter fallback semantics; removing an earlier unequipped starter never creates or selects a replacement");

                var source = new[] { new EquipmentInstance { id = 3, reinforcement = 4, durability = 37, skillId = 5, skillLevel = 7 }, EquipmentInstance.Create(4) };
                bag.Restore(source, 0); source[0].reinforcement = 0;
                var captured = bag.Capture(); captured[0].durability = 0;
                Require(bag[0].reinforcement == 4 && bag[0].durability == 37 && bag[0].skillId == 5 && bag[0].skillLevel == 7 && !ReferenceEquals(source[0], bag[0]), "Equipment snapshot aliased fields");
                selected = bag.Selected;
                Reject(() => bag.Restore(null, 0)); Reject(() => bag.Restore(new EquipmentInstance[0], 0));
                Reject(() => bag.Restore(new EquipmentInstance[] { source[0], null }, 0));
                Require(bag.Count == 2 && ReferenceEquals(bag.Selected, selected), "Invalid restore partially changed bag");
                checks.Add("Capture/Restore detach mutable reinforcement, durability and skill fields; null/empty/partial snapshots are rejected before live state changes");

                var rng = UnityEngine.Random.state; int expected = UnityEngine.Random.Range(0, int.MaxValue); UnityEngine.Random.state = rng;
                bag.Restore(bag.Capture(), int.MaxValue);
                Require(bag.SelectedIndex == 1 && UnityEngine.Random.Range(0, int.MaxValue) == expected, "Restore clamping or random reroll");
                bag.Restore(bag.Capture(), -1); Require(bag.SelectedIndex == 0, "Negative legacy selected slot");
                checks.Add("Restore clamps legacy selected indices without rerolling random skills or advancing Unity random state");

                var reinforced = EquipmentInstance.Create(3, 4); var duplicate = EquipmentInstance.Create(3);
                bag.Reset(reinforced); bag.Add(duplicate); int money = reinforced.ReinforceCost;
                int skill = reinforced.skillLevel, oldMaximum = reinforced.MaximumDurability, cost = reinforced.ReinforceCost;
                Require(bag.Reinforce(reinforced, ref money, 37) == EquipmentReinforceResult.Success && money == 0 && reinforced.reinforcement == 5 && reinforced.durability == reinforced.MaximumDurability * 37 / oldMaximum && reinforced.skillLevel == Math.Min(9, skill + 1) && duplicate.reinforcement == 0, "Selected reinforcement ratio/skill/cost");
                money = cost - 1; int durability = reinforced.durability;
                Require(bag.Reinforce(reinforced, ref money, 1) == EquipmentReinforceResult.InsufficientMoney && reinforced.reinforcement == 5 && reinforced.durability == durability && money == cost - 1, "Failed reinforcement changed fields");
                checks.Add("Reinforcement accepts exact balance, uses selected live durability ratio, charges once and upgrades skill at +5; insufficient money changes neither instance nor balance");

                money = 100000; duplicate.durability = 13;
                Require(bag.Reinforce(duplicate, ref money, 1) == EquipmentReinforceResult.Success && duplicate.durability == duplicate.MaximumDurability * 13 / duplicate.Data.durability, "Unequipped reinforcement used active durability");
                while (reinforced.reinforcement < 9) Require(bag.Reinforce(reinforced, ref money) == EquipmentReinforceResult.Success, "Premature cap");
                int beforeMoney = money;
                Require(reinforced.Attack == reinforced.Data.attack * 2 && reinforced.skillLevel == Math.Min(9, skill + 2) && bag.Reinforce(reinforced, ref money) == EquipmentReinforceResult.AtLimit && money == beforeMoney, "Level 9 cap/stat/skill");
                var unavailable = EquipmentInstance.Create(26); bag.Add(unavailable);
                Require(bag.Reinforce(unavailable, ref money) == EquipmentReinforceResult.AtLimit && bag.Reinforce(EquipmentInstance.Create(3), ref money) == EquipmentReinforceResult.Invalid && money == beforeMoney, "Restricted or foreign reinforcement");
                checks.Add("Unequipped reinforcement preserves its own durability ratio; +9 doubles base stats, grants original skill step, and cap/restricted/foreign instances never charge");
            }
            catch (Exception e) { error = e.ToString(); }
            finally { UnityEngine.Random.state = random; }
            Report("equipment-modules", checks, error);
        }
        [MenuItem("古剑/验证装备剧情与存档接入")]
        public static void RunIntegration()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var loader = game.Machine.LoadScript; var random = UnityEngine.Random.state;
            string save = game.saveFileName, error = null; float scale = Time.timeScale;
            var checks = new List<string>();
            try
            {
                Time.timeScale = 0; game.saveFileName = "validation-equipment-integration.json";
                game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
                game.Machine.Enqueue("200"); game.Machine.Tick();
                foreach (var actor in game.Actors.Values) actor.locked = true;
                game.Machine.LoadScript = name => name == "equipment-integration" ? new[] {
                    "x53(3,4,false)", "x53(3,0,false)", "x55(3)"
                } : loader(name);
                game.Machine.Enqueue("equipment-integration"); game.Machine.Tick();
                Require(!game.Machine.Running && game.Machine.Error == null && game.Equipment.Count == 3 && game.EquippedIndex == 2 && game.equipmentId == 3, "VM duplicate award/equip");
                var equipped = game.Equipment[2]; var earlier = game.Equipment[1]; game.durability = 37;
                int previousSkill = earlier.skillLevel; game.money = 200000; int beforeMoney = game.money, maximum = equipped.MaximumDurability;
                Require(game.ReinforceEquipment(2) && game.durability == equipped.MaximumDurability * 37 / maximum && game.money == beforeMoney - equipped.ReinforceCost && earlier.reinforcement == 4, "Live selected reinforcement");
                Require(game.ReinforceEquipment(1) && earlier.reinforcement == 5 && earlier.skillLevel == Math.Min(9, previousSkill + 1), "Duplicate reinforcement target");
                checks.Add("Production VM awards duplicate instances and x55 selects the last; equipped reinforcement syncs live durability and fee without modifying its same-ID sibling");

                int retainedDurability = game.durability;
                game.Machine.Command(0x54, new object[] { 3 });
                Require(game.Equipment.Count == 2 && game.EquippedIndex == 1 && ReferenceEquals(game.Equipment[1], equipped) && game.durability == retainedDurability, "Script removal of earlier duplicate");
                game.Machine.Command(0x56, new object[0]);
                game.Save(); var snapshot = game.EquipmentBag.Capture();
                equipped.reinforcement = 0; equipped.skillLevel = 1; game.durability = 1; game.money = 0;
                game.Load(); game.Machine.Tick();
                Require(game.EquippedIndex == 1 && game.Equipment.Count == 2 && game.Equipment[1].reinforcement == snapshot[1].reinforcement && game.Equipment[1].skillLevel == snapshot[1].skillLevel && game.durability == retainedDurability && !game.Actors[-1].WeaponVisible && game.money > 0, "Equipment save restoration");
                checks.Add("Removing the earlier same-ID instance preserves equipped identity and live durability; version-3 Save/Load restores slot, reinforcement, skill, durability, money and stowed state");

                game.Machine.Command(0x54, new object[] { 3 });
                Require(game.Equipment.Count == 1 && game.EquippedIndex == 0 && game.equipmentId == 0 && !game.Actors[-1].WeaponVisible, "Equipped script removal fallback/visibility");
                var previous = game.Equipment[0]; game.Machine.Command(0x54, new object[] { 0 });
                Require(game.Equipment.Count == 1 && game.Equipment[0].id == 0 && !ReferenceEquals(previous, game.Equipment[0]) && game.equipmentId == 0, "Starter script replacement");
                checks.Add("Production x54 removes the active sword using ID-0 fallback, preserves stowed visibility, and replaces an explicitly removed starter sword");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.saveFileName = save; game.Machine.LoadScript = loader;
                game.Begin(); game.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                Time.timeScale = scale; UnityEngine.Random.state = random;
            }
            Report("equipment-module-integration", checks, error);
        }
    }
}
