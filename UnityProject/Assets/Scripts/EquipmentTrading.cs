using System.Collections.Generic;
using System.Linq;

namespace GuJian
{
    public sealed class EquipmentTradeQuote
    {
        internal EquipmentTrading Owner;
        internal bool Completed;
        public int Id { get; internal set; }
        public int Shop { get; internal set; }
        public int Price { get; internal set; }
        public bool Selling { get; internal set; }
        public EquipmentInstance Instance { get; internal set; }
    }

    public sealed class EquipmentTrading
    {
        private readonly EquipmentInventory inventory;
        private readonly Dictionary<int, EquipmentDefinition> definitions;
        public EquipmentTrading(EquipmentInventory inventory, IEnumerable<EquipmentDefinition> definitions)
        { this.inventory = inventory; this.definitions = definitions.ToDictionary(d => d.id); }
        public EquipmentTradeQuote Quote(int idOrIndex, bool selling, int shop, int level)
        {
            if (shop < 2) return null;
            EquipmentInstance instance = null;
            if (selling)
            {
                if (idOrIndex < 0 || idOrIndex >= inventory.Count) return null;
                instance = inventory[idOrIndex];
                if (ReferenceEquals(instance, inventory.Selected)) return null;
                idOrIndex = instance.id;
            }
            if (!definitions.TryGetValue(idOrIndex, out var definition) ||
                (!selling && (definition.grade >= 3 || definition.level > level))) return null;
            return new EquipmentTradeQuote { Owner = this, Id = idOrIndex, Shop = shop, Selling = selling,
                Instance = instance, Price = selling ? definition.sell : definition.buy };
        }
        public ItemTradeResult Confirm(EquipmentTradeQuote quote, int shop, int level, Wallet wallet)
        {
            if (quote == null || quote.Owner != this || quote.Completed) return ItemTradeResult.Invalid;
            quote.Completed = true;
            if (quote.Shop != shop || !definitions.TryGetValue(quote.Id, out var definition)) return ItemTradeResult.Invalid;
            if (quote.Selling)
            {
                if (quote.Instance == null || quote.Instance.id != quote.Id || definition.sell != quote.Price) return ItemTradeResult.Invalid;
                checked { int balance = wallet.Balance + quote.Price; }
                if (!inventory.TryRemove(quote.Instance)) return ItemTradeResult.Invalid;
                wallet.Credit(quote.Price);
            }
            else
            {
                if (definition.grade >= 3 || definition.level > level || definition.buy != quote.Price) return ItemTradeResult.Invalid;
                if (wallet.Balance <= quote.Price) return ItemTradeResult.InsufficientMoney;
                var instance = EquipmentInstance.Create(quote.Id);
                wallet.Debit(quote.Price, true); inventory.Add(instance);
            }
            return ItemTradeResult.Success;
        }
    }
}
