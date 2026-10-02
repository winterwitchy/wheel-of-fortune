using System;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class SliceOverride
    {
        [SerializeField] private string _label;
        [SerializeField, Min(1)] private int _zone = 1;
        [SerializeField, Range(0, WheelLayout.SliceCount - 1)] private int _sliceIndex;
        [SerializeField] private SliceKind _kind;
        [SerializeField] private RewardItemData _reward;
        [SerializeField, Min(1)] private int _amount = 1;

        public SliceOverride() { }

        public SliceOverride(int zone, int sliceIndex, SliceKind kind, RewardItemData reward, int amount)
        {
            _zone = zone;
            _sliceIndex = sliceIndex;
            _kind = kind;
            _reward = reward;
            _amount = amount;
        }

        public int Zone => _zone;
        public int SliceIndex => _sliceIndex;
        public bool IsBomb => _kind == SliceKind.Bomb;

        public WheelSliceEntry ToSliceEntry()
        {
            return IsBomb ? WheelSliceEntry.CreateBomb() : WheelSliceEntry.CreateReward(_reward, _amount);
        }

        public void RefreshLabel()
        {
            var angle = _sliceIndex * 360 / WheelLayout.SliceCount;
            var content = IsBomb ? "BOMB" : DescribeReward();
            _label = $"Zone {_zone} · Slice {_sliceIndex} ({angle}°) · {content}";
        }

        private string DescribeReward()
        {
            var rewardName = _reward == null ? "None" : _reward.name;
            return $"{rewardName} x{_amount}";
        }
    }
}