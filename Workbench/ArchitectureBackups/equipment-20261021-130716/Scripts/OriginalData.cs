using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuJian
{
    [Serializable] public class Rows<T> { public T[] rows; }
    [Serializable] public class ScriptData { public string[] instructions; }
    [Serializable] public class OriginalRect { public int x, y, w, h; }
    [Serializable] public class AnimationSheet { public OriginalRect[] rects; }
    [Serializable] public class FramePiece { public int image, rect, transform, x, y, w, h, color; }
    [Serializable] public class OriginalFrame
    {
        public FramePiece[] pieces;
        public OriginalRect[] attackRects, bodyRects;
        public OriginalRect edgeRect;
    }
    [Serializable] public class AnimationStep { public int frame, vx, vy, attackType; public bool attack, vulnerable; }
    [Serializable] public class OriginalAction { public AnimationStep[] steps; }
    [Serializable] public class OriginalAnimation
    {
        public AnimationSheet[] sheets;
        public OriginalFrame[] frames;
        public OriginalAction[] actions;
        public int footWidth, footHeight;
    }
    [Serializable] public class SpriteDefinition
    {
        public int id, type;
        public string name, animation, imageList;
        public string[] images;
    }
    [Serializable] public class OriginalCell
    {
        public int x, y, land;
        public bool blocked;
        public int[] tiles, images, transforms;
        public bool[] above;
    }
    [Serializable] public class MapPart { public int layer, image, transform, x, y; }
    [Serializable] public class OriginalMap
    {
        public string id;
        public string[] images;
        public int type, cellWidth, cellHeight, columns, rows, backgroundColor;
        public OriginalCell[] cells;
        public MapPart[] parts;
        public int Width => columns * cellWidth;
        public int Height => rows * cellHeight;
        public bool CanPass(float x, float y)
        {
            int col = Mathf.FloorToInt(x / cellWidth), row = Mathf.FloorToInt(y / cellHeight);
            return col >= 0 && row >= 0 && col < columns && row < rows && !cells[col * rows + row].blocked;
        }
    }
    [Serializable] public class StageDefinition { public string script, name, map; }
    [Serializable] public class EquipmentDefinition
    {
        public int id, grade, level, skill, durability, hp, attack, defense, hit, dodge, critical, buy, sell;
        public string name, description, image, effects, icon;
    }
    [Serializable] public class ItemDefinition
    {
        public int id, buy, sell, hp, durability, type;
        public string name, description, effect;
    }
    [Serializable] public class SkillDefinition
    {
        public int id, rank, cooldown, holdDamage;
        public string name, description, damage, amend, cost;
    }
    [Serializable] public class EnemyDefinition
    {
        public int id, hold, hpCoefficient, attackCoefficient, hit, dodge, vision, experience, money;
        public string attackRanges, damageCoefficients, holdDamage, attackIntervals, equipmentDrops, itemDrops;
    }
    [Serializable] public class AttributeDefinition
    {
        public string hp, attack, defense, hit, dodge, critical, levels, durability;
        public int injuryCoefficient;
        public static int AtLevel(string formula, int level)
        {
            if (string.IsNullOrEmpty(formula)) return 0;
            var values = formula.Split(',');
            return int.Parse(values[0]) + (values.Length > 1 ? int.Parse(values[1]) * (level - 1) : 0);
        }
    }

    public static class OriginalAssets
    {
        private static readonly Dictionary<string, object> Cache = new Dictionary<string, object>();
        private static OriginalAssetCatalog catalog;
        public static OriginalAssetCatalog Catalog => catalog ? catalog : (catalog = Resources.Load<OriginalAssetCatalog>("OriginalAssetCatalog"));
        public static void ClearCache() { Cache.Clear(); }
        public static string[] Script(string name)
        {
            string id = StorySequence.NormalizeId(name);
            var story = Resources.Load<StorySequence>("Stories/" + id);
            if (story)
            {
                if (!string.IsNullOrEmpty(story.ScriptId) && story.ScriptId != id)
                    throw new InvalidOperationException("剧情资产标识与路径不一致：" + id + " / " + story.ScriptId);
                return story.ReadInstructions();
            }
            return (string[])Json<ScriptData>("Scripts/" + id).instructions.Clone();
        }
        public static bool HasScript(string name)
        {
            string id = StorySequence.NormalizeId(name);
            return Resources.Load<StorySequence>("Stories/" + id) || Resources.Load<TextAsset>("Original/Scripts/" + id);
        }
        public static T Json<T>(string path) where T : class
        {
            if (Cache.TryGetValue(path, out var cached)) return (T)cached;
            var asset = Resources.Load<TextAsset>("Original/" + path);
            if (!asset) throw new InvalidOperationException("Missing original data: " + path);
            var result = JsonUtility.FromJson<T>(asset.text);
            Cache[path] = result;
            return result;
        }
        public static Texture2D Texture(string path)
        {
            var result = Resources.Load<Texture2D>("Original/" + path);
            if (!result) throw new InvalidOperationException("Missing original image: " + path);
            result.filterMode = FilterMode.Point;
            return result;
        }
        public static T[] Table<T>(string name)
        {
            var database = Resources.Load<OriginalDatabase>("OriginalDatabase");
            return database && database.Get<T>(name) != null ? database.Get<T>(name) : Json<Rows<T>>("Data/" + name).rows;
        }
        public static Sprite Sprite(string image, OriginalRect rect)
        {
            var baked = Catalog ? Catalog.FindSprite(image, rect) : null;
            if (baked) return baked;
            throw new InvalidOperationException("Missing baked sprite: " + OriginalAssetCatalog.SpriteKey(image, rect));
        }
    }
}
