using System;
using UnityEngine;

namespace GuJian
{
    [CreateAssetMenu(menuName = "古剑/原版界面资源")]
    public sealed class OriginalUiLayout : ScriptableObject
    {
        [Serializable] public sealed class Portrait { public string speaker; public Texture2D texture; }
        [Serializable] public sealed class PreviewAnimation { public OriginalAnimation animation; public Texture2D[] sheets; }
        public PreviewAnimation[] skillPreviewEffects;
        public PreviewAnimation skillPreviewSupport;
        public PreviewAnimation criticalHitOverlay;
        public OriginalAnimation scene, menu, mainMenu;
        public Texture2D[] menuSheets, mainMenuSheets, marketplaceOptions, marketplaceInfo;
        public Texture2D settingsArrow;
        public Texture2D worldMap, spiritExperience;
        public Texture2D messagePanel, menuScrollSign;
        public OriginalAnimation keyPressTip;
        public Texture2D[] keyPressTipSheets;
        public Texture2D menuNumbers, menuGoldNumbers, menuSilverNumbers, stackNumbers, money, transactionArrow, attributeGainNumbers, attributeGainArrow;
        public Texture2D[] iconBackgrounds;
        public Rect menuBounds = new Rect(66.5f, 10, 400, 300);
        public Texture2D[] sceneSheets;
        public Texture2D health, durability, experience, energy, bossHealth, spiritDurability, roleHead, spiritHead, dialogue;
        public Texture2D itemIcons, weaponIcons, skillIcons, combo, comboNumbers, comboBonus;
        public Texture2D shortcutIcons;
        public Texture2D[] comboFrames, comboCountNumbers;
        public Texture2D[] combatNumbers;
        public Texture2D combatMiss;
        public Sprite[] combatDigits;
        public Sprite combatMissSprite;
        public Sprite[] enemyHealthBars;
        public Rect shortcutBounds = new Rect(330,279,38,38);
        [Min(1)] public int stageNameFontSize = 18;
        [Tooltip("Original Android font height varies with system metrics; tune after device comparison.")]
        [Min(1)] public int stageNameFontHeight = 24;
        public Portrait[] portraits;
        public Vector2 hudOrigin = new Vector2(266,160);
        public Rect dialogueBounds = new Rect(132,230,268,90);
        public int dialogueContentWidth = 236, dialogueLinesPerPage = 2, dialogueFontSize = 18;
        public Rect healthBounds = new Rect(53,11,145,9), durabilityBounds = new Rect(55,25,136,9);
        public Rect experienceBounds = new Rect(60,38,62,11);
    }
}
