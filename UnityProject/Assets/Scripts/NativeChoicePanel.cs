using UnityEngine;
using UnityEngine.UI;

namespace GuJian
{
    // Source panel atlas has four corners and four repeating edge tiles.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class NativeChoicePanel : MaskableGraphic
    {
        [SerializeField] private Texture texture;
        public override Texture mainTexture => texture ? texture : s_WhiteTexture;
        public void SetTexture(Texture value) { if (texture == value) return; texture = value; SetAllDirty(); }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if (!texture) return;
            float w = rectTransform.rect.width, h = rectTransform.rect.height;
            int tile = texture.height; if (w <= 0 || h <= 0 || tile <= 0) return;
            Quad(vh, 2, 2, Mathf.Max(0, w - 8), Mathf.Max(0, h - 12), 0, 0, 1, 1, new Color32(214, 140, 90, 255), true);
            float cw = Mathf.Min(w / 2, tile), ch = Mathf.Min(h / 2, tile);
            float right = w / 2 > tile ? w - tile : w / 2, bottom = h / 2 > tile ? h - tile : h / 2;
            for (float x = tile; x < w - tile; x += tile)
            {
                float part = Mathf.Min(tile, w - tile - x);
                Region(vh, x, 0, part, ch, tile * 4, 0, part, ch);
                Region(vh, x, bottom, part, ch, tile * 5, tile - ch, part, ch);
            }
            for (float y = tile; y < h - tile; y += tile)
            {
                float part = Mathf.Min(tile, h - tile - y);
                Region(vh, 0, y, cw, part, tile * 7, 0, cw, part);
                Region(vh, right, y, cw, part, tile * 6, 0, cw, part);
            }
            Region(vh, 0, 0, cw, ch, 0, 0, cw, ch);
            Region(vh, right, 0, cw, ch, tile, 0, cw, ch);
            Region(vh, 0, bottom, cw, ch, tile * 2, tile - ch, cw, ch);
            Region(vh, right, bottom, cw, ch, tile * 3, tile - ch, cw, ch);
        }
        private void Region(VertexHelper vh, float x, float y, float w, float h, float sx, float sy, float sw, float sh)
        {
            Quad(vh, x, y, w, h, sx / texture.width, 1 - (sy + sh) / texture.height,
                sw / texture.width, sh / texture.height, color, false);
        }
        private void Quad(VertexHelper vh, float x, float y, float w, float h, float u, float v, float uw, float vhgt, Color32 tint, bool solid)
        {
            if (w <= 0 || h <= 0) return;
            // Solid centre is drawn with the atlas itself: tinting its arbitrary pixel is incorrect.
            // A separate Image under this graphic supplies the fill instead.
            if (solid) return;
            Rect r = rectTransform.rect; float left = r.xMin + x, top = r.yMax - y;
            int start = vh.currentVertCount;
            vh.AddVert(new Vector3(left, top - h), tint, new Vector2(u, v));
            vh.AddVert(new Vector3(left, top), tint, new Vector2(u, v + vhgt));
            vh.AddVert(new Vector3(left + w, top), tint, new Vector2(u + uw, v + vhgt));
            vh.AddVert(new Vector3(left + w, top - h), tint, new Vector2(u + uw, v));
            vh.AddTriangle(start, start + 1, start + 2); vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
