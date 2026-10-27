using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private bool stageInfoVisible;
        private int stageInfoWidth, stageInfoX, stageInfoLine1, stageInfoLine2;
        private int stageInfoOffsetY, stageInfoCounter;
        private float stageInfoElapsed;
        private GUIStyle stageInfoStyle;

        private void ShowStageInfo()
        {
            EnsureStyles();
            if (stageInfoStyle == null) stageInfoStyle = new GUIStyle(GUIStyle.none) { font = font, richText = false };
            stageInfoStyle.fontSize = originalUi ? originalUi.stageNameFontSize : 18;
            stageInfoWidth = Mathf.FloorToInt(stageInfoStyle.CalcSize(new GUIContent(StageName ?? "")).x);
            stageInfoX = ((int)ViewWidth - stageInfoWidth) >> 1;
            stageInfoLine1 = -10 - (((int)ViewWidth + stageInfoWidth) >> 1);
            stageInfoLine2 = stageInfoLine1 + stageInfoWidth + 20;
            stageInfoCounter = stageInfoOffsetY = 0;
            stageInfoElapsed = 0;
            stageInfoVisible = true;
        }

        private void ResetStageInfo()
        {
            stageInfoVisible = false;
            stageInfoCounter = stageInfoOffsetY = 0;
            stageInfoElapsed = 0;
        }

        private void TickStageInfo(float delta)
        {
            if (!stageInfoVisible) return;
            stageInfoElapsed += delta;
            while (stageInfoElapsed >= .08f && stageInfoVisible)
            {
                stageInfoElapsed -= .08f;
                // GameCanvas.cycleStageInfo runs globally, including dialogue and menu states.
                if (stageInfoLine1 < stageInfoX - 10) stageInfoLine1 += 30;
                else stageInfoLine1 = stageInfoX - 10;
                if (stageInfoLine2 < ViewWidth) { stageInfoLine2 += 30; continue; }
                stageInfoLine2 = (int)ViewWidth;
                int height = originalUi ? originalUi.stageNameFontHeight : 24;
                if (stageInfoOffsetY < height) stageInfoOffsetY++;
                else if (stageInfoCounter < 24) stageInfoCounter++;
                else ResetStageInfo();
            }
        }

        private static void DrawStageLine(int start, int end, int y)
        {
            int left = Mathf.Max(0, Mathf.Min(start, end));
            int right = Mathf.Min((int)ViewWidth - 1, Mathf.Max(start, end));
            if (right >= left) GUI.DrawTexture(new Rect(left, y, right - left + 1, 1), Texture2D.whiteTexture);
        }

        private void DrawStageInfo()
        {
            if (!stageInfoVisible || stageInfoStyle == null) return;
            int y = (int)ViewHeight / 2 - 50;
            var color = GUI.color;
            try
            {
                GUI.color = Color.white;
                DrawStageLine(stageInfoLine1, stageInfoLine1 + stageInfoWidth + 20, y);
                DrawStageLine(stageInfoLine2, stageInfoLine2 + stageInfoX - 10, y);
                var bounds = new Rect(stageInfoX, y - stageInfoOffsetY, stageInfoWidth + 4, stageInfoStyle.lineHeight + 4);
                stageInfoStyle.normal.textColor = Color.white;
                for (int i = 0; i < 4; i++)
                {
                    var offset = i == 0 ? Vector2.up : i == 1 ? Vector2.down : i == 2 ? Vector2.left : Vector2.right;
                    GUI.Label(new Rect(bounds.position + offset, bounds.size), StageName, stageInfoStyle);
                }
                stageInfoStyle.normal.textColor = Color.black;
                GUI.Label(bounds, StageName, stageInfoStyle);
            }
            finally { GUI.color = color; }
        }
    }
}
