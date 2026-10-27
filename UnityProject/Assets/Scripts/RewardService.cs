using System;
using System.Collections.Generic;

namespace GuJian
{
    public struct ItemReward
    {
        public readonly int Id, Quantity;
        public ItemReward(int id, int quantity) { Id = id; Quantity = quantity; }
    }

    // Validate a whole reward before modifying any of its currency, items or swords.
    public sealed class RewardService
    {
        private readonly Wallet wallet;
        private readonly ItemInventory inventory;
        private readonly EquipmentInventory equipment;
        public RewardService(Wallet wallet, ItemInventory inventory, EquipmentInventory equipment)
        { this.wallet = wallet; this.inventory = inventory; this.equipment = equipment; }

        public void Grant(int money = 0, ItemReward[] items = null, EquipmentInstance[] swords = null)
        {
            if (money < 0) throw new ArgumentOutOfRangeException(nameof(money));
            checked { int balance = wallet.Balance + money; }
            var totals = new Dictionary<int, int>();
            if (items != null) foreach (var item in items)
            {
                if (item.Quantity < 0) throw new ArgumentOutOfRangeException(nameof(items));
                totals.TryGetValue(item.Id, out var current);
                totals[item.Id] = checked(current + item.Quantity);
            }
            foreach (var item in totals) checked { int count = inventory.Count(item.Key) + item.Value; }
            var identities = new HashSet<EquipmentInstance>();
            if (swords != null) foreach (var sword in swords)
                if (sword == null || equipment.IndexOf(sword) >= 0 || !identities.Add(sword))
                    throw new ArgumentException("Reward swords must be distinct new instances", nameof(swords));
            wallet.Credit(money);
            foreach (var item in totals) inventory.Add(item.Key, item.Value);
            if (swords != null) foreach (var sword in swords) equipment.Add(sword);
        }
    }
}
