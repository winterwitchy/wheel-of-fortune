using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Data;

namespace WheelGame.Views
{
    public sealed class ZoneProgressBarViewMono : MonoBehaviour, IZoneProgressView
    {
        #region Nested Types

        [Serializable]
        private sealed class UpcomingZoneBadge
        {
            [SerializeField] private ZoneDefinition _zone;
            [SerializeField] private TMP_Text _valueText;

            public ZoneDefinition Zone
            {
                get { return _zone; }
            }

            public TMP_Text ValueText
            {
                get { return _valueText; }
            }
        }

        #endregion

        #region Constants

        private const string CELL_NAME_FORMAT = "ui_rect_zone_cell_{0}";

        #endregion

        #region Serialized Fields

        [Header("Strip")]
        [SerializeField] private RectTransform _strip;
        [SerializeField] private ZoneCellViewMono _cellPrefab;
        [SerializeField, Min(3)] private int _visibleCellCount = 11;
        [SerializeField, Min(1f)] private float _cellSpacing = 76f;
        [SerializeField, Range(0f, 1f)] private float _passedAlpha = 0.35f;

        [Header("Current Zone")]
        [SerializeField] private Image _currentBackground;
        [SerializeField] private RectTransform _currentPunchTarget;
        [SerializeField, Min(0f)] private float _currentPunchScale = 0.15f;
        [SerializeField, Min(0f)] private float _currentPunchDuration = 0.25f;

        [Header("Upcoming Zones")]
        [SerializeField] private List<UpcomingZoneBadge> _badges = new List<UpcomingZoneBadge>();

        [Header("Slide")]
        [SerializeField, Min(0f)] private float _slideDuration = 0.35f;
        [SerializeField] private Ease _slideEase = Ease.OutCubic;

        #endregion

        #region Private Fields

        private readonly List<ZoneCellViewMono> _cells = new List<ZoneCellViewMono>();
        private Func<int, ZoneVisuals> _visualsLookup;
        private Func<string, int> _nextZoneLookup;
        private Tween _slideTween;
        private int _shownZone;
        private int _pendingZone;

        #endregion

        #region Public Methods

        public void Initialize(Func<int, ZoneVisuals> visualsLookup, Func<string, int> nextZoneLookup)
        {
            _visualsLookup = visualsLookup;
            _nextZoneLookup = nextZoneLookup;
            CreateCellsIfNeeded();
        }

        public void Show(int zone)
        {
            ShowUpcomingZones();

            CreateCellsIfNeeded();
            StopSlide();

            if (IsAdvancingByOne(zone))
            {
                StartSlide(zone);
                return;
            }

            Layout(zone);
        }

        #endregion

        #region Unity Callbacks

        private void OnDisable()
        {
            StopSlide();
        }

        #endregion

        #region Layout

        private void CreateCellsIfNeeded()
        {
            if (_cells.Count > 0)
            {
                return;
            }

            var centerIndex = _visibleCellCount / 2;
            var cellCount = _visibleCellCount + 1;

            for (var i = 0; i < cellCount; i++)
            {
                var cell = Instantiate(_cellPrefab, _strip);
                cell.name = string.Format(CultureInfo.InvariantCulture, CELL_NAME_FORMAT, i);

                var cellTransform = (RectTransform)cell.transform;
                cellTransform.anchoredPosition = new Vector2((i - centerIndex) * _cellSpacing, 0f);

                cell.Hide();
                _cells.Add(cell);
            }
        }

        private void Layout(int zone)
        {
            _shownZone = zone;
            _strip.anchoredPosition = new Vector2(0f, _strip.anchoredPosition.y);

            var centerIndex = _visibleCellCount / 2;

            for (var i = 0; i < _cells.Count; i++)
            {
                var cellZone = zone - centerIndex + i;

                if (cellZone < 1)
                {
                    _cells[i].Hide();
                    continue;
                }

                var alpha = 1f;

                if (cellZone < zone)
                {
                    alpha = _passedAlpha;
                }

                _cells[i].Show(cellZone, _visualsLookup(cellZone).NumberColor, alpha);
            }

            ApplyCurrentVisuals(_visualsLookup(zone));
        }

        private void ApplyCurrentVisuals(ZoneVisuals visuals)
        {
            _currentBackground.sprite = visuals.CurrentZoneSprite;
            _currentBackground.color = visuals.CurrentZoneTint;
        }

        private void ShowUpcomingZones()
        {
            foreach (var badge in _badges)
            {
                if (!badge.Zone || !badge.ValueText)
                {
                    continue;
                }

                badge.ValueText.text = _nextZoneLookup(badge.Zone.Id).ToString(CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region Slide

        private bool IsAdvancingByOne(int zone)
        {
            if (_shownZone <= 0)
            {
                return false;
            }

            if (_slideDuration <= 0f)
            {
                return false;
            }

            return zone == _shownZone + 1;
        }

        private void StartSlide(int zone)
        {
            _pendingZone = zone;
            _slideTween = _strip
                .DOAnchorPosX(-_cellSpacing, _slideDuration)
                .SetEase(_slideEase)
                .SetLink(gameObject)
                .OnComplete(HandleSlideCompleted);
        }

        private void HandleSlideCompleted()
        {
            _slideTween = null;
            Layout(_pendingZone);
            PunchCurrentZone();
        }

        private void StopSlide()
        {
            if (_slideTween == null)
            {
                return;
            }

            _slideTween.Kill();
            _slideTween = null;
            Layout(_pendingZone);
        }

        private void PunchCurrentZone()
        {
            if (!_currentPunchTarget)
            {
                return;
            }

            _currentPunchTarget.DOKill(true);
            _currentPunchTarget
                .DOPunchScale(Vector3.one * _currentPunchScale, _currentPunchDuration)
                .SetLink(gameObject);
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_visibleCellCount % 2 == 0)
            {
                _visibleCellCount++;
            }
        }
#endif
    }
}
