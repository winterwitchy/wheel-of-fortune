using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public sealed class RewardCellViewMono : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private RectTransform _punchTarget;
        [SerializeField, Min(0f)] private float _punchScale = 0.2f;
        [SerializeField, Min(0f)] private float _punchDuration = 0.3f;

        #endregion

        #region Properties

        public RectTransform IconTransform
        {
            get { return _icon.rectTransform; }
        }

        #endregion

        #region Public Methods

        public void Show(Sprite icon, int amount)
        {
            _icon.sprite = icon;
            _icon.color = Color.white;
            _amountText.text = AmountFormatter.FormatCount(amount);
        }

        public void ShowPlaceholder(Sprite icon)
        {
            _icon.sprite = icon;
            _icon.color = Color.clear;
            _amountText.text = string.Empty;
        }

        public void PlayPunch()
        {
            if (!_punchTarget)
            {
                return;
            }

            _punchTarget.DOKill(true);
            _punchTarget
                .DOPunchScale(Vector3.one * _punchScale, _punchDuration)
                .SetLink(gameObject);
        }

        #endregion

        #region Unity Callbacks

        private void OnDisable()
        {
            if (_punchTarget)
            {
                _punchTarget.DOKill(true);
            }
        }

        #endregion
    }
}
