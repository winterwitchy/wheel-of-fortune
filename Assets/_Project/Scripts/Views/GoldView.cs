using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace WheelGame.Views
{
    public sealed class GoldView : MonoBehaviour, ICurrencyView
    {
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private RectTransform _punchTarget;

        private int _shownGold = -1;

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