using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Views
{
    public sealed class WheelView : MonoBehaviour
    {
        [Serializable]
        private sealed class WheelSkin
        {
            [SerializeField] private ZoneType _zoneType;
            [SerializeField] private string _title;
            [SerializeField] private Color _titleColor = Color.white;
            [SerializeField] private Sprite _baseSprite;
            [SerializeField] private Sprite _indicatorSprite;

            public ZoneType ZoneType => _zoneType;
            public string Title => _title;
            public Color TitleColor => _titleColor;
            public Sprite BaseSprite => _baseSprite;
            public Sprite IndicatorSprite => _indicatorSprite;
        }

        private const string SpinButtonName = "ui_button_spin";

        [Header("References")]
        [SerializeField] private RectTransform _rotator;
        [SerializeField] private RectTransform _sliceContainer;
        [SerializeField] private WheelSliceView _slicePrefab;
        [SerializeField] private Image _baseImage;
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _spinButton;

        [Header("Layout")]
        [SerializeField, Min(0f)] private float _sliceRadius = 205f;
        [SerializeField] private float _angleOffset;

        [Header("Spin")]
        [SerializeField, Min(0.1f)] private float _spinDuration = 3.5f;
        [SerializeField, Min(1)] private int _fullTurns = 5;
        [SerializeField, Range(0f, 0.4f)] private float _settleOvershoot = 0.12f;
        [SerializeField, Min(0f)] private float _settleDuration = 0.35f;

        [Header("Visuals")]
        [SerializeField] private Sprite _bombSprite;
        [SerializeField] private List<WheelSkin> _skins = new List<WheelSkin>();

        private readonly List<WheelSliceView> _slices = new List<WheelSliceView>();
        private Tween _spinTween;
        private Action _spinCompleted;

        public event Action SpinClicked;

        private static float SliceAngle => 360f / WheelLayout.SliceCount;

        public void Show(ZoneType zoneType, IReadOnlyList<WheelSliceEntry> wheel)
        {
            CreateSlicesIfNeeded();
            ApplySkin(zoneType);

            for (var i = 0; i < _slices.Count; i++)
            {
                var entry = wheel[i];

                if (entry.IsBomb)
                    _slices[i].ShowBomb(_bombSprite);
                else
                    _slices[i].ShowReward(entry.Reward.Icon, entry.Amount);
            }
        }

        public void SetSpinInteractable(bool interactable)
        {
            _spinButton.interactable = interactable;
        }

        public void PlaySpin(int sliceIndex, Action onComplete)
        {
            if (_spinTween != null)
            {
                _spinTween.Kill();
            }

            _spinCompleted = onComplete;

            var current = Mathf.Repeat(_rotator.localEulerAngles.z, 360f);
            var target = sliceIndex * SliceAngle - _angleOffset;
            var distance = Mathf.Repeat(current - target, 360f);
            var end = current - (_fullTurns * 360f + distance);
            var overshoot = _settleOvershoot * SliceAngle;

            var sequence = DOTween.Sequence();
            sequence.Append(_rotator
                .DOLocalRotate(new Vector3(0f, 0f, end - overshoot), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart));
            sequence.Append(_rotator
                .DOLocalRotate(new Vector3(0f, 0f, end), _settleDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutSine));
            sequence.SetLink(gameObject);
            sequence.OnComplete(HandleSpinTweenCompleted);

            _spinTween = sequence;
        }

        private void HandleSpinTweenCompleted()
        {
            var callback = _spinCompleted;
            _spinCompleted = null;

            if (callback != null)
            {
                callback.Invoke();
            }
        }

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(HandleSpinClicked);
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
        }

        private void HandleSpinClicked()
        {
            SpinClicked?.Invoke();
        }

        private void CreateSlicesIfNeeded()
        {
            if (_slices.Count > 0)
                return;

            for (var i = 0; i < WheelLayout.SliceCount; i++)
            {
                var slice = Instantiate(_slicePrefab, _sliceContainer);
                slice.name = $"ui_wheel_slice_{i}";

                var angle = _angleOffset - i * SliceAngle;
                var radians = angle * Mathf.Deg2Rad;
                var sliceTransform = (RectTransform)slice.transform;
                sliceTransform.anchoredPosition = new Vector2(-Mathf.Sin(radians), Mathf.Cos(radians)) * _sliceRadius;
                sliceTransform.localEulerAngles = new Vector3(0f, 0f, angle);

                _slices.Add(slice);
            }
        }

        private void ApplySkin(ZoneType zoneType)
        {
            var skin = _skins.Find(candidate => candidate.ZoneType == zoneType);
            if (skin == null)
            {
                Debug.LogWarning($"{name}: no wheel skin for {zoneType}.", this);
                return;
            }

            _baseImage.sprite = skin.BaseSprite;
            _indicatorImage.sprite = skin.IndicatorSprite;
            _titleText.text = skin.Title;
            _titleText.color = skin.TitleColor;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spinButton = ButtonLookup.FindInChildren(this, SpinButtonName);
        }

        [ContextMenu("Debug/Spin To Random Slice")]
        private void DebugSpin()
        {
            CreateSlicesIfNeeded();
            PlaySpin(UnityEngine.Random.Range(0, WheelLayout.SliceCount), null);
        }
#endif
    }
}