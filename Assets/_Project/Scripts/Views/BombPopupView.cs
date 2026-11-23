using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core.Events;

namespace WheelGame.Views
{
    public sealed class BombPopupView : PopupView, IBombPopupView
    {
        private const string GiveUpButtonName = "ui_button_popup_bomb_give_up";
        private const string ReviveButtonName = "ui_button_popup_bomb_revive";

        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private TMP_Text _reviveCostText;

        private IEventBus _bus;

        public void Construct(IEventBus bus)
        {
            _bus = bus;
        }

        public void Show(int reviveCost, bool canAffordRevive)
        {
            _reviveCostText.text = reviveCost.ToString(CultureInfo.InvariantCulture);
            _reviveButton.interactable = canAffordRevive;
            Open();
        }

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

        private void HandleGiveUpClicked()
        {
            _bus.Publish(new GiveUpRequestedEvent());
        }

        private void HandleReviveClicked()
        {
            _bus.Publish(new ReviveRequestedEvent());
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _giveUpButton = ButtonLookup.FindInChildren(this, GiveUpButtonName);
            _reviveButton = ButtonLookup.FindInChildren(this, ReviveButtonName);
        }
#endif
    }
}