using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GuJian
{
    public sealed class NativeEquipmentView : MonoBehaviour
    {
        [SerializeField] private GuJianGame game;
        [SerializeField] private RectTransform viewport, atlas, content, frame, preview;
        [SerializeField] private GameObject panel;
        [SerializeField] private NativeEquipmentRow rowTemplate;
        [SerializeField] private Text description, skill, money;
        [SerializeField] private Text[] attributes;
        [SerializeField] private Button equipButton, backButton;
        [SerializeField] private Button[] tabs;
        private readonly List<NativeEquipmentRow> rows = new List<NativeEquipmentRow>();
        private readonly List<RawImage> framePieces = new List<RawImage>(), previewPieces = new List<RawImage>();
        private int lastSelection = -1, lastFrame = -1;
        private EquipmentInstance lastPreviewItem;
        private OriginalAnimation lastPreviewAnimation;
        private string lastPreviewImage;
        public void Bind(GuJianGame value) { game = value; }
        public bool ReplacesEquipment => isActiveAndEnabled && game && viewport && atlas && content && frame && preview && panel && rowTemplate &&
            rowTemplate.transform.Find("Icon") && rowTemplate.transform.Find("Label") && rowTemplate.transform.Find("Focus") && description && skill && money && equipButton && backButton &&
            attributes != null && attributes.Length == 7 && System.Array.TrueForAll(attributes, a => a) && tabs != null && tabs.Length == 6 && System.Array.TrueForAll(tabs, b => b);
        internal void Select(int index) { if (game) game.SelectEquipmentRow(index); }
        private void Awake()
        {
            if (equipButton) equipButton.onClick.AddListener(() => { if (game) game.ActivateEquipmentRow(); });
            if (backButton) backButton.onClick.AddListener(() => { if (game) game.EquipmentClose(); });
            int[] pages = { 7, 2, 1, 6, 0, 3 };
            if (tabs != null) for (int i = 0; i < Mathf.Min(tabs.Length, pages.Length); i++)
            { int page = pages[i]; if (tabs[i]) tabs[i].onClick.AddListener(() => { if (game) game.EquipmentSelectPage(page); }); }
        }
        private void OnDisable() { if (panel) panel.SetActive(false); lastSelection = -1; }
        private static void Place(RectTransform r, float x, float y, float w, float h)
        { r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); }
        private void LateUpdate()
        {
            if (!ReplacesEquipment) { if (panel) panel.SetActive(false); return; }
            float scale = Mathf.Min(Screen.width / GuJianGame.ViewWidth, Screen.height / GuJianGame.ViewHeight);
            viewport.localScale = Vector3.one * scale; viewport.anchoredPosition = Vector2.zero;
            bool visible = game.NativeEquipmentVisible; panel.SetActive(visible);
            if (!visible) { lastSelection = -1; lastPreviewItem = null; return; }
            var layout = game.CharacterLayout; Rect bounds = layout.menuBounds; Place(atlas, bounds.x, bounds.y, 320, 240);
            atlas.localScale = new Vector3(bounds.width / 320f, bounds.height / 240f, 1);
            var action = layout.menu.actions[1]; int frameIndex = action.steps[Mathf.Abs(game.InventoryUiTick) % action.steps.Length].frame;
            if (lastFrame != frameIndex) { lastFrame = frameIndex; NativeAtlasRenderer.Draw(frame, framePieces, layout.menu, frameIndex, i => layout.menuSheets[i], new Vector2(160, 120), 1); }
            var state = game.EquipmentMenuState(); int count = state.Rows.Length;
            content.sizeDelta = new Vector2(98, Mathf.Max(124, count * 31));
            if (state.Selection != lastSelection)
            {
                float top = state.Selection * 31, scroll = content.anchoredPosition.y;
                if (top < scroll) scroll = top; else if (top + 31 > scroll + 124) scroll = top + 31 - 124;
                content.anchoredPosition = new Vector2(0, Mathf.Clamp(scroll, 0, Mathf.Max(0, count * 31 - 124))); lastSelection = state.Selection;
            }
            for (int i = 0; i < count; i++)
            {
                if (i == rows.Count) { var row = Instantiate(rowTemplate, content); row.gameObject.SetActive(true); rows.Add(row); }
                var r = rows[i]; r.gameObject.SetActive(true); r.owner = this; r.index = i; Place((RectTransform)r.transform, 0, i * 31, 98, 30);
                var highlight = r.transform.Find("Focus").GetComponent<Image>(); highlight.enabled = i == state.Selection;
                var label = r.transform.Find("Label").GetComponent<Text>(); label.font = game.InterfaceFont; label.text = state.Rows[i].Name;
                label.color = i == state.Selection ? new Color(.12f, .07f, .025f) : new Color(.94f, .87f, .68f);
                var icon = r.transform.Find("Icon").GetComponent<RawImage>(); var sprite = state.Rows[i].Icon;
                if (sprite) { icon.texture = sprite.texture; Rect source = sprite.textureRect; icon.uvRect = new Rect(source.x / sprite.texture.width, source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height); }
                else if (layout.weaponIcons && !string.IsNullOrEmpty(state.Rows[i].LegacyIcon))
                {
                    var parts = state.Rows[i].LegacyIcon.Split(','); int y = int.Parse(parts[0]), x = int.Parse(parts[1]);
                    icon.texture = layout.weaponIcons; icon.uvRect = new Rect(x * 22f / icon.texture.width, 1 - (y + 1) * 22f / icon.texture.height, 22f / icon.texture.width, 22f / icon.texture.height);
                }
                else icon.texture = null;
                icon.enabled = icon.texture;
            }
            for (int i = count; i < rows.Count; i++) rows[i].gameObject.SetActive(false);
            description.font = skill.font = money.font = game.InterfaceFont; money.text = game.InventoryMoney + " \u4e24";
            equipButton.interactable = count > 0;
            description.text = count > 0 ? state.Rows[state.Selection].Description : "";
            skill.text = count > 0 ? state.Rows[state.Selection].Skill : "";
            for (int i = 0; i < attributes.Length; i++) { attributes[i].font = game.InterfaceFont; attributes[i].text = count > 0 ? state.Rows[state.Selection].Values[i].ToString() : ""; }
            preview.gameObject.SetActive(count > 0);
            var selectedItem = count > 0 ? game.Equipment[state.Selection] : null;
            var animation = count > 0 ? game.EquipmentPreviewAnimation : null;
            string imageName = selectedItem == null ? null : selectedItem.Data.image;
            if (selectedItem != null && (selectedItem != lastPreviewItem || animation != lastPreviewAnimation || imageName != lastPreviewImage))
            {
                lastPreviewItem = selectedItem; lastPreviewAnimation = animation; lastPreviewImage = imageName;
                var anim = animation; Rect r = game.EquipmentPreviewBounds;
                float factor = Mathf.Min(2, 68 / Mathf.Max(1, r.width), 145 / Mathf.Max(1, r.height));
                var origin = new Vector2((68 - r.width * factor) / 2 - r.x * factor, (145 - r.height * factor) / 2 - r.y * factor);
                int selected = state.Selection; NativeAtlasRenderer.Draw(preview, previewPieces, anim, anim.actions[0].steps[0].frame, i => game.EquipmentPreviewTexture(i, selected), origin, factor);
            }
            if (count == 0) lastPreviewItem = null;
        }
    }
}
