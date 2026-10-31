using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GuJian
{
    internal static class NativeAtlasRenderer
    {
        internal static void Draw(RectTransform parent, List<RawImage> pool, OriginalAnimation animation, int frameIndex, Func<int, Texture> resolve, Vector2 origin, float scale)
        {
            var pieces = animation.frames[frameIndex].pieces;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (i == pool.Count)
                {
                    var obj = new GameObject("Atlas piece", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                    obj.transform.SetParent(parent, false); var image = obj.GetComponent<RawImage>(); image.raycastTarget = false; pool.Add(image);
                }
                var piece = pieces[i]; var part = pool[i]; part.gameObject.SetActive(true);
                int w, h; float u = 0, v = 0, uw = 1, vh = 1;
                if (piece.image < 0)
                {
                    part.texture = Texture2D.whiteTexture; w = piece.w; h = piece.h;
                    part.color = new Color32((byte)(piece.color >> 16), (byte)(piece.color >> 8), (byte)piece.color, 255);
                }
                else
                {
                    var texture = resolve(piece.image); var source = animation.sheets[piece.image].rects[piece.rect];
                    if (!texture || source.w <= 0 || source.h <= 0) { part.gameObject.SetActive(false); continue; }
                    part.texture = texture; part.color = Color.white; w = source.w; h = source.h;
                    u = source.x / (float)texture.width; v = 1 - (source.y + source.h) / (float)texture.height;
                    uw = source.w / (float)texture.width; vh = source.h / (float)texture.height;
                }
                part.uvRect = new Rect(u, v, uw, vh); int mode = piece.transform; bool rotated = mode >= 4;
                var rect = part.rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(w * scale, h * scale);
                rect.anchoredPosition = new Vector2(origin.x + (piece.x + (rotated ? h : w) / 2f) * scale,
                    -(origin.y + (piece.y + (rotated ? w : h) / 2f) * scale));
                float angle = mode == 3 ? 180 : mode == 4 || mode == 6 ? -90 : mode == 5 || mode == 7 ? 90 : 0;
                rect.localRotation = Quaternion.Euler(0, 0, -angle);
                rect.localScale = new Vector3(mode == 2 || mode == 4 || mode == 7 ? -1 : 1, mode == 1 ? -1 : 1, 1);
            }
            for (int i = pieces.Length; i < pool.Count; i++) pool[i].gameObject.SetActive(false);
        }
    }
}
