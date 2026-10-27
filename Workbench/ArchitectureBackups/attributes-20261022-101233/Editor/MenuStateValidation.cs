using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class MenuStateValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static GuJianGame game;
        private static object Get(string name) => typeof(GuJianGame).GetField(name, F).GetValue(game);
        private static void Set(string name, object value) => typeof(GuJianGame).GetField(name, F).SetValue(game, value);
        private static object Call(string name, params object[] args) => typeof(GuJianGame).GetMethod(name, F).Invoke(game, args);
        private static void Key(int code) => Call("HandleOriginalMenuKey", code);
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Stage(int page, bool spirit = false)
        {
            game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
            game.Machine.Enqueue("200"); game.Machine.Tick();
            foreach (var actor in game.Actors.Values) actor.locked = true;
            if (spirit) game.Machine.Command(0x64, new object[] { 1 });
            Set("menu", true); Require(game.SelectMenuPage(page), "Menu page setup rejected");
            Set("modalInputFrame", -1);
        }
        private static void Give(int id, int count) { game.Inventory.RemoveUpTo(id, int.MaxValue); game.Inventory.Add(id, count); }
        private static void Capture(string name)
        {
            var method = typeof(HudFeedbackValidation).GetMethod("Render", BindingFlags.Static | BindingFlags.NonPublic);
            var image = (Texture2D)method.Invoke(null, null);
            try { File.WriteAllBytes("../Reference/Validation/" + name + ".png", image.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }
        [MenuItem("古剑/验证菜单状态与物品弹窗")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play first");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            float scale = Time.timeScale; string save = game.saveFileName;
            var random = UnityEngine.Random.state;
            var checks = new List<string>(); string error = null;
            Time.timeScale = 0; game.saveFileName = "validation-menu-state.json";
            try
            {
                Stage(1);
                Key(3); Require((int)Get("menuPage") == 0, "Right from items must skip missing spirit");
                Key(2); Require((int)Get("menuPage") == 1, "Left from mission must skip missing spirit");
                Require(!game.SelectMenuPage(6) && (int)Get("menuPage") == 1, "Direct tab selected missing spirit");
                Set("menuPage", 7); Key(2); Require((int)Get("menuPage") == 3, "First tab left wrap");
                Set("atlasSelection", 2); Key(3); Require((int)Get("menuPage") == 7, "Last tab right wrap");
                checks.Add("No-spirit page navigation skips spirit in both directions, rejects direct spirit-tab selection, and wraps first/last tabs");

                Stage(1, true); Key(3); Require((int)Get("menuPage") == 6, "Existing spirit tab skipped");
                Key(3); Require((int)Get("menuPage") == 0, "Spirit next page");
                Key(2); Require((int)Get("menuPage") == 6, "Existing spirit reverse page");
                checks.Add("With spirit present, item/spirit/mission page order is preserved in both directions");

                Stage(1); Give(0, 105);
                Key(4); Require((bool)Get("itemConfirming") && (int)Get("itemConfirmChoice") == 0, "Original OK did not open use/discard");
                Key(2); Key(2); Require((int)Get("itemConfirmChoice") == 0, "Repeated Left must stay on Use");
                Key(3); Key(3); Require((int)Get("itemConfirmChoice") == 1, "Repeated Right must stay on Discard");
                Key(0); Key(1); Require((int)Get("itemConfirmChoice") == 1 && (int)Get("menuPage") == 1 && (int)Get("atlasSelection") == 0, "Popup leaked direction input to underlying menu");
                Capture("menu-item-discard");
                Key(10); Require(!(bool)Get("itemConfirming") && game.ItemCount(0) == 105 && (bool)Get("menu"), "Soft-back consumed inventory or closed menu");
                checks.Add("Item popup starts Use; Left/Right select absolute choices and remain idempotent; Up/Down are modal; original key10 cancels without consumption");

                Set("atlasSelection", 1); Key(9); Require((bool)Get("itemConfirming"), "Original soft9 did not open popup");
                Key(3); Key(9); Require(game.ItemCount(0) == 99 && !(bool)Get("itemConfirming"), "Soft9 discard did not remove only selected six-item stack");
                game.Actors[-1].hp = 1;
                Set("atlasSelection", 0); Key(4); Key(2); Capture("menu-item-use"); Key(4);
                Require(game.ItemCount(0) < 99 && game.Actors[-1].hp > 1 && !(bool)Get("itemConfirming"), "Use failed through production native-key handler");
                checks.Add("Original keys4/9 both open and confirm; discard removes selected six-item remainder while retaining99; Use consumes medicine and heals through production handler");

                Stage(1); Give(0, 2); game.PressControl(4); Require((bool)Get("itemConfirming"), "Abstract primary did not route menu");
                game.PressControl(3); game.PressControl(3);
                Require((int)Get("itemConfirmChoice") == 1, "Abstract right differs from native right");
                game.PressControl(5); Require(!(bool)Get("itemConfirming") && (bool)Get("menu") && game.ItemCount(0) == 2, "Abstract5 must map to original soft-back10");
                game.PressControl(5); Require(!(bool)Get("menu"), "Abstract Back did not close regular menu");
                checks.Add("Public PressControl routes primary/directions through same production menu handler; abstract5 cancels popup then closes menu, without scene action");

                Stage(1); Give(0, 2); Key(4); Require((bool)Get("itemConfirming"), "Restart fixture");
                game.Begin(); game.Machine.Clear();
                Require(!(bool)Get("itemConfirming") && (int)Get("itemConfirmId") == -1 && (int)Get("itemConfirmStack") == -1, "Begin retained stale item modal");
                checks.Add("Begin clears item-confirmation visibility, item identity, stack index and choice");

                Stage(2);
                var bag = game.EquipmentBag;
                for (int i = 0; i < 9; i++) bag.Add(EquipmentInstance.Create(0));
                Key(0); Require((int)Get("atlasSelection") == 9, "Equipment up did not wrap");
                Capture("menu-equipment-last");
                var scroll = (Vector2)Get("atlasScroll");
                Require(scroll.y > 0 && scroll.y <= 9 * 31 && scroll.y + 124 >= 10 * 31, "Focused equipment is outside actual GameView scroll viewport");
                Key(1); Require((int)Get("atlasSelection") == 0, "Equipment down did not wrap");
                Capture("menu-equipment-first");
                Require(((Vector2)Get("atlasScroll")).y == 0, "Focus wrap did not return actual viewport to top");
                checks.Add("Ten-row equipment list wraps9->0/0->9; actual GameView draw scrolls focused last row into124px viewport and restores top on wrap");

                Stage(0); Set("atlasBranch", false); Key(1); Require((bool)Get("atlasBranch"), "Down branch mission");
                Key(0); Require(!(bool)Get("atlasBranch"), "Up main mission");
                checks.Add("Mission Up/Down choose main/branch explicitly");

                Stage(5); Set("shopType", 1);
                Key(3); Require(!(bool)Get("shopSelling") && game.DialogueVisible, "Empty Sell must show original message");
                Require((string)Get("dialogue") == "无可出售物品", "Empty Sell message text");
                game.PressControl(4); Set("modalInputFrame", -1); Give(0, 2);
                Key(3); Require((bool)Get("shopSelling"), "Nonempty Sell rejected");
                Key(2); Require(!(bool)Get("shopSelling"), "Buy tab rejected");
                Key(4); Require(game.TradeConfirming, "Shop confirm did not open trade");
                int money = game.money, quantity = game.ItemCount(0);
                Key(10); Require(!game.TradeConfirming && game.money == money && game.ItemCount(0) == quantity, "Trade Back changed inventory");
                checks.Add("Shop Sell-empty original message, Buy/Sell transition, trade opening and soft-back cancellation use shared production handler");

                Stage(5); Set("shopType", 1); game.money = 100000;
                Require(game.SelectItemTrade(0, false), "Buy fixture");
                Key(0); Require(game.TradeQuantity == 99, "Buy Up1 must wrap to99");
                Key(1); Require(game.TradeQuantity == 1, "Buy Down99 must wrap to1");
                Key(1); Require(game.TradeQuantity == 2, "Buy Down must increment");
                Key(0); Require(game.TradeQuantity == 1, "Buy Up must decrement");
                var ui = (OriginalUiLayout)Resources.Load<OriginalUiLayout>("OriginalUiLayout");
                int tradeFontHeight = Mathf.CeilToInt(((GUIStyle)typeof(GuJianGame).GetField("atlasText", F).GetValue(game)).CalcSize(new GUIContent("测")).y);
                int tradeTotalY = ((ui.transactionArrow ? ui.transactionArrow.height : 0) << 1) + 75 + tradeFontHeight + 3 + 20 - (320 - 240) / 2;
                var tradeBackground = ui.menu.frames[37].pieces.First(p => p.image < 0 && p.w > 150 && p.h > 80);
                int frameTop = 120 + tradeBackground.y, frameBottom = frameTop + tradeBackground.h;
                Require(tradeTotalY >= frameTop && tradeTotalY + Mathf.Max(18, tradeFontHeight) <= frameBottom, "Trade total text falls outside the source modal background");
                Require(tradeTotalY + Mathf.Max(18, tradeFontHeight) < 134, "Trade total text overlaps the embedded confirm/cancel controls");
                Capture("menu-trade-buy-confirm");
                Key(2); Require(game.TradeQuantity == 1, "Left changed trade quantity");
                Capture("menu-trade-buy-cancel");
                money = game.money; Key(4);
                Require(!game.TradeConfirming && game.money == money && game.ItemCount(0) == 0, "Chosen Cancel charged purchase");
                checks.Add("Buy popup Up/Down decrement/increment with1/99 wrap; Left toggles to Cancel without changing quantity; OK on Cancel spends nothing");

                Require(game.SelectItemTrade(0, false), "Buy fixture reopen");
                Key(3); Key(3); Key(1); int price = game.TradePrice; Key(9);
                Require(!game.TradeConfirming && game.ItemCount(0) == 2 && game.money == money - price, "Right toggle twice/soft9 purchase");
                Give(0, 7); Require(game.SelectItemTrade(0, true), "Sale fixture");
                Key(0); Require(game.TradeQuantity == 7, "Sale Up1 must wrap to owned7");
                Key(1); Require(game.TradeQuantity == 1, "Sale Down7 must wrap to1");
                Key(1); price = game.TradePrice; money = game.money; Capture("menu-trade-sell-confirm"); Key(4);
                Require(game.ItemCount(0) == 5 && game.money == money + price, "Selected sale quantity/credit");
                checks.Add("Right toggles popup choice twice back to Confirm; soft9 purchases exactly two; sale quantity wraps to owned7 and selling two retains5 with exact credit");

                Require(game.SelectItemTrade(16, false), "Special fixture");
                Key(0); Key(1); Require(game.TradeQuantity == 1, "Permanent medicine quantity must stay1"); Key(10);
                Set("shopType", 2); Require(game.SelectWeaponTrade(0, false), "Weapon fixture");
                Key(0); Key(1); Require(game.TradeQuantity == 1, "Weapon quantity must stay1");
                Capture("menu-trade-weapon-confirm");
                Key(3); Capture("menu-trade-weapon-cancel");
                money = game.money; int equipmentCount = game.Equipment.Count; Key(9);
                Require(!game.TradeConfirming && game.money == money && game.Equipment.Count == equipmentCount, "Weapon Cancel charged or created instance");
                checks.Add("Permanent-medicine/weapon quantities stay1; Buy/Sell select saved original17/18 and19/20 popup frames; soft9 Cancel changes no money/equipment");

                Stage(1); Give(0, 2);
                game.Machine.Command(0x28, new object[] { "模态消息", 0 });
                Key(4); Require(!(bool)Get("itemConfirming"), "Native menu key bypassed message");
                game.PressControl(4); game.PressControl(4);
                Require(!(bool)Get("itemConfirming") && (bool)Get("menu"), "Message confirmation leaked to item modal in same frame");
                checks.Add("Message modal blocks underlying native-menu actions; public confirmation cannot open item popup in same frame after closing message");

                Stage(1, true); game.Machine.Command(0x6A, new object[] { 3 });
                game.PressControl(3);
                Require(game.TutorialKey == -1 && (int)Get("menuPage") == 1, "Menu routing intercepted existing tutorial response");
                checks.Add("Existing tutorial response retains priority over menu routing; normal menu side effects do not consume tutorial direction");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.Begin(); game.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = save; Time.timeScale = scale; UnityEngine.Random.state = random;
            }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/menu-state.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_MENU_STATE " + report.passed + " " + error);
            game = null;
        }
    }
}
