using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private sealed class OriginalMessageEntry
        {
            public string text;
            public int duration;
            public bool fullScreen;
        }
        private readonly System.Collections.Generic.Queue<OriginalMessageEntry> originalMessageQueue = new System.Collections.Generic.Queue<OriginalMessageEntry>();
        private double originalUiCycle;
        private int originalUiTicks;
        private bool originalMessage, originalMessageFullScreen, originalMessageBlocksScript;
        private int originalMessageDuration, originalMessageMillis;
        private double originalMessageCycle;
        private float originalMessageOffset, originalMessageHeight, originalMessageLineHeight;
        private bool originalMessageLayout, originalMessageScroll;
        private GUIStyle originalMessageStyle;
        private string originalMessageRichText;
        private OriginalTextMarkup.DrawRun[] originalMessageDrawRuns;

        private void ResetOriginalMessage()
        {
            ResetOriginalBlackScreen();
            originalMessageQueue.Clear();
            ResetOriginalMessageView();
        }

        private void ResetOriginalMessageView()
        {
            originalMessage = originalMessageFullScreen = originalMessageBlocksScript = false;
            originalMessageDuration = originalMessageMillis = 0;
            originalMessageCycle = 0;
            originalMessageOffset = originalMessageHeight = originalMessageLineHeight = 0;
            originalMessageLayout = originalMessageScroll = false;
            originalMessageRichText = null;
            originalMessageDrawRuns = null;
        }

        private void ShowOriginalMessage(string text, int milliseconds, bool fullScreen, bool pauseScript = true)
        {
            // GameCanvas.showMessage resets the active duration before enqueueing.
            originalMessageDuration = milliseconds;
            originalMessageMillis = 0;
            var entry = new OriginalMessageEntry { text = text, duration = milliseconds, fullScreen = fullScreen };
            if (originalMessage && dialogue != null)
            {
                originalMessageQueue.Enqueue(entry);
                originalMessageBlocksScript |= pauseScript;
                return;
            }
            Talk("", text);
            ActivateOriginalMessage(entry);
            originalMessageBlocksScript = pauseScript;
        }

        private void ActivateOriginalMessage(OriginalMessageEntry entry)
        {
            dialoguePages.Clear();
            options = null;
            dialogue = Clean(entry.text);
            originalMessageRichText = OriginalTextMarkup.ToUnity(entry.text);
            originalMessageDrawRuns = OriginalTextMarkup.ToDrawRuns(entry.text);
            originalMessage = true;
            originalMessageFullScreen = entry.fullScreen;
            originalMessageDuration = entry.duration;
            originalMessageMillis = 0;
            originalMessageLayout = originalMessageScroll = false;
            originalMessageOffset = originalMessageHeight = originalMessageLineHeight = 0;
        }

        private void CloseOriginalMessage()
        {
            modalInputFrame = Time.frameCount;
            if (originalMessageQueue.Count > 0)
            {
                ActivateOriginalMessage(originalMessageQueue.Dequeue());
                return;
            }
            dialogue = null;
            dialoguePages.Clear();
            options = null;
            ResetOriginalMessage();
        }

        private void TickOriginalUiClock(float delta)
        {
            if (delta <= 0) return;
            originalUiCycle += delta;
            while (originalUiCycle + .000001 >= .08)
            {
                originalUiCycle -= .08;
                originalUiTicks++;
                CycleRoleSkillPreview();
            }
        }

        private bool HandleOriginalMessageControl(int code)
        {
            if (!originalMessage || dialogue == null) return false;
            modalInputFrame = Time.frameCount;
            if (originalMessageDuration == 0 && code == 4 || originalMessageDuration == -1 && code == 5)
                CloseOriginalMessage();
            return true;
        }

        private bool PollOriginalMessageKeys()
        {
            if (!originalMessage || dialogue == null) return false;
            if (GameInput.Pressed(KeyCode.J) || GameInput.Pressed(KeyCode.Space) || GameInput.Pressed(KeyCode.Return)) HandleOriginalMessageControl(4);
            else if (GameInput.Pressed(KeyCode.Escape)) HandleOriginalMessageControl(5);
            return true;
        }

        private void TickOriginalMessage(float delta)
        {
            if (!originalMessage || dialogue == null || delta <= 0) return;
            originalMessageCycle += delta;
            while (originalMessageCycle + .000001 >= .08)
            {
                originalMessageCycle -= .08;
                if (originalMessageScroll)
                {
                    originalMessageOffset -= Mathf.Max(1, (int)originalMessageLineHeight / 10);
                    if (originalMessageOffset <= -originalMessageHeight)
                        originalMessageOffset = originalMessageFullScreen ? ViewHeight - 50 : ViewHeight / 3;
                }
                if (originalMessageDuration <= 0) continue;
                originalMessageMillis += 80;
                if (originalMessageMillis >= originalMessageDuration)
                {
                    CloseOriginalMessage();
                    if (!originalMessage) return;
                }
            }
        }

        private void DrawOriginalMessage()
        {
            if (originalMessageStyle == null)
                originalMessageStyle = new GUIStyle(bodyStyle) { padding = new RectOffset(0, 0, 0, 0), alignment = TextAnchor.UpperLeft, richText = true };
            originalMessageStyle.normal.textColor = Color.white;
            float width = ViewWidth - 20;
            float maxHeight = originalMessageFullScreen ? ViewHeight - 50 : ViewHeight / 3;
            if (!originalMessageLayout)
            {
                originalMessageHeight = originalMessageStyle.CalcHeight(new GUIContent(originalMessageRichText), width);
                originalMessageLineHeight = Mathf.Max(1, originalMessageStyle.lineHeight);
                originalMessageScroll = originalMessageHeight > maxHeight;
                originalMessageOffset = originalMessageScroll ? originalMessageLineHeight : 0;
                originalMessageLayout = true;
            }
            float visibleHeight = Mathf.Min(originalMessageHeight, maxHeight);
            float y = originalMessageFullScreen ? 50 : (ViewHeight - originalMessageHeight) / 2 - 10;
            if (originalMessageFullScreen)
            {
                var color = GUI.color; GUI.color = Color.black;
                GUI.DrawTexture(new Rect(0, 0, ViewWidth, ViewHeight), Texture2D.whiteTexture); GUI.color = color;
            }
            else if (originalUi && originalUi.messagePanel) OriginalGui.TiledPanel(originalUi.messagePanel, new Rect(0, y - 10, ViewWidth, visibleHeight + 20));
            else GUI.Box(new Rect(0, y - 10, ViewWidth, visibleHeight + 20), "");
            originalMessageStyle.alignment = !originalMessageFullScreen && originalMessageHeight <= originalMessageLineHeight + 1 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
            GUI.BeginGroup(new Rect(10, y, width, maxHeight));
            OriginalGui.StyledLabel(new Rect(0, originalMessageOffset, width, originalMessageHeight), originalMessageDrawRuns, originalMessageStyle);
            GUI.EndGroup();
            if (!originalMessageFullScreen && originalMessageDuration == 0)
            {
                if (originalUi && originalUi.keyPressTip != null)
                    OriginalGui.Animation(originalUi.keyPressTip, originalUi.keyPressTipSheets, 4, originalUiTicks, new Vector2(ViewWidth - 10, y + visibleHeight));
                if (GUI.Button(new Rect(ViewWidth - 21, y + visibleHeight - 11, 22, 23), GUIContent.none, GUIStyle.none)) HandleOriginalMessageControl(4);
            }
        }
    }
}
