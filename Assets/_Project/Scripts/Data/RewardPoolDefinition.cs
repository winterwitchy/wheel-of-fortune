using System.Collections.Generic;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Data
{
    public abstract class RewardPoolDefinition : ScriptableObject
    {
        public abstract IRewardSource CreateSource(CoreModelFactory factory);

        public virtual void LinkSource(IRewardSource source, CoreModelFactory factory)
        {
        }

        public abstract void CollectValidationErrors(List<string> errors);

        protected void CollectItemErrors(IReadOnlyList<RewardItemData> items, List<string> errors)
        {
            if (items.Count == 0)
            {
                errors.Add($"{name} has no rewards.");
                return;
            }

            foreach (var item in items)
            {
                if (!item)
                {
                    errors.Add($"{name} has an empty reward entry.");
                    return;
                }

                if (!item.HasId)
                {
                    errors.Add($"{name}: {item.name} has no id.");
                }
            }
        }
    }
}
