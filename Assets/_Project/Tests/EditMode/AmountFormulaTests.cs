using NUnit.Framework;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class AmountFormulaTests
    {
        [TestCase(1, 25)]
        [TestCase(3, 25)]
        [TestCase(4, 50)]
        [TestCase(7, 125)]
        [TestCase(9, 225)]
        [TestCase(30, 2275)]
        public void Evaluate_CashFormula(int zone, int expected)
        {
            Assert.AreEqual(expected, new AmountFormula(GrowthCurve.Quadratic, 10, 1, 25).Evaluate(zone));
        }

        [TestCase(1, 1)]
        [TestCase(7, 7)]
        public void Evaluate_GoldFormula(int zone, int expected)
        {
            Assert.AreEqual(expected, new AmountFormula(GrowthCurve.Linear, 1, 0, 1).Evaluate(zone));
        }

        [TestCase(1, 5)]
        [TestCase(4, 15)]
        [TestCase(13, 35)]
        public void Evaluate_PointFormula(int zone, int expected)
        {
            Assert.AreEqual(expected, new AmountFormula(GrowthCurve.Linear, 2, 1, 5).Evaluate(zone));
        }

        [TestCase(1, 2)]
        [TestCase(3, 2)]
        [TestCase(4, 4)]
        [TestCase(9, 6)]
        [TestCase(30, 10)]
        public void Evaluate_ConsumableFormula(int zone, int expected)
        {
            Assert.AreEqual(expected, new AmountFormula(GrowthCurve.SquareRoot, 1, 0, 2).Evaluate(zone));
        }
    }
}