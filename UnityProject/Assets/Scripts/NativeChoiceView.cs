using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuJian
{
    public sealed class NativeChoiceView : MonoBehaviour
    {
        [SerializeField] private GuJianGame game;
        [SerializeField] private RectTransform viewport, listPanel, infoPanel, infoClip;
        [SerializeField] private GameObject panel;
        [SerializeField] private NativeChoicePanel listFrame, infoFrame;
        [SerializeField] private Text infoText;
        [SerializeField] private NativeChoiceRow[] rows;
        [SerializeField] private Image[] highlights;
        [SerializeField] private Text[] labels;
        [SerializeField] private RectTransform scrollbar, thumb;
        public void Bind(GuJianGame value) { game = value; }
        public bool ReplacesChoice => isActiveAndEnabled && game && viewport && panel && listPanel && infoPanel && infoClip && listFrame && infoFrame && infoText && rows != null && rows.Length == 3 && labels != null && labels.Length == 3 && highlights != null && highlights.Length == 3 && scrollbar && thumb && listFrame.GetComponent<CanvasRenderer>() && infoFrame.GetComponent<CanvasRenderer>() && System.Array.TrueForAll(rows, x => x) && System.Array.TrueForAll(labels, x => x) && System.Array.TrueForAll(highlights, x => x);
        internal void Select(int index) { if (game) game.SelectChoice(index); }
        private void OnDisable() { if (panel) panel.SetActive(false); }
        private static void Place(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h);
        }
        private void LateUpdate()
        {
            if (!ReplacesChoice) { if (panel) panel.SetActive(false); return; }
            Rect safe = Screen.safeArea;
            if (safe.width <= 0 || safe.height <= 0) safe = new Rect(0, 0, Screen.width, Screen.height);
            float scale = Mathf.Min(safe.width / GuJianGame.ViewWidth, safe.height / GuJianGame.ViewHeight);
            viewport.anchoredPosition = safe.center - new Vector2(Screen.width, Screen.height) / 2;
            viewport.localScale = Vector3.one * scale;
            bool visible = game.ChoiceVisible; panel.SetActive(visible); if (!visible) return;
            var choice = game.CurrentChoice;
            var layout = game.CharacterLayout;
            listFrame.SetTexture(layout ? layout.messagePanel : null); infoFrame.SetTexture(layout ? layout.messagePanel : null);
            infoText.font = game.InterfaceFont;
            foreach (var label in labels) label.font = game.InterfaceFont;
            int lineHeight = Mathf.Max(1, Mathf.CeilToInt(game.InterfaceFont.lineHeight * (infoText.fontSize / (float)Mathf.Max(1, game.InterfaceFont.fontSize))));
            infoText.text = choice.Information ?? "";
            Place(infoText.rectTransform, 0, 0, 180, 1);
            int infoHeight = choice.Information == null ? 0 : Mathf.CeilToInt(infoText.preferredHeight);
            game.ConfigureChoiceLayout(lineHeight, infoHeight); choice = game.CurrentChoice;
            int itemHeight = lineHeight + 5, listY = choice.Information == null ? 80 : 80 + infoHeight;
            int count = Mathf.Min(3, choice.Options.Length), listX = ((int)GuJianGame.ViewWidth - 120) >> 1;
            Place(listPanel, listX - 30, listY - 15, 180, itemHeight * count + 30);
            bool showInfo = choice.Information != null; infoPanel.gameObject.SetActive(showInfo); infoClip.gameObject.SetActive(showInfo);
            if (showInfo)
            {
                Place(infoPanel, ((int)GuJianGame.ViewWidth - 180) / 2 - 10, 40, 200, infoHeight + 20);
                Place(infoClip, ((int)GuJianGame.ViewWidth - 180) / 2, 50, 180, 80);
                Place(infoText.rectTransform, 0, choice.InformationOffset, 180, infoHeight);
            }
            for (int i = 0; i < rows.Length; i++)
            {
                int index = choice.FirstVisible + i; bool active = index < choice.Options.Length;
                rows[i].gameObject.SetActive(active); if (!active) continue;
                rows[i].owner = this; rows[i].index = index;
                Place((RectTransform)rows[i].transform, listX - 5, listY + i * itemHeight, 130, itemHeight);
                highlights[i].enabled = index == choice.Focus;
                labels[i].color = index == choice.Focus ? Color.white : new Color32(212, 210, 76, 255);
                labels[i].text = choice.Options[index] ?? "Null";
                float width = Mathf.Max(120, labels[i].preferredWidth);
                float offset = index == choice.Focus && width > 120 ? Mathf.Max(0, choice.MoveTicks * 2) : 0;
                if (offset >= width) game.ResetChoiceMarquee();
                Place(labels[i].rectTransform, 5 - offset, (itemHeight - lineHeight) / 2, width, lineHeight);
            }
            scrollbar.gameObject.SetActive(count > 0);
            if (count > 0)
            {
                int h = itemHeight * count, total = h - 2;
                int block = Mathf.Max(3, total / choice.Options.Length), y = choice.Focus * total / choice.Options.Length;
                if (y + block > total) y = total - block;
                Place(scrollbar, listX + 127, listY, 6, h); Place(thumb, 1, y + 1, 4, block);
            }
        }
    }
}
