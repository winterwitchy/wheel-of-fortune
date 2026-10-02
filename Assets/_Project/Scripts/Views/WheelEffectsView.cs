using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core;

namespace WheelGame.Views
{
    public sealed class WheelEffectsView : MonoBehaviour
    {
        [Serializable]
        private sealed class ZoneEffectStyle
        {
            [SerializeField] private ZoneType _zoneType;
            [SerializeField] private Color _glowColor = Color.white;
            [SerializeField] private Color _raysColor = Color.white;
            [SerializeField, Min(0.1f)] private float _scale = 1f;
            [SerializeField, Min(1f)] private float _secondsPerTurn = 40f;

            public ZoneType ZoneType => _zoneType;
            public Color GlowColor => _glowColor;
            public Color RaysColor => _raysColor;
            public float Scale => _scale;
            public float SecondsPerTurn => _secondsPerTurn;
        }

        [Header("References")]
        [SerializeField] private RectTransform _effectsRoot;
        [SerializeField] private Image _glow;
        [SerializeField] private Image _rays;
        [SerializeField] private RectTransform _shakeTarget;
        [SerializeField] private Image _bombFlash;

        [Header("Idle")]
        [SerializeField, Min(0.1f)] private float _pulseDuration = 1.6f;
        [SerializeField, Range(0f, 1f)] private float _pulseAmount = 0.12f;

        [Header("Zone Change")]
        [SerializeField, Min(0f)] private float _styleTransition = 0.6f;
        [SerializeField] private List<ZoneEffectStyle> _styles = new List<ZoneEffectStyle>();

        [Header("Bomb")]
        [SerializeField, Min(0.1f)] private float _bombDuration = 0.5f;
        [SerializeField, Min(0f)] private float _shakeStrength = 30f;
        [SerializeField, Range(0f, 1f)] private float _flashAlpha = 0.5f;
        [SerializeField] private Color _bombGlowColor = Color.red;

        private Tween _pulseTween;
        private Tween _raysTween;

        public void SetZoneType(ZoneType zoneType)
        {
            var style = _styles.Find(candidate => candidate.ZoneType == zoneType);
            if (style == null)
            {
                Debug.LogWarning($"{name}: no effect style for {zoneType}.", this);
                return;
            }

            _glow.DOKill();
            _rays.DOKill();
            _effectsRoot.DOKill();

            _glow.DOColor(style.GlowColor, _styleTransition).SetLink(gameObject);
            _rays.DOColor(style.RaysColor, _styleTransition).SetLink(gameObject);
            _effectsRoot.DOScale(style.Scale, _styleTransition).SetEase(Ease.OutBack).SetLink(gameObject);
            StartRaysRotation(style.SecondsPerTurn);
        }

        public void PlayBomb(Action onComplete)
        {
            _shakeTarget.DOKill(true);
            _bombFlash.DOKill();

            DOTween.Sequence()
                .Join(_shakeTarget.DOShakeAnchorPos(_bombDuration, _shakeStrength))
                .Join(_bombFlash.DOFade(_flashAlpha, _bombDuration * 0.25f).SetLoops(2, LoopType.Yoyo))
                .Join(_glow.DOColor(_bombGlowColor, _bombDuration * 0.5f).SetLoops(2, LoopType.Yoyo))
                .SetLink(gameObject)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void OnEnable()
        {
            _pulseTween = _glow.rectTransform
                .DOScale(1f + _pulseAmount, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);

            StartRaysRotation(40f);
        }

        private void OnDisable()
        {
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