using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        [SerializeField, Range(10, 90)] private int autoRecoverPercent = 70;
        private int autoMedicine = -1;
        private EquipmentInstance autoMedicineWeapon;
        public int AutoRecoverPercent => autoRecoverPercent;
        public void SetAutoRecoverPercent(int value) { autoRecoverPercent = Mathf.Clamp(value / 10 * 10, 10, 90); }

        private void CancelAutoMedicine()
        {
            autoMedicine = -1; autoMedicineWeapon = null;
        }
        private int LargestAvailableMedicine(int highestId)
        {
            for (int id = highestId; id > highestId - 4; id--) if (ItemCount(id) > 0) return id;
            return -1;
        }
        private void TickAutoMedicine()
        {
            var spirit = Actor(-2);
            if (!Role || Role.hp <= 0 || !spirit || integrated || spirit.locked || spirit.frozen || spirit.hidden || spirit.ScriptControlled ||
                spirit.actionIndex > 1 || menu || reinforceOpen || DialogueVisible || CriticalActive || Machine.Running) return;
            int selected = -1;
            if (durability == 0) { if (ItemCount(10) > 0) selected = 10; }
            else if (durability < autoRecoverPercent * Equipped.MaximumDurability / 100)
            {
                int missing = Equipped.MaximumDurability - durability;
                int preferred = missing >= 2000 ? 7 : missing >= 800 ? 6 : missing >= 300 ? 5 : 4;
                selected = ItemCount(preferred) > 0 ? preferred : LargestAvailableMedicine(7);
            }
            if (Role.hp < autoRecoverPercent * Role.maxHp / 100)
            {
                int missing = Role.maxHp - Role.hp;
                int preferred = missing >= 5000 ? 3 : missing >= 2000 ? 2 : missing >= 1000 ? 1 : 0;
                if (ItemCount(preferred) > 0) selected = preferred;
                else if (selected < 0) selected = LargestAvailableMedicine(3);
            }
            if (selected < 0) return;
            autoMedicine = selected; autoMedicineWeapon = Equipped;
            spirit.PlayOnce(3);
        }
        private void OnItemActionFinished(OriginalActor actor)
        {
            if (actor.actorId == -1 && actor.LogicalAction == 3 && actor.hp <= 0 && string.IsNullOrEmpty(actor.deathScript))
            {
                if (!UseItem(9)) Talk("", "胜败乃兵家常事。可从菜单读取进度。");
            }
            if (actor.actorId != -2 || actor.actionIndex != 3 || autoMedicine < 0) return;
            int id = autoMedicine; var weapon = autoMedicineWeapon; CancelAutoMedicine();
            if (Role && Role.hp > 0 && !integrated && ReferenceEquals(weapon, Equipped)) UseItem(id);
            actor.ResetAction();
        }

        public int ItemCount(int id) => Inventory.Count(id);

        public int ItemStackCount(int id) => Inventory.StackCount(id);
        public int ItemStackQuantity(int id, int stack) => Inventory.StackQuantity(id, stack);
        public bool UseItem(int id) => UseItemStack(id, 0);
        public bool UseItemStack(int id, int stack)
        {
            if (!Role || ItemStackQuantity(id, stack) <= 0) return false;
            // The original bag holds at most 99 per stack; one use consumes one stack.
            var definition = OriginalAssets.Table<ItemDefinition>("item").FirstOrDefault(i => i.id == id);
            bool wholeStack = definition != null && definition.ConsumeWholeStack;
            int count = wholeStack ? ItemStackQuantity(id, stack) : 1;
            if (!ApplyItem(id, count)) return false;
            Inventory.TryRemove(id, count, stack);
            Toast("使用了" + OriginalAssets.Table<ItemDefinition>("item").First(i => i.id == id).name + (count > 1 ? " × " + count : ""));
            return true;
        }

        private void SetCurrentDurability(int value)
        {
            Weapons.SetCurrent(value, SpiritAttribute(1));
            RefreshAttributes(Role, false);
        }

        private bool ApplyItem(int id, int count)
        {
            var item = OriginalAssets.Table<ItemDefinition>("item").FirstOrDefault(i => i.id == id);
            if (item == null || count < 1) return false;
            int maximum = integrated ? SpiritAttribute(1) : Equipped.MaximumDurability;
            int value = item.effectValue;
            var fields = (item.effect ?? "").Split(',');
            if (!item.customRules && fields.Length > 1) int.TryParse(fields[1], out value);
            ItemEffectKind kind = item.EffectKind;
            switch (kind)
            {
                case ItemEffectKind.HealHealth:
                    if (Role.hp <= 0 || Role.hp >= Role.maxHp) return false;
                    Role.Health.Heal(item.hp); healthDecrement = 0; break;
                case ItemEffectKind.RestoreDurability:
                    if (durability >= maximum || (integrated && durability == 0)) return false;
                    SetCurrentDurability(durability + item.durability); break;
                case ItemEffectKind.UpgradeWeaponSkill:
                    if ((integrated ? spiritSkill : Equipped.skillId) < 0 || (integrated ? spiritSkillLevel : Equipped.skillLevel) >= 9) return false;
                    if (integrated) spiritSkillLevel++; else EquipmentBag.SetSkill(Equipped, Equipped.skillId, Equipped.skillLevel + 1);
                    break;
                case ItemEffectKind.Revive:
                    if (Role.LogicalAction != 3 || !fighters.TryGetValue(-1, out var fighter)) return false;
                    Role.Health.Revive(); fighter.knockbackSpeed = 0; fighter.attackMode = -1;
                    Role.ResetAction(); RefreshAttributes(Role, true); break;
                case ItemEffectKind.FullyRepair: SetCurrentDurability(maximum); break;
                case ItemEffectKind.SwordSpiritExperience:
                    if (SpiritLevel == 0 || SpiritLevel >= 40) return false;
                    AddSpiritExperience(value * count); break;
                case ItemEffectKind.Money: Rewards.Grant(checked(value * count)); break;
                case ItemEffectKind.PlayerLevel:
                    if (level + value > 999) return false;
                    level += value; RefreshAttributes(Role, true); break;
                case ItemEffectKind.AttributeBonus:
                    int attributeIndex = item.customRules ? item.attributeIndex : fields.Length > 0 && int.TryParse(fields[0], out var parsedIndex) ? parsedIndex : id - 17;
                    if (attributeIndex < 0 || attributeIndex >= permanentBonuses.Length) return false;
                    permanentBonuses[attributeIndex] += value; RefreshAttributes(Role, false); break;
                case ItemEffectKind.RandomWeaponSkill:
                    var permitted = (item.randomSkillIds ?? "").Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(int.Parse).ToArray();
                    int[] skills = OriginalAssets.Table<SkillDefinition>("skill").Where(s => permitted.Length > 0 ? permitted.Contains(s.id) : s.id >= 5 && s.id <= 21).Select(s => s.id).ToArray();
                    if (skills.Length == 0) return false;
                    int skill = skills[Random.Range(0, skills.Length)];
                    if (integrated) { spiritSkill = skill; spiritSkillLevel = 1; }
                    else EquipmentBag.SetSkill(Equipped, skill, 1);
                    equippedSkill = skill; ResetSkillCooldown(); LoadSkillVisual(skill); break;
                default: return false;
            }
            return true;
        }

    }
}
