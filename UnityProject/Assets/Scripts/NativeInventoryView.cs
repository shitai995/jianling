using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GuJian
{
    public sealed class NativeInventoryView : MonoBehaviour
    {
        [SerializeField] private GuJianGame game;
        [SerializeField] private RectTransform viewport, atlas, content, frame;
        [SerializeField] private GameObject panel;
        [SerializeField] private NativeInventoryRow rowTemplate;
        [SerializeField] private Text description, autoLabel, moneyLabel;
        [SerializeField] private Button activateButton, autoButton, backButton;
        [SerializeField] private Button[] tabButtons;
        private readonly List<NativeInventoryRow> rows = new List<NativeInventoryRow>();
        private int lastSelection = -1, lastFrame = -1;
        private readonly List<RawImage> framePieces = new List<RawImage>();
        public void Bind(GuJianGame value) { game = value; }
        public bool ReplacesInventory => isActiveAndEnabled && game && viewport && atlas && content && panel && rowTemplate && rowTemplate.transform.Find("Icon") && rowTemplate.transform.Find("Label") && description && autoLabel && activateButton && autoButton && backButton && moneyLabel && frame && tabButtons != null && tabButtons.Length == 6 && System.Array.TrueForAll(tabButtons, b => b);
        internal void Select(int index) { if (game) game.SelectInventoryRow(index); }
        private void Awake()
        {
            if (activateButton) activateButton.onClick.AddListener(() => { if (game) game.ActivateInventoryRow(); });
            if (autoButton) autoButton.onClick.AddListener(() => { if (game) game.CycleInventoryAutoRecover(); });
            if (backButton) backButton.onClick.AddListener(() => { if (game) game.InventoryClose(); });
            int[] pages = { 7, 2, 1, 6, 0, 3 };
            if (tabButtons != null) for (int i = 0; i < Mathf.Min(tabButtons.Length, pages.Length); i++)
            { int page = pages[i]; if (tabButtons[i]) tabButtons[i].onClick.AddListener(() => { if (game) game.InventorySelectPage(page); }); }
        }
        private void OnDisable() { if (panel) panel.SetActive(false); lastSelection = -1; }
        private static void Place(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
        }
        private void DrawFrame(OriginalUiLayout layout)
        {
            var animation = layout.menu; var steps = animation.actions[2].steps;
            if (steps == null || steps.Length == 0) return;
            int index = steps[Mathf.Abs(game.InventoryUiTick) % steps.Length].frame;
            if (index == lastFrame) return;
            lastFrame = index;
            NativeAtlasRenderer.Draw(frame, framePieces, animation, index, i => layout.menuSheets[i], new Vector2(160, 120), 1);
        }
        private void LateUpdate()
        {
            if (!ReplacesInventory) { if (panel) panel.SetActive(false); return; }
            float scale = Mathf.Min(Screen.width / GuJianGame.ViewWidth, Screen.height / GuJianGame.ViewHeight);
            viewport.localScale = Vector3.one * scale;
            viewport.anchoredPosition = Vector2.zero;
            bool visible = game.NativeInventoryVisible; panel.SetActive(visible);
            if (!visible) { lastSelection = -1; return; }
            var layout = game.CharacterLayout; Rect bounds = layout.menuBounds;
            Place(atlas, bounds.x, bounds.y, 320, 240);
            atlas.localScale = new Vector3(bounds.width / 320f, bounds.height / 240f, 1);
            DrawFrame(layout);
            var state = game.InventoryMenuState(); int count = state.Rows.Length;
            content.sizeDelta = new Vector2(187, Mathf.Max(145, count * 31));
            if (state.Selection != lastSelection)
            {
                float top = state.Selection * 31, scroll = content.anchoredPosition.y;
                if (top < scroll) scroll = top; else if (top + 31 > scroll + 145) scroll = top + 31 - 145;
                content.anchoredPosition = new Vector2(0, Mathf.Clamp(scroll, 0, Mathf.Max(0, count * 31 - 145)));
                lastSelection = state.Selection;
            }
            for (int i = 0; i < count; i++)
            {
                if (i == rows.Count) { var r = Instantiate(rowTemplate, content); r.gameObject.SetActive(true); rows.Add(r); }
                var row = rows[i]; row.gameObject.SetActive(true); row.owner = this; row.index = i;
                Place((RectTransform)row.transform, 0, i * 31, 187, 30);
                var text = row.transform.Find("Label").GetComponent<Text>(); text.font = game.InterfaceFont;
                text.text = (i == state.Selection ? "› " : "") + state.Rows[i].Name + " ×" + state.Rows[i].Quantity;
                var icon = row.transform.Find("Icon").GetComponent<RawImage>(); var sprite = state.Rows[i].Icon;
                if (sprite)
                {
                    icon.texture = sprite.texture; Rect r = sprite.textureRect;
                    icon.uvRect = new Rect(r.x / sprite.texture.width, r.y / sprite.texture.height, r.width / sprite.texture.width, r.height / sprite.texture.height);
                }
                else if (layout.itemIcons)
                {
                    icon.texture = layout.itemIcons; int id = state.Rows[i].ItemId == 23 ? 8 : state.Rows[i].ItemId;
                    icon.uvRect = new Rect(id * 22f / layout.itemIcons.width, 1 - 22f / layout.itemIcons.height, 22f / layout.itemIcons.width, 22f / layout.itemIcons.height);
                }
                icon.enabled = icon.texture;
            }
            for (int i = count; i < rows.Count; i++) rows[i].gameObject.SetActive(false);
            description.font = autoLabel.font = moneyLabel.font = game.InterfaceFont;
            moneyLabel.text = game.InventoryMoney + " \u4e24";
            description.text = count > 0 ? state.Rows[state.Selection].Description : "";
            autoLabel.text = "用药 " + state.AutoRecoverPercent + "%";
            activateButton.interactable = count > 0;
        }
    }
}
