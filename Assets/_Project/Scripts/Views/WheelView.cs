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
        [SerializeField, Range(0f, 0.45f)] private float _landingJitter = 0.3f;

        [Header("Visuals")]
        [SerializeField] private Sprite _bombSprite;
        [SerializeField] private List<WheelSkin> _skins = new List<WheelSkin>();

        private readonly List<WheelSliceView> _slices = new List<WheelSliceView>();
        private Tween _spinTween;

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
            _spinTween?.Kill();

            var current = Mathf.Repeat(_rotator.localEulerAngles.z, 360f);
            var jitter = UnityEngine.Random.Range(-_landingJitter, _landingJitter) * SliceAngle;
            var target = sliceIndex * SliceAngle - _angleOffset + jitter;
            var distance = Mathf.Repeat(current - target, 360f);
            var end = current - (_fullTurns * 360f + distance);

            _spinTween = _rotator
                .DOLocalRotate(new Vector3(0f, 0f, end), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .SetLink(gameObject)
                .OnComplete(() => onComplete?.Invoke());
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