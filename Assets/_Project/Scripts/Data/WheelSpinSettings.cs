using System;
using DG.Tweening;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class WheelSpinSettings
    {
        #region Serialized Fields

        [SerializeField, Min(0.1f)] private float _duration = 3.5f;
        [SerializeField, Min(1)] private int _fullTurns = 5;
        [SerializeField] private Ease _ease = Ease.OutQuart;

        #endregion

        #region Properties

        public float Duration
        {
            get { return _duration; }
        }

        public int FullTurns
        {
            get { return _fullTurns; }
        }

        public Ease Ease
        {
            get { return _ease; }
        }

        #endregion
    }
}
