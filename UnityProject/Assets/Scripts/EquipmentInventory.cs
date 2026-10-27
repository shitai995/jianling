using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GuJian
{
    public enum EquipmentReinforceResult { Success, Invalid, AtLimit, InsufficientMoney }
    public sealed class EquipmentReinforcePreview
    {
        public EquipmentInstance Current { get; internal set; }
        public EquipmentInstance Next { get; internal set; }
        public int CurrentDurability { get; internal set; }
        public int Cost { get; internal set; }
    }

    // Owns bag membership and selected identity. Equipment payload fields remain live for combat.
    public sealed class EquipmentInventory : IReadOnlyList<EquipmentInstance>
    {
        private readonly List<EquipmentInstance> items = new List<EquipmentInstance>();
        public EquipmentInstance Selected { get; private set; }
        public int SelectedIndex => items.IndexOf(Selected);
        public int Count => items.Count;
        public EquipmentInstance this[int index] => items[index];
        public IEnumerator<EquipmentInstance> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int IndexOf(EquipmentInstance instance) => items.IndexOf(instance);
        public int FindById(int id, bool last = false) => last ? items.FindLastIndex(e => e.id == id) : items.FindIndex(e => e.id == id);
        public void Add(EquipmentInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (items.Contains(instance)) throw new ArgumentException("An equipment instance can only occupy one bag slot");
            items.Add(instance);
        }
        public bool SetDurability(EquipmentInstance instance, int value)
        {
            if (instance == null || !items.Contains(instance)) return false;
            instance.durability = value;
            return true;
        }
        public bool RefillDurability(EquipmentInstance instance)
        {
            return instance != null && SetDurability(instance, instance.MaximumDurability);
        }
        public void RefillAllDurability()
        {
            foreach (var item in items) item.durability = item.MaximumDurability;
        }
        public bool SetSkill(EquipmentInstance instance, int skillId, int skillLevel)
        {
            if (instance == null || !items.Contains(instance)) return false;
            instance.skillId = skillId;
            instance.skillLevel = skillLevel;
            return true;
        }
        public void Reset(EquipmentInstance initial)
        {
            if (initial == null) throw new ArgumentNullException(nameof(initial));
            items.Clear(); items.Add(initial); Selected = initial;
        }
        public bool TrySelect(int index)
        {
            if (index < 0 || index >= items.Count) return false;
            Selected = items[index]; return true;
        }
        // Sales and feeding may only consume the same unequipped object that was selected.
        public bool TryRemove(EquipmentInstance instance)
        {
            if (instance == null || ReferenceEquals(instance, Selected)) return false;
            return items.Remove(instance);
        }
        // Script removal may remove the active sword, selecting the original ID-0 fallback.
        public bool RemoveFirst(int id, Func<EquipmentInstance> createFallback)
        {
            int index = FindById(id);
            if (index < 0) return false;
            var removed = items[index];
            if (ReferenceEquals(removed, Selected))
            {
                int fallback = FindById(0);
                if (fallback < 0 || fallback == index)
                {
                    var replacement = createFallback();
                    Add(replacement); Selected = replacement;
                }
                else Selected = items[fallback];
            }
            items.RemoveAt(index);
            return true;
        }
        public EquipmentReinforceResult Reinforce(EquipmentInstance instance, ref int money, int? liveDurability = null)
        {
            var wallet = new Wallet(); wallet.Restore(money);
            var result = Reinforce(instance, wallet, liveDurability);
            money = wallet.Balance; return result;
        }
        public EquipmentReinforceResult Reinforce(EquipmentInstance instance, Wallet wallet, int? liveDurability = null)
        {
            if (instance == null || !items.Contains(instance)) return EquipmentReinforceResult.Invalid;
            if (!instance.CanReinforce) return EquipmentReinforceResult.AtLimit;
            int cost = instance.ReinforceCost;
            if (wallet.Balance < cost) return EquipmentReinforceResult.InsufficientMoney;
            if (liveDurability.HasValue && ReferenceEquals(instance, Selected)) instance.durability = liveDurability.Value;
            instance.Reinforce(); wallet.Debit(cost);
            return EquipmentReinforceResult.Success;
        }
        public EquipmentReinforcePreview PreviewReinforce(EquipmentInstance instance, int? liveDurability = null)
        {
            if (instance == null || !items.Contains(instance) || !instance.CanReinforce) return null;
            int current = ReferenceEquals(instance, Selected) && liveDurability.HasValue ? liveDurability.Value : instance.durability;
            var next = Copy(instance); next.durability = current; next.Reinforce();
            return new EquipmentReinforcePreview { Current = instance, Next = next, CurrentDurability = current, Cost = instance.ReinforceCost };
        }
        // Save snapshots must not share mutable equipment fields with live combat state.
        public EquipmentInstance[] Capture() => items.Select(Copy).ToArray();
        public void Restore(EquipmentInstance[] snapshot, int selectedIndex)
        {
            if (snapshot == null || snapshot.Length == 0 || snapshot.Any(e => e == null)) throw new ArgumentException("Equipment snapshot requires at least one non-null sword");
            var restored = snapshot.Select(Copy).ToArray();
            int index = Math.Max(0, Math.Min(snapshot.Length - 1, selectedIndex));
            items.Clear(); items.AddRange(restored); Selected = items[index];
        }
        private static EquipmentInstance Copy(EquipmentInstance item) => new EquipmentInstance {
            id = item.id, reinforcement = item.reinforcement, durability = item.durability,
            skillId = item.skillId, skillLevel = item.skillLevel
        };
    }
}
