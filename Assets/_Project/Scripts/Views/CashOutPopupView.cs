using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Data;

namespace WheelGame.Views
{
    public sealed class CashOutPopupView : PopupView
    {
        private const string ContinueButtonName = "ui_button_continue";

        [SerializeField] private RectTransform _cellContainer;
        [SerializeField] private RewardCellView _cellPrefab;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Button _continueButton;

        private readonly List<RewardCellView> _cells = new List<RewardCellView>();

        public event Action ContinueClicked;

        public void Show(IReadOnlyDictionary<RewardItemData, int> rewards)
        {
            foreach (var cell in _cells)
                Destroy(cell.gameObject);

            _cells.Clear();

            foreach (var pair in rewards)
            {
                var cell = Instantiate(_cellPrefab, _cellContainer);
                cell.Show(pair.Key.Icon, pair.Value);
                _cells.Add(cell);
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