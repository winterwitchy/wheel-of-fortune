using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core.Events;

namespace WheelGame.Views
{
    public sealed class BombPopupViewMono : PopupViewMono, IBombPopupView
    {
        #region Constants

        private const string GIVE_UP_BUTTON_NAME = "ui_button_popup_bomb_give_up";
        private const string REVIVE_BUTTON_NAME = "ui_button_popup_bomb_revive";

        #endregion

        #region Serialized Fields

        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private TMP_Text _reviveCostText;

        #endregion

        #region Fields

        private IEventBus _bus;

        #endregion

        #region Public Methods

        public void Construct(IEventBus bus)
        {
            _bus = bus;
        }

        public void Show(int reviveCost, bool canAffordRevive)
        {
            _reviveCostText.text = AmountFormatter.FormatCompact(reviveCost);
            _reviveButton.interactable = canAffordRevive;
            Open();
        }

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            _giveUpButton.onClick.AddListener(HandleGiveUpClicked);
            _reviveButton.onClick.AddListener(HandleReviveClicked);
        }

        private void OnDisable()
        {
            _giveUpButton.onClick.RemoveListener(HandleGiveUpClicked);
            _reviveButton.onClick.RemoveListener(HandleReviveClicked);
        }

        #endregion

        #region Private Methods

        private void HandleGiveUpClicked()
        {
            _bus.Publish(new GiveUpRequestedEvent());
        }

        private void HandleReviveClicked()
        {
            _bus.Publish(new ReviveRequestedEvent());
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _giveUpButton = ButtonLookup.FindInChildren(this, GIVE_UP_BUTTON_NAME);
            _reviveButton = ButtonLookup.FindInChildren(this, REVIVE_BUTTON_NAME);
        }
#endif

        #endregion
    }
}