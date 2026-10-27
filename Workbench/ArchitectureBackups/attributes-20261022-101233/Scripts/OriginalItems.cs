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
            int count = id >= 11 && id <= 15 ? ItemStackQuantity(id, stack) : 1;
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
            int value = 0;
            var fields = (item.effect ?? "").Split(',');
            if (fields.Length > 1) int.TryParse(fields[1], out value);
            if (id < 4)
            {
                if (Role.hp <= 0 || Role.hp >= Role.maxHp) return false;
                Role.hp = Mathf.Min(Role.maxHp, Role.hp + item.hp);
                healthDecrement = 0;
            }
            else if (id < 8)
            {
                if (durability >= maximum || (integrated && durability == 0)) return false;
                SetCurrentDurability(durability + item.durability);
            }
            else switch (id)
            {
                case 8:
                    if ((integrated ? spiritSkill : Equipped.skillId) < 0 || (integrated ? spiritSkillLevel : Equipped.skillLevel) >= 9) return false;
                    if (integrated) spiritSkillLevel++; else Equipped.skillLevel++;
                    break;
                case 9:
                    if (Role.LogicalAction != 3 || !fighters.TryGetValue(-1, out var fighter)) return false;
                    fighter.dead = false; fighter.knockbackSpeed = 0; fighter.attackMode = -1;
                    Role.ResetAction(); RefreshAttributes(Role, true); break;
                case 10: SetCurrentDurability(maximum); break;
                case 11: case 12: case 13:
                    if (SpiritLevel == 0 || SpiritLevel >= 40) return false;
                    AddSpiritExperience(value * count); break;
                case 14: case 15: money += value * count; break;
                case 16:
                    if (level + value > 999) return false;
                    level += value; RefreshAttributes(Role, true); break;
                case 17: case 18: case 19: case 20: case 21: case 22:
                    permanentBonuses[id - 17] += value; RefreshAttributes(Role, false); break;
                case 23:
                    int skill = Random.Range(5, 22);
                    if (integrated) { spiritSkill = skill; spiritSkillLevel = 1; }
                    else { Equipped.skillId = skill; Equipped.skillLevel = 1; }
                    equippedSkill = skill; ResetSkillCooldown(); LoadSkillVisual(skill); break;
                default: return false;
            }
            return true;
        }
    }
}
