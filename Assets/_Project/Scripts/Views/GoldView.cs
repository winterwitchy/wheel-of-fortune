using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public sealed class GoldView : MonoBehaviour, ICurrencyView
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private RectTransform _punchTarget;

        private int _shownGold = -1;

        public void SetIcon(Sprite icon)
        {
            _iconImage.sprite = icon;
        }

        public void Show(int amount)
        {
            var changed = _shownGold >= 0 && amount != _shownGold;
            _shownGold = amount;
            _amountText.text = amount.ToString(CultureInfo.InvariantCulture);

            if (!changed)
                return;

            _punchTarget.DOKill(true);
            _punchTarget.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(gameObject);
        }
    }
}