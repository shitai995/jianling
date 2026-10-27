using System;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private EquipmentInstance spiritFood;
        private bool spiritFoodChoosing;
        private int spiritMenuFace, spiritMenuExitTicks;
        private float spiritMenuExitElapsed;
        private string spiritMenuMessage, spiritFeedTip;
        public bool SpiritFoodChoosing => spiritFoodChoosing;
        private float spiritElapsed;
        private int spiritDurabilityCounter;
        private string spiritFeedResult;
        private static readonly string[] SpiritAttributeNames = { "气血", "耐久", "攻击", "防御", "命中", "闪避", "暴击" };
        public int SpiritLevel => Actor(-2) ? Actor(-2).level : 0;
        public int SpiritExperience => spiritExperience;
        public int SpiritExperienceRequired => SpiritLevel * SpiritLevel * SpiritLevel * 25;
        public int SpiritDurability => integrated ? durability : spiritDurability;
        public int SpiritSkill => spiritSkill;
        public int SpiritSkillLevel => spiritSkillLevel;
        [Serializable]
        private sealed class SpiritFeedState
        {
            public int remaining, total, startLevel, startThreshold, face, flashTicks;
            public float elapsed;
            public string foodName, message;
            public int[] before;
        }
        private SpiritFeedState spiritFeed;
        public bool SpiritFeedAnimating => spiritFeed != null && spiritFeed.remaining > 0;
        public int SpiritFeedRemainingExperience => spiritFeed == null ? 0 : spiritFeed.remaining;
        public int SpiritFeedFace => spiritFeed == null ? spiritMenuFace : spiritFeed.face;
        public int SpiritFeedFlashFrame => spiritFeed == null || spiritFeed.flashTicks <= 0 ? -1 : 4 - spiritFeed.flashTicks;
        public float SpiritFeedProgress => spiritExperience / (float)Mathf.Max(1, SpiritExperienceRequired);
        public string SpiritFeedAnimationMessage => spiritFeed == null ? spiritMenuMessage : spiritFeed.message;
        public bool SpiritFeedCompleted => spiritFeed != null && !SpiritFeedAnimating;
        public int SpiritFeedAttributeGain(int index) => spiritFeed != null && spiritFeed.before != null && index >= 0 && index < spiritFeed.before.Length
            ? SpiritAttribute(index) - spiritFeed.before[index] : 0;
        private void TickSpiritFeedAnimation(float delta)
        {
            TickSpiritMenuExit(delta);
            TickSpiritMenuInfo(delta);
            if (spiritFeed == null || (!SpiritFeedAnimating && spiritFeed.flashTicks == 0)) return;
            spiritFeed.elapsed += delta;
            while (spiritFeed.elapsed >= .08f)
            {
                spiritFeed.elapsed -= .08f;
                if (SpiritFeedAnimating)
                {
                    int step = Mathf.Min(spiritFeed.remaining, Mathf.Max(1, SpiritExperienceRequired / 15));
                    int gap = SpiritExperienceRequired - spiritExperience;
                    if (step <= gap)
                    {
                        spiritExperience += step;
                        spiritFeed.remaining -= step;
                    }
                    else
                    {
                        spiritFeed.remaining -= Mathf.Max(0, gap);
                        spiritExperience = SpiritExperienceRequired;
                        AdvanceSpiritFeedLevel();
                    }
                    if (spiritFeed.remaining == 0) EndSpiritFeed();
                }
                if (spiritFeed.flashTicks > 0) spiritFeed.flashTicks--;
            }
        }
        private void AdvanceSpiritFeedLevel()
        {
            var spirit = Actor(-2);
            if (!spirit || spirit.level >= 40)
            {
                spiritFeed.total -= spiritFeed.remaining;
                spiritFeed.remaining = 0;
                return;
            }
            int oldMaximum = SpiritAttribute(1), current = SpiritDurability;
            spiritExperience -= SpiritExperienceRequired;
            spirit.level++;
            if (spirit.level == 40) spiritExperience = SpiritExperienceRequired;
            SetSpiritDurability(oldMaximum > 0 ? SpiritAttribute(1) * current / oldMaximum : 0);
            UpdateSpiritEquipment(); RefreshAttributes(Role, false);
            spiritFeed.flashTicks = 5;
        }
        public bool CompleteSpiritFeed()
        {
            if (!SpiritFeedAnimating) return false;
            int beforeLevel = SpiritLevel;
            AddSpiritExperience(spiritFeed.remaining);
            spiritFeed.remaining = 0;
            if (SpiritLevel > beforeLevel) spiritFeed.flashTicks = 5;
            EndSpiritFeed(); return true;
        }
        private void EndSpiritFeed()
        {
            string[] lines;
            if (SpiritLevel == 40)
                lines = new[] { "虽然我已经恢复到最强状态了，不过享用一下武器也是不错的感觉哦~谢谢你啦。" };
            else if (spiritFeed.total < spiritFeed.startThreshold * 5 / 100)
            {
                spiritFeed.face = 3;
                lines = new[] { "不行哦~这样的武器对我帮助不大。", "提升很微弱，还是给我别的武器吧。", "几乎没有成长，下次能给我更好的武器么？" };
            }
            else if (spiritFeed.total < spiritFeed.startThreshold * 10 / 100)
            {
                spiritFeed.face = 1;
                lines = new[] { "灵气有点少，不过有些成长。", "成长了一些，下次还会更多吧？" };
            }
            else if (spiritFeed.total < spiritFeed.startThreshold * 20 / 100)
            {
                spiritFeed.face = 0;
                lines = new[] { "成长了很多~多亏你啦~", "提高了不少能力，这样很快就能帮到你了。" };
            }
            else
            {
                spiritFeed.face = 5;
                lines = SpiritLevel > spiritFeed.startLevel ? new[] { "升级了~能力大幅提高，这下应该可以帮到你了。" } : new[] { "灵气十分充足~再给我来一份吧~", "吃到好东西了~能力大幅提高！" };
            }
            spiritFeed.message = lines[UnityEngine.Random.Range(0, lines.Length)];
            spiritFeedResult = "吞噬了" + spiritFeed.foodName + "，经验 +" + spiritFeed.total + "\n等级 " + spiritFeed.startLevel + " → " + SpiritLevel + "\n" + string.Join("  ", Enumerable.Range(0, 7).Where(i => SpiritAttribute(i) != spiritFeed.before[i]).Select(i => SpiritAttributeNames[i] + " +" + (SpiritAttribute(i) - spiritFeed.before[i])));
        }
        public bool SpiritFeedConfirming => spiritFood != null;
        public int GetSpiritAttribute(int index) => index >= 0 && index < 7 ? SpiritAttribute(index) : 0;

        private void SetScriptedSpiritLevel(int value)
        {
            var spirit = Actor(-2);
            if (!spirit) return;
            int previousMaximum = SpiritAttribute(1), previousCurrent = SpiritDurability;
            spirit.level = (short)value;
            // Famulus.initData scales durability while retaining XP, bonuses and skill.
            spiritDurability = previousMaximum > 0 ? SpiritAttribute(1) * previousCurrent / previousMaximum : 0;
            if (integrated) durability = spiritDurability;
            UpdateSpiritEquipment();
        }

        private void SetSpiritDurability(int value)
        {
            spiritDurability = Mathf.Clamp(value, 0, SpiritAttribute(1));
            if (integrated) durability = spiritDurability;
        }

        private void UpdateSpiritEquipment()
        {
            if (!Role || !Actor(-2)) return;
            if (integrated) equippedSkill = spiritSkill;
            int lv = SpiritLevel;
            Role.EquipSpiritWeapon(lv < 11 ? "w_97" : lv < 21 ? "w_98" : lv < 31 ? "w_99" : "w_00");
        }

        private static int SpiritFoodExperience(EquipmentInstance item)
        {
            // GameCanvas.feed: normal offline multiplier is one; +9 counts as twelve upgrades.
            int amount = item.Data.level * (item.reinforcement + 1 + item.reinforcement / 9 * 2) * 5;
            return item.Data.level * Mathf.Min(amount, 10000);
        }

        private static int[] SpiritFoodBonuses(EquipmentInstance item)
        {
            var bonuses = new[] { item.Hp / 50, item.durability / 50, item.Attack / 50, item.Defense / 200,
                item.Data.hit / 20, item.Data.dodge / 20, item.Data.critical / 20 };
            if (item.reinforcement == 9)
                foreach (string effect in item.Data.effects.Split(';'))
                {
                    var fields = effect.Split(',');
                    if (fields.Length == 0 || !int.TryParse(fields[0], out int type) || type < 0 || type >= bonuses.Length) continue;
                    // Original equip reader uses the first pair of each semicolon-delimited entry.
                    if (fields.Length > 1 && int.TryParse(fields[1], out int amount)) bonuses[type] += amount;
                }
            return bonuses;
        }

        private bool CanInheritSpiritSkill(EquipmentInstance item) => item.skillId >= 0 &&
            (spiritSkill < 0 || item.skillId != spiritSkill || item.skillLevel > spiritSkillLevel);


        private int[] SpiritFoodCandidates() => Enumerable.Range(0, Equipment.Count).Where(i => i != EquippedIndex).ToArray();

        private void ResetSpiritMenu()
        {
            CancelSpiritFeed();
            spiritFoodChoosing = false;
            spiritMenuExitTicks = 0; spiritMenuExitElapsed = 0;
            spiritMenuMessage = null; spiritMenuFace = 0; spiritInfoSource = null;
        }

        private void EnterSpiritMenu()
        {
            ResetSpiritMenu();
            spiritFeed = null;
            SetSpiritMenuInfo(0);
        }

        private void SetSpiritMenuInfo(int state)
        {
            int weaponLevel = Equipped == null ? 0 : Equipped.Data.level;
            string[][] lines;
            if (SpiritLevel < (weaponLevel >> 3))
            {
                spiritMenuFace = 3;
                lines = new[] {
                    new[] { "嗯？对不起，太久没吃东西了，有点没力气……", "最近你好像对我不是很照顾了，有些遗憾……" },
                    new[] { "还以为你把我扔在一边了……还好不是。", "还好没有成为被遗弃的武器……" },
                    new[] { "如果不想养我，干脆把我卖掉吧……我果然是被遗弃的武器呢。" },
                    new[] { "我的力量大不如前了，如果有点武器吃就好了。" } };
            }
            else if (SpiritLevel < (weaponLevel >> 2))
            {
                spiritMenuFace = 2;
                lines = new[] {
                    new[] { "不想和你说话，这样下去我都要生锈了。", "终于想起我来了吗？没有责任感的主人。" },
                    new[] { "终于想起要给我武器吃了么，没有责任感的主人？", "还好不是完全没有责任感的主人。" },
                    new[] { "如果不想让我成长，不如把我重新封印如何？不让灵剑得到任何成长，这是对我的侮辱。" },
                    new[] { "喂！再不给我武器吃的话，我就不理你了！" } };
            }
            else if (SpiritLevel < (weaponLevel >> 1) && SpiritLevel < 40)
            {
                spiritMenuFace = state == 2 ? 2 : 1;
                lines = new[] {
                    new[] { "你来啦，这次有没有带好吃的武器给我啊？", "喂……我肚子饿了，给我点东西吃吧。" },
                    new[] { "有武器吃吗？又可以成长了~", "会是什么武器呢？好奇……" },
                    new[] { "虽然很可惜，不过也没办法，下次吧。", "我早就猜到了，你这小气鬼。" },
                    new[] { "嗯，好吧，下次来记得带武器给我吃哦。", "好吧，一会儿见", "好吧，一会儿见" } };
            }
            else
            {
                spiritMenuFace = state == 2 ? 1 : 0;
                lines = new[] {
                    new[] { "肚子很饱，心情很舒畅。叫我有什么事呢？", "今天天气不错哦，你身体还好吗？", "有没有受伤啊？我可以替你使用药品恢复哦。" },
                    new[] { "哇~要给我吃什么呢？有点小小的期待哦~", "经常给我武器吃真是谢谢你哦~", "经常给我武器吃真是谢谢你哦~" },
                    new[] { "不给了吗？那就下次再说吧。" },
                    new[] { "这样就走了？再陪人家聊一下嘛~", "这样就走了？再陪人家聊一下嘛~", "这样就走了？再陪人家聊一下嘛~" } };
            }
            spiritInfoSource = null;
            var choices = lines[Mathf.Clamp(state, 0, 3)];
            spiritMenuMessage = choices[UnityEngine.Random.Range(0, choices.Length)];
        }

        private void OpenSpiritFoodList()
        {
            CancelSpiritFeed(); spiritFeed = null;
            if (SpiritFoodCandidates().Length == 0)
            {
                spiritFoodChoosing = false; spiritMenuFace = 2;
                spiritMenuMessage = "你身上根本没有多余的武器嘛！大骗子！";
                return;
            }
            spiritFoodChoosing = true; atlasSelection = 0; atlasScroll = Vector2.zero;
            SetSpiritMenuInfo(1);
        }

        private void ActivateSpiritFoodSelection()
        {
            if (!spiritFoodChoosing) { OpenSpiritFoodList(); return; }
            var foods = SpiritFoodCandidates();
            if (foods.Length == 0) { OpenSpiritFoodList(); return; }
            if (!SelectSpiritFood(foods[Mathf.Clamp(atlasSelection, 0, foods.Length - 1)])) return;
            if (spiritSkill >= 0 && CanInheritSpiritSkill(spiritFood))
            {
                var skill = OriginalAssets.Table<SkillDefinition>("skill").FirstOrDefault(s => s.id == spiritFood.skillId);
                spiritFeedTip = "是否继承技能" + (skill == null ? "" : skill.name) + spiritFood.skillLevel + "级?\n继承技能将替换灵剑现有的技能";
            }
            else ConfirmSpiritFeed(false);
        }

        private bool HandleSpiritMenuKey(int code)
        {
            if (spiritMenuExitTicks > 0) return true;
            if (SpiritFeedAnimating)
            {
                if (code == 10) CompleteSpiritFeed();
                return true;
            }
            if (menuPage != 6) return false;
            if (code == 2 || code == 3) return false;
            if (code == 0 || code == 1)
            {
                if (spiritFoodChoosing)
                {
                    var foods = SpiritFoodCandidates();
                    if (foods.Length > 0)
                    {
                        atlasSelection = (atlasSelection + (code == 1 ? 1 : foods.Length - 1)) % foods.Length;
                        // Original isFeedTip does not block candidate navigation.
                        if (SpiritFeedConfirming) spiritFood = Equipment[foods[atlasSelection]];
                    }
                }
                return true;
            }
            if (SpiritFeedConfirming)
            {
                if (code == 9 || code == 10)
                {
                    if (code == 10)
                    {
                        // Soft10 copies the currently selected candidate even if the
                        // selection changed after the original inheritance tip opened.
                        spiritSkill = spiritFood.skillId; spiritSkillLevel = spiritFood.skillLevel;
                        ResetSkillCooldown();
                    }
                    ConfirmSpiritFeed(false);
                }
                return true;
            }
            if (code == 4 || code == 9) { ActivateSpiritFoodSelection(); return true; }
            if (code == 10)
            {
                if (spiritFoodChoosing)
                {
                    spiritFoodChoosing = false; SetSpiritMenuInfo(2);
                }
                else
                {
                    spiritFeed = null; SetSpiritMenuInfo(3);
                    spiritMenuExitTicks = 21; spiritMenuExitElapsed = 0;
                }
                return true;
            }
            return true;
        }

        private void TickSpiritMenuExit(float delta)
        {
            if (spiritMenuExitTicks <= 0 || !menu) return;
            spiritMenuExitElapsed += delta;
            while (spiritMenuExitElapsed >= .08f && spiritMenuExitTicks > 0)
            {
                spiritMenuExitElapsed -= .08f;
                spiritMenuExitTicks--;
                if (spiritMenuExitTicks == 0) CloseMenu();
            }
        }

        public bool SelectSpiritFood(int index)
        {
            if (SpiritFeedAnimating || !Actor(-2) || index < 0 || index >= Equipment.Count || index == EquippedIndex) return false;
            spiritFood = Equipment[index];
            return true;
        }

        public void CancelSpiritFeed() { spiritFood = null; spiritFeedTip = null; }

        public bool ConfirmSpiritFeed(bool inheritSkill)
        {
            int index = spiritFood == null ? -1 : EquipmentBag.IndexOf(spiritFood);
            spiritFood = null;
            return FeedSpirit(index, inheritSkill);
        }

        public bool FeedSpirit(int index, bool inheritSkill = false)
        {
            if (SpiritFeedAnimating || !Actor(-2) || index < 0 || index >= Equipment.Count || index == EquippedIndex) return false;
            var item = Equipment[index];
            spiritFoodChoosing = false; spiritMenuExitTicks = 0; spiritFeedTip = null;
            int beforeLevel = SpiritLevel, oldMaximum = SpiritAttribute(1), current = SpiritDurability;
            var before = Enumerable.Range(0, 7).Select(SpiritAttribute).ToArray();
            var gains = SpiritFoodBonuses(item);
            for (int i = 0; i < gains.Length; i++) spiritBonuses[i] += gains[i];
            SetSpiritDurability(oldMaximum > 0 ? SpiritAttribute(1) * current / oldMaximum : 0);
            bool inherit = (inheritSkill || spiritSkill < 0) && CanInheritSpiritSkill(item);
            if (inherit)
            {
                spiritSkill = item.skillId; spiritSkillLevel = item.skillLevel;
                ResetSkillCooldown();
            }
            int experienceGain = SpiritLevel < 40 ? SpiritFoodExperience(item) : 0;
            if (!EquipmentBag.TryRemove(item)) return false;
            if (spiritFood == item) spiritFood = null;
            spiritFeed = new SpiritFeedState { remaining = experienceGain, total = experienceGain,
                startLevel = beforeLevel, startThreshold = SpiritExperienceRequired, face = 4,
                foodName = item.Data.name, before = before };
            if (experienceGain == 0) EndSpiritFeed();
            UpdateSpiritEquipment();
            RefreshAttributes(Role, false);
            Toast("灵儿吞噬了" + item.Data.name);
            return true;
        }

        private string SpiritSkillName()
        {
            var skill = OriginalAssets.Table<SkillDefinition>("skill").FirstOrDefault(s => s.id == spiritSkill);
            return skill == null ? "无" : skill.name + " " + spiritSkillLevel + "级";
        }

        private void TickSpirit(float delta)
        {
            var spirit = Actor(-2);
            if (!spirit || spirit.frozen || !Role || Role.hp <= 0 || menu || reinforceOpen || DialogueVisible || criticalStage != 0) return;
            spiritElapsed += delta;
            while (spiritElapsed >= .08f)
            {
                spiritElapsed -= .08f;
                if (integrated && infiniteDurability) continue;
                if (!integrated && SpiritDurability >= SpiritAttribute(1)) continue;
                if (spiritDurabilityCounter++ < 10) continue;
                spiritDurabilityCounter = 0;
                int amount = SpiritAttribute(1) / 100;
                SetSpiritDurability(SpiritDurability + (integrated ? -amount : amount));
                if (integrated && SpiritDurability == 0)
                {
                    ToggleIntegrate();
                    Toast("剑灵耐久耗尽，合体解除");
                }
            }
        }

        private void DrawSpiritMenu()
        {
            if (!Actor(-2)) { GUI.Label(new Rect(10, 12, 410, 60), "剑灵尚未加入。", bodyStyle); return; }
            GUI.Label(new Rect(5, 0, 430, 25), "灵儿  Lv." + SpiritLevel + " / 40    耐久 " + SpiritDurability + "/" + SpiritAttribute(1), bodyStyle);
            GUI.Label(new Rect(5, 25, 430, 25), "经验 " + spiritExperience + "/" + SpiritExperienceRequired + "    技能 " + SpiritSkillName(), bodyStyle);
            GUI.Label(new Rect(5, 52, 430, 48), string.Join("  ", Enumerable.Range(0, 7).Where(i => i != 1)
                .Select(i => SpiritAttributeNames[i] + " " + SpiritAttribute(i))), bodyStyle);
            GUI.Label(new Rect(5, 102, 430, 72), spiritFeedResult ?? "选择多余的武器吞噬，可获得经验和永久属性。", bodyStyle);
            int row = 0;
            for (int i = 0; i < Equipment.Count; i++)
            {
                if (i == EquippedIndex) continue;
                var item = Equipment[i]; float y = 183 + row++ * 42;
                GUI.Label(new Rect(5, y, 332, 34), item.Data.name + " +" + item.reinforcement + "  经验 +" + (SpiritLevel < 40 ? SpiritFoodExperience(item) : 0), bodyStyle);
                if (GUI.Button(new Rect(345, y, 84, 30), "吞噬", buttonStyle)) SelectSpiritFood(i);
            }
            if (row == 0) GUI.Label(new Rect(5, 183, 420, 32), "没有多余的武器。当前装备不能吞噬。", bodyStyle);
        }

        private void DrawSpiritFeedConfirmation()
        {
            var item = spiritFood;
            var color = GUI.color;
            GUI.color = new Color(.06f, .07f, .10f, 1);
            GUI.DrawTexture(new Rect(47, 60, 439, 233), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.Box(new Rect(47, 60, 439, 233), "");
            GUI.Label(new Rect(60, 70, 412, 28), "吞噬 " + item.Data.name + " +" + item.reinforcement, titleStyle);
            string gains = string.Join("  ", SpiritFoodBonuses(item).Select((v, i) => v == 0 ? null : SpiritAttributeNames[i] + " +" + v).Where(s => s != null));
            GUI.Label(new Rect(63, 105, 407, 60), "经验 +" + (SpiritLevel < 40 ? SpiritFoodExperience(item) : 0) + "\n" + gains, bodyStyle);
            bool chooseSkill = CanInheritSpiritSkill(item) && spiritSkill >= 0;
            string skill = item.skillId >= 0 ? OriginalAssets.Table<SkillDefinition>("skill").First(s => s.id == item.skillId).name + " " + item.skillLevel + "级" : "无";
            GUI.Label(new Rect(63, 168, 407, 58), chooseSkill ? "武器技能：" + skill + "\n继承会替换当前技能：" + SpiritSkillName() : "确认后武器将被消耗。", bodyStyle);
            if (GUI.Button(new Rect(63, 242, 125, 33), chooseSkill ? "继承并吞噬" : "确认吞噬", buttonStyle)) ConfirmSpiritFeed(true);
            if (chooseSkill && GUI.Button(new Rect(198, 242, 136, 33), "保留技能并吞噬", buttonStyle)) ConfirmSpiritFeed(false);
            if (GUI.Button(new Rect(345, 242, 125, 33), "取消", buttonStyle)) CancelSpiritFeed();
        }
    }
}
