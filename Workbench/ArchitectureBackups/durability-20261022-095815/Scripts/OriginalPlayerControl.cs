using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        public bool TryBackstep(float horizontal)
        {
            if (!Role || Role.locked || Role.frozen || Role.hp <= 0 || Role.ScriptControlled || CriticalActive ||
                menu || reinforceOpen || DialogueVisible || (horizontal >= 0 ? 1 : -1) == (Role.mirror ? 1 : -1) || horizontal == 0) return false;
            int action = Role.LogicalAction;
            if (action != 2 && (action < 7 || action > 10)) return false;
            ResetAttackChain(); CancelSupportCast();
            Role.Act(4 + Role.actionBank, true); Role.keepLastFrame = false;
            return true;
        }
    }
}
