using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        [SerializeField] private OriginalUiLayout originalUi;
        private bool portraitOnLeft;
        public Texture2D DialoguePortrait
        {
            get
            {
                if (!originalUi || originalUi.portraits == null) return null;
                if (!string.IsNullOrEmpty(portrait))
                {
                    string id = System.IO.Path.GetFileNameWithoutExtension(portrait);
                    var specified = originalUi.portraits.FirstOrDefault(p => p.texture && p.texture.name == id);
                    return specified == null ? null : specified.texture;
                }
                var match = originalUi.portraits.FirstOrDefault(p => p.speaker == speaker);
                return match == null ? null : match.texture;
            }
        }
        private void DrawOriginalHud()
        {
            if (!originalUi || !Role) return;
            var ui = originalUi; int tick = Mathf.FloorToInt(Time.time / .08f);
            if (introductory) { var color=GUI.color; GUI.color=Color.black; GUI.DrawTexture(new Rect(0,0,ViewWidth,ViewHeight),Texture2D.whiteTexture); GUI.color=color; }
            OriginalGui.Animation(ui.scene,ui.sceneSheets,0,0,ui.hudOrigin);
            OriginalGui.Bar(ui.health,ui.healthBounds,Role.hp / (float)Mathf.Max(1,Role.maxHp));
            DrawHealthDecrement();
            int ordinaryDurability=integrated?Equipped.durability:durability;
            OriginalGui.Animation(ui.scene,ui.sceneSheets,ordinaryDurability > 0 ? 1 : swordBreakVisible ? 2 : 4,
                ordinaryDurability == 0 && swordBreakVisible ? swordBreakCounter - 1 : 0,ui.hudOrigin);
            OriginalGui.Bar(ui.durability,ui.durabilityBounds,ordinaryDurability / (float)Mathf.Max(1,Equipped.MaximumDurability));
            OriginalGui.Animation(ui.scene,ui.sceneSheets,5,0,ui.hudOrigin);
            if (ui.roleHead) GUI.DrawTexture(new Rect(4,-2,ui.roleHead.width,ui.roleHead.height),ui.roleHead);
            if(ui.energy && energy<2000)
            {
                int missing=ui.energy.height*(2000-Mathf.Clamp(energy,0,2000))/2000;
                OriginalGui.Region(ui.energy,new Rect(0,missing,ui.energy.width,ui.energy.height-missing),new Rect(39,34+missing,ui.energy.width,ui.energy.height-missing));
            }
            else if(energy>=2000) OriginalGui.Animation(ui.scene,ui.sceneSheets,energyHudFlash?7:8,energyHudFlash?energyHudCounter-1:tick,ui.hudOrigin);
            OriginalGui.Bar(ui.experience,ui.experienceBounds,experience / Mathf.Max(1f,(float)level*level*level*25));
            OriginalGui.Number(ui.menuNumbers,level,new Vector2(179,37),10,1,-2);
            var boss = actors.Values.FirstOrDefault(a => a.Definition.type == 10 && a.hp > 0 && !a.hidden);
            if (boss)
            {
                OriginalGui.Frame(ui.scene,ui.sceneSheets,8,ui.hudOrigin);
                OriginalGui.Bar(ui.bossHealth,new Rect(58,54,158,9),boss.hp/(float)Mathf.Max(1,boss.maxHp));
            }
            if (Actor(-2))
            {
                OriginalGui.Animation(ui.scene,ui.sceneSheets,11,0,ui.hudOrigin);
                OriginalGui.Bar(ui.spiritDurability,new Rect(203,303,100,8),(integrated ? durability : spiritDurability)/(float)Mathf.Max(1,SpiritAttribute(1)));
                if (ui.spiritHead) GUI.DrawTexture(new Rect(148,273,ui.spiritHead.width,ui.spiritHead.height),ui.spiritHead);
            }
            DrawOriginalCombo();
            DrawSkillShortcut();
            if (introductory) OriginalGui.Frame(ui.scene,ui.sceneSheets,23,ui.hudOrigin);
        }
        private Rect DrawOriginalDialogue()
        {
            Rect bounds = originalUi ? originalUi.dialogueBounds : new Rect(10,204,513,108);
            var image = DialoguePortrait;
            if (image && !storyScreen)
                GUI.DrawTexture(new Rect(portraitOnLeft ? bounds.x : bounds.xMax-image.width,bounds.y-image.height+5,image.width,image.height),image);
            if (originalUi && originalUi.dialogue)
            {
                var old=GUI.matrix;
                if (portraitOnLeft) GUI.matrix=old*Matrix4x4.TRS(new Vector3(bounds.x*2+bounds.width,0,0),Quaternion.identity,new Vector3(-1,1,1));
                GUI.DrawTexture(bounds,originalUi.dialogue); GUI.matrix=old;
            }
            else GUI.Box(bounds,"");
            return bounds;
        }
        private void DrawItemIcon(int id, Rect bounds)
        {
            if (!originalUi || !originalUi.itemIcons || id < 0) return;
            OriginalGui.Region(originalUi.itemIcons,new Rect((id == 23 ? 8 : id)*22,0,22,22),bounds);
        }
        private void DrawEquipmentIcon(EquipmentDefinition data, Rect bounds)
        {
            if (!originalUi || !originalUi.weaponIcons || string.IsNullOrEmpty(data.icon)) return;
            var parts = data.icon.Split(',');
            OriginalGui.Region(originalUi.weaponIcons,new Rect(int.Parse(parts[1])*22,int.Parse(parts[0])*22,22,22),bounds);
        }
    }
}
