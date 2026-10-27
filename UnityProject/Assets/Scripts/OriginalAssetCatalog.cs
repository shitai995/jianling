using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuJian
{
    // Built by OriginalAssetBaker. Runtime only loads these saved Unity assets.
    public sealed class OriginalAssetCatalog : ScriptableObject
    {
        [Serializable] public sealed class SpriteEntry { public string key; public Sprite sprite; }
        [Serializable] public sealed class MapEntry { public string id; public OriginalMap data; public GameObject prefab; }
        [Serializable] public sealed class ActorEntry { public int id; public OriginalActor prefab; }
        [Serializable] public sealed class EffectEntry { public string animation; public OriginalActor prefab; }
        [Serializable] public sealed class StageEntry { public string script; public OriginalStageLayout prefab; }
        public SpriteEntry[] sprites;
        public MapEntry[] maps;
        public ActorEntry[] actors;
        public EffectEntry[] effects;
        public StageEntry[] stages;
        public Sprite solid;
        private Dictionary<string, Sprite> spriteLookup;
        private SpriteEntry[] indexedSprites;
        public static string SpriteKey(string image, OriginalRect r) => image + ":" + r.x + "," + r.y + "," + r.w + "," + r.h;
        public Sprite FindSprite(string image, OriginalRect rect)
        {
            if (sprites == null) return null;
            if (spriteLookup == null || indexedSprites != sprites)
            {
                indexedSprites = sprites;
                spriteLookup = new Dictionary<string, Sprite>();
                foreach (var entry in sprites) spriteLookup[entry.key] = entry.sprite;
            }
            return spriteLookup.TryGetValue(SpriteKey(image, rect), out var sprite) ? sprite : null;
        }
    }
}
