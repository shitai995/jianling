using System;
using System.Collections.Generic;
using System.Linq;

namespace GuJian
{
    // Owns aggregate quantities. Stack indices are projections of the original 99-item rows.
    public sealed class ItemInventory
    {
        public const int StackLimit = 99;
        private readonly Dictionary<int, int> quantities = new Dictionary<int, int>();
        public int Count(int id) => quantities.TryGetValue(id, out var count) ? count : 0;
        public int StackCount(int id) { int n = Count(id); return n / StackLimit + (n % StackLimit == 0 ? 0 : 1); }
        public int StackQuantity(int id, int stack) => stack < 0 ? 0 : (int)Math.Max(0L, Math.Min(StackLimit, (long)Count(id) - (long)stack * StackLimit));
        public int TotalStacks => quantities.Keys.Sum(StackCount);
        public void Clear() => quantities.Clear();
        public void Add(int id, int quantity)
        {
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            int next = checked(Count(id) + quantity);
            if (next > 0) quantities[id] = next;
        }
        public bool TryRemove(int id, int quantity, int stack = -1)
        {
            if (quantity <= 0 || Count(id) < quantity || (stack >= 0 && StackQuantity(id, stack) < quantity)) return false;
            int next = Count(id) - quantity;
            if (next == 0) quantities.Remove(id); else quantities[id] = next;
            return true;
        }
        // Script removals clamp at zero, as in the original command 0x4B.
        public void RemoveUpTo(int id, int quantity)
        {
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            int removed = Math.Min(Count(id), quantity);
            if (removed > 0) TryRemove(id, removed);
        }
        public int[][] StackRows() => quantities.Keys.OrderBy(id => id)
            .SelectMany(id => Enumerable.Range(0, StackCount(id)).Select(stack => new[] { id, stack })).ToArray();
        public KeyValuePair<int, int>[] Capture() => quantities.OrderBy(p => p.Key).ToArray();
        public void Restore(int[] ids, int[] counts)
        {
            if (ids == null || counts == null || ids.Length != counts.Length) throw new ArgumentException("Inventory snapshot arrays must match");
            var restored = new Dictionary<int, int>();
            for (int i = 0; i < ids.Length; i++)
            {
                if (counts[i] < 0) throw new ArgumentException("Inventory quantities cannot be negative");
                // Existing saves use aggregate quantities; the last duplicate ID wins.
                restored[ids[i]] = counts[i];
            }
            quantities.Clear();
            foreach (var pair in restored) if (pair.Value > 0) quantities.Add(pair.Key, pair.Value);
        }
    }
}
