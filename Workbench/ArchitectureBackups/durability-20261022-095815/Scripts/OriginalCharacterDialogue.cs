using System.Text;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private OriginalDialogueText.Line[] characterLines;
        private int characterStartLine, characterLineHeight;
        private GUIStyle characterTextStyle, characterNameStyle;
        private int CharacterLinesPerPage => originalUi ? Mathf.Max(1, originalUi.dialogueLinesPerPage) : 2;
        private int CharacterContentWidth => originalUi ? originalUi.dialogueContentWidth : 236;

        private void OpenOriginalCharacterDialogue(string who, string text, string image, bool onLeft)
        {
            Talk(who, "", image, onLeft: onLeft);
            if (!font) font = Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 16);
            int size = originalUi ? originalUi.dialogueFontSize : 18;
            characterTextStyle = new GUIStyle { font = font, fontSize = size, richText = true, wordWrap = false,
                alignment = TextAnchor.UpperLeft, padding = new RectOffset(0, 0, 0, 0) };
            characterTextStyle.normal.textColor = Color.white;
            characterNameStyle = new GUIStyle(characterTextStyle) { richText = false, alignment = TextAnchor.UpperCenter };
            // Android Font uses ceil(descent-ascent)+2. Unity supplies its own font metrics.
            characterLineHeight = Mathf.CeilToInt(font.lineHeight * (size / (float)Mathf.Max(1, font.fontSize))) + 2;
            System.Func<string, int> measure = s => (int)characterTextStyle.CalcSize(new GUIContent(s)).x;
            characterLines = OriginalDialogueText.Format(text, CharacterContentWidth, measure("你"), measure);
            characterStartLine = 0; characterDialogue = true;
            dialoguePages.Clear();
            for (int first = CharacterLinesPerPage; first < characterLines.Length; first += CharacterLinesPerPage)
                dialoguePages.Enqueue(CharacterPageText(first));
            dialogue = CharacterPageText(0);
            modalInputFrame = Time.frameCount;
        }

        private string CharacterPageText(int first)
        {
            var text = new StringBuilder();
            for (int i = first; i < Mathf.Min(first + CharacterLinesPerPage, characterLines.Length); i++)
            { if (i > first) text.Append('\n'); text.Append(characterLines[i].Text); }
            return text.ToString();
        }

        private bool HandleOriginalCharacterControl(int code)
        {
            if (!characterDialogue || dialogue == null) return false;
            modalInputFrame = Time.frameCount;
            if (code == 4) ContinueDialogue();
            return true;
        }

        private bool PollOriginalCharacterKeys()
        {
            if (!characterDialogue || dialogue == null) return false;
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) HandleOriginalCharacterControl(4);
            return true;
        }

        private void DrawOriginalCharacterDialogue()
        {
            Rect bounds = originalUi ? originalUi.dialogueBounds : new Rect(132, 230, 268, 90);
            var image = DialoguePortrait; bool left = image && portraitOnLeft;
            if (originalUi && originalUi.dialogue)
            {
                var matrix = GUI.matrix;
                if (left) GUI.matrix = matrix * Matrix4x4.TRS(new Vector3(bounds.x * 2 + bounds.width, 0, 0), Quaternion.identity, new Vector3(-1, 1, 1));
                GUI.DrawTexture(bounds, originalUi.dialogue); GUI.matrix = matrix;
            }
            float nameX = left ? ViewWidth - bounds.x - 42 : bounds.x + 42;
            GUI.Label(new Rect(nameX - 80, bounds.y + 6, 160, characterLineHeight), speaker, characterNameStyle);
            float sy = ViewHeight - CharacterLinesPerPage * characterLineHeight - 8;
            if (image) GUI.DrawTexture(new Rect(left ? bounds.x : ViewWidth - bounds.x - image.width,
                sy - 8 - image.height, image.width, image.height), image);
            GUI.BeginGroup(new Rect(bounds.x + 12, sy, CharacterContentWidth + 1, CharacterLinesPerPage * characterLineHeight + 1));
            for (int i = 0; i < CharacterLinesPerPage && characterStartLine + i < characterLines.Length; i++)
            {
                float x = 0;
                foreach (var span in characterLines[characterStartLine + i].spans)
                {
                    OriginalGui.StyledLabel(new Rect(x, i * characterLineHeight, span.width + 4, characterLineHeight + 2), span.drawRuns, characterTextStyle);
                    x += span.width;
                }
            }
            GUI.EndGroup();
            // Mouse/touch confirmation shares the panel; the source has no extra continue button.
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && bounds.Contains(e.mousePosition))
            { HandleOriginalCharacterControl(4); e.Use(); }
        }
    }
}
