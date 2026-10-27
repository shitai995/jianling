using System.Collections.Generic;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private bool originalBlackScreen, originalBlackReturnToScene, originalBlackScriptPaused;
        private string[] originalBlackLines;
        private int[] originalBlackWidths;
        private int originalBlackY, originalBlackLineHeight, originalBlackLinesPerPage;
        private int originalBlackLine, originalBlackPage, originalBlackWidth, originalBlackStay, originalBlackImageY;
        private double originalBlackCycle;
        private Texture2D originalBlackImage;
        private GUIStyle originalBlackStyle;

        private void ResetOriginalBlackScreen()
        {
            originalBlackScreen = originalBlackReturnToScene = originalBlackScriptPaused = false;
            originalBlackLines = null; originalBlackWidths = null; originalBlackImage = null;
            originalBlackLine = originalBlackPage = originalBlackWidth = originalBlackStay = 0;
            originalBlackCycle = 0;
        }

        private void OpenOriginalBlackScreen(string text, bool returnToScene, int y, int linesPerPage, string imagePath, int imageY)
        {
            Talk("", text, story: true);
            dialoguePages.Clear();
            dialogue = text ?? "";
            if (!font) font = Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 16);
            originalBlackStyle = new GUIStyle { font = font, fontSize = 18, wordWrap = false, richText = false,
                alignment = TextAnchor.UpperLeft, padding = new RectOffset(0, 0, 0, 0) };
            originalBlackStyle.normal.textColor = Color.white;
            originalBlackLineHeight = Mathf.CeilToInt(font.lineHeight * (18f / Mathf.Max(1, font.fontSize))) + 2;

            // Tool.getStringArray excludes the character reaching the width boundary
            // and appends a newline, including an empty line after a trailing newline.
            string source = dialogue + '\n';
            var lines = new List<string>();
            int begin = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != '\n' && MeasureBlackText(source.Substring(begin, i - begin + 1)) < ViewWidth - 20) continue;
                lines.Add(source.Substring(begin, i - begin));
                begin = source[i] == '\n' ? i + 1 : i;
            }
            originalBlackLines = lines.ToArray();
            originalBlackWidths = new int[lines.Count];
            for (int i = 0; i < lines.Count; i++) originalBlackWidths[i] = MeasureBlackText(lines[i]);
            originalBlackY = y == 0 ? Mathf.Min(60, ((int)ViewHeight - lines.Count * originalBlackLineHeight) >> 1) : y;
            originalBlackLinesPerPage = linesPerPage;
            originalBlackReturnToScene = returnToScene;
            originalBlackImage = string.IsNullOrEmpty(imagePath) ? null : OriginalAssets.Texture("pic/" + imagePath);
            originalBlackImageY = imageY;
            originalBlackScreen = originalBlackScriptPaused = true;
            modalInputFrame = Time.frameCount;
        }

        private int MeasureBlackText(string text) => (int)originalBlackStyle.CalcSize(new GUIContent(text)).x;

        private void TickOriginalBlackScreen(float delta)
        {
            if (!originalBlackScreen || delta <= 0) return;
            originalBlackCycle += delta;
            while (originalBlackScreen && originalBlackCycle + .000001 >= .08)
            {
                originalBlackCycle -= .08;
                if (originalBlackLine < originalBlackLines.Length)
                {
                    if (originalBlackWidth < originalBlackWidths[originalBlackLine] - 2)
                        originalBlackWidth += 2;
                    else
                    {
                        originalBlackLine++;
                        originalBlackWidth = 0;
                        if (originalBlackLinesPerPage > 0)
                        {
                            originalBlackPage = originalBlackLine / originalBlackLinesPerPage;
                            if (originalBlackLine % originalBlackLinesPerPage == originalBlackLinesPerPage - 1 ||
                                originalBlackLine == originalBlackLines.Length - 1) originalBlackStay = 0;
                        }
                    }
                }
                else if (originalBlackStay < 20) originalBlackStay++;
                else
                {
                    originalBlackScreen = false;
                    originalBlackLines = null; originalBlackWidths = null; originalBlackImage = null;
                    originalBlackStay = originalBlackPage = 0;
                    originalBlackCycle = 0;
                    dialogue = null; storyScreen = false;
                    // The original return-to-scene branch leaves the paused script intact.
                    if (!originalBlackReturnToScene) originalBlackScriptPaused = false;
                    modalInputFrame = Time.frameCount;
                }
            }
        }

        private bool HandleOriginalBlackScreenControl(int code)
        {
            if (!originalBlackScreen) return false;
            modalInputFrame = Time.frameCount;
            if (code == 4 && originalBlackLine < originalBlackLines.Length)
            {
                if (originalBlackLinesPerPage > 0 && originalBlackLine % originalBlackLinesPerPage == originalBlackLinesPerPage - 1 ||
                    originalBlackLine == originalBlackLines.Length - 1)
                {
                    originalBlackStay = 0;
                    originalBlackWidth = originalBlackWidths[originalBlackLine];
                }
                else
                {
                    originalBlackLine++;
                    if (originalBlackLinesPerPage > 0) originalBlackPage = originalBlackLine / originalBlackLinesPerPage;
                    originalBlackWidth = 0;
                }
            }
            return true;
        }

        private void PollOriginalBlackScreenKeys()
        {
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter)) HandleOriginalBlackScreenControl(4);
        }

        private void DrawOriginalBlackScreen()
        {
            var color = GUI.color; GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, ViewWidth, ViewHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;
            if (originalBlackImage)
                GUI.DrawTexture(new Rect(0, originalBlackImageY, originalBlackImage.width, originalBlackImage.height), originalBlackImage);
            int first = originalBlackLinesPerPage * originalBlackPage;
            for (int i = first; i <= originalBlackLine && i < originalBlackLines.Length; i++)
            {
                int y = originalBlackY + originalBlackLineHeight * (i - first);
                int width = i == originalBlackLine ? originalBlackWidth : (int)ViewWidth - 20;
                if (width <= 0) continue;
                GUI.BeginGroup(new Rect(10, y, width, originalBlackLineHeight));
                GUI.Label(new Rect(0, 0, ViewWidth - 20, originalBlackLineHeight), originalBlackLines[i], originalBlackStyle);
                GUI.EndGroup();
            }
            GUI.color = color;
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && new Rect(0, 0, ViewWidth, ViewHeight).Contains(e.mousePosition))
            { HandleOriginalBlackScreenControl(4); e.Use(); }
        }
    }
}
