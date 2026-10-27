using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ArchitectureBoundaryValidation
    {
        private static void Require(bool value, string message)
        {
            if (!value) throw new Exception(message);
        }

        [MenuItem("古剑/验证钱包、奖励与武器交易模块")]
        public static void Run()
        {
            var checks = new List<string>();
            string error = null;
            try
            {
                var wallet = new Wallet();
                wallet.Restore(100);
                Require(wallet.Debit(100) && wallet.Balance == 0, "Exact-balance debit");
                Require(!wallet.Debit(1) && !wallet.Debit(-1) && !wallet.Credit(-1), "Invalid wallet mutation");
                wallet.Restore(int.MaxValue);
                bool overflow = false;
                try { wallet.Credit(1); } catch (OverflowException) { overflow = true; }
                Require(overflow && wallet.Balance == int.MaxValue, "Wallet overflow changed balance");
                var walletSnapshot = wallet.Capture(); wallet.Restore(0); wallet.Restore(walletSnapshot);
                Require(wallet.Balance == int.MaxValue, "Wallet snapshot");
                var walletJson = JsonUtility.ToJson(walletSnapshot);
                var walletRoundTrip = JsonUtility.FromJson<WalletSnapshot>(walletJson);
                Require(walletRoundTrip != null && walletRoundTrip.balance == int.MaxValue, "Wallet snapshot JSON round-trip");
                checks.Add("Wallet preserves exact-balance deductions, rejects negative mutations, keeps its balance on overflow, and serializes snapshots");

                var skillRuntime = new SkillRuntime(new WeaponDurability(new EquipmentInventory()));
                skillRuntime.SetEnergy(731);
                skillRuntime.SetSkillCooldown(9);
                skillRuntime.SetBreakCooldown(4);
                skillRuntime.RestoreArmor(12, 3);
                skillRuntime.SetRecovery(8, 2);
                skillRuntime.BeginSupport(14, 5, 260);
                var skillSnapshot = JsonUtility.FromJson<SkillRuntimeSnapshot>(JsonUtility.ToJson(skillRuntime.Capture()));
                Require(skillSnapshot != null && skillSnapshot.energy == 731 && skillSnapshot.skillCooldownTicks == 9 &&
                    skillSnapshot.breakCooldownTicks == 4 && skillSnapshot.superArmorTicks == 12 && skillSnapshot.superArmorElapsed == 3 &&
                    skillSnapshot.recoveryTicks == 8 && skillSnapshot.recoveryAction == 2 && skillSnapshot.pendingSkill == 14 &&
                    skillSnapshot.pendingVersion == 5 && skillSnapshot.pendingAmount == 260, "Skill runtime snapshot JSON round-trip");
                checks.Add("Skill resource, cooldown, armor, recovery, and pending support state survive JsonUtility round-trip");

                var actionInventory = new ItemInventory();
                actionInventory.Add(7, 205);
                var itemAction = new InventoryItemAction();
                Require(itemAction.Open(actionInventory, 7, 2) && itemAction.Choice == 0, "Open final inventory stack");
                itemAction.Select(1);
                var discarded = itemAction.Confirm(actionInventory, (id, stack) => false);
                Require(discarded.Kind == InventoryItemActionKind.Discarded && discarded.ItemId == 7 && discarded.Quantity == 7 &&
                    actionInventory.Count(7) == 198 && !itemAction.IsOpen, "Discard selected stack");
                Require(itemAction.Open(actionInventory, 7, 1), "Open item use confirmation");
                var used = itemAction.Confirm(actionInventory, (id, stack) => actionInventory.TryRemove(id, 1, stack));
                Require(used.Kind == InventoryItemActionKind.Used && actionInventory.Count(7) == 197, "Commit item use through production callback");
                Require(itemAction.Open(actionInventory, 7, 1), "Open stale stack confirmation");
                actionInventory.TryRemove(7, actionInventory.StackQuantity(7, 1), 1);
                bool useCallbackCalled = false;
                var stale = itemAction.Confirm(actionInventory, (id, stack) => { useCallbackCalled = true; return true; });
                Require(stale.Kind == InventoryItemActionKind.None && !useCallbackCalled && !itemAction.IsOpen, "Stale confirmation should not mutate inventory");
                actionInventory.Add(8, 1);
                Require(itemAction.Open(actionInventory, 8, 0), "Open rejected item confirmation");
                var rejected = itemAction.Confirm(actionInventory, (id, stack) => false);
                Require(rejected.Kind == InventoryItemActionKind.None && actionInventory.Count(8) == 1, "Rejected use consumed inventory");
                checks.Add("Inventory item confirmation owns selection and commit state; discard affects only the selected stack, use delegates to gameplay rules, and stale/rejected actions preserve inventory");

                int currentHp = 100, maximumHp = 100;
                var health = new CombatHealth(() => currentHp, value => currentHp = value, () => maximumHp);
                Require(health.Damage(70, true, false) == HealthOutcome.Special && currentHp == 30 && health.SpecialTriggered && !health.Dead,
                    "Special threshold transition");
                Require(health.Damage(20, true, false) == HealthOutcome.Alive && currentHp == 10, "Special callback repeated");
                Require(health.Damage(10, true, false) == HealthOutcome.Died && currentHp == 0 && health.Dead, "Death transition");
                Require(health.Damage(1, true, false) == HealthOutcome.AlreadyDead && currentHp == 0, "Repeated damage after death");
                health.Revive(); Require(!health.Dead && health.SpecialTriggered && currentHp == 0, "Revive changed HP or one-shot state");
                health.Restore(60); health.SetDeathState(true); health.Respawn();
                Require(!health.Dead && health.SpecialTriggered && currentHp == maximumHp, "Respawn cleared one-shot special state");
                checks.Add("CombatHealth distinguishes one-shot threshold, alive, death, repeated damage, state-only revive and full respawn; respawn preserves the consumed special callback");

                var saveObject = typeof(GuJianGame).GetMethod("HasSaveObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                Func<string, bool> hasWallet = json => (bool)saveObject.Invoke(null, new object[] { json, "wallet" });
                Require(hasWallet("{\"wallet\":{\"balance\":0}}") && hasWallet("{\n  \"wallet\" : \n {} }"), "Snapshot formatting or zero balance");
                Require(!hasWallet("{\"wallet\": null}") && !hasWallet("{}"), "Null or absent snapshot");
                Require(!hasWallet("{\"other\":{\"wallet\":{}},\"values\":[{\"wallet\":{}}]}") &&
                    !hasWallet("{\"text\":\"\\\"wallet\\\":{}\",\"escaped\\\"key\":1}"), "Nested or escaped snapshot key");
                Require(hasWallet("{\"text\":\"{}[]\\\"\",\"wallet\":{}}"), "String content changed snapshot depth");
                checks.Add("Optional save snapshots recognize only top-level object values; absent/null, nested names and escaped string content cannot shadow legacy fields");

                wallet.Restore(10);
                var items = new ItemInventory();
                var equipment = new EquipmentInventory();
                equipment.Reset(EquipmentInstance.Create(0));
                var rewards = new RewardService(wallet, items, equipment);
                rewards.Grant(5, new[] { new ItemReward(1, 2), new ItemReward(1, 3) });
                Require(wallet.Balance == 15 && items.Count(1) == 5, "Combined reward grant");
                bool invalidReward = false;
                try { rewards.Grant(1, new[] { new ItemReward(1, 1), new ItemReward(2, -1) }); }
                catch (ArgumentOutOfRangeException) { invalidReward = true; }
                Require(invalidReward && wallet.Balance == 15 && items.Count(1) == 5 && items.Count(2) == 0, "Invalid batch reward was not atomic");
                wallet.Restore(int.MaxValue);
                bool rewardOverflow = false;
                try { rewards.Grant(1, new[] { new ItemReward(3, 1) }); } catch (OverflowException) { rewardOverflow = true; }
                Require(rewardOverflow && wallet.Balance == int.MaxValue && items.Count(3) == 0, "Overflow reward was not atomic");
                wallet.Restore(15); items.Add(4, int.MaxValue);
                bool itemOverflow = false;
                try { rewards.Grant(1, new[] { new ItemReward(1, 1), new ItemReward(4, 1) }); } catch (OverflowException) { itemOverflow = true; }
                Require(itemOverflow && wallet.Balance == 15 && items.Count(1) == 5 && items.Count(4) == int.MaxValue, "Item overflow reward was not atomic");
                var duplicate = EquipmentInstance.Create(0);
                bool duplicateRejected = false;
                try { rewards.Grant(1, new[] { new ItemReward(1, 1) }, new[] { duplicate, duplicate }); } catch (ArgumentException) { duplicateRejected = true; }
                Require(duplicateRejected && wallet.Balance == 15 && items.Count(1) == 5 && equipment.Count == 1, "Duplicate sword reward was not atomic");
                checks.Add("Combined currency/item awards commit together; an invalid later item or balance overflow leaves every reward unchanged");

                wallet.Restore(300000);
                var definitions = OriginalAssets.Table<EquipmentDefinition>("equip");
                var trading = new EquipmentTrading(equipment, definitions);
                var quote = trading.Quote(3, false, 2, 99);
                Require(quote != null && trading.Confirm(quote, 2, 99, wallet) == ItemTradeResult.Success && equipment.Count == 2 && wallet.Balance == 300000 - quote.Price, "Weapon purchase");
                Require(trading.Confirm(quote, 2, 99, wallet) == ItemTradeResult.Invalid, "Weapon quote replay");
                equipment.TrySelect(1);
                var sale = trading.Quote(3, true, 2, 99);
                Require(sale == null, "Equipped weapon sale accepted");
                var equippedId = equipment.Selected.id;
                Require(equippedId == 3, "Purchase selection changed unexpectedly");
                checks.Add("Weapon transactions recheck and consume quotes once; purchase charges exact catalog price, while the selected sword cannot be sold");
            }
            catch (Exception e) { error = e.ToString(); }

            var report = new ReconstructionValidation.Report
            {
                passed = error == null,
                utc = DateTime.UtcNow.ToString("O"),
                checks = checks.ToArray(),
                error = error
            };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/architecture-boundaries.json", JsonUtility.ToJson(report, true));
            if (!report.passed) throw new Exception(report.error);
        }
    }
}
