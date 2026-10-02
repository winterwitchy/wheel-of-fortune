using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace WheelGame.Views
{
    public sealed class GoldView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private RectTransform _punchTarget;

        private int _shownGold = -1;

        public void Show(int gold)
        {
            var changed = _shownGold >= 0 && gold != _shownGold;
            _shownGold = gold;
            _amountText.text = gold.ToString(CultureInfo.InvariantCulture);

            if (!changed)
                return;

            _punchTarget.DOKill(true);
            _punchTarget.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(gameObject);
        }
    }
}