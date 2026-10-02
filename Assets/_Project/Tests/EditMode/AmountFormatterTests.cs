using NUnit.Framework;
using WheelGame.Views;

namespace WheelGame.Tests.EditMode
{
    public sealed class AmountFormatterTests
    {
        [TestCase(25, "x25")]
        [TestCase(999, "x999")]
        [TestCase(1000, "x1K")]
        [TestCase(1500, "x1.5K")]
        [TestCase(11375, "x11.4K")]
        [TestCase(1500000, "x1.5M")]
        public void Format_ReturnsCompactText(int amount, string expected)
        {
            Assert.AreEqual(expected, AmountFormatter.Format(amount));
        }
    }
}