using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private bool comboHudFlash, energyHudFlash;
        private int comboHudCounter, energyHudCounter;
        private void NotifyComboHud()
        {
            // Role.cycle advances its effect counter after processAttack in the same cycle.
            comboHudFlash=true; comboHudCounter=1;
        }
        private void GainHitEnergy()
        {
            if(energy<1995) { energy+=5; return; }
            if(energy!=2000) energyHudFlash=true;
            energy=2000; energyHudCounter=1;
        }
        private void TickRoleHudFeedback()
        {
            if(comboHudFlash)
            {
                if(comboHudCounter<2) comboHudCounter++;
                else {comboHudFlash=false;comboHudCounter=0;}
            }
            if(energyHudFlash && originalUi && originalUi.scene.actions.Length>7)
            {
                if(energyHudCounter<originalUi.scene.actions[7].steps.Length) energyHudCounter++;
                else {energyHudFlash=false;energyHudCounter=0;}
            }
        }
        private void ResetHudFeedback()
        { comboHudFlash=energyHudFlash=false; comboHudCounter=energyHudCounter=0; }
        private void DrawOriginalCombo()
        {
            var ui=originalUi;
            if(comboHits<=1 || !ui || ui.comboFrames==null || ui.comboFrames.Length<3 || ui.comboCountNumbers==null || ui.comboCountNumbers.Length<3) return;
            int phase=comboHudFlash?comboHudCounter:0;
            var image=ui.comboFrames[phase]; var baseline=ui.comboFrames[0]; var numbers=ui.comboCountNumbers[phase];
            if(!image || !baseline || !numbers || !ui.comboCountNumbers[0]) return;
            float x=ViewWidth-baseline.width;
            GUI.DrawTexture(new Rect(x,35,image.width,image.height),image);
            OriginalGui.Number(numbers,comboHits,new Vector2(x,baseline.height+31-ui.comboCountNumbers[0].height),10,1,-1);
            // The source checks actual AP gain, including the minimum +1 above nine hits.
            var roleData=OriginalAssets.Table<AttributeDefinition>("role")[0];
            int roleAttack=OriginalCombatMath.Growth(roleData.attack,level-1);
            int baseAttack=CombatAttributeCalculator.ComboBaseAttack(roleAttack,integrated?SpiritAttribute(2):Equipped.Attack,integrated?spiritBonuses[2]:0);
            if(fighters.TryGetValue(-1,out var fighter) && fighter.attack>baseAttack && ui.comboBonus)
            {
                float bx=ViewWidth-ui.comboBonus.width, y=baseline.height+37;
                GUI.DrawTexture(new Rect(bx,y,ui.comboBonus.width,ui.comboBonus.height),ui.comboBonus);
                OriginalGui.Number(ui.comboNumbers,100+ComboAttackPercent,new Vector2(bx,y),10,1,-1);
            }
        }
        public int ShortcutIconIndex
        {
            get { int i=SkillBehavior(equippedSkill)-5; return i==6?7:i==7?6:i; }
        }
        private int ShortcutCycle
        {
            get
            {
                var data=OriginalAssets.Table<SkillDefinition>("skill").FirstOrDefault(s=>s.id==equippedSkill);
                return data==null?0:disableCooldown?1:data.cooldown;
            }
        }
        private void DrawSkillShortcut()
        {
            var ui=originalUi; int cycle=ShortcutCycle;
            if(!ui || !ui.shortcutIcons || equippedSkill<5 || cycle<=0) return;
            int elapsed=Mathf.Clamp(cycle-skillCooldownTicks,0,cycle);
            if(elapsed<=0) return;
            var bounds=ui.shortcutBounds;
            OriginalGui.Region(ui.shortcutIcons,new Rect(ShortcutIconIndex*38,0,38,38),bounds);
            if(elapsed>=cycle) return;
            var old=GUI.color; GUI.color=skillCooldownTicks<3?Color.white:Color.black;
            if(skillCooldownTicks<3) GUI.DrawTexture(bounds,Texture2D.whiteTexture);
            else
            {
                // Preserve the original integer 38px division before scaling the editable destination.
                int coveredStart=elapsed*38/cycle;
                GUI.DrawTexture(new Rect(bounds.x,bounds.y+bounds.height*coveredStart/38f,bounds.width,bounds.height*(38-coveredStart)/38f),Texture2D.whiteTexture);
            }
            GUI.color=old;
        }
    }
}
