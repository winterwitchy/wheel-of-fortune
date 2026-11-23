using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    [Serializable]
    public sealed class ProgressData
    {
        #region Fields

        public List<ProgressEntry> Currencies = new List<ProgressEntry>();
        public List<ProgressEntry> Items = new List<ProgressEntry>();

        #endregion
    }

    [Serializable]
    public sealed class ProgressEntry
    {
        #region Fields

        public string Id;
        public int Amount;

        #endregion

        #region Constructors

        public ProgressEntry()
        {
        }

        public ProgressEntry(string id, int amount)
        {
            Id = id;
            Amount = amount;
        }

        #endregion
    }
}
