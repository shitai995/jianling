using System;
using System.IO;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private enum SystemMenuPage { None, Records, Settings, Help, Marketplace, PurchaseFailure, MarketplaceConfirm, MarketplaceResult }
        private SystemMenuPage systemMenuPage;
        private int systemSelection, settingSelection, recordSelection, recordOption, activeRecordIndex, marketplaceSelection, marketplaceConfirmChoice;
        private bool recordOptionOpen, systemShopReturn;
        private readonly string[] recordNames = new string[2];
        private GUIStyle systemTextStyle;
        private OriginalDialogueText.Line[] systemHelpLines;
        private int systemHelpLineHeight, systemHelpOffset;
        private float systemHelpElapsed;
        private const string OriginalHelpText = "移动：方向键\n攻击：剑型按钮\n技能：‘技’字按钮\n崩击：‘崩’字按钮\n超杀：‘杀’字按钮\n剑灵合体：点击剑灵头像\n查看地图：右上角指南针按钮\n系统菜单：右上角房屋型按钮\n\n";
        private int smsExperienceMultiplier = 1, smsGoldAmount = 10, smsTotalSpend, smsGiftTier;
        private bool smsUnlimitedDurability;
        private bool smsGlobalStateLoaded;

        [Serializable]
        private sealed class SmsGlobalState
        {
            public int version = 1;
            public int experienceMultiplier = 1, goldAmount = 10, totalSpend;
            public bool unlimitedDurability, allStagesUnlocked;
        }

        [Serializable]
        private sealed class LegacySmsSaveData
        {
            public int smsExperienceMultiplier = 1, smsGoldAmount = 10, smsTotalSpend, smsGiftTier;
            public bool smsUnlimitedDurability;
        }

        public int SmsExperienceMultiplier => smsExperienceMultiplier;
        public int SmsTotalSpend => smsTotalSpend;
        public int SmsGiftTier => smsGiftTier;

        private void ResetOriginalSystemMenu()
        {
            systemMenuPage = SystemMenuPage.None; atlasSelection = 0;
            systemSelection = settingSelection = recordSelection = recordOption = 0;
            recordOptionOpen = systemShopReturn = false;
            marketplaceConfirmChoice = 0;
            systemHelpLines = null; systemHelpElapsed = 0;
        }

        private string SmsGlobalStatePath => Path.Combine(Application.persistentDataPath, "gujian-sms-global.json");

        private void LoadSmsGlobalState()
        {
            if (smsGlobalStateLoaded) return;
            smsGlobalStateLoaded = true;
            if (!File.Exists(SmsGlobalStatePath)) return;
            try
            {
                var state = JsonUtility.FromJson<SmsGlobalState>(File.ReadAllText(SmsGlobalStatePath));
                if (state == null || state.version != 1) return;
                smsExperienceMultiplier = Mathf.Clamp(state.experienceMultiplier, 1, 4);
                smsGoldAmount = Mathf.Clamp(state.goldAmount, 10, 30);
                smsTotalSpend = Mathf.Max(0, state.totalSpend);
                smsUnlimitedDurability = state.unlimitedDurability;
                allStagesUnlocked = state.allStagesUnlocked;
            }
            catch (IOException) { }
            catch (ArgumentException) { }
        }

        private void SaveSmsGlobalState()
        {
            var state = new SmsGlobalState { experienceMultiplier = smsExperienceMultiplier, goldAmount = smsGoldAmount,
                totalSpend = smsTotalSpend, unlimitedDurability = smsUnlimitedDurability, allStagesUnlocked = allStagesUnlocked };
            string temporary = SmsGlobalStatePath + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(state, true));
            if (File.Exists(SmsGlobalStatePath)) File.Replace(temporary, SmsGlobalStatePath, SmsGlobalStatePath + ".bak");
            else File.Move(temporary, SmsGlobalStatePath);
        }

        private void ImportLegacySmsState(string saveJson)
        {
            if (File.Exists(SmsGlobalStatePath) || string.IsNullOrEmpty(saveJson)) return;
            try
            {
                var legacy = JsonUtility.FromJson<LegacySmsSaveData>(saveJson);
                if (legacy == null) return;
                smsExperienceMultiplier = Mathf.Clamp(legacy.smsExperienceMultiplier, 1, 4);
                smsGoldAmount = Mathf.Clamp(legacy.smsGoldAmount, 10, 30);
                smsTotalSpend = Mathf.Max(0, legacy.smsTotalSpend);
                smsUnlimitedDurability = legacy.smsUnlimitedDurability;
                smsGiftTier = Mathf.Clamp(legacy.smsGiftTier, 0, 3);
                SaveSmsGlobalState();
            }
            catch (ArgumentException) { }
        }

        private int LoadSmsGiftTier(int slot)
        {
            string path = RecordPath(Mathf.Clamp(slot, 0, 1));
            if (!File.Exists(path)) return 0;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                return data == null ? 0 : Mathf.Clamp(data.smsGiftTier, 0, 3);
            }
            catch (IOException) { return 0; }
            catch (ArgumentException) { return 0; }
        }

        private void PersistSmsGiftTier(int slot, int tier)
        {
            string path = RecordPath(Mathf.Clamp(slot, 0, 1));
            if (!File.Exists(path)) return;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data == null) return;
                data.smsGiftTier = Mathf.Clamp(tier, 0, 3);
                string temporary = path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
                if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
                else File.Move(temporary, path);
            }
            catch (IOException) { }
            catch (ArgumentException) { }
        }

        private string RecordPath(int slot)
        {
            string name = slot == 0 ? saveFileName : Path.Combine(Path.GetDirectoryName(saveFileName) ?? "",
                Path.GetFileNameWithoutExtension(saveFileName) + "-slot1" + Path.GetExtension(saveFileName));
            return Path.Combine(Application.persistentDataPath, name);
        }

        private void RefreshRecordNames()
        {
            for (int i = 0; i < recordNames.Length; i++)
            {
                string path = RecordPath(i);
                recordNames[i] = "";
                if (!File.Exists(path)) continue;
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                recordNames[i] = data == null ? "" : string.IsNullOrEmpty(data.recordName) ? "存档点" : data.recordName;
            }
        }

        private void OpenSystemSubpage(SystemMenuPage page)
        {
            systemMenuPage = page;
            if (page == SystemMenuPage.Settings) settingSelection = 0;
            if (page == SystemMenuPage.Help) { systemHelpLines = null; systemHelpElapsed = 0; }
            if (page == SystemMenuPage.Records)
            {
                recordSelection = activeRecordIndex;
                recordOption = 0; recordOptionOpen = false;
                RefreshRecordNames();
            }
        }

        private void ActivateSystemSelection()
        {
            systemSelection = Mathf.Clamp(atlasSelection, 0, 5);
            switch (systemSelection)
            {
                case 0:
                    if (!allStagesUnlocked) ShowOriginalMessage("尚未开启！", 0, false);
                    else OpenSystemSubpage(SystemMenuPage.Marketplace);
                    break;
                case 1:
                    OpenShop(0); systemShopReturn = true;
                    atlasPage = 5; atlasSelection = 0; atlasScroll = Vector2.zero;
                    break;
                case 2: OpenSystemSubpage(SystemMenuPage.Records); break;
                case 3: OpenSystemSubpage(SystemMenuPage.Settings); break;
                case 4: OpenSystemSubpage(SystemMenuPage.Help); break;
                case 5:
                    ResetOriginalSystemMenu(); menu = scriptedMenu = false;
                    AtTitle = true; PlayMusic("cover.mid");
                    break;
            }
        }

        private bool HandleSystemMenuKey(int code)
        {
            if (systemMenuPage == SystemMenuPage.None) return false;
            if (systemMenuPage == SystemMenuPage.Settings)
            {
                if (code == 0 || code == 1) settingSelection = settingSelection == 0 ? 1 : 0;
                else if (code == 2 || code == 3)
                {
                    int delta = code == 2 ? -1 : 1;
                    if (settingSelection == 0) ChangeSoundVolume(delta);
                    else SetAutoRecoverPercent(autoRecoverPercent + delta * 10);
                }
                else if (code == 9 || code == 10) systemMenuPage = SystemMenuPage.None;
                return true;
            }
            if (systemMenuPage == SystemMenuPage.Help)
            {
                if (code == 9 || code == 10) { systemMenuPage = SystemMenuPage.None; systemHelpLines = null; }
                return true;
            }
            if (systemMenuPage == SystemMenuPage.Marketplace)
            {
                int optionCount = MarketplaceOptionCount();
                marketplaceSelection = Mathf.Clamp(marketplaceSelection, 0, optionCount - 1);
                if (code == 0 || code == 1) marketplaceSelection = (marketplaceSelection + (code == 1 ? 1 : optionCount - 1)) % optionCount;
                else if (code == 4 || code == 9) OpenMarketplaceSelection();
                else if (code == 10) systemMenuPage = SystemMenuPage.None;
                return true;
            }
            if (systemMenuPage == SystemMenuPage.PurchaseFailure)
            {
                if (code == 9 || code == 10) systemMenuPage = SystemMenuPage.Marketplace;
                return true;
            }
            if (systemMenuPage == SystemMenuPage.MarketplaceConfirm)
            {
                if (code == 2 || code == 3) marketplaceConfirmChoice = marketplaceConfirmChoice == 0 ? 1 : 0;
                else if (code == 10) systemMenuPage = SystemMenuPage.Marketplace;
                else if (code == 4 || code == 9)
                {
                    if (marketplaceConfirmChoice == 0) CompleteMarketplaceSelection();
                    else systemMenuPage = SystemMenuPage.Marketplace;
                }
                return true;
            }
            if (systemMenuPage == SystemMenuPage.MarketplaceResult)
            {
                if (code == 4 || code == 9 || code == 10) systemMenuPage = SystemMenuPage.Marketplace;
                return true;
            }
            if (code == 0 || code == 1)
            {
                if (recordOptionOpen) recordOption = recordOption == 0 ? 1 : 0;
                else recordSelection = recordSelection == 0 ? 1 : 0;
            }
            else if (code == 10)
            {
                if (recordOptionOpen) recordOptionOpen = false;
                else systemMenuPage = SystemMenuPage.None;
            }
            else if (code == 4 || code == 9)
            {
                if ((recordNames[recordSelection] ?? "").Length > 1)
                {
                    if (!recordOptionOpen) { recordOptionOpen = true; recordOption = 0; }
                    else if (recordOption == 0)
                    {
                        activeRecordIndex = recordSelection;
                        recordOptionOpen = false; systemMenuPage = SystemMenuPage.None;
                        Load();
                    }
                    else
                    {
                        // Deleting one slot leaves the other slot and its save data intact.
                        File.Delete(RecordPath(recordSelection));
                        recordOptionOpen = false; RefreshRecordNames();
                    }
                }
                else
                {
                    int selected = recordSelection;
                    Begin(); activeRecordIndex = selected; RestoreSmsGiftTier(LoadSmsGiftTier(selected));
                }
            }
            return true;
        }

        private void PrepareSystemText()
        {
            int size = originalUi ? originalUi.dialogueFontSize : 18;
            if (systemTextStyle != null && systemTextStyle.fontSize == size && systemTextStyle.font == font) return;
            systemHelpLines = null;
            systemTextStyle = new GUIStyle { font = font, fontSize = size, richText = false, wordWrap = false,
                alignment = TextAnchor.UpperLeft, padding = new RectOffset(0, 0, 0, 0) };
            systemTextStyle.normal.textColor = Color.white;
        }

        private void SystemLabel(Rect bounds, string text, Color color, bool centered = false)
        {
            PrepareSystemText();
            systemTextStyle.normal.textColor = color;
            systemTextStyle.alignment = centered ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
            GUI.Label(bounds, text, systemTextStyle);
            systemTextStyle.alignment = TextAnchor.UpperLeft;
        }

        private void MainUiFrame(int frame, Vector2? origin = null)
        {
            OriginalGui.Frame(originalUi.mainMenu, originalUi.mainMenuSheets, frame, origin ?? new Vector2(160, 120));
        }

        private void DrawSystemSubpage()
        {
            PrepareSystemText();
            if (systemMenuPage == SystemMenuPage.Settings) DrawOriginalSettings();
            else if (systemMenuPage == SystemMenuPage.Help) DrawOriginalHelp();
            else if (systemMenuPage == SystemMenuPage.Records) DrawOriginalRecords();
            else if (systemMenuPage == SystemMenuPage.Marketplace) DrawOriginalMarketplace();
            else if (systemMenuPage == SystemMenuPage.MarketplaceConfirm) DrawMarketplaceConfirm();
            else if (systemMenuPage == SystemMenuPage.MarketplaceResult) DrawMarketplaceResult();
            else
            {
                MainUiFrame(12);
                SystemLabel(new Rect(20, 46, 280, 147), "发送失败。", Color.white);
            }
            if (systemMenuPage == SystemMenuPage.PurchaseFailure)
            {
                if (AtlasClick(new Rect(5, 211, 52, 23))) HandleOriginalMenuKey(9);
            }
            if (AtlasClick(new Rect(263, 211, 52, 23))) HandleOriginalMenuKey(10);
        }

        private void DrawOriginalSettings()
        {
            MainUiFrame(3);
            MainUiFrame(SoundVolumeStep + 5);
            MainUiFrame(4, new Vector2(160, 120 + settingSelection * 40));
            OriginalGui.Number(originalUi.menuNumbers, autoRecoverPercent, new Vector2(195, 171), 10, 1, -2);
            var arrow = originalUi.settingsArrow;
            if (arrow)
            {
                int tick = originalUiTicks % 3;
                GUI.DrawTexture(new Rect(218 + tick, 128 + settingSelection * 40, arrow.width, arrow.height), arrow);
                var matrix = GUI.matrix;
                GUI.matrix = matrix * Matrix4x4.TRS(new Vector3(91 - tick + arrow.width, 128 + settingSelection * 40, 0), Quaternion.identity, new Vector3(-1, 1, 1));
                GUI.DrawTexture(new Rect(0, 0, arrow.width, arrow.height), arrow);
                GUI.matrix = matrix;
            }
            for (int row = 0; row < 2; row++)
            {
                int selected = row;
                if (AtlasClick(new Rect(90, 124 + row * 40, 50, 30))) { settingSelection = selected; HandleOriginalMenuKey(2); }
                if (AtlasClick(new Rect(190, 124 + row * 40, 50, 30))) { settingSelection = selected; HandleOriginalMenuKey(3); }
            }
            if (AtlasClick(new Rect(5, 211, 52, 23))) HandleOriginalMenuKey(9);
        }

        private void DrawOriginalHelp()
        {
            MainUiFrame(12);
            if (systemHelpLines == null)
            {
                Func<string, int> measure = text => (int)systemTextStyle.CalcSize(new GUIContent(text)).x;
                int characterWidth = measure("你");
                systemHelpLineHeight = Mathf.CeilToInt(font.lineHeight * (systemTextStyle.fontSize / (float)Mathf.Max(1, font.fontSize))) + 2;
                systemHelpLines = OriginalDialogueText.Format(OriginalHelpText, 280, characterWidth, measure);
                systemHelpOffset = systemHelpLines.Length * systemHelpLineHeight > 147 ? systemHelpLineHeight : 0;
                systemHelpElapsed = 0;
            }
            GUI.BeginGroup(new Rect(20, 46, 281, 148));
            for (int i = 0; i < systemHelpLines.Length; i++)
            {
                float x = 0;
                foreach (var span in systemHelpLines[i].spans)
                {
                    // Original StyledText defaults to white with a one-pixel black edge.
                    var old = systemTextStyle.richText; systemTextStyle.richText = true;
                    OriginalGui.StyledLabel(new Rect(x, systemHelpOffset + i * systemHelpLineHeight, span.width + 4, systemHelpLineHeight + 4), span.drawRuns, systemTextStyle);
                    systemTextStyle.richText = old; x += span.width;
                }
            }
            GUI.EndGroup();
            if (AtlasClick(new Rect(5, 211, 52, 23))) HandleOriginalMenuKey(9);
        }

        private void TickOriginalSystemMenu(float delta)
        {
            if (!menu || systemMenuPage != SystemMenuPage.Help || systemHelpLines == null || systemHelpLines.Length * systemHelpLineHeight <= 147) return;
            systemHelpElapsed += delta;
            while (systemHelpElapsed >= .08f)
            {
                systemHelpElapsed -= .08f;
                systemHelpOffset -= systemHelpLineHeight / 10;
                if (systemHelpOffset <= -systemHelpLines.Length * systemHelpLineHeight) systemHelpOffset = 147;
            }
        }

        private void DrawOriginalRecords()
        {
            OriginalGui.Animation(originalUi.mainMenu, originalUi.mainMenuSheets, recordSelection, originalUiTicks, new Vector2(160, 120));
            int height = Mathf.CeilToInt(systemTextStyle.lineHeight);
            for (int i = 0; i < 2; i++)
            {
                float y = 42 + i * 78;
                string name = recordNames[i] ?? "";
                if (name.Length > 1)
                {
                    SystemLabel(new Rect(0, y + 36 - height, 320, height + 4), "存档点", Color.green, true);
                    SystemLabel(new Rect(0, y + 40, 320, height + 4), name, i == recordSelection ? Color.green : Color.white, true);
                }
                else SystemLabel(new Rect(0, y + (76 - height) / 2, 320, height + 4), "无存档", i == recordSelection ? Color.green : Color.white, true);
                if (!recordOptionOpen && AtlasClick(new Rect(16, y, 288, 76))) recordSelection = i;
            }
            if (recordOptionOpen)
            {
                int optionHeight = height + 6;
                int width = Mathf.CeilToInt(systemTextStyle.CalcSize(new GUIContent("读取")).x) + 24;
                int x = (320 - width) / 2, y = recordSelection * 78 + 80 - optionHeight;
                string[] names = { "读取", "删除" };
                for (int i = 0; i < 2; i++)
                {
                    var bounds = new Rect(x, y - 2 + i * optionHeight, width, optionHeight);
                    OriginalGui.TiledPanel(originalUi.messagePanel, bounds);
                    SystemLabel(new Rect(x, y + i * optionHeight + 3, width, height + 4), names[i], i == recordOption ? new Color32(255, 108, 0, 255) : Color.white, true);
                    if (AtlasClick(bounds)) { recordOption = i; HandleOriginalMenuKey(9); }
                }
            }
            if (AtlasClick(new Rect(5, 211, 52, 23))) HandleOriginalMenuKey(9);
        }

        private void DrawOriginalMarketplace()
        {
            MainUiFrame(16);
            int optionCount = MarketplaceOptionCount();
            marketplaceSelection = Mathf.Clamp(marketplaceSelection, 0, optionCount - 1);
            int optionTop = optionCount == 5 ? 43 : optionCount == 4 ? 58 : 74;
            for (int i = 0; i < optionCount; i++)
            {
                var bounds = new Rect(50, optionTop + i * 31, 80, 28);
                OriginalGui.TiledPanel(originalUi.messagePanel, bounds);
                bool gift = IsMarketplaceGiftOption(i);
                bool experience = IsMarketplaceExperienceOption(i);
                if (gift)
                {
                    var giftImage = originalUi.marketplaceOptions[marketplaceSelection == i ? 10 : 4];
                    var suffix = originalUi.marketplaceOptions[marketplaceSelection == i ? 11 : 5];
                    OriginalGui.Region(giftImage, new Rect(smsGiftTier * 25, 0, 25, 13), new Rect(65, optionTop + i * 31 + 9, 25, 13));
                    if (suffix) GUI.DrawTexture(new Rect(90, optionTop + i * 31 + 9, suffix.width, suffix.height), suffix);
                }
                else
                {
                    int imageIndex = experience ? 3 : i;
                    var image = originalUi.marketplaceOptions[imageIndex + (marketplaceSelection == i ? 6 : 0)];
                    if (image) GUI.DrawTexture(new Rect(65, optionTop + i * 31 + 9, image.width, image.height), image);
                }
                if (AtlasClick(bounds)) marketplaceSelection = i;
            }
            bool selectedGift = IsMarketplaceGiftOption(marketplaceSelection);
            bool selectedExperience = IsMarketplaceExperienceOption(marketplaceSelection);
            int infoIndex = selectedGift ? 4 : selectedExperience ? 3 : marketplaceSelection;
            var info = originalUi.marketplaceInfo[infoIndex];
            if (info) GUI.DrawTexture(new Rect(173, selectedGift ? 77 : selectedExperience ? 64 : 57, info.width, info.height), info);
            if (marketplaceSelection == 0 && originalUi.marketplaceInfo[5])
            {
                var extra = originalUi.marketplaceInfo[5];
                GUI.DrawTexture(new Rect(173, 64 + info.height, extra.width, extra.height), extra);
            }
            AtlasLabel(173, 170, 142, 38, MarketplaceStatusText());
            if (AtlasClick(new Rect(5, 211, 52, 23))) HandleOriginalMenuKey(9);
        }

        private string MarketplaceStatusText()
        {
            if (IsMarketplaceGiftOption(marketplaceSelection))
            {
                int threshold = smsGiftTier == 0 ? 6 : smsGiftTier == 1 ? 12 : 20;
                return "本地模拟：" + (smsGiftTier >= 3 ? "礼包已全部领取" : "累计消费 " + smsTotalSpend + "/" + threshold + " 元\n可领取后按确认查看礼包");
            }
            if (IsMarketplaceExperienceOption(marketplaceSelection)) return "本地模拟：升级经验提升至" + (smsExperienceMultiplier + 1) + "倍";
            switch (marketplaceSelection)
            {
                case 0: return "本地模拟：" + smsGoldAmount + "金 / " + (smsGoldAmount * 10000) + "银\n下次再增加2金";
                case 1: return "本地模拟：按当前等级赠送适用武器";
                default: return "本地模拟：满状态、满怒气、耐久不损";
            }
        }

        private int MarketplaceOptionCount() => 3 + (smsExperienceMultiplier < 4 ? 1 : 0) + (smsGiftTier < 3 ? 1 : 0);
        private bool IsMarketplaceExperienceOption(int index) => smsExperienceMultiplier < 4 && index == 3;
        private bool IsMarketplaceGiftOption(int index) => smsGiftTier < 3 && index == (smsExperienceMultiplier < 4 ? 4 : 3);

        private void OpenMarketplaceSelection()
        {
            marketplaceSelection = Mathf.Clamp(marketplaceSelection, 0, MarketplaceOptionCount() - 1);
            if (IsMarketplaceGiftOption(marketplaceSelection))
            {
                int threshold = smsGiftTier == 0 ? 6 : smsGiftTier == 1 ? 12 : 20;
                if (smsGiftTier >= 3 || smsTotalSpend < threshold) return;
            }
            marketplaceConfirmChoice = 0;
            systemMenuPage = SystemMenuPage.MarketplaceConfirm;
        }

        private string MarketplaceConfirmationText()
        {
            if (IsMarketplaceGiftOption(marketplaceSelection))
            {
                int tier = smsGiftTier, limit = tier == 0 ? 6 : tier == 1 ? 12 : 20;
                return "领取累计消费满" + limit + "元礼包？\n" + (tier == 0 ? "隐藏武器、药品和5万银两" : tier == 1 ? "隐藏武器、药品和15万银两" : "35万银两及永久耐久不损");
            }
            if (IsMarketplaceExperienceOption(marketplaceSelection))
                return "模拟升级经验倍率：" + smsExperienceMultiplier + "倍 → " + (smsExperienceMultiplier + 1) + "倍。\n对主角、剑灵吞噬经验及脚本／战斗经验生效。累计消费增加2元。";
            switch (marketplaceSelection)
            {
                case 0: return "模拟购买 " + smsGoldAmount + " 金（" + (smsGoldAmount * 10000) + " 银）。\n累计消费增加2元；下次金币增加2金，最高30金。";
                case 1: return "模拟领取当前等级可用的赠送武器。\n累计消费增加1元。";
                case 2: return "模拟使用场景特权：恢复气血和怒气，修复耐久并开启耐久不损。\n累计消费增加1元。";
                default: return "";
            }
        }

        private void DrawMarketplaceConfirm()
        {
            MainUiFrame(13);
            SystemLabel(new Rect(28, 58, 264, 100), MarketplaceConfirmationText(), Color.white);
            string[] options = { "确认", "返回" };
            for (int i = 0; i < options.Length; i++)
            {
                var bounds = new Rect(73 + i * 91, 170, 82, 29);
                OriginalGui.TiledPanel(originalUi.messagePanel, bounds);
                SystemLabel(new Rect(bounds.x, bounds.y + 5, bounds.width, 20), options[i], i == marketplaceConfirmChoice ? new Color32(255, 108, 0, 255) : Color.white, true);
                if (AtlasClick(bounds)) { marketplaceConfirmChoice = i; HandleSystemMenuKey(9); }
            }
        }

        private void DrawMarketplaceResult()
        {
            MainUiFrame(12);
            SystemLabel(new Rect(24, 62, 272, 112), marketplaceResultText, Color.white);
        }

        private string marketplaceResultText;

        private void CompleteMarketplaceSelection()
        {
            bool isGiftOption = IsMarketplaceGiftOption(marketplaceSelection);
            bool claimedGift = false;
            bool experiencePurchase = IsMarketplaceExperienceOption(marketplaceSelection);
            bool completed = !isGiftOption;
            if (isGiftOption)
            {
                claimedGift = GrantMarketplaceGift();
                completed = claimedGift;
                marketplaceResultText = claimedGift ? "礼包领取成功，奖励已加入当前存档。" : "不满足领取条件或礼包已领取。";
            }
            else if (experiencePurchase)
            {
                smsExperienceMultiplier = Mathf.Min(4, smsExperienceMultiplier + 1);
                smsTotalSpend += 2;
                if (smsExperienceMultiplier >= 4) marketplaceSelection = 0;
                marketplaceResultText = "模拟购买成功，当前经验倍率为 " + smsExperienceMultiplier + " 倍。";
            }
            else switch (marketplaceSelection)
            {
                case 0:
                    money += smsGoldAmount * 10000;
                    smsGoldAmount = Mathf.Min(30, smsGoldAmount + 2);
                    smsTotalSpend += 2;
                    marketplaceResultText = "模拟购买成功，已获得银两。请保存进度。";
                    break;
                case 1:
                    int[] weaponIds = { 5, 8, 11, 14, 17, 20 };
                    int index = Mathf.Clamp((level - 1) / 10, 0, weaponIds.Length - 1);
                    EquipmentBag.Add(EquipmentInstance.Create(weaponIds[index]));
                    smsTotalSpend += 1;
                    marketplaceResultText = "模拟领取成功，赠送武器已加入背包。请保存进度。";
                    break;
                case 2:
                    if (Role)
                    {
                        if (Role.hp <= 0 && fighters.TryGetValue(-1, out var fighter)) { fighter.dead = false; Role.ResetAction(); }
                        Role.hp = Role.maxHp; energy = 2000;
                        infiniteDurability = true; maximumPower = true;
                        if (Equipped != null) { durability = Equipped.MaximumDurability; Equipped.durability = durability; }
                        if (integrated) SetSpiritDurability(SpiritAttribute(1));
                    }
                    smsTotalSpend += 1;
                    marketplaceResultText = "模拟使用成功，本场景特权已生效。离开场景后会按原版规则结束临时效果。";
                    break;
            }
            if (completed) SaveSmsGlobalState();
            marketplaceSelection = Mathf.Clamp(marketplaceSelection, 0, MarketplaceOptionCount() - 1);
            systemMenuPage = completed ? SystemMenuPage.MarketplaceResult : SystemMenuPage.Marketplace;
        }

        private bool GrantMarketplaceGift()
        {
            int threshold = smsGiftTier == 0 ? 6 : smsGiftTier == 1 ? 12 : 20;
            if (smsGiftTier >= 3 || smsTotalSpend < threshold) return false;
            if (smsGiftTier == 0)
            {
                money += 50000;
                foreach (var pair in new[] { new[] { 3, 20 }, new[] { 7, 20 }, new[] { 16, 3 }, new[] { 17, 3 }, new[] { 18, 3 } }) Inventory.Add(pair[0], pair[1]);
                EquipmentBag.Add(EquipmentInstance.Create(27));
            }
            else if (smsGiftTier == 1)
            {
                money += 150000; EquipmentBag.Add(EquipmentInstance.Create(29));
                foreach (var pair in new[] { new[] { 9, 2 }, new[] { 10, 5 }, new[] { 17, 2 }, new[] { 18, 2 }, new[] { 19, 2 }, new[] { 20, 2 }, new[] { 21, 2 }, new[] { 22, 2 } }) Inventory.Add(pair[0], pair[1]);
            }
            else { money += 350000; smsUnlimitedDurability = true; }
            smsGiftTier++;
            marketplaceSelection = Mathf.Max(0, marketplaceSelection - 1);
            PersistSmsGiftTier(activeRecordIndex, smsGiftTier);
            return true;
        }

        private void RestoreSmsGiftTier(int giftTier)
        {
            smsGiftTier = Mathf.Clamp(giftTier, 0, 3);
        }
    }
}
