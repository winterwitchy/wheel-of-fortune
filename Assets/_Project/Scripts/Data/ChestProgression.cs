using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class ChestProgression
    {
        [SerializeField] private List<RewardItemData> _sequence = new List<RewardItemData>();
        [SerializeField, Min(1)] private int _windowSize = 3;
        [SerializeField, Min(1)] private int _eraLength = 30;

        public ChestProgression() { }

        public ChestProgression(List<RewardItemData> sequence, int windowSize, int eraLength)
        {
            _sequence = sequence;
            _windowSize = windowSize;
            _eraLength = eraLength;
        }

        public IReadOnlyList<RewardItemData> Sequence => _sequence;
        public int WindowSize => _windowSize;

        public RewardItemData GetOption(int zone, int windowIndex)
        {
            var era = (zone - 1) / _eraLength;
            var index = Math.Min(era + windowIndex, _sequence.Count - 1);
            return _sequence[index];
        }
    }
}