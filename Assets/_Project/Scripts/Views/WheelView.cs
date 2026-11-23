using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Core;
using WheelGame.Core.Events;
using WheelGame.Data;

namespace WheelGame.Views
{
    public sealed class WheelView : MonoBehaviour, IWheelView
    {
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

        [Header("Visuals")]
        [SerializeField] private Sprite _bombSprite;

        private readonly List<WheelSliceView> _slices = new List<WheelSliceView>();
        private Tween _spinTween;
        private bool _isSpinPending;
        private IRewardIconProvider _icons;
        private IEventBus _bus;
        private WheelSpinSettings _spinSettings;

        private static float SliceAngle => 360f / WheelLayout.SLICE_COUNT;

        public void Construct(IEventBus bus, IRewardIconProvider icons, WheelSpinSettings spinSettings)
        {
            _bus = bus;
            _icons = icons;
            _spinSettings = spinSettings;
        }

        public void Show(ZoneVisuals visuals, IReadOnlyList<WheelSliceEntry> wheel)
        {
            CreateSlicesIfNeeded();
            ApplyVisuals(visuals);

            for (var i = 0; i < _slices.Count; i++)
            {
                var entry = wheel[i];

                if (entry.IsBomb)
                {
                    _slices[i].ShowBomb(_bombSprite);
                }
                else
                {
                    _slices[i].ShowReward(_icons.GetIcon(entry.Reward.Id), entry.Amount);
                }
            }
        }

        public Vector3 GetSliceIconPosition(int sliceIndex)
        {
            return _slices[sliceIndex].IconPosition;
        }

        public void SetSpinInteractable(bool interactable)
        {
            _spinButton.interactable = interactable;
        }

        public void PlaySpin(int sliceIndex)
        {
            if (_spinTween != null)
            {
                _spinTween.Kill();
            }

            _isSpinPending = true;

            var current = Mathf.Repeat(_rotator.localEulerAngles.z, 360f);
            var target = sliceIndex * SliceAngle - _angleOffset;
            var distance = Mathf.Repeat(current - target, 360f);
            var end = current - (_spinSettings.FullTurns * 360f + distance);

            _spinTween = _rotator
                .DOLocalRotate(new Vector3(0f, 0f, end), _spinSettings.Duration, RotateMode.FastBeyond360)
                .SetEase(_spinSettings.Ease)
                .SetLink(gameObject)
                .OnComplete(HandleSpinTweenFinished)
                .OnKill(HandleSpinTweenFinished);
        }

        private void HandleSpinTweenFinished()
        {
            _spinTween = null;

            if (!_isSpinPending)
            {
                return;
            }

            _isSpinPending = false;
            _bus.Publish(new SpinAnimationFinishedEvent());
        }

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(HandleSpinClicked);
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
            _isSpinPending = false;
        }

        private void HandleSpinClicked()
        {
            _bus.Publish(new SpinRequestedEvent());
        }

        private void CreateSlicesIfNeeded()
        {
            if (_slices.Count > 0)
                return;

            for (var i = 0; i < WheelLayout.SLICE_COUNT; i++)
            {
                var slice = Instantiate(_slicePrefab, _sliceContainer);
                slice.name = $"ui_rect_wheel_slice_{i}";

                var angle = _angleOffset - i * SliceAngle;
                var radians = angle * Mathf.Deg2Rad;
                var sliceTransform = (RectTransform)slice.transform;
                sliceTransform.anchoredPosition = new Vector2(-Mathf.Sin(radians), Mathf.Cos(radians)) * _sliceRadius;
                sliceTransform.localEulerAngles = new Vector3(0f, 0f, angle);

                _slices.Add(slice);
            }
        }

        private void ApplyVisuals(ZoneVisuals visuals)
        {
            _baseImage.sprite = visuals.WheelBase;
            _indicatorImage.sprite = visuals.WheelIndicator;
            _titleText.text = visuals.Title;
            _titleText.color = visuals.TitleColor;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spinButton = ButtonLookup.FindInChildren(this, SpinButtonName);
        }
#endif
    }
}