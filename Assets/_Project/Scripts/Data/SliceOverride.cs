using System;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class SliceOverride
    {
        #region Serialized Fields

        [SerializeField] private string _label;
        [SerializeField, Min(1)] private int _zone = 1;
        [SerializeField, Range(0, WheelLayout.SLICE_COUNT - 1)] private int _sliceIndex;
        [SerializeField] private SliceKind _kind;
        [SerializeField] private RewardItemData _reward;
        [SerializeField, Min(1)] private int _amount = 1;

        #endregion

        #region Constructors

        public SliceOverride()
        {
        }

        public SliceOverride(int zone, int sliceIndex, SliceKind kind, RewardItemData reward, int amount)
        {
            _zone = zone;
            _sliceIndex = sliceIndex;
            _kind = kind;
            _reward = reward;
            _amount = amount;
        }

        #endregion

        #region Properties

        public int Zone
        {
            get { return _zone; }
        }

        public int SliceIndex
        {
            get { return _sliceIndex; }
        }

        public bool IsBomb
        {
            get { return _kind == SliceKind.Bomb; }
        }

        public RewardItemData Reward
        {
            get { return _reward; }
        }

        public int Amount
        {
            get { return _amount; }
        }

        public bool IsValid
        {
            get
            {
                if (_sliceIndex < 0 || _sliceIndex >= WheelLayout.SLICE_COUNT)
                {
                    return false;
                }

                if (IsBomb)
                {
                    return true;
                }

                if (!_reward || !_reward.HasId)
                {
                    return false;
                }

                return _amount >= 1;
            }
        }

        #endregion

        #region Public Methods

        public void RefreshLabel()
        {
            var angle = _sliceIndex * 360 / WheelLayout.SLICE_COUNT;
            var content = "BOMB";

            if (!IsBomb)
            {
                content = DescribeReward();
            }

            _label = $"Zone {_zone} · Slice {_sliceIndex} ({angle}°) · {content}";
        }

        #endregion

        #region Private Methods

        private string DescribeReward()
        {
            var rewardName = "None";

            if (_reward)
            {
                rewardName = _reward.name;
            }

            return $"{rewardName} x{_amount}";
        }

        #endregion
    }
}
