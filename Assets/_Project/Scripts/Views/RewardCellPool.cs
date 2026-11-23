using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using WheelGame.Data;

namespace WheelGame.Views
{
    public sealed class RewardCellPool
    {
        private readonly RewardCellView _prefab;
        private readonly RectTransform _container;
        private readonly string _nameFormat;
        private readonly List<RewardCellView> _cells = new List<RewardCellView>();
        private readonly Dictionary<RewardItemData, RewardCellView> _activeCells = new Dictionary<RewardItemData, RewardCellView>();

        public RewardCellPool(RewardCellView prefab, RectTransform container, string nameFormat)
        {
            _prefab = prefab;
            _container = container;
            _nameFormat = nameFormat;
        }

        public bool TryGet(RewardItemData reward, out RewardCellView cell)
        {
            return _activeCells.TryGetValue(reward, out cell);
        }

        public RewardCellView GetOrCreate(RewardItemData reward)
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

        private RewardCellView TakeInactiveCell()
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
    }
}
