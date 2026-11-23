using System;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class ZoneVisuals
    {
        #region Serialized Fields

        [Header("Wheel")]
        [SerializeField] private string _title;
        [SerializeField] private Color _titleColor = Color.white;
        [SerializeField] private Sprite _wheelBase;
        [SerializeField] private Sprite _wheelIndicator;

        [Header("Effects")]
        [SerializeField] private Color _glowColor = Color.white;
        [SerializeField] private Color _raysColor = Color.white;
        [SerializeField, Min(0.1f)] private float _effectScale = 1f;
        [SerializeField, Min(1f)] private float _raysSecondsPerTurn = 40f;

        [Header("Progress Bar")]
        [SerializeField] private Color _numberColor = Color.white;
        [SerializeField] private Sprite _currentZoneSprite;
        [SerializeField] private Color _currentZoneTint = Color.white;

        #endregion

        #region Properties

        public string Title
        {
            get { return _title; }
        }

        public Color TitleColor
        {
            get { return _titleColor; }
        }

        public Sprite WheelBase
        {
            get { return _wheelBase; }
        }

        public Sprite WheelIndicator
        {
            get { return _wheelIndicator; }
        }

        public Color GlowColor
        {
            get { return _glowColor; }
        }

        public Color RaysColor
        {
            get { return _raysColor; }
        }

        public float EffectScale
        {
            get { return _effectScale; }
        }

        public float RaysSecondsPerTurn
        {
            get { return _raysSecondsPerTurn; }
        }

        public Color NumberColor
        {
            get { return _numberColor; }
        }

        public Sprite CurrentZoneSprite
        {
            get { return _currentZoneSprite; }
        }

        public Color CurrentZoneTint
        {
            get { return _currentZoneTint; }
        }

        #endregion
    }
}
