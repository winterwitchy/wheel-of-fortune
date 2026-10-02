using WheelGame.Core;

namespace WheelGame.Infrastructure
{
    public sealed class UnityRandomProvider : IRandomProvider
    {
        public int Range(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }
    }
}