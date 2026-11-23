using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core.Events;

namespace WheelGame.Views
{
    public sealed class RewardClaimView : MonoBehaviour, IRewardClaimView
    {
        #region Serialized Fields

        [SerializeField] private RectTransform _flyer;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amountText;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float _popDuration = 0.2f;
        [SerializeField, Min(1f)] private float _popScale = 1.25f;
        [SerializeField, Min(0f)] private float _flyDuration = 0.5f;
        [SerializeField] private Ease _flyEase = Ease.InCubic;
        [SerializeField, Min(0f)] private float _arrivalScale = 0.6f;

        #endregion

        #region Private Fields

        private Sequence _sequence;
        private bool _isClaimPending;
        private IEventBus _bus;

        #endregion

        #region Public Methods

        public void Construct(IEventBus bus)
        {
            _bus = bus;
        }

        public void Play(Sprite icon, int amount, Vector3 fromWorldPosition, RectTransform target)
        {
            StopCurrent();

            _isClaimPending = true;
            _icon.sprite = icon;
            _amountText.text = AmountFormatter.Format(amount);
            _flyer.position = fromWorldPosition;
            _flyer.localScale = Vector3.one;
            _flyer.gameObject.SetActive(true);

            _sequence = DOTween.Sequence();
            _sequence.Append(_flyer.DOScale(_popScale, _popDuration).SetEase(Ease.OutBack));
            _sequence.Append(_flyer.DOMove(target.position, _flyDuration).SetEase(_flyEase));
            _sequence.Join(_flyer.DOScale(_arrivalScale, _flyDuration).SetEase(_flyEase));
            _sequence.SetLink(gameObject);
            _sequence.OnComplete(HandleSequenceFinished);
            _sequence.OnKill(HandleSequenceFinished);
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _flyer.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _isClaimPending = false;
            StopCurrent();
        }

        #endregion

        #region Private Methods

        private void StopCurrent()
        {
            if (_sequence == null)
            {
                return;
            }

            _sequence.Kill();
        }

        private void HandleSequenceFinished()
        {
            _sequence = null;
            _flyer.gameObject.SetActive(false);

            if (!_isClaimPending)
            {
                return;
            }

            _isClaimPending = false;
            _bus.Publish(new ClaimAnimationFinishedEvent());
        }

        #endregion
    }
}
