using System;
using UnityEngine;

namespace GuJian
{
    [Serializable] public sealed class MiniMapRoom
    {
        public int stage, x, y;
        public int[] exits;
        public bool save;
    }
    [Serializable] public sealed class MiniMapArea
    {
        public int width, height;
        public MiniMapRoom[] rooms;
    }
    [Serializable] public sealed class MiniMapData { public MiniMapArea[] areas; }
    [CreateAssetMenu(menuName="古剑/区域小地图")]
    public sealed class OriginalMiniMapLayout : ScriptableObject
    {
        public MiniMapArea[] areas;
        public Texture2D current, mission, save;
    }
}
