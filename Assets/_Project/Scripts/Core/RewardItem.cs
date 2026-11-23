using System;

namespace WheelGame.Core
{
    public sealed class RewardItem : IEquatable<RewardItem>
    {
        #region Constructors

        public RewardItem(string id, bool isCurrency)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Reward id is missing.", nameof(id));
            }

            Id = id;
            IsCurrency = isCurrency;
        }

        #endregion

        #region Properties

        public string Id { get; }

        public bool IsCurrency { get; }

        #endregion

        #region Public Methods

        public bool Equals(RewardItem other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RewardItem);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id;
        }

        #endregion
    }
}
