using System;
using System.Collections.Generic;
using System.Linq;

namespace GuJian
{
    public enum ItemTradeResult { Success, Invalid, InsufficientMoney, CannotApply }

    public sealed class ItemTradeQuote
    {
        internal readonly ItemTrading Owner;
        internal bool Completed;
        public int Id { get; }
        public int Stack { get; }
        public int Shop { get; }
        public bool Selling { get; }
        public int UnitPrice { get; }
        public int Quantity { get; internal set; } = 1;
        public int TotalPrice => checked(UnitPrice * Quantity);
        internal ItemTradeQuote(ItemTrading owner, int id, int stack, int shop, bool selling, int price)
        { Owner = owner; Id = id; Stack = stack; Shop = shop; Selling = selling; UnitPrice = price; }
    }

    // Item commerce rules are independent of the menu, scene, VM and save file format.
    public sealed class ItemTrading
    {
        private readonly ItemInventory inventory;
        private readonly Dictionary<int, ItemDefinition> items;
        private readonly int[] specialPurchases = new int[7];
        public ItemTrading(ItemInventory inventory, IEnumerable<ItemDefinition> items)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            this.items = items.ToDictionary(i => i.id);
        }
        public bool ForSale(int id, int spiritLevel, int shop = 0)
        {
            if (!items.TryGetValue(id, out var item)) return false;
            int mask = item.customRules ? item.shopMask : id >= 0 && id < 24 && id != 14 && id != 15 ? 3 : 0;
            return shop >= 0 && shop < 2 && (mask & (1 << shop)) != 0 && !(item.EffectKind == ItemEffectKind.SwordSpiritExperience && spiritLevel >= 40);
        }
        public int PurchasePrice(int id, int shop)
        {
            if (!items.TryGetValue(id, out var item)) return 0;
            int price = item.buy;
            if (item.PurchaseGroup >= 0 && item.PurchaseGroup < specialPurchases.Length)
                price = Math.Min(item.customRules ? item.priceCap : 20000,
                    price + specialPurchases[item.PurchaseGroup] * (item.customRules ? Math.Max(0, item.priceIncreaseStep) : 1000));
            
            return shop == 0 ? price * 2 : price;
        }
        public ItemTradeQuote Quote(int id, bool selling, int stack, int shop, int spiritLevel)
        {
            if (shop < 0 || shop >= 2 || !items.TryGetValue(id, out var item)) return null;
            if (selling && stack < 0) stack = 0;
            if (selling ? inventory.StackQuantity(id, stack) <= 0 : !ForSale(id, spiritLevel, shop)) return null;
            return new ItemTradeQuote(this, id, selling ? stack : -1, shop, selling, selling ? item.sell : PurchasePrice(id, shop));
        }
        private int Maximum(ItemTradeQuote quote) => quote.Selling ? inventory.StackQuantity(quote.Id, quote.Stack) :
            items[quote.Id].customRules ? Math.Max(1, items[quote.Id].maxPurchaseQuantity) : quote.Id >= 16 ? 1 : ItemInventory.StackLimit;
        public int MaximumQuantity(ItemTradeQuote quote) => quote == null || quote.Owner != this ? 0 : Maximum(quote);
        public void SetQuantity(ItemTradeQuote quote, int quantity)
        {
            if (quote == null || quote.Owner != this || quote.Completed) return;
            quote.Quantity = Math.Max(1, Math.Min(Math.Max(1, Maximum(quote)), quantity));
        }
        public void MoveQuantity(ItemTradeQuote quote, int delta)
        {
            if (quote == null || quote.Owner != this || quote.Completed) return;
            int maximum = Math.Max(1, Maximum(quote));
            quote.Quantity = (int)((((long)quote.Quantity - 1 + delta) % maximum + maximum) % maximum) + 1;
        }
        // applyItem synchronously applies an immediate-use item; false must leave gameplay state unchanged.
        public ItemTradeResult Confirm(ItemTradeQuote quote, int shop, int spiritLevel, ref int money, Func<int, int, bool> applyItem)
        {
            var wallet = new Wallet(); wallet.Restore(money);
            var result = Confirm(quote, shop, spiritLevel, wallet, applyItem);
            money = wallet.Balance; return result;
        }
        public ItemTradeResult Confirm(ItemTradeQuote quote, int shop, int spiritLevel, Wallet wallet, Func<int, int, bool> applyItem)
        {
            if (quote == null || quote.Owner != this || quote.Completed) return ItemTradeResult.Invalid;
            quote.Completed = true;
            if (quote.Shop != shop || quote.Quantity < 1 || quote.Quantity > Maximum(quote)) return ItemTradeResult.Invalid;
            int price = quote.TotalPrice;
            if (quote.Selling)
            {
                checked { int balance = wallet.Balance + price; }
                if (!inventory.TryRemove(quote.Id, quote.Quantity, quote.Stack)) return ItemTradeResult.Invalid;
                wallet.Credit(price);
                return ItemTradeResult.Success;
            }
            // Original shops require a balance strictly greater than the total.
            if (wallet.Balance <= price) return ItemTradeResult.InsufficientMoney;
            if (!ForSale(quote.Id, spiritLevel, shop) || PurchasePrice(quote.Id, shop) != quote.UnitPrice) return ItemTradeResult.Invalid;
            bool immediate = items[quote.Id].ImmediateUseInShop;
            if (immediate)
            {
                if (applyItem == null || !applyItem(quote.Id, quote.Quantity)) return ItemTradeResult.CannotApply;
                int group = items[quote.Id].PurchaseGroup;
                
                if (group >= 0 && group < specialPurchases.Length) specialPurchases[group] = Math.Min(19, specialPurchases[group] + 1);
            }
            else inventory.Add(quote.Id, quote.Quantity);
            wallet.Debit(price, true);
            return ItemTradeResult.Success;
        }
        public int[] CapturePurchaseCounts() => (int[])specialPurchases.Clone();
        public void RestorePurchaseCounts(int[] counts)
        {
            Array.Clear(specialPurchases, 0, specialPurchases.Length);
            if (counts != null) for (int i = 0; i < Math.Min(counts.Length, specialPurchases.Length); i++) specialPurchases[i] = Math.Max(0, Math.Min(19, counts[i]));
        }
    }
}
