namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private void RefillRoleHealth()
        {
            if (!Role) return;
            bool dead = Role.Health.Dead || Role.hp <= 0;
            Role.Health.Refill();
            if (!dead) return;
            if (fighters.TryGetValue(-1, out var fighter))
            { fighter.knockbackSpeed = 0; fighter.attackMode = -1; }
            Role.ResetAction();
        }
    }
}
