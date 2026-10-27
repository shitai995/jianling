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

        public T[] Get<T>(string table = null)
        {
            object result = typeof(T) == typeof(SpriteDefinition) ? (object)sprites :
                typeof(T) == typeof(StageDefinition) ? stages :
                typeof(T) == typeof(EquipmentDefinition) ? equipment :
                typeof(T) == typeof(ItemDefinition) ? items :
                typeof(T) == typeof(SkillDefinition) ? skills :
                typeof(T) == typeof(EnemyDefinition) ? enemies :
                typeof(T) == typeof(AttributeDefinition) && table == "role" ? roleAttributes :
                typeof(T) == typeof(AttributeDefinition) && table == "ego" ? swordSpiritAttributes :
                typeof(T) == typeof(AttributeDefinition) && table == "enemybase" ? enemyAttributes : null;
            return result as T[];
        }
    }
}
