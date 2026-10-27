using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private bool roleSkillPreview;
        private int roleSkillPreviewIndex;
        private Texture2D[] roleSkillPreviewSheets;
        public bool RoleSkillPreviewVisible => menu && menuPage == 7 && roleSkillPreview;
        private int RoleSkillPreviewType => SkillBehavior(equippedSkill) < 14 ? -1 : SkillBehavior(equippedSkill) < 17 ? 1 : SkillBehavior(equippedSkill) < 20 ? 2 : 0;
        private int RoleSkillPreviewAction => (SkillBehavior(equippedSkill) < 5 ? 0 : SkillBehavior(equippedSkill) < 14 ? SkillBehavior(equippedSkill) + 7 : 21) + (integrated ? 27 : 0);
        private int RoleSkillPreviewLength
        {
            get
            {
                if (!Role) return 0;
                // cycleGameMenu uses the ordinary action table even while integrated.
                int action = SkillBehavior(equippedSkill) < 5 ? 11 : SkillBehavior(equippedSkill) < 14 ? SkillBehavior(equippedSkill) + 7 : 21;
                action = Mathf.Clamp(action, 0, Role.Animation.actions.Length - 1);
                int count = Role.Animation.actions[action].steps.Length;
                if (RoleSkillPreviewType >= 0)
                    count += originalUi.skillPreviewSupport.animation.actions[RoleSkillPreviewType].steps.Length;
                return count;
            }
        }
        private void ResetRoleSkillPreview()
        {
            roleSkillPreview = false; roleSkillPreviewIndex = 0;
        }
        private void StartRoleSkillPreview()
        {
            if (!menu || menuPage != 7 || roleSkillPreview || !Role || WeaponPreviewVisible) return;
            roleSkillPreview = true; roleSkillPreviewIndex = 0;
        }
        private void CycleRoleSkillPreview()
        {
            if (!menu || !roleSkillPreview || !Role || systemMenuPage != SystemMenuPage.None || menuPage == 5 || WeaponPreviewVisible || DialogueVisible || reinforceOpen) return;
            if (roleSkillPreviewIndex < RoleSkillPreviewLength - 1) roleSkillPreviewIndex++;
            else ResetRoleSkillPreview();
        }
        private void DrawPreviewAnimation(OriginalAnimation animation, Texture2D[] sheets, int action, int tick, bool mirrored)
        {
            var matrix = GUI.matrix;
            try
            {
                GUI.matrix = matrix * Matrix4x4.TRS(new Vector3(155 + (mirrored ? animation.footWidth : 0), 233, 0),
                    Quaternion.identity, new Vector3(mirrored ? -1 : 1, 1, 1));
                OriginalGui.Animation(animation, sheets, action, tick, Vector2.zero);
            }
            finally { GUI.matrix = matrix; }
        }
        private void DrawOriginalSkillPreview()
        {
            var color = GUI.color; GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, ViewWidth, ViewHeight), Texture2D.whiteTexture); GUI.color = color;
            if (roleSkillPreviewSheets == null || roleSkillPreviewSheets.Length != Role.Definition.images.Length)
                roleSkillPreviewSheets = new Texture2D[Role.Definition.images.Length];
            for (int i = 0; i < roleSkillPreviewSheets.Length; i++) roleSkillPreviewSheets[i] = Role.ImageTexture(i);
            int action = RoleSkillPreviewAction, logical = action - (integrated ? 27 : 0);
            int castLength = Role.Animation.actions[21].steps.Length;
            if (RoleSkillPreviewType >= 0 && roleSkillPreviewIndex >= castLength)
            {
                DrawPreviewAnimation(Role.Animation, roleSkillPreviewSheets, 0, originalUiTicks, true);
                var support = originalUi.skillPreviewSupport;
                DrawPreviewAnimation(support.animation, support.sheets, RoleSkillPreviewType, roleSkillPreviewIndex - castLength, false);
                return;
            }
            OriginalUiLayout.PreviewAnimation effect = null;
            if (SkillBehavior(equippedSkill) >= 5 && SkillBehavior(equippedSkill) <= 12) effect = originalUi.skillPreviewEffects[SkillBehavior(equippedSkill) - 5];
            else if (SkillBehavior(equippedSkill) >= 14) effect = originalUi.skillPreviewEffects[8];
            if (effect != null && (logical == 12 || logical == 14 || logical == 16 || logical == 21))
                DrawPreviewAnimation(effect.animation, effect.sheets, 1, roleSkillPreviewIndex, true);
            DrawPreviewAnimation(Role.Animation, roleSkillPreviewSheets, action, roleSkillPreviewIndex, true);
            if (effect != null && (logical >= 12 && logical <= 19 || logical == 21))
                DrawPreviewAnimation(effect.animation, effect.sheets, 0, roleSkillPreviewIndex, true);
        }
    }
}