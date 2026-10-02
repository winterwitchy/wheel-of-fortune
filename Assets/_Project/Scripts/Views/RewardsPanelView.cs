using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Data;

namespace WheelGame.Views
{
    public sealed class RewardsPanelView : MonoBehaviour
    {
        private const string LeaveButtonName = "ui_button_leave";

        [SerializeField] private RectTransform _cellContainer;
        [SerializeField] private RewardCellView _cellPrefab;
        [SerializeField] private Button _leaveButton;

        private readonly Dictionary<RewardItemData, RewardCellView> _cells = new Dictionary<RewardItemData, RewardCellView>();

        public event Action LeaveClicked;

        public void Show(IReadOnlyDictionary<RewardItemData, int> rewards)
        {
            RemoveMissingCells(rewards);

            foreach (var pair in rewards)
            {
                if (!_cells.TryGetValue(pair.Key, out var cell))
                {
                    cell = Instantiate(_cellPrefab, _cellContainer);
                    _cells.Add(pair.Key, cell);
                }

                cell.Show(pair.Key.Icon, pair.Value);
            }
        }

        public void SetLeaveInteractable(bool interactable)
        {
            _leaveButton.interactable = interactable;
        }

        private void RemoveMissingCells(IReadOnlyDictionary<RewardItemData, int> rewards)
        {
            var missing = new List<RewardItemData>();

            foreach (var reward in _cells.Keys)
            {
                if (!rewards.ContainsKey(reward))
                    missing.Add(reward);
            }

            foreach (var reward in missing)
            {
                Destroy(_cells[reward].gameObject);
                _cells.Remove(reward);
            }
        }

        private void OnEnable()
        {
            _leaveButton.onClick.AddListener(HandleLeaveClicked);
        }

        private void OnDisable()
        {
            _leaveButton.onClick.RemoveListener(HandleLeaveClicked);
        }

        private void HandleLeaveClicked()
        {
            LeaveClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _leaveButton = ButtonLookup.FindInChildren(this, LeaveButtonName);
        }
#endif
    }
}