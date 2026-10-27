using System;

namespace GuJian
{
    public enum InventoryItemActionKind { None, Used, Discarded }

    public readonly struct InventoryItemActionResult
    {
        public readonly InventoryItemActionKind Kind;
        public readonly int ItemId;
        public readonly int Quantity;

        public InventoryItemActionResult(InventoryItemActionKind kind, int itemId, int quantity)
        { Kind = kind; ItemId = itemId; Quantity = quantity; }
    }

    public sealed class InventoryItemAction
    {
        public bool IsOpen { get; private set; }
        public int ItemId { get; private set; } = -1;
        public int Stack { get; private set; } = -1;
        public int Choice { get; private set; }

        public bool Open(ItemInventory inventory, int itemId, int stack)
        {
            if (inventory == null || inventory.StackQuantity(itemId, stack) <= 0) return false;
            ItemId = itemId;
            Stack = stack;
            Choice = 0;
            IsOpen = true;
            return true;
        }

        public void Select(int choice)
        {
            if (IsOpen) Choice = choice <= 0 ? 0 : 1;
        }

        public void Cancel()
        {
            IsOpen = false;
            ItemId = Stack = -1;
            Choice = 0;
        }

        public InventoryItemActionResult Confirm(ItemInventory inventory, Func<int, int, bool> useStack)
        {
            if (!IsOpen) return default;
            int itemId = ItemId, stack = Stack, choice = Choice;
            int quantity = inventory == null ? 0 : inventory.StackQuantity(itemId, stack);
            Cancel();
            if (quantity <= 0) return default;

            if (choice == 0)
                return useStack != null && useStack(itemId, stack)
                    ? new InventoryItemActionResult(InventoryItemActionKind.Used, itemId, quantity)
                    : default;

            return inventory.TryRemove(itemId, quantity, stack)
                ? new InventoryItemActionResult(InventoryItemActionKind.Discarded, itemId, quantity)
                : default;
        }
    }
}
