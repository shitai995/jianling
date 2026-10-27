using UnityEngine;
namespace GuJian
{
    public partial class GuJianGame
    {
        private int choiceFocus;
        private int choiceFirst;
        private bool choiceClosable;
        private string choiceInfoRichText;
        private OriginalTextMarkup.DrawRun[] choiceInfoDrawRuns;
        private GUIStyle choiceInfoStyle, choiceItemStyle;
        private int choiceFontHeight, choiceInfoHeight, choiceMoveTicks;
        private float choiceInfoOffset;
        private double choiceCycle;
        private bool choiceLayout;
        private Rect choiceInfoBounds, choiceListBounds;
        private int modalInputFrame = -1;

        private void OpenOriginalChoice(string info, string[] choices, bool closable)
        {
            Talk("", info, choices: choices);
            choiceFocus = choiceFirst = choiceMoveTicks = 0;
            dialoguePages.Clear();
            dialogue = Clean(info);
            choiceInfoRichText = string.IsNullOrEmpty(info) ? null : OriginalTextMarkup.ToUnity(info);
            choiceInfoDrawRuns = choiceInfoRichText == null ? null : OriginalTextMarkup.ToDrawRuns(info);
            choiceLayout = false;
            choiceCycle = 0;
            choiceInfoOffset = 0;
            choiceClosable = closable;
            LastChoice = -1;
        }

        private bool HandleOriginalChoiceControl(int code)
        {
            if (dialogue == null || options == null) return false;
            modalInputFrame = Time.frameCount;
            if (options.Length > 0)
            {
                int previous = choiceFocus;
                if (code == 0) choiceFocus = Mathf.Max(0, choiceFocus - 1);
                else if (code == 1) choiceFocus = Mathf.Min(options.Length - 1, choiceFocus + 1);
                if (previous != choiceFocus) choiceMoveTicks = 0;
                if (choiceFocus < choiceFirst) choiceFirst = choiceFocus;
                if (choiceFocus >= choiceFirst + 3) choiceFirst = choiceFocus - 2;
                // GameCanvas.handleKey_Choice accepts both the FIRE/OK key (4)
                // and the soft OK key (9).  The latter is the touch/menu
                // confirmation control on the original device.
                if (code == 4 || code == 9) CompleteOriginalChoice(choiceFocus);
            }
            if (code == 5 && choiceClosable) CompleteOriginalChoice(-1);
            return true;
        }

        private void CompleteOriginalChoice(int index)
        {
            if (options == null || index < -1 || index >= options.Length || (index == -1 && !choiceClosable)) return;
            modalInputFrame = Time.frameCount;
            dialoguePages.Clear();
            ContinueDialogue(index);
            choiceFocus = choiceFirst = 0;
            choiceClosable = false;
        }

