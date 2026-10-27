using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GuJian
{
    public enum EquipmentReinforceResult { Success, Invalid, AtLimit, InsufficientMoney }

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
            if (instance == null || !items.Contains(instance)) return EquipmentReinforceResult.Invalid;
            if (!instance.CanReinforce) return EquipmentReinforceResult.AtLimit;
            int cost = instance.ReinforceCost;
            if (money < cost) return EquipmentReinforceResult.InsufficientMoney;
            if (liveDurability.HasValue && ReferenceEquals(instance, Selected)) instance.durability = liveDurability.Value;
            instance.Reinforce(); money -= cost;
            return EquipmentReinforceResult.Success;
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
