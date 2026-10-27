using System;

namespace GuJian
{
    [Serializable]
    public sealed class CombatHealthSnapshot
    {
        public bool dead, specialTriggered;
    }
    public enum HealthOutcome { Alive, Special, Died, AlreadyDead }

    // The actor's serialized HP remains the sole storage, including inspector/fixture edits.
    // This module owns gameplay mutations and the one-shot special/death lifecycle.
    public sealed class CombatHealth
    {
        private readonly Func<int> read, maximum;
        private readonly Action<int> write;
        public bool Dead { get; private set; }
        public bool SpecialTriggered { get; private set; }
        public int Current => read();
        public CombatHealth(Func<int> read, Action<int> write, Func<int> maximum)
        {
            this.read = read ?? throw new ArgumentNullException(nameof(read));
            this.write = write ?? throw new ArgumentNullException(nameof(write));
            this.maximum = maximum ?? throw new ArgumentNullException(nameof(maximum));
        }
        public HealthOutcome Damage(int amount, bool hasSpecial, bool bloodProtection)
        {
            if (Dead) return HealthOutcome.AlreadyDead;
            write(Math.Max(0, Current - amount));
            if (!SpecialTriggered && hasSpecial && Current <= maximum() / 3)
            {
                SpecialTriggered = true; write(Math.Max(1, Current));
                return HealthOutcome.Special;
            }
            if (bloodProtection) write(Math.Max(maximum() / 10, Current));
            if (Current > 0) return HealthOutcome.Alive;
            Dead = true;
            return HealthOutcome.Died;
        }
        public int Heal(int amount)
        {
            if (Current <= 0 || Dead) return 0;
            int before = Current;
            write(Math.Min(maximum(), Current + amount));
            return Current - before;
        }
        public void Restore(int hp) => write(Math.Max(0, Math.Min(hp, maximum())));
        public void Revive() { Dead = false; }
        public void Refill() => Respawn();
        // Respawn restores HP and death state but preserves the one-shot special callback flag.
        public void Respawn() { write(maximum()); Dead = false; }
        public void SetDeathState(bool dead) { Dead = dead; }
        public void SetSpecialState(bool triggered) { SpecialTriggered = triggered; }
        public CombatHealthSnapshot Capture() => new CombatHealthSnapshot { dead = Dead, specialTriggered = SpecialTriggered };
        public void RestoreState(CombatHealthSnapshot snapshot)
        {
            Dead = snapshot != null && snapshot.dead;
            SpecialTriggered = snapshot != null && snapshot.specialTriggered;
        }
    }

    public sealed class HealthTrail
    {
        public int Amount { get; set; }
        private float elapsed;
        public void Clear() { Amount = 0; elapsed = 0; }
        public void Tick(float delta, int maximumHp, int textureWidth)
        {
            elapsed += delta;
            while (elapsed >= .08f)
            {
                elapsed -= .08f;
                if (Amount > 0 && textureWidth > 0) Amount = Math.Max(0, Amount - (maximumHp / textureWidth << 1));
            }
        }
    }
}
