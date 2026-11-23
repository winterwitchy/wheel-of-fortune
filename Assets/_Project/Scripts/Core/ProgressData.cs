using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    [Serializable]
    public sealed class ProgressData
    {
        public List<ProgressEntry> Currencies = new List<ProgressEntry>();
        public List<ProgressEntry> Items = new List<ProgressEntry>();
    }

    [Serializable]
    public sealed class ProgressEntry
    {
        public string Id;
        public int Amount;

        public ProgressEntry()
        {
        }

        public ProgressEntry(string id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
