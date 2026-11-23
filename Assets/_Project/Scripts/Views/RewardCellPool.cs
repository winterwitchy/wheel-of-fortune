using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Views
{
    public sealed class RewardCellPool
    {
        #region Fields

        private readonly RewardCellViewMono _prefab;
        private readonly RectTransform _container;
        private readonly string _nameFormat;
        private readonly List<RewardCellViewMono> _cells = new List<RewardCellViewMono>();
        private readonly Dictionary<RewardItem, RewardCellViewMono> _activeCells = new Dictionary<RewardItem, RewardCellViewMono>();

        #endregion

        #region Constructors

        public RewardCellPool(RewardCellViewMono prefab, RectTransform container, string nameFormat)
        {
            _prefab = prefab;
            _container = container;
            _nameFormat = nameFormat;
        }

        #endregion

        #region Public Methods

        public bool TryGet(RewardItem reward, out RewardCellViewMono cell)
        {
            return _activeCells.TryGetValue(reward, out cell);
        }

        public RewardCellViewMono GetOrCreate(RewardItem reward)
        {
            if (_activeCells.TryGetValue(reward, out var cell))
            {
                return cell;
            }

            cell = TakeInactiveCell();
            cell.transform.SetAsLastSibling();
            cell.gameObject.SetActive(true);
            _activeCells.Add(reward, cell);
            return cell;
        }

        public void ReleaseAll()
        {
            foreach (var cell in _cells)
            {
                cell.gameObject.SetActive(false);
            }

            _activeCells.Clear();
        }

        #endregion

        #region Private Methods

        private RewardCellViewMono TakeInactiveCell()
        {
            foreach (var cell in _cells)
            {
                if (!cell.gameObject.activeSelf)
                {
                    return cell;
                }
            }

            var created = Object.Instantiate(_prefab, _container);
            created.name = string.Format(CultureInfo.InvariantCulture, _nameFormat, _cells.Count);
            _cells.Add(created);
            return created;
        }

        #endregion
    }
}
