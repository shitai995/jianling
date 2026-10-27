using UnityEngine;

namespace GuJian
{
    public sealed class OriginalGroundDrop : MonoBehaviour
    {
        public enum DropKind { Money, Item, Weapon }
        [SerializeField] private SpriteRenderer[] pieces;
        [SerializeField] private OriginalActor weapon;
        [SerializeField] private string weaponImage = "w_01";
        public DropKind Kind { get; private set; }
        public int Value { get; private set; }
        public EquipmentInstance Equipment { get; private set; }
        public Vector2 Position { get; private set; }
        public int RemainingTicks { get; private set; }
        public bool Expired { get; private set; }
        public bool Landed { get; private set; }
        public OriginalActor WeaponActor => weapon;
        private int speed, direction;
        private bool falling;

#if UNITY_EDITOR
        public void PrepareAsset(SpriteRenderer[] renderers, OriginalActor actor = null, string image = null)
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Prepare drop assets in edit mode");
            pieces = renderers; weapon = actor;
            if (image != null) weaponImage = image;
        }
#endif
        public void Initialize(DropKind kind, int value, EquipmentInstance equipment, Vector2 position, int dropDirection)
        {
            Kind = kind; Value = value; Equipment = equipment; Position = position;
            RemainingTicks = 300; speed = 10; direction = dropDirection;
            Expired = Landed = falling = false;
            if (weapon)
            {
                weapon.Initialize(new SpriteDefinition { id = -1000, type = 17, name = "武器掉落", animation = "z_weapondrop",
                    images = new[] { "action/" + weaponImage, "action/i_00" } }, -1000, position, 2);
                weapon.ActionFinished += FinishLanding;
                weapon.Act(0);
            }
            Refresh();
        }
        private void FinishLanding(OriginalActor actor)
        {
            if (actor.actionIndex == 0) { Landed = true; actor.Act(1); }
        }
        // One original NPCSprite.cycle (80 ms). The caller controls scene/menu/critical pauses.
        public bool Step(OriginalActor role, OriginalMap map)
        {
            if (Expired) return false;
            if (Kind == DropKind.Weapon) { weapon.Tick(.08f); return false; }
            if (RemainingTicks <= 0) { Expired = true; return false; }
            var point = Position;
            bool collect = false;
            if (RemainingTicks < 260)
            {
                bool closeX = false, closeY = false;
                if (point.x <= role.mapPosition.x - 15) point.x += 15;
                else if (point.x > role.mapPosition.x + role.Animation.footWidth + 15) point.x -= 15;
                else closeX = true;
                if (point.y <= role.mapPosition.y - 15) point.y += 15;
                else if (point.y > role.mapPosition.y + role.Animation.footHeight + 15) point.y -= 15;
                else closeY = true;
                collect = closeX && closeY;
            }
            else if (!Landed)
            {
                if (!falling)
                {
                    if (speed > 1) { if (point.y >= speed) point.y -= speed; speed -= 2; }
                    else falling = true;
                }
                else if (speed <= 8) { speed += 2; if (point.y <= map.Height - speed) point.y += speed; }
                else Landed = true;
                if (direction == 0 && point.x > 1) point.x -= 2;
                else if (direction == 1 && point.x < map.Width - 1) point.x += 2;
            }
            Position = point; RemainingTicks--; Refresh();
            return collect;
        }
        public bool Touches(OriginalActor role)
        {
            if (Kind != DropKind.Weapon || Expired) return false;
            var p = role.mapPosition;
            return p.x < Position.x + weapon.Animation.footWidth && Position.x < p.x + role.Animation.footWidth &&
                p.y < Position.y + weapon.Animation.footHeight && Position.y < p.y + role.Animation.footHeight;
        }
        private void Refresh()
        {
            if (weapon) { weapon.mapPosition = Position; weapon.RefreshFrame(); return; }
            transform.position = new Vector3(Position.x, -Position.y, 0);
            for (int i = 0; i < pieces.Length; i++) pieces[i].sortingOrder = i;
        }
    }
}
