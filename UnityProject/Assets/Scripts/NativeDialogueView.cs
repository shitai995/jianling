using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuJian
{
    // A view of the existing dialogue state: the VM and its pagination remain authoritative.
    public sealed class NativeDialogueView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GuJianGame game;
        [SerializeField] private RectTransform viewport, content;
        [SerializeField] private GameObject panel;
        [SerializeField] private RawImage background, portrait;
        [SerializeField] private Text speaker, spanTemplate;
        private readonly List<Text> spans = new List<Text>();
        private OriginalDialogueText.Line[] renderedLines;
        private int renderedStart = -1;

        public bool ReplacesDialogue => isActiveAndEnabled && game && viewport && content && panel && background && portrait && speaker && spanTemplate;
        public void Bind(GuJianGame target) { game = target; }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && game && game.CharacterVisible)
                game.SubmitAction(GameAction.Primary);
        }
        private void OnDisable() { if (panel) panel.SetActive(false); renderedLines = null; }
        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
        }
        private static Color Rgb(int value) => new Color32((byte)(value >> 16), (byte)(value >> 8), (byte)value, 255);
        private void LateUpdate()
        {
            if (!ReplacesDialogue) return;
            Rect safe = Screen.safeArea;
            if (safe.width <= 0 || safe.height <= 0) safe = new Rect(0, 0, Screen.width, Screen.height);
            float scale = Mathf.Min(safe.width / GuJianGame.ViewWidth, safe.height / GuJianGame.ViewHeight);
            viewport.anchoredPosition = safe.center - new Vector2(Screen.width, Screen.height) / 2f;
            viewport.localScale = Vector3.one * scale;
            bool visible = game.CharacterVisible;
            panel.SetActive(visible);
            if (!visible) { renderedLines = null; return; }
            var layout = game.CharacterLayout;
            Rect bounds = layout ? layout.dialogueBounds : new Rect(132, 230, 268, 90);
            var image = game.DialoguePortrait;
            bool left = image && game.CharacterPortraitOnLeft;
            background.texture = layout ? layout.dialogue : null;
            background.uvRect = left ? new Rect(1, 0, -1, 1) : new Rect(0, 0, 1, 1);
            Place(background.rectTransform, bounds.x, bounds.y, bounds.width, bounds.height);
            int height = game.CharacterLineHeight, lines = game.CharacterPageSize;
            float sy = GuJianGame.ViewHeight - lines * height - 8;
            float nameX = left ? GuJianGame.ViewWidth - bounds.x - 42 : bounds.x + 42;
            Place(speaker.rectTransform, nameX - 80, bounds.y + 6, 160, height);
            speaker.font = game.InterfaceFont; speaker.fontSize = layout ? layout.dialogueFontSize : 18;
            speaker.text = game.CharacterSpeaker;
            portrait.gameObject.SetActive(image);
            if (image)
            {
                portrait.texture = image;
                Place(portrait.rectTransform, left ? bounds.x : GuJianGame.ViewWidth - bounds.x - image.width,
                    sy - 8 - image.height, image.width, image.height);
            }
            Place(content, bounds.x + 12, sy, game.CharacterWidth + 1, lines * height + 1);
            if (renderedLines != game.CharacterLines || renderedStart != game.CharacterStartLine)
            {
                renderedLines = game.CharacterLines; renderedStart = game.CharacterStartLine;
                int used = 0;
                for (int i = 0; renderedLines != null && i < lines && renderedStart + i < renderedLines.Length; i++)
                {
                    float x = 0;
                    foreach (var span in renderedLines[renderedStart + i].spans)
                    {
                        if (used == spans.Count)
                        {
                            var item = Instantiate(spanTemplate, content); item.gameObject.SetActive(true); spans.Add(item);
                        }
                        var text = spans[used++]; text.gameObject.SetActive(true);
                        text.font = game.InterfaceFont; text.fontSize = layout ? layout.dialogueFontSize : 18;
                        text.text = span.text; text.color = Rgb(span.foreground);
                        Place(text.rectTransform, x, i * height, span.width + 4, height + 2);
                        var outline = text.GetComponent<Outline>(); var shadow = System.Array.Find(text.GetComponents<Shadow>(), effect => !(effect is Outline));
                        outline.enabled = span.fontType == 1; outline.effectColor = Rgb(span.edge);
                        shadow.enabled = span.fontType == 2; shadow.effectColor = Rgb(span.edge);
                        x += span.width;
                    }
                }
                for (int i = used; i < spans.Count; i++) spans[i].gameObject.SetActive(false);
            }
        }
    }
}
