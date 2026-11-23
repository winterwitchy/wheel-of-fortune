using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public sealed class RewardCellView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private RectTransform _punchTarget;
        [SerializeField, Min(0f)] private float _punchScale = 0.2f;
        [SerializeField, Min(0f)] private float _punchDuration = 0.3f;

        public RectTransform IconTransform
        {
            get { return _icon.rectTransform; }
        }

        public void Show(Sprite icon, int amount)
        {
            _icon.sprite = icon;
            _icon.color = Color.white;
            _amountText.text = AmountFormatter.Format(amount);
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

        private void OnDisable()
        {
            if (_punchTarget)
            {
                _punchTarget.DOKill(true);
            }
        }
    }
}
