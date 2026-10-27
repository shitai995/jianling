using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class SystemMenuValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private const string VolumeKey = "gujian.sound-volume-step";
        private static GuJianGame game;
        private static object Get(string n) => typeof(GuJianGame).GetField(n, F).GetValue(game);
        private static void Set(string n, object v) => typeof(GuJianGame).GetField(n, F).SetValue(game, v);
        private static object Call(string n, params object[] args) => typeof(GuJianGame).GetMethod(n, F).Invoke(game, args);
        private static void Key(int code) => Call("HandleOriginalMenuKey", code);
        private static string Page => Get("systemMenuPage").ToString();
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        private static string Hash(string path)
        {
            if (!File.Exists(path)) return null;
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }
        private static void Stage()
        {
            game.Begin(); game.Machine.Clear(); game.Machine.Values["event"] = -999;
            game.Machine.Enqueue("200"); game.Machine.Tick();
            foreach (var actor in game.Actors.Values) actor.locked = true;
            Set("menu", true); game.SelectMenuPage(7); game.SelectMenuPage(3);
            Set("modalInputFrame", -1); Set("originalUiTicks", 0);
        }
        private static void Select(int index)
        {
            Set("atlasSelection", index); Key(4);
        }
        private static void Capture(string name)
        {
            var method = typeof(HudFeedbackValidation).GetMethod("Render", BindingFlags.NonPublic | BindingFlags.Static);
            var image = (Texture2D)method.Invoke(null, null);
            try { File.WriteAllBytes("../Reference/Validation/" + name + ".png", image.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }
        [MenuItem("古剑/验证系统六项与子页面")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play first");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            string originalSave = game.saveFileName;
            int oldRecord = (int)Get("activeRecordIndex"), oldVolumeStep = (int)Get("soundVolumeStep"), oldTicks = (int)Get("originalUiTicks");
            string formal0 = (string)Call("RecordPath", 0), formal1 = (string)Call("RecordPath", 1);
            string formalHash0 = Hash(formal0), formalHash1 = Hash(formal1);
            string smsGlobalPath = (string)typeof(GuJianGame).GetProperty("SmsGlobalStatePath", F).GetValue(game);
            bool hadSmsGlobal = File.Exists(smsGlobalPath);
            byte[] smsGlobalBackup = hadSmsGlobal ? File.ReadAllBytes(smsGlobalPath) : null;
            bool oldUnlock = game.allStagesUnlocked, hadVolume = PlayerPrefs.HasKey(VolumeKey);
            int oldSmsExperience = (int)Get("smsExperienceMultiplier"), oldSmsGold = (int)Get("smsGoldAmount"), oldSmsSpend = (int)Get("smsTotalSpend"), oldSmsGift = (int)Get("smsGiftTier");
            bool oldSmsDurability = (bool)Get("smsUnlimitedDurability"), oldSmsLoaded = (bool)Get("smsGlobalStateLoaded");
            int prefVolume = PlayerPrefs.GetInt(VolumeKey, 2);
            float scale = Time.timeScale, listener = AudioListener.volume;
            var random = UnityEngine.Random.state;
            var checks = new List<string>(); string error = null;
            game.saveFileName = "validation-system-menu.json"; Time.timeScale = 0;
            try
            {
                Stage();
                Require((int)Call("MenuInputCount") == 6, "System root must have six entries");
                Key(0); Require((int)Get("atlasSelection") == 5, "Root Up did not wrap0->5");
                Key(1); Require((int)Get("atlasSelection") == 0, "Root Down did not wrap5->0");
                for (int i = 0; i < 6; i++) { Set("atlasSelection", i); Capture("system-root-" + i); }
                checks.Add("Six original system entries wrap0/5 and render original actions5,8,9,10,11,12 with source atlas assets");

                Stage(); int beforeStep = (int)Get("soundVolumeStep"); Set("atlasSelection", 0);
                Key(2); Require((int)Get("menuPage") == 0 && (int)Get("soundVolumeStep") == beforeStep, "Root Left adjusted settings instead of changing tab");
                Key(3); Require((int)Get("menuPage") == 3, "Root Right did not return from mission");
                checks.Add("Root Left/Right change tabs; volume and recovery only change in independent settings state");

                Stage(); Select(3);
                Require(Page == "Settings" && (int)Get("settingSelection") == 0 && (int)Get("atlasSelection") == 3, "Settings entry/focus");
                Set("soundVolumeStep", 2); Call("ApplySoundVolume");
                Key(2); Require((int)Get("soundVolumeStep") == 1 && Mathf.Abs(AudioListener.volume - .25f) < .0001f, "Volume left step");
                Key(3); Require((int)Get("soundVolumeStep") == 2 && Mathf.Abs(AudioListener.volume - .5f) < .0001f, "Volume right step");
                for (int i = 0; i < 8; i++) Key(2);
                Require((int)Get("soundVolumeStep") == 0 && AudioListener.volume == 0, "Volume minimum clamp");
                Capture("system-settings-muted");
                for (int i = 0; i < 8; i++) Key(3);
                Require((int)Get("soundVolumeStep") == 4 && AudioListener.volume == 1, "Volume maximum clamp");
                Key(0); Require((int)Get("settingSelection") == 1, "Settings Up must toggle");
                game.SetAutoRecoverPercent(70); Key(2); Require((int)Get("autoRecoverPercent") == 60, "Recovery left step");
                Key(3); Require((int)Get("autoRecoverPercent") == 70, "Recovery right step");
                for (int i = 0; i < 10; i++) Key(2);
                Require((int)Get("autoRecoverPercent") == 10, "Recovery min clamp");
                for (int i = 0; i < 10; i++) Key(3);
                Require((int)Get("autoRecoverPercent") == 90, "Recovery max clamp");
                Capture("system-settings-recovery");
                checks.Add("Independent settings toggles two rows, volume has five clamped0/25/50/75/100 levels, auto-recovery clamps10..90 in10-point steps");

                Key(4); Require(Page == "Settings", "FIRE incorrectly closed settings");
                Require(!game.SelectMenuPage(2), "Direct tab bypassed settings state");
                Key(9); Require(Page == "None" && (bool)Get("menu") && (int)Get("atlasSelection") == 3, "Soft9 did not return to retained root focus");
                Key(4); Key(10); Require(Page == "None" && (int)Get("atlasSelection") == 3, "Soft10 settings return");
                Key(4); game.PressControl(5); Require(Page == "None" && (bool)Get("menu"), "Public Back5 should leave settings without closing menu");
                checks.Add("Settings ignores FIRE, blocks direct tab entry, and native soft9/10 or public Back5 return to original root selection");

                Set("systemTextStyle", new GUIStyle { fontSize = 0 });
                Stage(); Select(4); Capture("system-help-start");
                Require(((GUIStyle)Get("systemTextStyle")).fontSize == Resources.Load<OriginalUiLayout>("OriginalUiLayout").dialogueFontSize, "Cached zero-size style must restore configured font size");
                Require(Page == "Help", "Help entry");
                var lines = (OriginalDialogueText.Line[])Get("systemHelpLines");
                string text = string.Join("\n", lines.Select(l => l.Text).ToArray());
                Require(text.Contains("移动：方向键") && text.Contains("房屋型按钮"), "Original help text missing");
                int height = (int)Get("systemHelpLineHeight"), offset = (int)Get("systemHelpOffset");
                Require(offset == height, "Original long help initial scroll offset");
                Call("TickOriginalSystemMenu", .079f); Require((int)Get("systemHelpOffset") == offset, "Help scrolled early");
                Call("TickOriginalSystemMenu", .001001f); Require((int)Get("systemHelpOffset") == offset - height / 10, "Help80ms integer step");
                for (int i = 0; i < 15; i++) Call("TickOriginalSystemMenu", .080001f);
                Capture("system-help-visible");
                Key(4); Require(Page == "Help", "FIRE incorrectly closed help");
                Key(9); Require(Page == "None" && (int)Get("atlasSelection") == 4, "Help soft9 focus return");
                Key(4); Key(10); Require(Page == "None", "Help soft10 return");
                checks.Add("Help uses original instructions, independent native frame12,280px wrapping and147px viewport with80ms scroll; FIRE inert, both softkeys return");

                Stage(); int normalPrice = OriginalAssets.Table<ItemDefinition>("item").First(i => i.id == 0).buy;
                Select(1);
                Require((int)Get("menuPage") == 5 && (int)Get("shopType") == 0 && game.ItemPurchasePrice(0) == normalPrice * 2, "Portable shop wrong type or price");
                Capture("system-portable-shop");
                Key(10); Require((int)Get("menuPage") == 3 && (int)Get("atlasSelection") == 1 && (bool)Get("menu"), "Portable shop Back lost system return");
                Key(10); Require(!(bool)Get("menu") && (int)Get("systemSelection") == 0, "Root Back should close/reset system selection");
                checks.Add("Portable shop opens original type0 double-price shop; Back returns to system item1, next Back closes and resets root");

                Stage(); game.money = 111; game.Save();
                string slot0 = (string)Call("RecordPath", 0), slot1 = (string)Call("RecordPath", 1);
                string slot0Hash = Hash(slot0);
                Require(Path.GetFileName(slot0) == "validation-system-menu.json" && Path.GetFileName(slot1) == "validation-system-menu-slot1.json", "Legacy slot0 filename changed");
                Set("activeRecordIndex", 1); game.money = 222; game.Machine.Command(0x68, new object[0]);
                Require(Hash(slot0) == slot0Hash && File.Exists(slot1), "Checkpoint wrote over other slot");
                checks.Add("Existing base filename remains slot0; slot1 is separate; original x68 checkpoint saves active slot without modifying slot0");

                Stage(); Select(2);
                Require(Page == "Records" && (int)Get("recordSelection") == 0, "Record entry");
                Key(1); Require((int)Get("recordSelection") == 1, "Record Down toggle");
                Key(0); Require((int)Get("recordSelection") == 0, "Record Up toggle");
                Capture("system-records");
                string aHash = Hash(slot0), bHash = Hash(slot1);
                Key(4); Require((bool)Get("recordOptionOpen") && (int)Get("recordOption") == 0, "Read/Delete popup");
                Capture("system-record-popup");
                Key(1); Require((int)Get("recordOption") == 1, "Popup Down toggle");
                Key(0); Require((int)Get("recordOption") == 0, "Popup Up toggle");
                Key(10);
                Require(Page == "Records" && !(bool)Get("recordOptionOpen") && Hash(slot0) == aHash && Hash(slot1) == bHash, "Popup cancel wrote data");
                Key(10); Require(Page == "None" && (int)Get("atlasSelection") == 2, "Records Back did not return to root");
                checks.Add("Two-slot selector and independent read/delete popup toggle with Up/Down; soft10 cancels popup before leaving records and preserves both hashes");

                Key(4); Key(1); Key(9); Key(9); game.Machine.Tick();
                Require(Page == "None" && !(bool)Get("menu") && game.money == 222 && (int)Get("activeRecordIndex") == 1, "Loading slot1 lost data or active record index");
                game.money = 333; game.Save();
                Require(Hash(slot0) == slot0Hash, "Save after slot1 load wrote slot0");
                checks.Add("Record soft9 reads selected slot1 and restores222 money; subsequent Save retains selected slot and leaves slot0 untouched");

                Set("menu", true); game.SelectMenuPage(3); Select(2);
                Require((int)Get("recordSelection") == 1, "Record reentry must retain active slot");
                Key(4); Key(1); Key(4);
                Require(Page == "Records" && !File.Exists(slot1) && Hash(slot0) == slot0Hash, "Delete did not isolate selected slot");
                Capture("system-record-deleted");
                checks.Add("Deleting selected validation slot1 removes only that slot and leaves slot0 byte-for-byte unchanged");

                Key(4);
                Require(Page == "None" && !(bool)Get("menu") && (int)Get("activeRecordIndex") == 1 && game.money == 0 && game.Machine.Running, "Empty slot did not begin new game in selected record");
                game.Machine.Clear();
                checks.Add("Confirming empty slot starts new game in selected slot1 with original start script queued");

                Stage(); string legacy = File.ReadAllText(slot0).Replace("\"recordName\"", "\"legacyRecordName\"");
                File.WriteAllText(slot0, legacy);
                Select(2); Require(((string[])Get("recordNames"))[0] == "存档点", "Legacy name fallback");
                Key(4); Key(4); game.Machine.Tick();
                Require(game.money == 111 && (int)Get("activeRecordIndex") == 0, "Legacy slot0 failed to load");
                checks.Add("Version3 saves lacking new optional recordName remain visible and load with original money and slot0 identity");

                Stage(); Select(3); game.Begin(); game.Machine.Clear();
                Require(Page == "None" && !(bool)Get("recordOptionOpen") && (int)Get("systemSelection") == 0 && (int)Get("activeRecordIndex") == 0, "New game retained system/slot transient state");
                checks.Add("Begin clears subpage, read/delete popup, root focus and transient record selection; Load explicitly restores its selected record");

                Stage(); Set("marketplaceSelection", 0); game.allStagesUnlocked = false; Select(0);
                Require(Page == "None" && game.DialogueVisible && (string)Get("dialogue") == "尚未开启！", "Locked marketplace original message");
                game.PressControl(4); Set("modalInputFrame", -1); game.allStagesUnlocked = true; Key(4);
                Require(Page == "Marketplace", "Unlocked marketplace did not open");
                int catalogueStates = 0;
                for (int multiplier = 1; multiplier <= 4; multiplier++)
                for (int giftTier = 0; giftTier <= 3; giftTier++)
                {
                    Set("smsExperienceMultiplier", multiplier); Set("smsGiftTier", giftTier);
                    int expectedCount = 3 + (multiplier < 4 ? 1 : 0) + (giftTier < 3 ? 1 : 0);
                    Require((int)Call("MarketplaceOptionCount") == expectedCount, "Catalogue option count mismatch at " + multiplier + "/" + giftTier);
                    for (int i = 0; i < expectedCount; i++)
                    {
                        Set("marketplaceSelection", i);
                        bool expectedExperience = multiplier < 4 && i == 3;
                        bool expectedGift = giftTier < 3 && i == (multiplier < 4 ? 4 : 3);
                        Require((bool)Call("IsMarketplaceExperienceOption", i) == expectedExperience &&
                                (bool)Call("IsMarketplaceGiftOption", i) == expectedGift, "Catalogue row mapping mismatch at " + multiplier + "/" + giftTier + "/" + i);
                        string confirmation = (string)Call("MarketplaceConfirmationText");
                        if (expectedGift) Require(confirmation.Contains("累计消费满"), "Gift row did not open gift confirmation");
                        else if (expectedExperience) Require(confirmation.Contains("升级经验倍率"), "Experience row did not open multiplier confirmation");
                        else Require(!confirmation.Contains("礼包？") && !confirmation.Contains("升级经验倍率"), "Base row mapped to a special purchase");
                    }
                    Set("marketplaceSelection", 0); Key(0);
                    Require((int)Get("marketplaceSelection") == expectedCount - 1, "Catalogue up did not wrap to the dynamic last row");
                    Key(1); Require((int)Get("marketplaceSelection") == 0, "Catalogue down did not wrap to row zero");
                    catalogueStates++;
                }
                checks.Add(catalogueStates + " experience/gift progression combinations render 3/4/5 catalogue rows, map multiplier and gift rows to the correct confirmation, and wrap focus within the visible list");
                Set("smsExperienceMultiplier", 3); Set("smsGiftTier", 0); Set("smsTotalSpend", 4); Set("marketplaceSelection", 3);
                Key(4); Key(9);
                Require((int)Get("smsExperienceMultiplier") == 4 && (int)Get("marketplaceSelection") == 0 && (int)Call("MarketplaceOptionCount") == 4, "Final experience purchase did not remove its row and restore first-row focus");
                Key(4); Set("smsTotalSpend", 6); Set("marketplaceSelection", 3);
                Key(4); Key(9);
                Require((int)Get("smsGiftTier") == 1 && (int)Get("marketplaceSelection") == 2 && (int)Call("MarketplaceOptionCount") == 4,
                    "Gift claim did not advance its tier and focus: tier=" + Get("smsGiftTier") + ", focus=" + Get("marketplaceSelection") + ", rows=" + Call("MarketplaceOptionCount") + ", page=" + Page + ", spend=" + Get("smsTotalSpend"));
                Key(4);
                checks.Add("Purchasing multiplier 4 removes its row and returns focus to row zero; claiming a gift advances the tier and adjusts focus to preserve the current item");
                Set("smsExperienceMultiplier", 1); Set("smsGiftTier", 0); Set("smsTotalSpend", 0); Set("marketplaceSelection", 0);
                int money = game.money, equips = game.Equipment.Count; Capture("system-marketplace");
                Key(1); Require((int)Get("marketplaceSelection") == 1, "Catalogue down selection");
                Set("marketplaceConfirmChoice", 0); Key(9); Require(Page == "MarketplaceConfirm", "Catalogue confirmation did not open");
                Key(9); Require(Page == "MarketplaceResult" && game.money == money && game.Equipment.Count == equips + 1, "Local weapon purchase did not apply exactly once");
                Key(4); Require(Page == "Marketplace", "Purchase result return");
                Set("marketplaceSelection", 0); Set("smsGoldAmount", 10); Set("smsTotalSpend", 0); money = game.money;
                Key(4); Key(4);
                Require(Page == "MarketplaceResult" && game.money == money + 100000 && (int)Get("smsGoldAmount") == 12 && (int)Get("smsTotalSpend") == 2, "Local gold purchase state");
                checks.Add("Offline catalogue selections execute confirmation and grant the exact level-appropriate weapon or gold, advance spend/gold tiers, and preserve result/return states");
                Key(4); Set("marketplaceSelection", 3); Set("smsExperienceMultiplier", 1); Set("smsTotalSpend", 0);
                Key(4); Key(4);
                Require((int)Get("smsExperienceMultiplier") == 2 && (int)Get("smsTotalSpend") == 2, "Local experience multiplier purchase");
                checks.Add("Experience purchase increments the persistent multiplier up to its cap and records the original two-unit spend");
                game.allStagesUnlocked = true; Call("SaveSmsGlobalState");
                Stage();
                Require(game.allStagesUnlocked && (int)Get("smsExperienceMultiplier") == 2 && (int)Get("smsGoldAmount") == 12 && (int)Get("smsTotalSpend") == 2, "Global marketplace state did not survive Begin/new slot");
                checks.Add("Global marketplace flags, multiplier, gold tier, and accumulated spend survive a new save slot");
                game.Save(true);
                Set("activeRecordIndex", 0); Call("PersistSmsGiftTier", 0, 1);
                Set("activeRecordIndex", 1); game.Save(true); Call("PersistSmsGiftTier", 1, 2);
                Require((int)Call("LoadSmsGiftTier", 0) == 1 && (int)Call("LoadSmsGiftTier", 1) == 2, "Gift tier leaked across save slots");
                Set("activeRecordIndex", 0); RestoreTierForValidation(0);
                checks.Add("Gift claim tiers round-trip independently in both save records");
                Stage(); Set("smsGiftTier", 0); Set("smsTotalSpend", 6); Set("marketplaceSelection", 4);
                int oldMoney = game.money, oldItem = game.ItemCount(3);
                Require((bool)Call("GrantMarketplaceGift") && (int)Get("smsGiftTier") == 1 && game.money == oldMoney + 50000 && game.ItemCount(3) == oldItem + 20, "First marketplace gift threshold/reward");
                Require(!(bool)Call("GrantMarketplaceGift") && game.money == oldMoney + 50000, "Gift was claimable twice");
                checks.Add("Cumulative-spend gift threshold awards the correct first-tier bundle once and blocks duplicate claiming");
                Stage(); Set("smsGiftTier", 0); Set("smsTotalSpend", 1); Set("marketplaceSelection", 3);
                var legacySavePath = (string)Call("RecordPath", 0);
                string legacyJson = Regex.Replace(File.ReadAllText(legacySavePath), "\\\"smsGiftTier\\\"\\s*:\\s*\\d+", "\"smsGiftTier\": 2,\n  \"smsExperienceMultiplier\": 3,\n  \"smsGoldAmount\": 16,\n  \"smsTotalSpend\": 17,\n  \"smsUnlimitedDurability\": true");
                File.WriteAllText(legacySavePath, legacyJson);
                File.Delete(smsGlobalPath); Set("smsGlobalStateLoaded", false); Set("smsExperienceMultiplier", 1); Set("smsGoldAmount", 10); Set("smsTotalSpend", 0); Set("smsUnlimitedDurability", false);
                Call("ImportLegacySmsState", legacyJson);
                Require((int)Get("smsExperienceMultiplier") == 3 && (int)Get("smsGoldAmount") == 16 && (int)Get("smsTotalSpend") == 17 && (bool)Get("smsUnlimitedDurability"), "Legacy global SMS migration");
                checks.Add("Version3 legacy SMS purchase fields migrate into global state while the slot gift tier remains in its own save record");
                File.Delete(smsGlobalPath); Set("smsGlobalStateLoaded", false);
                checks.Add("Locked marketplace retains the original unavailable message; local purchase confirmation and back navigation remain usable");

                Stage(); Select(5);
                Require(game.AtTitle && !(bool)Get("menu") && Page == "None", "Return main title failed");
                checks.Add("Original sixth system entry returns to title and clears game-menu state");

                var ui = AssetDatabase.LoadAssetAtPath<OriginalUiLayout>("Assets/Resources/OriginalUiLayout.asset");
                Require(ui.mainMenu.frames.Length == 17 && ui.mainMenuSheets.Length == 4 && ui.mainMenuSheets.All(t => t && EditorUtility.IsPersistent(t)) &&
                    ui.settingsArrow && EditorUtility.IsPersistent(ui.settingsArrow) && ui.marketplaceOptions.Length == 12 && ui.marketplaceOptions.All(t => t && EditorUtility.IsPersistent(t)) &&
                    ui.marketplaceInfo.Length == 6 && ui.marketplaceInfo.All(t => t && EditorUtility.IsPersistent(t)), "System assets not saved");
                checks.Add("Original mainUI17 frames, four sheets, settings arrow and catalogue images reference persistent imported assets");
                Require(Hash(formal0) == formalHash0 && Hash(formal1) == formalHash1, "Formal player save was modified");
                checks.Add("Both formal player slot hashes remain unchanged across isolated record, delete, load, checkpoint and restart tests");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.Begin(); game.Machine.Clear(); typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = originalSave; Set("activeRecordIndex", oldRecord); game.allStagesUnlocked = oldUnlock;
                Set("smsExperienceMultiplier", oldSmsExperience); Set("smsGoldAmount", oldSmsGold); Set("smsTotalSpend", oldSmsSpend);
                Set("smsGiftTier", oldSmsGift); Set("smsUnlimitedDurability", oldSmsDurability); Set("smsGlobalStateLoaded", oldSmsLoaded);
                if (hadVolume) PlayerPrefs.SetInt(VolumeKey, prefVolume); else PlayerPrefs.DeleteKey(VolumeKey);
                if (hadSmsGlobal) File.WriteAllBytes(smsGlobalPath, smsGlobalBackup); else if (File.Exists(smsGlobalPath)) File.Delete(smsGlobalPath);
                PlayerPrefs.Save(); Set("soundVolumeStep", oldVolumeStep); Call("ApplySoundVolume"); AudioListener.volume = listener;
                Time.timeScale = scale; UnityEngine.Random.state = random; Set("originalUiTicks", oldTicks);
            }
            var report = new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error };
            File.WriteAllText("../Reference/Validation/system-menu.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_SYSTEM_MENU " + report.passed + " " + error); game = null;
        }

        private static void RestoreTierForValidation(int tier) => typeof(GuJianGame).GetMethod("RestoreSmsGiftTier", F).Invoke(game, new object[] { tier });
    }
}
