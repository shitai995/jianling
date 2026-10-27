using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuJian
{
    [CreateAssetMenu(menuName = "古剑/游戏数据库")]
    public sealed class OriginalDatabase : ScriptableObject
    {
        public SpriteDefinition[] sprites;
        public StageDefinition[] stages;
        public EquipmentDefinition[] equipment;
        public ItemDefinition[] items;
        public SkillDefinition[] skills;
        public EnemyDefinition[] enemies;
        public AttributeDefinition[] roleAttributes, swordSpiritAttributes, enemyAttributes;
        [Header("扩展内容（独立于原版基线表）")]
        public EquipmentDefinition[] addedEquipment;
        public ItemDefinition[] addedItems;
        public SkillDefinition[] addedSkills;
        public EnemyDefinition[] addedEnemies;
        public SpriteDefinition[] addedSprites;
        public StageDefinition[] addedStages;

        public T[] Get<T>(string table = null)
        {
            object result = typeof(T) == typeof(SpriteDefinition) ? (object)Merge(sprites, addedSprites) :
                typeof(T) == typeof(StageDefinition) ? Merge(stages, addedStages) :
                typeof(T) == typeof(EquipmentDefinition) ? Merge(equipment, addedEquipment) :
                typeof(T) == typeof(ItemDefinition) ? Merge(items, addedItems) :
                typeof(T) == typeof(SkillDefinition) ? Merge(skills, addedSkills) :
                typeof(T) == typeof(EnemyDefinition) ? Merge(enemies, addedEnemies) :
                typeof(T) == typeof(AttributeDefinition) && table == "role" ? roleAttributes :
                typeof(T) == typeof(AttributeDefinition) && table == "ego" ? swordSpiritAttributes :
                typeof(T) == typeof(AttributeDefinition) && table == "enemybase" ? enemyAttributes : null;
            return result as T[];
        }
        private sealed class TableView { public Array baseline, additions, combined; }
        [NonSerialized] private Dictionary<Type, TableView> tableViews;
        public void ClearTableCache() { tableViews?.Clear(); }
        private void OnValidate() { ClearTableCache(); }
        private T[] Merge<T>(T[] baseline, T[] additions)
        {
            if (additions == null || additions.Length == 0) return baseline;
            if (tableViews == null) tableViews = new Dictionary<Type, TableView>();
            if (!tableViews.TryGetValue(typeof(T), out var cached) || cached.baseline != baseline || cached.additions != additions)
            {
                cached = new TableView { baseline = baseline, additions = additions, combined =
                    (baseline ?? Array.Empty<T>()).Concat(additions).ToArray() };
                tableViews[typeof(T)] = cached;
            }
            return (T[])cached.combined;
        }
    }
}
