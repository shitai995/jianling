using System;

namespace GuJian
{
    [Serializable]
    public sealed class WalletSnapshot { public int balance; }
    // The single owner of player currency and its original transaction boundaries.
    public sealed class Wallet
    {
        public int Balance { get; private set; }
        public void Restore(int balance) => Balance = Math.Max(0, balance);
        public bool Credit(int amount)
        {
            if (amount < 0) return false;
            Balance = checked(Balance + amount);
            return true;
        }
        public bool Debit(int amount, bool requireGreaterBalance = false)
        {
            if (amount < 0 || (requireGreaterBalance ? Balance <= amount : Balance < amount)) return false;
            Balance -= amount;
            return true;
        }
        public WalletSnapshot Capture() => new WalletSnapshot { balance = Balance };
        public void Restore(WalletSnapshot snapshot) => Restore(snapshot == null ? 0 : snapshot.balance);
    }
}
