using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core;
using WheelGame.Core.Events;

namespace WheelGame.Views
{
    public sealed class CashOutPopupViewMono : PopupViewMono, ICashOutPopupView
    {
        #region Constants

        private const string CONTINUE_BUTTON_NAME = "ui_button_popup_cashout_continue";
        private const string CELL_NAME_FORMAT = "ui_rect_popup_cashout_cell_{0}";

        #endregion

        #region Serialized Fields

        [SerializeField] private RectTransform _cellContainer;
        [SerializeField] private RewardCellViewMono _cellPrefab;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Button _continueButton;

        #endregion

        #region Fields

        private RewardCellPool _cells;
        private IRewardIconProvider _icons;
        private IEventBus _bus;

        #endregion

        #region Public Methods

        public void Construct(IEventBus bus, IRewardIconProvider icons)
        {
            _bus = bus;
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

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            _continueButton.onClick.AddListener(HandleContinueClicked);
        }

        private void OnDisable()
        {
            _continueButton.onClick.RemoveListener(HandleContinueClicked);
        }

        #endregion

        #region Private Methods

        private void HandleContinueClicked()
        {
            _bus.Publish(new ContinueRequestedEvent());
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _continueButton = ButtonLookup.FindInChildren(this, CONTINUE_BUTTON_NAME);
        }
#endif

        #endregion
    }
}