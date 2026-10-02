using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public sealed class BombPopupView : PopupView
    {
        private const string GiveUpButtonName = "ui_button_give_up";
        private const string ReviveButtonName = "ui_button_revive";

        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private TMP_Text _reviveCostText;

        public event Action GiveUpClicked;
        public event Action ReviveClicked;

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
            GiveUpClicked?.Invoke();
        }

        private void HandleReviveClicked()
        {
            ReviveClicked?.Invoke();
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