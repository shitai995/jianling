using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private bool shopSelling;
        private int shopSelection;
        private int shopBuySelection, shopSellSelection;
        private int tradeRepeatDirection;
        private float tradeRepeatAt;
        public int ShopSelectedItemId => shopType < 2 ? OriginalAssets.Table<ItemDefinition>("item").Where(i => ItemForSale(i.id)).Select(i => i.id).ElementAtOrDefault(shopSelection) : -1;
        public void MoveShopSelection(int delta)
        {
            if (!ShopOpen || TradeConfirming || WeaponPreviewVisible) return;
            int count = OriginalAssets.Table<ItemDefinition>("item").Count(i => ItemForSale(i.id));
            if (count == 0) return;
            shopSelection = (shopSelection + delta % count + count) % count;
            menuScroll.y = Mathf.Max(0, 42 + shopSelection * 45 - 110);
        }
        private sealed class Trade
        {
            public ItemTradeQuote item;
            public int weaponId, weaponPrice, weaponShop, choice;
            public bool weaponSelling, weapon;
            public EquipmentInstance instance;
            public int id => item == null ? weaponId : item.Id;
            public int quantity => item == null ? 1 : item.Quantity;
            public int unitPrice => item == null ? weaponPrice : item.UnitPrice;
            public int shop => item == null ? weaponShop : item.Shop;
            public bool selling => item == null ? weaponSelling : item.Selling;
        }
        private Trade trade;
        public bool TradeConfirming => trade != null;
        public int TradeQuantity => trade == null ? 0 : trade.quantity;
        public int TradePrice => trade == null ? 0 : trade.unitPrice * trade.quantity;

        private void OpenShop(int type)
        {
            CloseWeaponPreview(); systemShopReturn = false;
            shopType = type; shopSelling = false; shopSelection = 0;
            shopBuySelection = shopSellSelection = 0;
            scriptedMenu = Machine.CurrentScript == "guide3"; CancelTrade(); CancelSpiritFeed();
            menuPage = 5; menu = true; menuScroll = Vector2.zero;
        }
        public void CancelTrade() { trade = null; }
        public int ItemPurchasePrice(int id) => ItemTrading.PurchasePrice(id, shopType);
        private bool ItemForSale(int id) => ItemTrading.ForSale(id, SpiritLevel);
        private bool ShopOpen => menu && menuPage == 5;
        public bool SelectItemTrade(int id, bool selling, int stack = -1)
        {
            if (!ShopOpen || WeaponPreviewVisible || shopType >= 2) return false;
            var quote = ItemTrading.Quote(id, selling, stack, shopType, SpiritLevel);
            if (quote == null) return false;
            trade = new Trade { item = quote };
            return true;
        }
        // For sales the argument is a bag index; for purchases it is a definition id.
        public bool SelectWeaponTrade(int indexOrId, bool selling)
        {
            if (!ShopOpen || WeaponPreviewVisible || shopType < 2) return false;
            EquipmentInstance instance = null;
            EquipmentDefinition item;
            if (selling)
            {
                if (indexOrId < 0 || indexOrId >= equipment.Count || indexOrId == equippedIndex) return false;
                instance = equipment[indexOrId]; item = instance.Data;
            }
            else
            {
                item = OriginalAssets.Table<EquipmentDefinition>("equip").FirstOrDefault(e => e.id == indexOrId);
                if (item == null || item.grade >= 3 || item.level > level) return false;
            }
            trade = new Trade { weaponId = item.id, weaponShop = shopType, weapon = true, weaponSelling = selling, instance = instance, weaponPrice = selling ? item.sell : item.buy };
            return true;
        }
        public void SetTradeQuantity(int quantity)
        {
            if (trade != null && !trade.weapon) ItemTrading.SetQuantity(trade.item, quantity);
        }
        private void MoveTradeQuantity(int delta)
        {
            if (trade != null && !trade.weapon) ItemTrading.MoveQuantity(trade.item, delta);
        }
        private void TickTradeQuantityRepeat()
        {
            if (!TradeConfirming || trade.weapon || (!trade.selling && trade.id >= 16))
            {
                tradeRepeatDirection = 0;
                return;
            }
            bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            int direction = up == down ? 0 : up ? -1 : 1;
            if (direction == 0) { tradeRepeatDirection = 0; return; }
            if (direction != tradeRepeatDirection)
            {
                tradeRepeatDirection = direction;
                tradeRepeatAt = Time.unscaledTime + .35f;
                return;
            }
            if (Time.unscaledTime < tradeRepeatAt) return;
            MoveTradeQuantity(direction);
            tradeRepeatAt = Time.unscaledTime + .08f;
        }
        public bool ConfirmTrade()
        {
            var t = trade; trade = null;
            if (t == null || !ShopOpen || WeaponPreviewVisible || t.shop != shopType) return false;
            int price = t.unitPrice * t.quantity;
            if (!t.weapon)
            {
                var result = ItemTrading.Confirm(t.item, shopType, SpiritLevel, ref money, ApplyItem);
                if (result == ItemTradeResult.InsufficientMoney) Toast("银两不足");
                else if (result == ItemTradeResult.CannotApply) Toast("当前无法使用该物品");
                else if (result == ItemTradeResult.Success) Toast(t.selling ? "出售成功，获得 " + price + " 两" : "购买成功，花费 " + price + " 两");
                return result == ItemTradeResult.Success;
            }
            // Equipment still owns its instance identity and equipped-slot rules.
            if (t.selling)
            {
                int index = equipment.IndexOf(t.instance);
                if (index < 0 || index == equippedIndex) return false;
                equipment.RemoveAt(index); if (index < equippedIndex) equippedIndex--;
                money += price; Toast("出售成功，获得 " + price + " 两"); return true;
            }
            if (money <= price) { Toast("银两不足"); return false; }
            var item = OriginalAssets.Table<EquipmentDefinition>("equip").First(e => e.id == t.id);
            if (item.grade >= 3 || item.level > level || item.buy != t.unitPrice) return false;
            equipment.Add(EquipmentInstance.Create(t.id));
            money -= price; Toast("购买成功，花费 " + price + " 两"); return true;
        }

        private float ShopHeight()
        {
            int count = shopType < 2 ? (shopSelling ? ShopItemRows().Length : OriginalAssets.Table<ItemDefinition>("item").Count(i => ItemForSale(i.id)))
                : shopSelling ? equipment.Count - 1 : OriginalAssets.Table<EquipmentDefinition>("equip").Count(e => e.grade < 3 && e.level <= level);
            return Mathf.Max(230, 42 + count * 45);
        }
        private void DrawShop()
        {
            if (GUI.Button(new Rect(5, 0, 95, 30), shopSelling ? "切换购买" : "切换出售", buttonStyle)) { shopSelling = !shopSelling; menuScroll = Vector2.zero; }
            GUI.Label(new Rect(115, 2, 315, 26), (shopSelling ? "出售" : "购买") + "    银两 " + money, bodyStyle);
            int row = 0;
            if (shopType < 2)
            {
                var rows = shopSelling ? ShopItemRows() : OriginalAssets.Table<ItemDefinition>("item").Where(i => ItemForSale(i.id)).Select(i => new[] { i.id, -1 }).ToArray();
                foreach (var rowData in rows)
                {
                    var item = OriginalAssets.Table<ItemDefinition>("item").First(i => i.id == rowData[0]);
                    float y = 42 + row++ * 45;
                    if (!shopSelling && item.id == ShopSelectedItemId) GUI.Box(new Rect(0, y - 2, 430, 36), "");
                    DrawItemIcon(item.id, new Rect(5, y + 4, 22, 22));
                    int quantity = shopSelling ? ItemStackQuantity(item.id, rowData[1]) : 0;
                    GUI.Label(new Rect(32, y, 295, 34), item.name + (shopSelling ? " ×" + quantity : "") + "  " + (shopSelling ? item.sell : ItemPurchasePrice(item.id)) + " 两", bodyStyle);
                    if (GUI.Button(new Rect(332, y, 95, 30), shopSelling ? "出售" : "购买", buttonStyle)) SelectItemTrade(item.id, shopSelling, rowData[1]);
                }
            }
            else if (shopSelling)
            {
                for (int i = 0; i < equipment.Count; i++)
                {
                    if (i == equippedIndex) continue;
                    var item = equipment[i]; float y = 42 + row++ * 45;
                    if (!shopSelling && item.id == ShopSelectedItemId) GUI.Box(new Rect(0, y - 2, 430, 36), "");
                    DrawItemIcon(item.id, new Rect(5, y + 4, 22, 22));
                    GUI.Label(new Rect(32, y, 295, 34), item.Data.name + " +" + item.reinforcement + "  " + item.Data.sell + " 两", bodyStyle);
                    if (GUI.Button(new Rect(332, y, 95, 30), "出售", buttonStyle)) SelectWeaponTrade(i, true);
                }
            }
            else foreach (var item in OriginalAssets.Table<EquipmentDefinition>("equip").Where(e => e.grade < 3 && e.level <= level))
            {
                float y = 42 + row++ * 45;
                GUI.Label(new Rect(5, y, 255, 34), item.name + "  " + item.buy + " 两", bodyStyle);
                if (GUI.Button(new Rect(263, y, 76, 30), "试穿", buttonStyle)) PreviewShopWeapon(item.id);
                if (GUI.Button(new Rect(346, y, 81, 30), "购买", buttonStyle)) SelectWeaponTrade(item.id, false);
            }
            if (row == 0) GUI.Label(new Rect(5, 50, 420, 40), "没有可出售的物品。", bodyStyle);
        }
        private int[][] ShopItemRows() => Inventory.StackRows();
        private void DrawTradeConfirmation()
        {
            var t = trade; var color = GUI.color;
            GUI.color = new Color(.06f, .07f, .10f, 1);
            GUI.DrawTexture(new Rect(65, 67, 403, 220), Texture2D.whiteTexture); GUI.color = color;
            GUI.Box(new Rect(65, 67, 403, 220), "");
            string name, description;
            if (t.weapon)
            {
                var item = OriginalAssets.Table<EquipmentDefinition>("equip").First(e => e.id == t.id);
                name = item.name + (t.instance == null ? "" : " +" + t.instance.reinforcement); description = Clean(item.description);
            }
            else
            {
                var item = OriginalAssets.Table<ItemDefinition>("item").First(i => i.id == t.id);
                name = item.name; description = Clean(item.description);
            }
            GUI.Label(new Rect(80, 77, 370, 28), (t.selling ? "出售 " : "购买 ") + name, titleStyle);
            GUI.Label(new Rect(80, 110, 370, 59), description, bodyStyle);
            if (GUI.Button(new Rect(80, 173, 42, 30), "−", buttonStyle)) SetTradeQuantity(t.quantity - 1);
            GUI.Label(new Rect(135, 176, 90, 28), "数量 " + t.quantity, bodyStyle);
            if (GUI.Button(new Rect(226, 173, 42, 30), "+", buttonStyle)) SetTradeQuantity(t.quantity + 1);
            GUI.Label(new Rect(280, 176, 170, 28), "合计 " + TradePrice + " 两", bodyStyle);
            GUI.Label(new Rect(80, 208, 370, 29), !t.selling && t.id >= 11 && t.id <= 22 && !t.weapon ? "购买后立即生效" : "确认后完成交易", bodyStyle);
            if (GUI.Button(new Rect(94, 244, 145, 30), "确认", buttonStyle)) ConfirmTrade();
            if (GUI.Button(new Rect(292, 244, 145, 30), "取消", buttonStyle)) CancelTrade();
        }
    }
}
