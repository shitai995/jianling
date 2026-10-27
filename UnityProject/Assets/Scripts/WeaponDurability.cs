using System;

namespace GuJian
{
    // Ordinary durability lives on each equipment instance; spirit durability and mode live here.
    // Current is a view of the active source, never a second copy to synchronize.
    public sealed class WeaponDurability
    {
        private readonly EquipmentInventory equipment;
        public int Spirit { get; private set; }
        public bool Integrated { get; private set; }
        public int RepairCost { get; private set; }
        public int Current
        {
            get => Integrated ? Spirit : equipment.Selected?.durability ?? 50;
            set
            {
                if (Integrated) Spirit = value;
                else if (equipment.Selected != null) equipment.SetDurability(equipment.Selected, value);
                else throw new InvalidOperationException("Select an ordinary weapon before setting durability");
            }
        }
        public WeaponDurability(EquipmentInventory equipment)
        { this.equipment = equipment ?? throw new ArgumentNullException(nameof(equipment)); }
        public void Reset() { Spirit = 0; Integrated = false; RepairCost = 0; }
        // Save values retain original integer values; gameplay changes use the bounded setters.
        public void Restore(int spirit, bool integrated) { Spirit = spirit; Integrated = integrated; }
        public int Maximum(int spiritMaximum) => Integrated ? spiritMaximum : equipment.Selected?.MaximumDurability ?? 0;
        private static int Clamp(int value, int maximum) => Math.Max(0, Math.Min(value, maximum));
        public void SetCurrent(int value, int spiritMaximum) => Current = Clamp(value, Maximum(spiritMaximum));
        public void SetSpirit(int value, int maximum) => Spirit = Clamp(value, maximum);
        public int Spend(int amount) { Current = Math.Max(0, Current - amount); return Current; }
        public void RefillCurrent(int spiritMaximum) => Current = Maximum(spiritMaximum);
        public void BreakOrdinary() { if (equipment.Selected != null) equipment.SetDurability(equipment.Selected, 0); }
        public bool TryToggle(int spiritMaximum, bool infinite)
        {
            if (!Integrated)
            {
                int cost = spiritMaximum / 10;
                if (!infinite && Spirit <= cost) return false;
                if (!infinite) Spirit -= cost;
            }
            Integrated = !Integrated;
            return true;
        }
        public int QuoteRepair()
        {
            int missing = 0, damaged = 0;
            foreach (var item in equipment)
            {
                int amount = item.MaximumDurability - item.durability;
                if (amount > 0) { missing += amount; damaged++; }
            }
            return RepairCost = missing / 10 + damaged * 500;
        }
        // Original x87 charges the previous x89 quote, even if wear changes afterwards.
        public bool TryRepair(ref int money)
        {
            var wallet = new Wallet(); wallet.Restore(money);
            bool result = TryRepair(wallet); money = wallet.Balance; return result;
        }
        public bool TryRepair(Wallet wallet)
        {
            if (!wallet.Debit(RepairCost)) return false;
            equipment.RefillAllDurability();
            return true;
        }
    }
}
