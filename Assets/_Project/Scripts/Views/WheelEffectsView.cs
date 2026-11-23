using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core.Events;
using WheelGame.Data;

namespace WheelGame.Views
{
    public sealed class WheelEffectsView : MonoBehaviour, IWheelEffectsView
    {
        [Header("References")]
        [SerializeField] private RectTransform _effectsRoot;
        [SerializeField] private Image _glow;
        [SerializeField] private Image _rays;
        [SerializeField] private RectTransform _shakeTarget;
        [SerializeField] private Image _bombFlash;

        [Header("Idle")]
        [SerializeField, Min(0.1f)] private float _pulseDuration = 1.6f;
        [SerializeField, Range(0f, 1f)] private float _pulseAmount = 0.12f;
        [SerializeField, Min(1f)] private float _idleRaysSecondsPerTurn = 40f;

        [Header("Zone Change")]
        [SerializeField, Min(0f)] private float _styleTransition = 0.6f;

        [Header("Bomb")]
        [SerializeField, Min(0.1f)] private float _bombDuration = 0.5f;
        [SerializeField, Min(0f)] private float _shakeStrength = 30f;
        [SerializeField, Range(0f, 1f)] private float _flashAlpha = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _flashFadeRatio = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _glowFlashRatio = 0.5f;
        [SerializeField] private Color _bombGlowColor = Color.red;

        private Tween _pulseTween;
        private Tween _raysTween;
        private Tween _bombTween;
        private bool _isBombPending;
        private IEventBus _bus;

        public void Construct(IEventBus bus)
        {
            _bus = bus;
        }

        public void SetZoneVisuals(ZoneVisuals visuals)
        {
            _glow.DOKill();
            _rays.DOKill();
            _effectsRoot.DOKill();

            _glow.DOColor(visuals.GlowColor, _styleTransition).SetLink(gameObject);
            _rays.DOColor(visuals.RaysColor, _styleTransition).SetLink(gameObject);
            _effectsRoot.DOScale(visuals.EffectScale, _styleTransition).SetEase(Ease.OutBack).SetLink(gameObject);
            StartRaysRotation(visuals.RaysSecondsPerTurn);
        }

        public void PlayBomb()
        {
            _shakeTarget.DOKill(true);
            _bombFlash.DOKill();
            _isBombPending = true;

            _bombTween = DOTween.Sequence()
                .Join(_shakeTarget.DOShakeAnchorPos(_bombDuration, _shakeStrength))
                .Join(_bombFlash.DOFade(_flashAlpha, _bombDuration * _flashFadeRatio).SetLoops(2, LoopType.Yoyo))
                .Join(_glow.DOColor(_bombGlowColor, _bombDuration * _glowFlashRatio).SetLoops(2, LoopType.Yoyo))
                .SetLink(gameObject)
                .OnComplete(HandleBombTweenFinished)
                .OnKill(HandleBombTweenFinished);
        }

        private void HandleBombTweenFinished()
        {
            _bombTween = null;

            if (!_isBombPending)
            {
                return;
            }

            _isBombPending = false;
            _bus.Publish(new BombEffectFinishedEvent());
        }

        private void OnEnable()
        {
            _pulseTween = _glow.rectTransform
                .DOScale(1f + _pulseAmount, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);

            StartRaysRotation(_idleRaysSecondsPerTurn);
        }

        private void OnDisable()
        {
            _isBombPending = false;

            _pulseTween?.Kill();
            _raysTween?.Kill();
        }

        private void StartRaysRotation(float secondsPerTurn)
        {
            _raysTween?.Kill();
            _raysTween = _rays.rectTransform
                .DOLocalRotate(new Vector3(0f, 0f, -360f), secondsPerTurn, RotateMode.FastBeyond360)
                .SetRelative(true)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental)
                .SetLink(gameObject);
        }
    }
}