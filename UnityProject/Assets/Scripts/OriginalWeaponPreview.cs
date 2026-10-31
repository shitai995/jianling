using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private EquipmentInstance weaponPreview;
        private bool previewReturnMenu;
        private float weaponPreviewElapsed;
        private Rect weaponPreviewBounds;
        public bool WeaponPreviewVisible => weaponPreview != null;
        public int WeaponPreviewId => weaponPreview == null ? -1 : weaponPreview.id;
        public int WeaponPreviewReinforcement => weaponPreview == null ? 0 : weaponPreview.reinforcement;
        public int WeaponPreviewFrame => !WeaponPreviewVisible || !Role ? 0 : (int)(weaponPreviewElapsed / .08f) % Role.Animation.actions[0].steps.Length;
        public string WeaponPreviewSkillLabel
        {
            get
            {
                if (weaponPreview == null) return "";
                var data = weaponPreview.Data;
                if (data.skill == -1) return "随机技能";
                var skill = OriginalAssets.Table<SkillDefinition>("skill").FirstOrDefault(s => s.id == data.skill);
                return skill == null ? "无技能" : skill.name + " " + Mathf.Clamp((data.level - skill.rank) / 10 + 1, 1, 9) + "级";
            }
        }
        public int GetWeaponPreviewAttribute(int index)
        {
            if (weaponPreview == null) return 0;
            switch (index)
            {
                case 0: return weaponPreview.Hp;
                case 1: return weaponPreview.MaximumDurability;
                case 2: return weaponPreview.Attack;
                case 3: return weaponPreview.Defense;
                case 4: return weaponPreview.Data.hit;
                case 5: return weaponPreview.Data.dodge;
                case 6: return weaponPreview.Data.critical;
                default: return 0;
            }
        }
        public bool PreviewShopWeapon(int id)
        {
            if (!ShopOpen || shopType < 2 || shopSelling || TradeConfirming || WeaponPreviewVisible) return false;
            var data = OriginalAssets.Table<EquipmentDefinition>("equip").FirstOrDefault(e => e.id == id);
            if (data == null || data.grade >= 3 || data.level > level) return false;
            return OpenWeaponPreview(id, 0);
        }
        private bool OpenWeaponPreview(int id, int reinforcement)
        {
            if (!Role || WeaponPreviewVisible) return false;
            if (!OriginalAssets.Table<EquipmentDefinition>("equip").Any(e => e.id == id)) return false;
            // A display-only instance deliberately leaves its skill unset: previewing must not roll a random reward.
            weaponPreview = new EquipmentInstance { id = id, reinforcement = Mathf.Clamp(reinforcement, 0, 9) };
            previewReturnMenu = menu; menu = true; weaponPreviewElapsed = 0;
            weaponPreviewBounds = PreviewIdleBounds(Role.Animation);
            return true;
        }
        public void CloseWeaponPreview()
        {
            if (!WeaponPreviewVisible) return;
            weaponPreview = null; weaponPreviewElapsed = 0; menu = previewReturnMenu;
        }
        private void TickWeaponPreview(float delta)
        {
            if (!WeaponPreviewVisible) return;
            weaponPreviewElapsed += delta;
            if (GameInput.Pressed(KeyCode.Escape) || GameInput.Pressed(KeyCode.Return) || GameInput.Pressed(KeyCode.J) || GameInput.Pressed(KeyCode.Space)) CloseWeaponPreview();
        }
        private static Rect PreviewIdleBounds(OriginalAnimation animation)
        {
            Rect bounds = new Rect(); bool first = true;
            foreach (var step in animation.actions[0].steps)
            foreach (var piece in animation.frames[step.frame].pieces)
            {
                var size = PreviewPieceRect(animation, piece);
                if (size.width <= 0 || size.height <= 0) continue;
                if (first) { bounds = size; first = false; }
                else bounds = Rect.MinMaxRect(Mathf.Min(bounds.xMin, size.xMin), Mathf.Min(bounds.yMin, size.yMin), Mathf.Max(bounds.xMax, size.xMax), Mathf.Max(bounds.yMax, size.yMax));
            }
            return first ? new Rect(0, 0, 1, 1) : bounds;
        }
        private static Rect PreviewPieceRect(OriginalAnimation animation, FramePiece piece)
        {
            if (piece.image < 0) return new Rect(piece.x, piece.y, piece.w, piece.h);
            var r = animation.sheets[piece.image].rects[piece.rect];
            return new Rect(piece.x, piece.y, piece.transform >= 4 ? r.h : r.w, piece.transform >= 4 ? r.w : r.h);
        }
        private void DrawPreviewActor(Rect area)
        {
            var animation = Role.Animation;
            var frame = animation.frames[animation.actions[0].steps[WeaponPreviewFrame].frame];
            float scale = Mathf.Min(2, area.width / Mathf.Max(1, weaponPreviewBounds.width), area.height / Mathf.Max(1, weaponPreviewBounds.height));
            Vector2 origin = new Vector2(area.x + (area.width - weaponPreviewBounds.width * scale) / 2 - weaponPreviewBounds.x * scale,
                area.y + (area.height - weaponPreviewBounds.height * scale) / 2 - weaponPreviewBounds.y * scale);
            Matrix4x4 matrix = GUI.matrix; Color color = GUI.color;
            try
            {
                foreach (var piece in frame.pieces)
                {
                    var bounds = PreviewPieceRect(animation, piece);
                    if (bounds.width <= 0 || bounds.height <= 0) continue;
                    if (piece.image < 0)
                    {
                        GUI.color = new Color(((piece.color >> 16) & 255) / 255f, ((piece.color >> 8) & 255) / 255f, (piece.color & 255) / 255f);
                        GUI.DrawTexture(new Rect(origin.x + piece.x * scale, origin.y + piece.y * scale, bounds.width * scale, bounds.height * scale), Texture2D.whiteTexture);
                        GUI.color = color; continue;
                    }
                    string image = piece.image == 4 ? "action/" + weaponPreview.Data.image : Role.Definition.images[piece.image];
                    var texture = OriginalAssets.Texture(image);
                    var r = animation.sheets[piece.image].rects[piece.rect];
                    int mode = piece.transform;
                    bool flipX = mode == 2 || mode == 4 || mode == 7, flipY = mode == 1;
                    float angle = mode == 3 ? 180 : mode == 4 || mode == 6 ? -90 : mode == 5 || mode == 7 ? 90 : 0;
                    var center = new Vector3(origin.x + bounds.center.x * scale, origin.y + bounds.center.y * scale, 0);
                    GUI.matrix = matrix * Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), new Vector3(flipX ? -1 : 1, flipY ? -1 : 1, 1));
                    GUI.DrawTextureWithTexCoords(new Rect(-r.w * scale / 2, -r.h * scale / 2, r.w * scale, r.h * scale), texture,
                        new Rect((float)r.x / texture.width, 1f - (float)(r.y + r.h) / texture.height, (float)r.w / texture.width, (float)r.h / texture.height));
                    GUI.matrix = matrix;
                }
            }
            finally { GUI.matrix = matrix; GUI.color = color; }
        }
        private void DrawWeaponPreview()
        {
            if (originalUi && originalUi.menu != null && originalUi.menu.actions != null) { DrawAtlasWeaponPreview(); return; }
            var data = weaponPreview.Data;
            var old = GUI.color; GUI.color = new Color(.04f, .05f, .08f, .99f);
            GUI.DrawTexture(new Rect(22, 16, 489, 288), Texture2D.whiteTexture); GUI.color = old;
            GUI.Box(new Rect(22, 16, 489, 288), "");
            GUI.Label(new Rect(38, 25, 392, 30), data.name + (weaponPreview.reinforcement > 0 ? " +" + weaponPreview.reinforcement : "") + "    等级 " + data.level, titleStyle);
            if (GUI.Button(new Rect(440, 24, 56, 29), "返回", buttonStyle)) { CloseWeaponPreview(); return; }
            DrawPreviewActor(new Rect(43, 65, 181, 130));
            GUI.Label(new Rect(40, 207, 200, 77), Clean(data.description), bodyStyle);
            string[] names = { "气血", "耐久", "攻击", "防御", "命中", "闪避", "暴击" };
            for (int i = 0; i < names.Length; i++) GUI.Label(new Rect(257, 62 + i * 21, 232, 24), names[i] + "    " + GetWeaponPreviewAttribute(i), bodyStyle);
            GUI.Label(new Rect(257, 216, 235, 47), WeaponPreviewSkillLabel, bodyStyle);
            GUI.Label(new Rect(257, 271, 235, 25), previewReturnMenu && menuPage == 5 ? "售价 " + data.buy + " 两" : "回车 / Esc 继续", bodyStyle);
        }
    }
}
