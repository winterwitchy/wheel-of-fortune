using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public sealed class GoldViewMono : MonoBehaviour, ICurrencyView
    {
        #region Serialized Fields

        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private RectTransform _punchTarget;
        [SerializeField, Min(0f)] private float _punchScale = 0.2f;
        [SerializeField, Min(0f)] private float _punchDuration = 0.3f;

        #endregion

        #region Fields

        private int _shownGold = -1;

        #endregion

        #region Public Methods

        public void SetIcon(Sprite icon)
        {
            _iconImage.sprite = icon;
        }

        public void Show(int amount)
        {
            var changed = _shownGold >= 0 && amount != _shownGold;
            _shownGold = amount;
            _amountText.text = AmountFormatter.FormatCompact(amount);

            if (!changed)
            {
                return;
            }

            _punchTarget.DOKill(true);
            _punchTarget.DOPunchScale(Vector3.one * _punchScale, _punchDuration).SetLink(gameObject);
        }

        #endregion
    }
}