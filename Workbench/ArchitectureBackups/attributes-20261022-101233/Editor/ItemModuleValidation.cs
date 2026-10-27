using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ItemModuleValidation
    {
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Reject(Action action)
        {
            try { action(); } catch (ArgumentException) { return; }
            throw new Exception("Invalid inventory snapshot was accepted");
        }
        [MenuItem("古剑/验证背包剧情与存档接入")]
        public static void RunIntegration()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            string save = game.saveFileName, error = null;
            var loader = game.Machine.LoadScript;
            var random = UnityEngine.Random.state; float scale = Time.timeScale;
            var checks = new List<string>();
            try
            {
                Time.timeScale = 0;
                game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
                game.Machine.Enqueue("200"); game.Machine.Tick();
                foreach (var actor in game.Actors.Values) actor.locked = true;
                game.Machine.LoadScript = name => name == "inventory-integration" ? new[] {
                    "x51(0,205,false)", "x09(\"inventoryBefore\",x4A(0))",
                    "x4B(0,7)", "x09(\"inventoryAfter\",x4A(0))",
                    "x4B(0,1000)", "x09(\"inventoryEmpty\",x4A(0))",
                    "x51(14,101,false)", "x51(7,1,false)"
                } : loader(name);
                game.Machine.Enqueue("inventory-integration"); game.Machine.Tick();
                Require(!game.Machine.Running && game.Machine.Error == null && game.Machine.Get("inventoryBefore") == 205 && game.Machine.Get("inventoryAfter") == 198 && game.Machine.Get("inventoryEmpty") == 0, "VM inventory commands");
                Require(game.Inventory.Count(0) == 0 && game.ItemCount(14) == 101 && game.ItemStackQuantity(14, 1) == 2 && game.Inventory.TotalStacks == 3, "Host and module inventory diverged");
                checks.Add("Production VM executes x51/x4A/x4B through the shared inventory; additions, queries, partial removal and removal clamped at zero match host/stack views");

                Require(game.UseItemStack(14, 0) && game.money == 99000 && game.Inventory.Count(14) == 2, "VM reward item use");
                game.Machine.Command(0x63, new object[] { 1 }); game.money = 100000;
                Require(game.SelectItemTrade(17, false) && game.ConfirmTrade() && game.money == 99000 && game.ItemPurchasePrice(17) == 2000, "Integrated immediate purchase");
                Require(game.SelectItemTrade(14, true, 0), "Integrated sale selection"); game.SetTradeQuantity(99);
                Require(game.TradeQuantity == 2, "Trade bypassed VM inventory");
                checks.Add("VM-awarded gold uses the original 99-item batch; shop sales read its remainder stack and immediate stat purchases update the shared price state");

                game.saveFileName = "validation-inventory-integration.json"; game.Save();
                game.Inventory.Add(0, 5); game.ItemTrading.RestorePurchaseCounts(null); game.money = 1;
                game.Load(); game.Machine.Tick();
                Require(!game.TradeConfirming && game.Inventory.Count(0) == 0 && game.Inventory.Count(14) == 2 && game.Inventory.Count(7) == 1 && game.money == 99000 && game.ItemTrading.PurchasePrice(17, 1) == 2000, "Shared module save restore");
                checks.Add("Production Save/Load restores module inventory, money and progressive price state, and clears the pending sale");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.Machine.LoadScript = loader; game.saveFileName = save;
                game.Begin(); game.Machine.Clear();
                typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                Time.timeScale = scale; UnityEngine.Random.state = random;
            }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/item-module-integration.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_ITEM_MODULE_INTEGRATION " + report.passed + " " + error);
        }

        [MenuItem("古剑/验证背包与交易模块")]
        public static void Run()
        {
            var checks = new List<string>(); string error = null;
            try
            {
                var bag = new ItemInventory();
                bag.Add(0, 205); bag.Add(7, 1);
                Require(bag.StackCount(0) == 3 && bag.TotalStacks == 4 && bag.StackQuantity(0, 0) == 99 && bag.StackQuantity(0, 1) == 99 && bag.StackQuantity(0, 2) == 7, "99/99/7 projection");
                var rows = bag.StackRows(); rows[0][0] = 999;
                Require(bag.StackRows()[0][0] == 0 && bag.StackQuantity(0, int.MaxValue) == 0 && bag.StackQuantity(0, -1) == 0, "Rows leaked state or invalid stack overflowed");
                Require(!bag.TryRemove(0, 8, 2) && bag.Count(0) == 205 && bag.TryRemove(0, 7, 2) && bag.Count(0) == 198, "Selected-stack removal");
                checks.Add("205 items project to 99/99/7; sorted rows are detached; invalid stack indices cannot overflow; removal respects selected stack");

                var snapshot = bag.Capture(); snapshot[0] = new KeyValuePair<int, int>(0, 0);
                Require(bag.Count(0) == 198, "Snapshot leaked state");
                Reject(() => bag.Restore(new[] { 1 }, new int[0]));
                Reject(() => bag.Restore(new[] { 1, 2 }, new[] { 4, -1 }));
                Require(bag.Count(0) == 198 && bag.Count(7) == 1, "Rejected restore partially changed inventory");
                bag.Restore(new[] { 0, 1, 0 }, new[] { 2, 0, 6 });
                Require(bag.Count(0) == 6 && bag.Capture().Length == 1, "Legacy aggregate/duplicate/zero restoration");
                bag.RemoveUpTo(0, 99); Require(bag.Count(0) == 0 && bag.TotalStacks == 0, "Clamped removal");
                bag.Add(0, int.MaxValue); bool overflow = false;
                try { bag.Add(0, 1); } catch (OverflowException) { overflow = true; }
                Require(overflow && bag.Count(0) == int.MaxValue && bag.StackQuantity(0, int.MaxValue) == 0, "Quantity overflow changed inventory");
                checks.Add("Snapshots detach state; invalid restore is atomic; legacy duplicate IDs use last value and zero entries vanish; clamped removal and overflow preserve invariants");

                bag.Clear(); var trading = new ItemTrading(bag, OriginalAssets.Table<ItemDefinition>("item"));
                int money = 50;
                var quote = trading.Quote(0, false, -1, 1, 1);
                Require(trading.Confirm(quote, 1, 1, ref money, null) == ItemTradeResult.InsufficientMoney && money == 50 && bag.Count(0) == 0, "Strict balance boundary");
                money = 1000; quote = trading.Quote(0, false, -1, 1, 1); trading.SetQuantity(quote, 3);
                Require(quote.TotalPrice == 150 && trading.Confirm(quote, 1, 1, ref money, null) == ItemTradeResult.Success && money == 850 && bag.Count(0) == 3, "Purchase result");
                Require(trading.Confirm(quote, 1, 1, ref money, null) == ItemTradeResult.Invalid && money == 850 && bag.Count(0) == 3, "Quote replay");
                checks.Add("Ordinary purchases need strictly greater balance, add exact inventory and deduct exact money; completed quotes cannot transact twice");

                quote = trading.Quote(0, false, -1, 0, 1);
                Require(quote.UnitPrice == 100 && trading.Confirm(quote, 1, 1, ref money, null) == ItemTradeResult.Invalid && money == 850, "Shop change/markup");
                Require(trading.Quote(14, false, -1, 1, 1) == null && trading.Quote(15, false, -1, 1, 1) == null && trading.Quote(0, false, -1, 2, 1) == null, "Invalid merchandise/shop");
                var other = new ItemTrading(bag, OriginalAssets.Table<ItemDefinition>("item"));
                quote = trading.Quote(0, false, -1, 1, 1);
                Require(other.Confirm(quote, 1, 1, ref money, null) == ItemTradeResult.Invalid && money == 850, "Foreign quote");
                checks.Add("Shop0 doubles prices; wrong-shop and foreign-module quotes cannot charge; gold and equipment-shop item purchases are unavailable");

                bag.Restore(new[] { 0 }, new[] { 205 });
                quote = trading.Quote(0, true, 2, 1, 1); trading.SetQuantity(quote, 99);
                Require(quote.Quantity == 7 && trading.Confirm(quote, 1, 1, ref money, null) == ItemTradeResult.Success && money == 920 && bag.Count(0) == 198, "Third stack sale");
                quote = trading.Quote(0, true, 1, 1, 1); trading.SetQuantity(quote, 99); bag.RemoveUpTo(0, 150);
                Require(trading.Confirm(quote, 1, 1, ref money, null) == ItemTradeResult.Invalid && money == 920 && bag.Count(0) == 48, "Stale sale changed state");
                checks.Add("Sales cap at selected stack; inventory shrinking before confirmation invalidates the sale without changing money or remaining items");

                quote = trading.Quote(0, false, -1, 1, 1); trading.MoveQuantity(quote, -1);
                Require(quote.Quantity == 99, "Up quantity wrap"); trading.MoveQuantity(quote, 1);
                Require(quote.Quantity == 1, "Down quantity wrap"); trading.SetQuantity(quote, 1000);
                Require(quote.Quantity == 99, "Quantity cap");
                quote = trading.Quote(17, false, -1, 1, 1); trading.SetQuantity(quote, 99); trading.MoveQuantity(quote, -1);
                Require(quote.Quantity == 1, "Immediate medicine quantity");
                checks.Add("Quantity operations wrap 1/99, clamp direct selection, and keep special medicine purchases at one");

                money = 1000000; int applied = 0;
                quote = trading.Quote(17, false, -1, 1, 1);
                Require(trading.Confirm(quote, 1, 1, ref money, (id, count) => false) == ItemTradeResult.CannotApply && money == 1000000 && trading.PurchasePrice(17, 1) == 1000, "Failed immediate use charged");
                var stalePrice = trading.Quote(17, false, -1, 1, 1);
                quote = trading.Quote(17, false, -1, 1, 1);
                Require(trading.Confirm(quote, 1, 1, ref money, (id, count) => { applied += count; return true; }) == ItemTradeResult.Success && money == 999000 && applied == 1 && bag.Count(17) == 0 && trading.PurchasePrice(17, 1) == 2000, "Immediate purchase");
                Require(trading.Confirm(stalePrice, 1, 1, ref money, (id, count) => { applied++; return true; }) == ItemTradeResult.Invalid && applied == 1 && money == 999000, "Stale price accepted");
                checks.Add("Immediate use applies once without bag residue; failed use preserves price/money; a price changed by another purchase invalidates the old quote");

                quote = trading.Quote(11, false, -1, 1, 1);
                Require(trading.Confirm(quote, 1, 40, ref money, (id, count) => { applied++; return true; }) == ItemTradeResult.Invalid && applied == 1, "Spirit cap changed during confirmation");
                for (int i = 0; i < 20; i++)
                {
                    quote = trading.Quote(17, false, -1, 1, 1);
                    Require(trading.Confirm(quote, 1, 1, ref money, (id, count) => true) == ItemTradeResult.Success, "Repeated stat purchase");
                }
                Require(trading.PurchasePrice(17, 1) == 20000 && trading.PurchasePrice(17, 0) == 40000 && trading.CapturePurchaseCounts()[1] == 19, "Progressive price cap");
                checks.Add("Confirmation rechecks spirit level; progressive medicine price caps at 20000, coefficient 19, with shop0 markup applied afterward");

                var prices = trading.CapturePurchaseCounts(); prices[1] = 0;
                Require(trading.PurchasePrice(17, 1) == 20000, "Price snapshot leaked state");
                trading.RestorePurchaseCounts(new[] { -4, 999 });
                Require(trading.CapturePurchaseCounts().SequenceEqual(new[] { 0, 19, 0, 0, 0, 0, 0 }), "Price restore clamps");
                trading.RestorePurchaseCounts(null);
                Require(trading.PurchasePrice(17, 1) == 1000, "Legacy missing price state");
                checks.Add("Price snapshots detach state; restoration clamps coefficients and missing legacy fields restore defaults");
            }
            catch (Exception e) { error = e.ToString(); }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/item-modules.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_ITEM_MODULES " + report.passed + " " + error);
        }
    }
}
