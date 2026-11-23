using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core;

namespace WheelGame.Views
{
    public sealed class CashOutPopupView : PopupView
    {
        private const string ContinueButtonName = "ui_button_popup_cashout_continue";
        private const string CELL_NAME_FORMAT = "ui_rect_popup_cashout_cell_{0}";

        [SerializeField] private RectTransform _cellContainer;
        [SerializeField] private RewardCellView _cellPrefab;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Button _continueButton;

        private RewardCellPool _cells;
        private IRewardIconProvider _icons;

        public event Action ContinueClicked;

        public void Initialize(IRewardIconProvider icons)
        {
            _icons = icons;
        }

        public void Show(IReadOnlyDictionary<RewardItem, int> rewards)
        {
            if (_cells == null)
            {
                _cells = new RewardCellPool(_cellPrefab, _cellContainer, CELL_NAME_FORMAT);
            }

            _cells.ReleaseAll();

            foreach (var pair in rewards)
            {
                _cells.GetOrCreate(pair.Key).Show(_icons.GetIcon(pair.Key.Id), pair.Value);
            }

            _emptyState.SetActive(rewards.Count == 0);
            Open();
        }

        private void OnEnable()
        {
            _continueButton.onClick.AddListener(HandleContinueClicked);
        }

        private void OnDisable()
        {
            _continueButton.onClick.RemoveListener(HandleContinueClicked);
        }

        private void HandleContinueClicked()
        {
            ContinueClicked?.Invoke();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _continueButton = ButtonLookup.FindInChildren(this, ContinueButtonName);
        }
#endif
    }
}