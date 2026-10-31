using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuJian
{
    public sealed class NativeTitleView : MonoBehaviour
    {
        [SerializeField] private GuJianGame game;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private Button newGameButton, continueButton;
        [SerializeField] private Text status;
        private bool wasVisible;

        public bool ReplacesTitle => isActiveAndEnabled && viewport && titlePanel && newGameButton && continueButton;
        public Button NewGameButton => newGameButton;
        public Button ContinueButton => continueButton;
        public RectTransform Viewport => viewport;
        public void Bind(GuJianGame target) { game = target; }
        internal void HideForGameplay()
        {
            if (titlePanel) titlePanel.SetActive(false);
            wasVisible = false;
        }

        private void Awake()
        {
            if (newGameButton) newGameButton.onClick.AddListener(StartGame);
            if (continueButton) continueButton.onClick.AddListener(ContinueGame);
        }

        private void StartGame() { if (game) game.SubmitAction(GameAction.NewGame); }
        private void ContinueGame() { if (game) game.SubmitAction(GameAction.ContinueGame); }

        private void LateUpdate()
        {
            if (!game || !ReplacesTitle) return;
            // Keep the original logical viewport, inside the device's usable safe area.
            Rect safe = Screen.safeArea;
            if (safe.width <= 0 || safe.height <= 0) safe = new Rect(0, 0, Screen.width, Screen.height);
            float scale = Mathf.Min(safe.width / GuJianGame.ViewWidth, safe.height / GuJianGame.ViewHeight);
            viewport.anchoredPosition = safe.center - new Vector2(Screen.width, Screen.height) / 2f;
            viewport.localScale = Vector3.one * scale;

            bool visible = game.AtTitle;
            titlePanel.SetActive(visible);
            if (visible)
            {
                continueButton.interactable = game.CanContinue;
                var continueLabel = continueButton.GetComponentInChildren<Text>();
                if (continueLabel) continueLabel.color = continueButton.interactable ? Color.white : new Color(.55f, .61f, .63f, .7f);
                foreach (var label in titlePanel.GetComponentsInChildren<Text>(true))
                    if (label.font != game.InterfaceFont) label.font = game.InterfaceFont;
                if (status) status.text = game.LastActionError ?? "";
                var system = EventSystem.current;
                if (system && (!wasVisible || system.currentSelectedGameObject == null ||
                    (!continueButton.interactable && system.currentSelectedGameObject == continueButton.gameObject)))
                    system.SetSelectedGameObject(newGameButton.gameObject);
            }
            wasVisible = visible;
        }
    }
}
