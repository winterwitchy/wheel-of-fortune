using System.Globalization;

namespace WheelGame.Views
{
    public static class AmountFormatter
    {
        public static string Format(int amount)
        {
            if (amount >= 1_000_000)
                return $"x{(amount / 1_000_000f).ToString("0.#", CultureInfo.InvariantCulture)}M";

            if (amount >= 1_000)
                return $"x{(amount / 1_000f).ToString("0.#", CultureInfo.InvariantCulture)}K";

            return $"x{amount.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}