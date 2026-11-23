using System.Globalization;

namespace WheelGame.Views
{
    public static class AmountFormatter
    {
        #region Constants

        private const int THOUSAND = 1_000;
        private const int MILLION = 1_000_000;
        private const string COMPACT_FORMAT = "0.#";
        private const string COUNT_PREFIX = "x";

        #endregion

        #region Public Methods

        public static string FormatCompact(int amount)
        {
            if (amount >= MILLION)
            {
                return (amount / (float)MILLION).ToString(COMPACT_FORMAT, CultureInfo.InvariantCulture) + "M";
            }

            if (amount >= THOUSAND)
            {
                return (amount / (float)THOUSAND).ToString(COMPACT_FORMAT, CultureInfo.InvariantCulture) + "K";
            }

            return amount.ToString(CultureInfo.InvariantCulture);
        }

        public static string FormatCount(int amount)
        {
            return COUNT_PREFIX + FormatCompact(amount);
        }

        #endregion
    }
}
