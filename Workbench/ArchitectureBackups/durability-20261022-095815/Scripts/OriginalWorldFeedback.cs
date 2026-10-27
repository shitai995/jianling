using UnityEngine;

namespace GuJian
{
    // Feedback renderers and atlas slices are saved in the actor prefab/UI asset.
    public sealed class OriginalWorldFeedback : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] digits;
        [SerializeField] private SpriteRenderer miss;
        [SerializeField] private SpriteRenderer healthBar;
        public int Capacity => digits == null ? 0 : digits.Length;
#if UNITY_EDITOR
        public void PrepareAsset()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Prepare feedback in edit mode");
            if (digits == null || digits.Length != 50) digits = new SpriteRenderer[50];
            for (int i = 0; i < digits.Length; i++)
                if (!digits[i]) digits[i] = CreatePiece("Combat digit " + i);
            if (!miss) miss = CreatePiece("Combat miss");
            if (!healthBar) healthBar = CreatePiece("Enemy health bar");
        }
        private SpriteRenderer CreatePiece(string label)
        {
            var child = new GameObject(label); child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>(); renderer.enabled = false;
            return renderer;
        }
#endif
        public void Clear()
        {
            if (digits != null) foreach (var piece in digits) if (piece) piece.enabled = false;
            if (miss) miss.enabled = false;
            if (healthBar) healthBar.enabled = false;
        }
        public void EnemyHealth(OriginalActor actor, OriginalUiLayout ui)
        {
            // Enemy.act: state1 is action2 and actions4..13. Boss type10 has a separate HUD.
            int action = actor.LogicalAction;
            if ((actor.Definition.type != 9 && actor.Definition.type != 11) ||
                !(action == 2 || action >= 4 && action <= 13) || actor.maxHp <= 0) return;
            if (!healthBar || ui.enemyHealthBars == null || ui.enemyHealthBars.Length != 20)
                throw new System.InvalidOperationException("Missing saved enemy health bar resources");
            int width = Mathf.Clamp(unchecked(actor.hp * 19) / actor.maxHp, 0, 19);
            healthBar.sprite = ui.enemyHealthBars[width];
            if (!healthBar.sprite) throw new System.InvalidOperationException("Missing saved enemy health bar width " + width);
            int left = (actor.Animation.footWidth - 20) / 2;
            int top = actor.CurrentFrame.edgeRect.y - 8;
            Place(healthBar, left + 10.5f, -top - 2.5f, 2100);
        }
        public void Number(OriginalActor actor, OriginalUiLayout ui, int slot, int value, int type, int ticks)
        {
            if (slot >= 5 || value <= 0 || type < 0 || type > 3 || ui.combatDigits == null || ui.combatDigits.Length != 40) return;
            string text = value.ToString();
            var first = ui.combatDigits[type * 10]; if (!first) return;
            int width = (int)first.rect.width, height = (int)first.rect.height;
            int left = (actor.Animation.footWidth >> 1) - (((width - 1) * text.Length) >> 1);
            int top = actor.CurrentFrame.edgeRect.y - (10 - ticks) * 6;
            for (int i = 0; i < text.Length && i < 10; i++)
            {
                var renderer = digits[slot * 10 + i];
                renderer.sprite = ui.combatDigits[type * 10 + text[i] - '0'];
                Place(renderer, left + i * (width - 1) + width / 2f, -top - height / 2f, 2300 + (4 - slot) * 10 + i);
            }
        }
        public void Miss(OriginalActor actor, OriginalUiLayout ui, int ticks)
        {
            if (ticks <= 0 || !ui.combatMissSprite) return;
            var size = ui.combatMissSprite.rect.size;
            int x = actor.IsEnemy ? (actor.Animation.footWidth >> 1) - ((int)size.x >> 1) : (actor.Animation.footWidth - (int)size.x) >> 1;
            int top = -actor.CurrentFrame.edgeRect.h - (int)size.y - 10 + ticks;
            miss.sprite = ui.combatMissSprite;
            Place(miss, x + size.x / 2, -top - size.y / 2, 2200);
        }
        private static void Place(SpriteRenderer renderer, float x, float y, int order)
        {
            renderer.transform.localPosition = new Vector3(x, y, 0);
            renderer.sortingOrder = order; renderer.enabled = true;
        }
    }
}
