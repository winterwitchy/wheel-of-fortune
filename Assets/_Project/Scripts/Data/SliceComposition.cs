using System;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class SliceComposition
    {
        [SerializeField, Min(0)] private int _cash;
        [SerializeField, Min(0)] private int _gold;
        [SerializeField, Min(0)] private int _points;
        [SerializeField, Min(0)] private int _consumables;
        [SerializeField, Min(0)] private int _chests;
        [SerializeField, Min(0)] private int _skins;

        public SliceComposition()
        {
        }

        public SliceComposition(int cash, int gold, int points, int consumables, int chests, int skins)
        {
            _cash = cash;
            _gold = gold;
            _points = points;
            _consumables = consumables;
            _chests = chests;
            _skins = skins;
        }

        public int Cash
        {
            get { return _cash; }
        }

        public int Gold
        {
            get { return _gold; }
        }

        public int Points
        {
            get { return _points; }
        }

        public int Consumables
        {
            get { return _consumables; }
        }

        public int Chests
        {
            get { return _chests; }
        }

        public int Skins
        {
            get { return _skins; }
        }

        public int Total
        {
            get { return _cash + _gold + _points + _consumables + _chests + _skins; }
        }
    }
}
