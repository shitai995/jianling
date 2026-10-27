using UnityEngine;

namespace GuJian
{
    // Draw original atlas pieces directly. UI uses saved textures and animation data.
    public static class OriginalGui
    {
        public static void StyledLabel(Rect bounds, OriginalTextMarkup.DrawRun[] runs, GUIStyle style)
        {
            if (runs == null) return;
            foreach (var run in runs)
            {
                // Match Tool.drawString: draw each styled span's edge, then its foreground.
                if (run.fontType == 1)
                {
                    GUI.Label(new Rect(bounds.x, bounds.y - 1, bounds.width, bounds.height), run.edge, style);
                    GUI.Label(new Rect(bounds.x, bounds.y + 1, bounds.width, bounds.height), run.edge, style);
                    GUI.Label(new Rect(bounds.x - 1, bounds.y, bounds.width, bounds.height), run.edge, style);
                    GUI.Label(new Rect(bounds.x + 1, bounds.y, bounds.width, bounds.height), run.edge, style);
                }
                else if (run.fontType == 2)
                    GUI.Label(new Rect(bounds.x - 1, bounds.y - 1, bounds.width, bounds.height), run.edge, style);
                GUI.Label(bounds, run.foreground, style);
            }
        }

        public static void Region(Texture2D texture, Rect source, Rect target)
        {
            if (!texture || source.width <= 0 || source.height <= 0 || target.width <= 0 || target.height <= 0) return;
            GUI.DrawTextureWithTexCoords(target, texture, new Rect(source.x / texture.width, 1 - source.yMax / texture.height, source.width / texture.width, source.height / texture.height));
        }
        public static void TiledPanel(Texture2D texture, Rect bounds)
        {
            if (!texture) return;
            int tile = texture.height, x = (int)bounds.x, y = (int)bounds.y;
            int w = (int)bounds.width, h = (int)bounds.height;
            if (tile <= 0 || w <= 0 || h <= 0) return;
            int m = (w - tile * 2) / tile, n = (h - tile * 2) / tile;
            bool vertical = false, horizontal = false;
            if (w < tile * 2) m = 0; else vertical = m * tile < w - tile * 2;
            if (h < tile * 2) n = 0; else horizontal = n * tile < h - tile * 2;
            Color color = GUI.color;
            GUI.color = new Color32(214, 140, 90, 255);
            GUI.DrawTexture(new Rect(x + 2, y + 2, w - 8, h - 12), Texture2D.whiteTexture);
            GUI.color = color;
            int cw = Mathf.Min(w >> 1, tile), ch = Mathf.Min(h >> 1, tile);
            int right = (w >> 1) > tile ? x + w - tile : x + (w >> 1);
            int bottom = (h >> 1) > tile ? y + h - tile : y + (h >> 1);
            if (vertical)
            {
                int remain = w - tile * 2 - m * tile;
                Region(texture, new Rect(tile * 4, 0, remain, tile), new Rect(x + (m + 1) * tile, y, remain, tile));
                Region(texture, new Rect(tile * 5, tile - ch, remain, ch), new Rect(x + (m + 1) * tile, bottom, remain, ch));
            }
            if (horizontal)
            {
                int remain = h - tile * 2 - n * tile;
                Region(texture, new Rect(tile * 7, 0, tile, remain), new Rect(x, y + (n + 1) * tile, tile, remain));
                Region(texture, new Rect(tile * 6, 0, tile, remain), new Rect(x + w - tile, y + (n + 1) * tile, tile, remain));
            }
            for (int i = 0; i < m; i++)
            {
                Region(texture, new Rect(tile * 4, 0, tile, ch), new Rect(x + (i + 1) * tile, y, tile, ch));
                Region(texture, new Rect(tile * 5, tile - ch, tile, ch), new Rect(x + (i + 1) * tile, bottom, tile, ch));
            }
            for (int i = 0; i < n; i++)
            {
                Region(texture, new Rect(tile * 7, 0, cw, tile), new Rect(x, y + (i + 1) * tile, cw, tile));
                Region(texture, new Rect(tile * 6, 0, cw, tile), new Rect(right, y + (i + 1) * tile, cw, tile));
            }
            Region(texture, new Rect(0, 0, cw, ch), new Rect(x, y, cw, ch));
            Region(texture, new Rect(tile, 0, cw, ch), new Rect(right, y, cw, ch));
            Region(texture, new Rect(tile * 2, tile - ch, cw, ch), new Rect(x, bottom, cw, ch));
            Region(texture, new Rect(tile * 3, tile - ch, cw, ch), new Rect(right, bottom, cw, ch));
        }
        public static void Bar(Texture2D texture, Rect target, float ratio)
        {
            if (!texture) return;
            float amount = Mathf.Clamp01(ratio);
            Region(texture, new Rect(0,0,texture.width * amount,texture.height), new Rect(target.x,target.y,target.width * amount,target.height));
        }
        public static void Number(Texture2D texture, int number, Vector2 point, int digits = 10, int alignment = 0, int interval = 0)
        {
            if (!texture) return;
            float width = texture.width / (float)digits;
            string text = Mathf.Max(0,number).ToString();
            int length = Mathf.FloorToInt((width + interval) * text.Length);
            if (alignment == 1) point.x -= length;
            else if (alignment == 2) point.x -= length >> 1;
            foreach (char ch in text)
            {
                Region(texture,new Rect((ch-'0') * width,0,width,texture.height),new Rect(point.x,point.y,width,texture.height));
                point.x += width + interval;
            }
        }
        public static int NumberWidth(Texture2D texture, int number, int digits = 10, int interval = 0)
        {
            if (!texture) return 0;
            return Mathf.Max(1, Mathf.FloorToInt(texture.width / (float)digits)) * Mathf.Max(1, Mathf.Max(0, number).ToString().Length) + interval * Mathf.Max(1, Mathf.Max(0, number).ToString().Length);
        }
        // Original GameCanvas.drawPrice: gold is the high four-digit unit and silver is the remainder.
        // x is the left edge for alignment 0 and the right edge for alignment 1.
        public static void Price(OriginalUiLayout ui, int value, Vector2 point, int alignment)
        {
            if (!ui) return;
            int gold = Mathf.Max(0, value) / 10000, silver = Mathf.Max(0, value) % 10000;
            Texture2D goldDigits = ui.menuGoldNumbers ? ui.menuGoldNumbers : ui.menuNumbers;
            Texture2D silverDigits = ui.menuSilverNumbers ? ui.menuSilverNumbers : ui.menuNumbers;
            Texture2D money = ui.money;
            if (!goldDigits || !silverDigits || !money) return;
            int unit = money.width / 2, h = money.height;
            if (alignment == 0)
            {
                int x = Mathf.RoundToInt(point.x), y = Mathf.RoundToInt(point.y);
                int gw = gold > 0 ? NumberWidth(goldDigits, gold) : 0;
                if (gold > 0) { Number(goldDigits, gold, new Vector2(x, y), 10, 0, 0); x += gw + 2; Region(money, new Rect(0, 0, unit, h), new Rect(x, y, unit, h)); x += unit + 2; }
                int sw = NumberWidth(silverDigits, silver); Number(silverDigits, silver, new Vector2(x, y), 10, 0, 0);
                Region(money, new Rect(unit, 0, unit, h), new Rect(x + sw + 2, y, unit, h));
                return;
            }
            int right = Mathf.RoundToInt(point.x), y1 = Mathf.RoundToInt(point.y);
            int sw1 = NumberWidth(silverDigits, silver);
            Region(money, new Rect(unit, 0, unit, h), new Rect(right - 2 - unit, y1, unit, h));
            Number(silverDigits, silver, new Vector2(right - 4 - unit, y1), 10, 1, 0);
            if (gold > 0)
            {
                int gw1 = NumberWidth(goldDigits, gold);
                Region(money, new Rect(0, 0, unit, h), new Rect(right - sw1 - 6 - unit * 2, y1, unit, h));
                Number(goldDigits, gold, new Vector2(right - sw1 - 8 - unit * 2, y1), 10, 1, 0);
            }
        }
        public static void Animation(OriginalAnimation data, Texture2D[] images, int action, int tick, Vector2 origin)
        {
            if (data == null || action < 0 || action >= data.actions.Length) return;
            var steps = data.actions[action].steps; if (steps.Length == 0) return;
            Frame(data, images, steps[Mathf.Abs(tick) % steps.Length].frame, origin);
        }
        public static void Frame(OriginalAnimation data, Texture2D[] images, int frame, Vector2 origin)
        {
            if (data == null || frame < 0 || frame >= data.frames.Length) return;
            Matrix4x4 matrix = GUI.matrix; Color color = GUI.color;
            try
            {
                foreach (var piece in data.frames[frame].pieces)
                {
                    if (piece.image < 0)
                    {
                        GUI.color = new Color(((piece.color>>16)&255)/255f,((piece.color>>8)&255)/255f,(piece.color&255)/255f,color.a);
                        GUI.DrawTexture(new Rect(origin.x+piece.x,origin.y+piece.y,piece.w,piece.h),Texture2D.whiteTexture); GUI.color=color; continue;
                    }
                    if (piece.image >= images.Length || !images[piece.image]) continue;
                    var r = data.sheets[piece.image].rects[piece.rect];
                    if (r.w <= 0 || r.h <= 0) continue;
                    int mode = piece.transform;
                    bool rotated = mode >= 4;
                    Vector3 center = new Vector3(origin.x+piece.x+(rotated?r.h:r.w)/2f,origin.y+piece.y+(rotated?r.w:r.h)/2f,0);
                    float angle = mode==3 ? 180 : mode==4 || mode==6 ? -90 : mode==5 || mode==7 ? 90 : 0;
                    GUI.matrix = matrix * Matrix4x4.TRS(center,Quaternion.Euler(0,0,angle),new Vector3(mode==2 || mode==4 || mode==7 ? -1:1,mode==1 ? -1:1,1));
                    Region(images[piece.image],new Rect(r.x,r.y,r.w,r.h),new Rect(-r.w/2f,-r.h/2f,r.w,r.h));
                    GUI.matrix=matrix;
                }
            }
            finally { GUI.matrix=matrix; GUI.color=color; }
        }
    }
}
