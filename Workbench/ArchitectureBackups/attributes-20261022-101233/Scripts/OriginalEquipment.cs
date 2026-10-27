using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    [Serializable]
    public sealed class EquipmentInstance
    {
        public int id, reinforcement, durability, skillId = -2, skillLevel = 1;
        public EquipmentDefinition Data => OriginalAssets.Table<EquipmentDefinition>("equip").First(e => e.id == id);
        public int Hp => Bonus(Data.hp);
        public int Attack => Bonus(Data.attack);
        public int Defense => Bonus(Data.defense);
        public int MaximumDurability => Bonus(Data.durability);
        public int ReinforceCost => Data.buy * 10 / 100;
        public bool CanReinforce => id < 26 && reinforcement < 9;
        private int Bonus(int value) => value + (reinforcement < 9 ? value * reinforcement * 10 / 100 : value);
        public static EquipmentInstance Create(int id, int reinforcement = 0)
        {
            var e = new EquipmentInstance { id = id, reinforcement = Mathf.Clamp(reinforcement, 0, 9) };
            e.durability = e.MaximumDurability;
            e.skillId = e.Data.skill;
            if (e.skillId == -1)
            {
                var pool = OriginalAssets.Table<SkillDefinition>("skill").Where(s => s.id >= 5 && s.id < 23 && s.rank <= e.Data.level).ToArray();
                e.skillId = pool.Length > 0 ? pool[UnityEngine.Random.Range(0, pool.Length)].id : -2;
            }
            if (e.skillId >= 0)
            {
                var skill = OriginalAssets.Table<SkillDefinition>("skill").First(s => s.id == e.skillId);
                e.skillLevel = Mathf.Clamp((e.Data.level - skill.rank) / 10 + 1, 1, 9);
            }
            return e;
        }
        public void Reinforce()
        {
            if (reinforcement >= 9) return;
            int oldMaximum = MaximumDurability;
            reinforcement++;
            durability = oldMaximum > 0 ? MaximumDurability * durability / oldMaximum : 0;
            if ((reinforcement == 5 || reinforcement == 9) && skillId >= 0) skillLevel = Mathf.Min(9, skillLevel + 1);
        }
    }

    public sealed partial class GuJianGame
    {
        private int spiritDurability { get => Weapons.Spirit; set => Weapons.Restore(value, integrated); }
        private int spiritSkill = 13, spiritSkillLevel = 1;
        private int[] permanentBonuses = new int[6], spiritBonuses = new int[7];
        private EquipmentInstance Equipped => EquipmentBag.Selected;
        public IReadOnlyList<EquipmentInstance> Equipment => EquipmentBag;
        public int EquippedIndex => EquipmentBag.SelectedIndex;
        public bool ReinforceEquipment(int index)
        {
            if (index < 0 || index >= Equipment.Count) return false;
            var item = Equipment[index];
            var result = EquipmentBag.Reinforce(item, ref money);
            if (result == EquipmentReinforceResult.AtLimit) { Toast("强化等级已达上限或该物品不能强化"); return false; }
            if (result == EquipmentReinforceResult.InsufficientMoney) { Toast("银两不足"); return false; }
            if (result != EquipmentReinforceResult.Success) return false;
            RefreshAttributes(Role, false);
            Toast("强化成功！" + item.Data.name + " +" + item.reinforcement);
            return true;
        }
        private int SpiritAttribute(int index)
        {
            var spirit = Actor(-2);
            if (!spirit) return 0;
            int lv = Mathf.Clamp(spirit.level, 1, 40);
            var row = OriginalAssets.Table<AttributeDefinition>("ego").First(a =>
            {
                var p = a.levels.Split(','); return lv >= int.Parse(p[0]) && lv <= int.Parse(p[1]);
            });
            int lower = int.Parse(row.levels.Split(',')[0]);
            string[] fields = { row.hp, row.durability, row.attack, row.defense, row.hit, row.dodge, row.critical };
            return OriginalCombatMath.Growth(fields[index], lv - lower) + spiritBonuses[index];
        }
        private void AddSpiritExperience(int value)
        {
            var spirit = Actor(-2); if (!spirit || spirit.level >= 40 || value <= 0) return;
            spiritExperience += value * Mathf.Clamp(smsExperienceMultiplier, 1, 4);
            int previous = spirit.level;
            while (spirit.level < 40 && spiritExperience >= spirit.level * spirit.level * spirit.level * 25)
            {
                int oldMaximum = SpiritAttribute(1), current = SpiritDurability;
                spiritExperience -= spirit.level * spirit.level * spirit.level * 25;
                spirit.level++;
                SetSpiritDurability(oldMaximum > 0 ? SpiritAttribute(1) * current / oldMaximum : 0);
            }
            if (spirit.level == 40) spiritExperience = SpiritExperienceRequired;
            if (spirit.level > previous)
            {
                UpdateSpiritEquipment();
                RefreshAttributes(Role, false); Toast("剑灵等级提升为 " + spirit.level);
            }
        }
        private void SelectEquipment(int index)
        {
            if (index < 0 || index >= Equipment.Count) return;
            if (EquipmentBag.TrySelect(index)) ApplyEquipmentSelection();
        }
        private void ApplyEquipmentSelection()
        {
            var item = Equipped;
            equipmentId = item.id; weaponLevel = item.Data.level; equippedSkill = integrated ? spiritSkill : item.skillId;
            if (!integrated) Role.EquipWeapon(item.Data.image);
            ResetSkillCooldown();
            if (!integrated) breakCooldownTicks = 3;
            RefreshAttributes(Role, false);
        }
        private void RemoveEquipment(int id)
        {
            var previous = Equipped;
            if (!EquipmentBag.RemoveFirst(id, () => EquipmentInstance.Create(0))) return;
            if (!ReferenceEquals(previous, Equipped)) ApplyEquipmentSelection();
        }
    }
}
