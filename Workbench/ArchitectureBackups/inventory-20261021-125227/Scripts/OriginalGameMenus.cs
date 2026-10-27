using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private int menuPage, shopType, missionStageIndex, areaIndex;
        private int menuInputFrame = -1;
        private bool scriptedMenu;
        private bool worldMapOpen;
        private bool itemConfirming;
        private int itemConfirmId = -1;
        private int itemConfirmStack = -1;
        private int itemConfirmChoice;
        private string branchMissionTitle, branchMissionInfo;
        private readonly bool[] openAreas = new bool[13];
        private static readonly string[] AreaNames = { "藏剑山庄", "无双城", "紫草坞", "清溪村", "元宝湾", "潮音洞", "栖霞镇", "巨鲸帮", "顺风码头", "戾剑境", "剑冢", "无双城", "巨鲸帮" };
        private static readonly string[] AreaScripts = { "101", "334", "400", "500", "600", "700", "800", "900", "1000", "1100", "200", "300", "900" };
        private void SetAreaIndex(int index)
        {
            areaIndex = Mathf.Clamp(index, 0, AreaNames.Length - 1);
            openAreas[areaIndex] = true;
        }
        public bool TravelToArea(int index)
        {
            if (index < 0 || index >= AreaScripts.Length) return false;
            if (!openAreas[index])
            {
                // GameCanvas.handleKey_map uses script_showMessage for a locked area.
                ShowOriginalMessage("该区域尚未开放!", 0, false);
                return false;
            }
            // Original initGameMap marks the next scene entry as arriving from the world map.
            Machine.Values["stageID"] = 1;
            menu = worldMapOpen = false;
            Machine.Enqueue(AreaScripts[index]);
            return true;
        }
        [SerializeField] private AudioSource music;
        private const string SoundVolumeKey = "gujian.sound-volume-step";
        private static readonly float[] SoundVolumeLevels = { 0f, .25f, .5f, .75f, 1f };
        private int soundVolumeStep = -1;

        private int SoundVolumeStep
        {
            get
            {
                if (soundVolumeStep < 0) soundVolumeStep = Mathf.Clamp(PlayerPrefs.GetInt(SoundVolumeKey, 2), 0, SoundVolumeLevels.Length - 1);
                return soundVolumeStep;
            }
        }

        private void ApplySoundVolume()
        {
            float volume = SoundVolumeLevels[SoundVolumeStep];
            AudioListener.volume = volume;
            if (music)
            {
                if (volume <= 0f) music.Stop();
                else if (music.clip && !music.isPlaying) music.Play();
            }
        }

        private void ChangeSoundVolume(int delta)
        {
            int next = Mathf.Clamp(SoundVolumeStep + delta, 0, SoundVolumeLevels.Length - 1);
            if (next == SoundVolumeStep) return;
            soundVolumeStep = next;
            PlayerPrefs.SetInt(SoundVolumeKey, soundVolumeStep);
            PlayerPrefs.Save();
            ApplySoundVolume();
        }

        private string SoundVolumeLabel => Mathf.RoundToInt(SoundVolumeLevels[SoundVolumeStep] * 100f) + "%";

        private void PlayMusic(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            switch (name) { case "0": name = "zero"; break; case "1": name = "one"; break; case "2": case "3": case "5": name = "two"; break; case "4": name = "four"; break; }
            var clip = Resources.Load<AudioClip>("Original/Audio/" + name);
            if (!clip) return; // The original MIDI is retained alongside the offline GM rendering.
            if (!music) throw new InvalidOperationException("Startup scene requires its saved music AudioSource");
            if (music.clip == clip && music.isPlaying) return;
            music.clip = clip;
            ApplySoundVolume();
        }
        private void Equip(int id, bool healDurability)
        {
            if (id < 0) { Role.SetWeaponVisible(true); return; }
            int index = equipment.FindLastIndex(e => e.id == id);
            if (index >= 0) SelectEquipment(index);
        }
        private void ToggleIntegrate()
        {
            if (!Actor(-2)) return;
            if (!integrated)
            {
                if (!infiniteDurability && spiritDurability <= SpiritAttribute(1) / 10) { Toast("剑灵耐久不足"); return; }
                Equipped.durability = durability;
                if (!infiniteDurability) spiritDurability -= SpiritAttribute(1) / 10;
                durability = spiritDurability; equippedSkill = spiritSkill;
                int lv = Actor(-2).level;
                Role.EquipSpiritWeapon(lv < 11 ? "w_97" : lv < 21 ? "w_98" : lv < 31 ? "w_99" : "w_00");
            }
            else { spiritDurability = durability; durability = Equipped.durability; equippedSkill = Equipped.skillId; Role.EquipWeapon(Equipped.Data.image); }
            integrated = !integrated;
            Role.actionBank = integrated ? 27 : 0;
            ResetAttackChain();
            breakHit = false; breakCooldownTicks = 3;
            Actor(-2).hidden = integrated;
            if (fighters.TryGetValue(-1, out var fighter)) fighter.hold = integrated ? 99 : 49;
            ResetSkillCooldown();
            Role.Act(26 + Role.actionBank, true);
            Role.keepLastFrame = false;
            RefreshAttributes(Role, false);
        }
        private object HandleAdditionalCommand(int id, object[] args)
        {
            int N(int i) => i < args.Length ? OriginalScriptMachine.Int(args[i]) : 0;
            string S(int i) => i < args.Length ? Convert.ToString(args[i]) : "";
            switch (id)
            {
                case 0x48:
                    scriptedMenu = false;
                    Machine.Values["stageID"] = 1;
                    SetAreaIndex(N(0));
                    menuPage = 4; menu = worldMapOpen = true; break;
                case 0x57: OpenReinforce(); break;
                case 0x63: OpenShop(N(0)); break;
                case 0x66: SimulateOriginalKey(N(0)); break;
                case 0x67:
                    menu = N(0) != 2; scriptedMenu = menu; menuPage = 0; break;
                case 0x6A: tutorialKeys.Clear(); tutorialKey = N(0); wrongTutorialKey = false; break;
                case 0x6B: return wrongTutorialKey;
                case 0x6C: tutorialKeys.Clear(); tutorialKeys.AddRange(((object[])args[0]).Select(OriginalScriptMachine.Int)); tutorialKey = tutorialKeys.Count > 0 ? tutorialKeys[0] : -1; wrongTutorialKey = false; break;
                case 0x7B: ScriptDestroySword(); break;
                case 0x7D: maximumPower = N(0) != 0; energy = 1000; break;
                case 0x7E: BeginBossCritical(N(0)); break;
                case 0x7F: ToggleIntegrate(); break;
                case 0x80: BeginCriticalKill(N(0) != 0); break;
                case 0x81: UseSkill(4, 1, true); break;
                case 0x82: return durability == 0;
                case 0x83: timerSeconds = N(0); timerScript = S(1); timerPaused = false; break;
                case 0x84: timerPaused = true; break;
                case 0x85: timerPaused = false; break;
                case 0x86: timerScript = null; break;
                case 0x89: return CostOfRepair();
                case 0x94:
                    if (!OpenWeaponPreview(N(0), N(1))) throw new InvalidOperationException("无法预览武器 " + N(0));
                    break;
                case 0x97: return energy == 2000;
                case 0x98: energy = 2000; break;
                default: throw new NotSupportedException("Opcode x" + id.ToString("X2") + " 尚未完成移植");
            }
            return 0;
        }
        private readonly System.Collections.Generic.List<int> tutorialKeys = new System.Collections.Generic.List<int>();
        private void SimulateOriginalKey(int code)
        {
            switch (code)
            {
                case 4: Attack(); break;
                case 9: menu = true; break;
                case 10: menu = false; break;
                case 12: if (equippedSkill >= 5) UseSkill(equippedSkill); break;
                case 14: UseSkill(4); break;
                case 18: ToggleIntegrate(); break;
            }
        }
        public void RespondTutorial(int code)
        {
            if (tutorialKey < 0) return;
            if (tutorialKeys.Count > 0)
            {
                tutorialKeys.Remove(code);
                tutorialKey = tutorialKeys.Count > 0 ? tutorialKeys[0] : -1;
                if (tutorialKey < 0 && Role) Role.ResetAction();
                return;
            }
            wrongTutorialKey = tutorialKey != code; tutorialKey = -1;
            if (!wrongTutorialKey && menu && scriptedMenu)
            {
                if (menuPage == 5 && code == 0) MoveShopSelection(-1);
                else if (code == 3) menuPage = NextTutorialMenuPage();
                else if (code == 6) menu = scriptedMenu = false;
            }
        }
        private int NextTutorialMenuPage() => menuPage == 0 ? 2 : menuPage == 2 ? 1 : menuPage == 1 ? 6 : 0;
        public bool SelectMenuPage(int page)
        {
            if (systemMenuPage != SystemMenuPage.None || SpiritFeedAnimating || worldMapOpen || (page == 6 && !Actor(-2)) || (scriptedMenu && menuPage == 5)) return false;
            if (scriptedMenu && Machine.Running)
            {
                if (tutorialKey != 3 || page != NextTutorialMenuPage()) return false;
                RespondTutorial(3);
            }
            else { if (menuPage == 3) systemSelection = atlasSelection; menuPage = page; }
            atlasPage = menuPage; atlasSelection = menuPage == 3 ? systemSelection : 0; atlasScroll = Vector2.zero;
            menuScroll = Vector2.zero; ResetSpiritMenu(); CancelTrade();
            if (menuPage == 6) EnterSpiritMenu();
            return true;
        }
        public bool CloseMenu()
        {
            if (SpiritFeedAnimating || worldMapOpen) return false;
            if (scriptedMenu && Machine.Running)
            {
                if (tutorialKey != 6) return false;
                RespondTutorial(6);
            }
            ResetOriginalSystemMenu(); ResetSpiritMenu(); menu = scriptedMenu = false; return true;
        }
        private void TickTutorial()
        {
            if (tutorialKey < 0) return;
            int code = -1;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) code = 0;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) code = 1;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) code = 2;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) code = 3;
            else if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space)) code = 4;
            else if (Input.GetKeyDown(KeyCode.Escape)) code = 5;
            else if (Input.GetKeyDown(KeyCode.Return)) code = 6;
            else if (Input.GetKeyDown(KeyCode.K)) code = 7;
            else if (Input.GetKeyDown(KeyCode.L)) code = 8;
            else if (Input.GetKeyDown(KeyCode.I)) code = 9;
            else if (Input.GetKeyDown(KeyCode.O)) code = 10;
            else if (Input.GetKeyDown(KeyCode.P)) code = 11;
            if (code >= 0) RespondTutorial(code);
        }

        // Keyboard input uses original GameCanvas key codes; touch controls map explicitly below.
        private void TickOriginalMenuInput()
        {
            if (!menu || originalMessage || WeaponPreviewVisible) return;
            menuInputFrame = Time.frameCount;
            int code = -1;
            if (Input.GetKeyDown(KeyCode.Escape)) code = 10;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) code = 2;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) code = 3;
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) code = 0;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) code = 1;
            else if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space)) code = 4;
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) code = 9;
            else if (Input.GetKeyDown(KeyCode.P)) code = 18;
            if (code >= 0) HandleOriginalMenuKey(code);
            TickTradeQuantityRepeat();
        }

        private void HandleOriginalMenuKey(int code)
        {
            if (!menu || originalMessage || WeaponPreviewVisible) return;
            menuInputFrame = Time.frameCount;
            if (HandleSystemMenuKey(code) || HandleSpiritMenuKey(code)) return;
            if (code == 18)
            {
                if (menuPage == 5 && shopType >= 2 && !shopSelling)
                {
                    var rows = OriginalAssets.Table<EquipmentDefinition>("equip")
                        .Where(e => e.grade < 3 && e.level <= level).Select(e => e.id).ToArray();
                    if (rows.Length > 0) PreviewShopWeapon(rows[Mathf.Clamp(atlasSelection, 0, rows.Length - 1)]);
                }
                else if (menuPage != 5) StartRoleSkillPreview();
                return;
            }
            bool up = code == 0, down = code == 1, left = code == 2, right = code == 3;
            bool confirm = code == 4 || code == 9, back = code == 10;
            if (itemConfirming)
            {
                if (back) CancelItemConfirmation();
                else if (left) itemConfirmChoice = 0;
                else if (right) itemConfirmChoice = 1;
                else if (confirm) ConfirmItemConfirmation();
                return;
            }
            if (TradeConfirming)
            {
                if (back) CancelTrade();
                else if (left || right) trade.choice = trade.choice == 0 ? 1 : 0;
                else if (up || down) MoveTradeQuantity(down ? 1 : -1);
                else if (confirm) { if (trade.choice == 0) ConfirmTrade(); else CancelTrade(); }
                return;
            }
            if (worldMapOpen)
            {
                if (code >= 0 && code <= 3) MoveWorldMapSelection(code);
                if (confirm) TravelToArea(areaIndex);
                return;
            }
            if (menuPage == 5)
            {
                if (left) { SetShopSelling(false); return; }
                if (right) { SetShopSelling(true); return; }
            }
            if (back)
            {
                if (menuPage == 5 && systemShopReturn)
                {
                    systemShopReturn = false; systemSelection = 1;
                    SelectMenuPage(3); return;
                }
                CloseMenu(); return;
            }
            if (left || right)
            {
                int[] pages = { 7, 2, 1, 6, 0, 3 };
                int current = Array.IndexOf(pages, menuPage);
                if (current < 0) current = 0;
                do { current = (current + (right ? 1 : -1) + pages.Length) % pages.Length; }
                while (pages[current] == 6 && !Actor(-2));
                SelectMenuPage(pages[current]);
                return;
            }
            int count = MenuInputCount();
            if (up || down)
            {
                if (menuPage == 0)
                {
                    atlasBranch = down;
                    atlasScroll = Vector2.zero;
                    return;
                }
                if (count > 0) atlasSelection = (atlasSelection + (down ? 1 : count - 1)) % count;
                return;
            }
            if (confirm) ActivateMenuSelection();
        }

        private void OpenItemConfirmation(int id, int stack)
        {
            if (ItemStackQuantity(id, stack) <= 0) return;
            itemConfirmId = id;
            itemConfirmStack = stack;
            itemConfirmChoice = 0;
            itemConfirming = true;
        }

        private void CancelItemConfirmation()
        {
            itemConfirming = false;
            itemConfirmId = -1;
            itemConfirmStack = -1;
            itemConfirmChoice = 0;
        }

        private void ConfirmItemConfirmation()
        {
            int id = itemConfirmId;
            int stack = itemConfirmStack;
            int quantity = ItemStackQuantity(id, stack);
            int choice = itemConfirmChoice;
            CancelItemConfirmation();
            if (quantity <= 0) return;
            if (choice == 0)
            {
                UseItemStack(id, stack);
                return;
            }
            inventory[id] = Mathf.Max(0, ItemCount(id) - quantity);
            Toast("丢弃了" + OriginalAssets.Table<ItemDefinition>("item").First(i => i.id == id).name + " ×" + quantity);
        }

        private void MoveWorldMapSelection(int direction)
        {
            int eventIndex = Machine.Get("event");
            int next = areaIndex;
            switch (direction)
            {
                case 0:
                    switch (areaIndex)
                    {
                        case 0: next = 9; break;
                        case 1: case 11: next = 3; break;
                        case 2: next = eventIndex >= 1050 ? 11 : 1; break;
                        case 3: next = 8; break;
                        case 4: next = 5; break;
                        case 5: next = 6; break;
                    }
                    break;
                case 1:
                    switch (areaIndex)
                    {
                        case 1: case 11: next = 2; break;
                        case 3: next = eventIndex >= 1050 ? 11 : 1; break;
                        case 5: next = 4; break;
                        case 6: next = 5; break;
                        case 8: next = 3; break;
                        case 9: next = 0; break;
                    }
                    break;
                case 2:
                    switch (areaIndex)
                    {
                        case 1: case 11: next = 9; break;
                        case 2: next = 0; break;
                        case 4: next = eventIndex >= 1050 ? 11 : 1; break;
                        case 5: next = 3; break;
                        case 6: next = 8; break;
                        case 8: next = eventIndex > 1071 ? 12 : 7; break;
                        case 10: next = 2; break;
                    }
                    break;
                case 3:
                    switch (areaIndex)
                    {
                        case 0: next = 2; break;
                        case 1: case 11: next = 4; break;
                        case 3: next = 5; break;
                        case 7: case 12: next = 8; break;
                        case 8: next = 6; break;
                        case 9: next = eventIndex >= 1050 ? 11 : 1; break;
                    }
                    break;
            }
            areaIndex = next;
        }

        private int MenuInputCount()
        {
            if (menuPage == 3) return 6;
            if (menuPage == 2) return equipment.Count;
            if (menuPage == 1) return inventory.Sum(p => ItemStackCount(p.Key));
            if (menuPage == 6) return spiritFoodChoosing ? SpiritFoodCandidates().Length : 0;
            if (menuPage == 5)
            {
                if (shopType < 2) return shopSelling ? ShopItemRows().Length : OriginalAssets.Table<ItemDefinition>("item").Count(i => ItemForSale(i.id));
                return shopSelling ? Mathf.Max(0, equipment.Count - 1) : OriginalAssets.Table<EquipmentDefinition>("equip").Count(e => e.grade < 3 && e.level <= level);
            }
            return 0;
        }

        private void SetShopSelling(bool selling)
        {
            if (shopSelling == selling) return;
            if (selling && MenuInputCountForShop(selling) == 0)
            {
                ShowOriginalMessage("无可出售物品", 0, false);
                return;
            }
            if (shopSelling) shopSellSelection = atlasSelection;
            else shopBuySelection = atlasSelection;
            shopSelling = selling;
            atlasSelection = Mathf.Clamp(selling ? shopSellSelection : shopBuySelection, 0, Mathf.Max(0, MenuInputCountForShop(selling) - 1));
            if (selling) shopSellSelection = atlasSelection;
            else shopBuySelection = atlasSelection;
            atlasScroll = Vector2.zero;
            menuScroll = Vector2.zero;
        }

        private int MenuInputCountForShop(bool selling)
        {
            if (shopType < 2) return selling ? ShopItemRows().Length : OriginalAssets.Table<ItemDefinition>("item").Count(i => ItemForSale(i.id));
            return selling ? Mathf.Max(0, equipment.Count - 1) : OriginalAssets.Table<EquipmentDefinition>("equip").Count(e => e.grade < 3 && e.level <= level);
        }

        private void ActivateMenuSelection()
        {
            if (menuPage == 2 && equipment.Count > 0)
            {
                if (equipment[atlasSelection].Data.level <= level) SelectEquipment(atlasSelection);
                return;
            }
            if (menuPage == 1)
            {
                var stacks = inventory.OrderBy(p => p.Key).SelectMany(p => Enumerable.Range(0, ItemStackCount(p.Key)).Select(stack => new[] { p.Key, stack })).ToArray();
                if (stacks.Length > 0)
                {
                    var selected = stacks[Mathf.Clamp(atlasSelection, 0, stacks.Length - 1)];
                    OpenItemConfirmation(selected[0], selected[1]);
                }
                return;
            }
            if (menuPage == 6)
            {
                ActivateSpiritFoodSelection();
                return;
            }
            if (menuPage == 3)
            {
                ActivateSystemSelection();
                return;
            }
            if (menuPage == 5)
            {
                if (shopType < 2)
                {
                    var rows = shopSelling ? ShopItemRows() : OriginalAssets.Table<ItemDefinition>("item").Where(i => ItemForSale(i.id)).Select(i => new[] { i.id, -1 }).ToArray();
                    if (rows.Length == 0) return;
                    var row = rows[Mathf.Clamp(atlasSelection, 0, rows.Length - 1)];
                    SelectItemTrade(row[0], shopSelling, row[1]);
                }
                else
                {
                    var itemIds = shopSelling ? Enumerable.Range(0, equipment.Count).Where(i => i != equippedIndex).ToArray() : OriginalAssets.Table<EquipmentDefinition>("equip").Where(e => e.grade < 3 && e.level <= level).Select(e => e.id).ToArray();
                    if (itemIds.Length > 0) SelectWeaponTrade(itemIds[Mathf.Clamp(atlasSelection, 0, itemIds.Length - 1)], shopSelling);
                }
            }
        }
        private void DrawTutorial()
        {
            if (tutorialKey < 0) return;
            string[] names = { "W / ↑", "S / ↓", "A / ←", "D / →", "J / 空格", "Esc", "回车", "K", "L", "I", "O", "P" };
            string key = tutorialKey < names.Length ? names[tutorialKey] : tutorialKey.ToString();
            GUI.Box(new Rect(145, 15, 243, 32), "");
            GUI.Label(new Rect(155, 19, 223, 25), "请按 " + key, bodyStyle);
        }
        private void DrawMenu()
        {
            if (WeaponPreviewVisible) { DrawWeaponPreview(); return; }
            if (RoleSkillPreviewVisible) { DrawOriginalSkillPreview(); return; }
            if (originalUi && originalUi.menu != null && originalUi.menu.actions != null) { DrawAtlasMenu(); return; }
            var color = GUI.color;
            GUI.color = new Color(.04f, .05f, .08f, .98f);
            GUI.DrawTexture(new Rect(22, 16, 489, 288), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.Box(new Rect(22, 16, 489, 288), "");
            string[] tabs = { "任务", "物品", "装备", "剑灵", "存档", "地图" };
            int[] pages = { 0, 1, 2, 6, 3, 4 };
            if (worldMapOpen) GUI.Label(new Rect(40, 24, 400, 28), "世界地图", titleStyle);
            else
            {
                GUI.enabled = !SpiritFeedConfirming && !TradeConfirming;
                for (int i = 0; i < tabs.Length; i++)
                    if (GUI.Button(new Rect(32 + i * 69, 24, 65, 28), tabs[i], buttonStyle)) SelectMenuPage(pages[i]);
                if (GUI.Button(new Rect(458, 24, 42, 28), "×", buttonStyle) && CloseMenu()) return;
            }
            if (scriptedMenu && menuPage == 5 && tutorialKey == 0)
                if (GUI.Button(new Rect(386, 60, 80, 28), "↑ 选择", buttonStyle)) RespondTutorial(0);
            GUI.enabled = !SpiritFeedConfirming && !TradeConfirming && !(scriptedMenu && menuPage == 5);
            menuScroll = GUI.BeginScrollView(new Rect(32, 60, 468, 230), menuScroll, new Rect(0, 0, 442, MenuHeight()));
            switch (menuPage)
            {
                case 0:
                    DrawMissionMenu();
                    break;
                case 1:
                    GUI.Label(new Rect(5, 0, 285, 30), "剑灵自动用药阈值：" + autoRecoverPercent + "%", bodyStyle);
                    if (GUI.Button(new Rect(300, 0, 60, 27), "−10", buttonStyle)) SetAutoRecoverPercent(autoRecoverPercent - 10);
                    if (GUI.Button(new Rect(370, 0, 60, 27), "+10", buttonStyle)) SetAutoRecoverPercent(autoRecoverPercent + 10);
                    int row = 0;
                    foreach (var pair in inventory.Where(p => p.Value > 0).OrderBy(p => p.Key).ToArray())
                    {
                        var item = OriginalAssets.Table<ItemDefinition>("item").First(i => i.id == pair.Key);
                        for (int stack = 0; stack < ItemStackCount(pair.Key); stack++)
                        {
                        DrawItemIcon(item.id, new Rect(5,40+row*62,22,22));
                        GUI.Label(new Rect(32, (40 + row * 62), 283, 24), item.name + " × " + ItemStackQuantity(pair.Key, stack), bodyStyle);
                        GUI.Label(new Rect(5, (40 + row * 62) + 25, 335, 35), Clean(item.description), bodyStyle);
                    if (GUI.Button(new Rect(352, (40 + row * 62) + 8, 80, 30), "选择", buttonStyle)) OpenItemConfirmation(pair.Key, stack);
                        row++;
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < equipment.Count; i++)
                    {
                        var instance = equipment[i];
                        var e = instance.Data;
                        DrawEquipmentIcon(e, new Rect(5,i*62,22,22));
                        GUI.Label(new Rect(32, i * 62, 308, 24), e.name + " +" + instance.reinforcement + "  攻击 " + instance.Attack + "  防御 " + instance.Defense, bodyStyle);
                        GUI.Label(new Rect(5, i * 62 + 25, 335, 35), Clean(e.description), bodyStyle);
                        GUI.enabled = e.level <= level;
                        if (GUI.Button(new Rect(352, i * 62 + 8, 80, 30), equippedIndex == i ? "已装备" : "装备", buttonStyle)) SelectEquipment(i);
                        GUI.enabled = true;
                    }
                    break;
                case 3:
                    if (GUI.Button(new Rect(60, 30, 300, 35), "保存进度", buttonStyle)) Save();
                    GUI.enabled = File.Exists(SavePath);
                    if (GUI.Button(new Rect(60, 80, 300, 35), "读取进度", buttonStyle)) Load();
                    GUI.enabled = true;
                    if (GUI.Button(new Rect(60, 130, 300, 35), "返回标题", buttonStyle)) { menu = false; AtTitle = true; }
                    break;
                case 4:
                    for (int i = 0; i < AreaNames.Length; i++)
                    {
                        int index = i;
                        GUI.enabled = openAreas[i];
                        if (GUI.Button(new Rect(8 + i % 2 * 215, i / 2 * 36, 205, 32), AreaNames[i], buttonStyle))
                        {
                            TravelToArea(index);
                        }
                        GUI.enabled = true;
                    }
                    break;
                case 5: DrawShop(); break;
                case 6: DrawSpiritMenu(); break;
            }
            GUI.EndScrollView();
            GUI.enabled = true;
            if (SpiritFeedConfirming) DrawSpiritFeedConfirmation();
            if (TradeConfirming) DrawTradeConfirmation();
        }
        private float MenuHeight()
        {
            if (menuPage == 1) return Mathf.Max(230, 40 + inventory.Sum(p => ItemStackCount(p.Key)) * 62);
            if (menuPage == 2) return Mathf.Max(230, equipment.Count * 62);
            if (menuPage == 5) return ShopHeight();
            if (menuPage == 6) return Mathf.Max(230, 183 + Mathf.Max(0, equipment.Count - 1) * 42);
            return menuPage == 0 ? Mathf.Max(230, MissionMenuHeight()) : menuPage == 4 ? 265 : 230;
        }
    }
}