        private bool PollOriginalChoiceKeys()
        {
            if (dialogue == null || options == null) return false;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) HandleOriginalChoiceControl(0);
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) HandleOriginalChoiceControl(1);
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) HandleOriginalChoiceControl(4);
            else if (Input.GetKeyDown(KeyCode.Escape)) HandleOriginalChoiceControl(5);
            return true;
        }

        private void TickOriginalChoice(float delta)
        {
            if (dialogue == null || options == null || delta <= 0) return;
            choiceCycle += delta;
            while (choiceCycle + .000001 >= .08)
            {
                choiceCycle -= .08;
                choiceMoveTicks++;
                if (choiceLayout && choiceInfoHeight > 80)
                {
                    choiceInfoOffset -= Mathf.Max(1, choiceFontHeight / 10);
                    if (choiceInfoOffset <= -choiceInfoHeight) choiceInfoOffset = 80;
                }
            }
        }

        private static void ChoiceFill(Rect rect, int rgb)
        {
            Color before = GUI.color;
            GUI.color = new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = before;
        }

        private void DrawOriginalChoiceOptions()
        {
            if (options == null) return;
            if (choiceItemStyle == null)
            {
                choiceItemStyle = new GUIStyle(bodyStyle) { padding = new RectOffset(), wordWrap = false, richText = false, alignment = TextAnchor.UpperLeft };
                choiceInfoStyle = new GUIStyle(bodyStyle) { padding = new RectOffset(), wordWrap = true, richText = true, alignment = TextAnchor.UpperLeft };
            }
            if (!choiceLayout)
            {
                choiceFontHeight = Mathf.Max(1, Mathf.RoundToInt(choiceItemStyle.lineHeight));
                choiceInfoHeight = choiceInfoRichText == null ? 0 : Mathf.CeilToInt(choiceInfoStyle.CalcHeight(new GUIContent(choiceInfoRichText), 180));
                choiceInfoOffset = choiceInfoHeight > 80 ? choiceFontHeight : 0;
                choiceLayout = true;
            }
            int itemHeight = choiceFontHeight + 5;
            int listY = choiceInfoRichText == null ? (int)ViewHeight / 4 : 80 + choiceInfoHeight;
            int listX = ((int)ViewWidth - 120) >> 1;
            choiceInfoBounds = new Rect(((int)ViewWidth - 180) >> 1, 50, 180, 80);
            choiceListBounds = new Rect(listX, listY, 120, itemHeight * Mathf.Min(options.Length, 3));
            if (choiceInfoRichText != null)
            {
                OriginalGui.TiledPanel(originalUi.messagePanel, new Rect(choiceInfoBounds.x - 10, 40, 200, choiceInfoHeight + 20));
                choiceInfoStyle.normal.textColor = Color.white;
                GUI.BeginGroup(choiceInfoBounds);
                OriginalGui.StyledLabel(new Rect(0, choiceInfoOffset, 180, choiceInfoHeight), choiceInfoDrawRuns, choiceInfoStyle);
                GUI.EndGroup();
            }
            OriginalGui.TiledPanel(originalUi.messagePanel, new Rect(listX - 30, listY - 15, 180, choiceListBounds.height + 30));
            if (options.Length == 0) return;
            int focusY = listY + (choiceFocus - choiceFirst) * itemHeight;
            ChoiceFill(new Rect(listX - 5, focusY, 130, 1), 8612255);
            ChoiceFill(new Rect(listX - 5, focusY + itemHeight - 1, 130, 1), 8612255);
            ChoiceFill(new Rect(listX - 5, focusY, 1, itemHeight), 8612255);
            ChoiceFill(new Rect(listX + 124, focusY, 1, itemHeight), 8612255);
            ChoiceFill(new Rect(listX - 3, focusY + 2, 126, itemHeight - 4), 8612255);
            for (int i = choiceFirst; i < Mathf.Min(options.Length, choiceFirst + 3); i++)
            {
                string text = options[i] ?? "Null";
                int width = Mathf.CeilToInt(choiceItemStyle.CalcSize(new GUIContent(text)).x);
                int offset = 0;
                if (i == choiceFocus && width > 120)
                {
                    offset = choiceMoveTicks * 2;
                    // MovingString resets after drawing the first completely offscreen frame.
                    if (offset >= width && Event.current.type == EventType.Repaint) choiceMoveTicks = -61;
                }
                var row = new Rect(listX, listY + (i - choiceFirst) * itemHeight, 120, choiceFontHeight);
                row.y += (itemHeight - choiceFontHeight) >> 1;
                GUI.BeginGroup(row);
                choiceItemStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(-offset - 1, -1, Mathf.Max(120, width), choiceFontHeight), text, choiceItemStyle);
                choiceItemStyle.normal.textColor = i == choiceFocus ? Color.white : new Color32(212, 210, 76, 255);
                GUI.Label(new Rect(-offset, 0, Mathf.Max(120, width), choiceFontHeight), text, choiceItemStyle);
                GUI.EndGroup();
                if (GUI.Button(new Rect(listX - 5, listY + (i - choiceFirst) * itemHeight, 130, itemHeight), GUIContent.none, GUIStyle.none)) { CompleteOriginalChoice(i); return; }
            }
            int listHeight = (int)choiceListBounds.height;
            int blockTotal = listHeight - 2;
            int blockHeight = Mathf.Max(3, blockTotal / options.Length);
            int blockY = choiceFocus * blockTotal / options.Length;
            if (blockY + blockHeight > blockTotal) blockY = blockTotal - blockHeight;
            ChoiceFill(new Rect(listX + 127, listY, 6, listHeight), 13817549);
            ChoiceFill(new Rect(listX + 128, listY + blockY + 1, 4, blockHeight), 5725285);
        }
    }
}
