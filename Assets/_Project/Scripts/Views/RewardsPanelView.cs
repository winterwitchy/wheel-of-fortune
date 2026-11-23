using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core;
using WheelGame.Core.Events;

namespace WheelGame.Views
{
    public sealed class RewardsPanelView : MonoBehaviour, IRewardsPanelView
    {
        #region Constants

        private const string LEAVE_BUTTON_NAME = "ui_button_rewards_leave";
        private const string CELL_NAME_FORMAT = "ui_rect_rewards_row_{0}";

        #endregion

        #region Serialized Fields

        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RewardCellView _cellPrefab;
        [SerializeField] private Button _leaveButton;

        #endregion

        #region Private Fields

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

        public void Clear()
        {
            GetCells().ReleaseAll();
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, 0f);
        }

        public RectTransform PrepareClaimTarget(RewardItem reward, int shownAmount)
        {
            var cell = GetCells().GetOrCreate(reward);
            var icon = _icons.GetIcon(reward.Id);

            if (shownAmount > 0)
            {
                cell.Show(icon, shownAmount);
            }
            else
            {
                cell.ShowPlaceholder(icon);
            }

            Reveal((RectTransform)cell.transform);
            return cell.IconTransform;
        }

        public void ShowClaimedAmount(RewardItem reward, int amount)
        {
            var cell = GetCells().GetOrCreate(reward);
            cell.Show(_icons.GetIcon(reward.Id), amount);
            cell.PlayPunch();
        }

        public void SetLeaveInteractable(bool interactable)
        {
            _leaveButton.interactable = interactable;
        }

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            _leaveButton.onClick.AddListener(HandleLeaveClicked);
        }

        private void OnDisable()
        {
            _leaveButton.onClick.RemoveListener(HandleLeaveClicked);
        }

        #endregion

        #region Private Methods

        private RewardCellPool GetCells()
        {
            if (_cells == null)
            {
                _cells = new RewardCellPool(_cellPrefab, _content, CELL_NAME_FORMAT);
            }

            return _cells;
        }

        private void Reveal(RectTransform cell)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
            _scrollRect.StopMovement();

            var viewportHeight = _scrollRect.viewport.rect.height;
            var cellTop = -cell.anchoredPosition.y - cell.rect.height * (1f - cell.pivot.y);
            var cellBottom = cellTop + cell.rect.height;
            var scroll = _content.anchoredPosition.y;

            if (cellTop < scroll)
            {
                scroll = cellTop;
            }

            if (cellBottom > scroll + viewportHeight)
            {
                scroll = cellBottom - viewportHeight;
            }

            var maxScroll = Mathf.Max(0f, _content.rect.height - viewportHeight);
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, Mathf.Clamp(scroll, 0f, maxScroll));
        }

        private void HandleLeaveClicked()
        {
            _bus.Publish(new LeaveRequestedEvent());
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _leaveButton = ButtonLookup.FindInChildren(this, LEAVE_BUTTON_NAME);
        }
#endif
    }
}
