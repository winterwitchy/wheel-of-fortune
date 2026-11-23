using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WheelGame.Data;
using WheelGame.Views;

namespace WheelGame.Infrastructure
{
    public sealed class AddressableIconProvider : IRewardIconProvider, IDisposable
    {
        #region Fields

        private readonly Dictionary<string, Sprite> _icons = new Dictionary<string, Sprite>();
        private readonly List<AsyncOperationHandle<Sprite>> _handles = new List<AsyncOperationHandle<Sprite>>();

        #endregion

        #region Public Methods

        public IEnumerator Load(IEnumerable<RewardItemData> rewards)
        {
            var pending = new List<KeyValuePair<string, AsyncOperationHandle<Sprite>>>();

            foreach (var reward in rewards)
            {
                if (!reward || !reward.HasId || _icons.ContainsKey(reward.Id))
                {
                    continue;
                }

                if (!reward.HasIcon)
                {
                    Debug.LogWarning($"{reward.name} has no addressable icon.", reward);
                    continue;
                }

                var handle = Addressables.LoadAssetAsync<Sprite>(reward.Icon);
                _handles.Add(handle);
                pending.Add(new KeyValuePair<string, AsyncOperationHandle<Sprite>>(reward.Id, handle));
            }

            foreach (var pair in pending)
            {
                yield return pair.Value;

                if (pair.Value.Status == AsyncOperationStatus.Succeeded)
                {
                    _icons[pair.Key] = pair.Value.Result;
                }
                else
                {
                    Debug.LogWarning($"Could not load the icon of {pair.Key}.");
                }
            }
        }

        public Sprite GetIcon(string rewardId)
        {
            _icons.TryGetValue(rewardId, out var icon);
            return icon;
        }

        public void Dispose()
        {
            foreach (var handle in _handles)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            _handles.Clear();
            _icons.Clear();
        }

        #endregion
    }
}
