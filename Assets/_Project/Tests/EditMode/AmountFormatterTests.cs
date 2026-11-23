using NUnit.Framework;
using WheelGame.Views;

namespace WheelGame.Tests.EditMode
{
    public sealed class AmountFormatterTests
    {
        [TestCase(25, "25")]
        [TestCase(999, "999")]
        [TestCase(1000, "1K")]
        [TestCase(1500, "1.5K")]
        [TestCase(11375, "11.4K")]
        [TestCase(1500000, "1.5M")]
        public void FormatCompact_ShortensLargeAmounts(int amount, string expected)
        {
            Assert.AreEqual(expected, AmountFormatter.FormatCompact(amount));
        }

        [TestCase(25, "x25")]
        [TestCase(1500, "x1.5K")]
        public void FormatCount_PrefixesTheCompactAmount(int amount, string expected)
        {
            Assert.AreEqual(expected, AmountFormatter.FormatCount(amount));
        }
    }
}
