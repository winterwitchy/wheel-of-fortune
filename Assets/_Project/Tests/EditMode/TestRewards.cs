using System.Reflection;
using UnityEngine;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public static class TestRewards
    {
        public static RewardItemData Create(string id, bool isCurrency)
        {
            var reward = ScriptableObject.CreateInstance<RewardItemData>();
            SetField(reward, "_id", id);
            SetField(reward, "_isCurrency", isCurrency);
            return reward;
        }

        private static void SetField(RewardItemData reward, string fieldName, object value)
        {
            var field = typeof(RewardItemData).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(reward, value);
        }
    }
}
