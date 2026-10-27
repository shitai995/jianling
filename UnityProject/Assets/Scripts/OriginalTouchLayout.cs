using System;
using UnityEngine;

namespace GuJian
{
    [CreateAssetMenu(menuName = "古剑/触屏操作布局")]
    public sealed class OriginalTouchLayout : ScriptableObject
    {
        [Serializable] public sealed class Control
        {
            public string label;
            public Rect bounds;
            [Tooltip("0上 1下 2左 3右 4普攻/交谈 5菜单 7技能 8崩击 9合体 10葬剑 11交谈 12小地图")]
            public int code;
            public Texture2D normal, pressed;
        }
        public OriginalAnimation direction;
        public Texture2D[] directionSheets;
        public OriginalAnimation criticalReady;
        public Texture2D[] criticalReadySheets;
        public Texture2D back, backPressed;
        [HideInInspector] public int presentationVersion;
        public Rect directionBounds = new Rect(-1, 197, 124, 124);
        public bool visible = true;
        [Range(.1f, 1)] public float opacity = 1;
        public Control[] controls;
    }
}
