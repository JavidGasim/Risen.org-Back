using Risen.Business.Utils;
using System;
using Xunit;

namespace Risen.Business.Tests
{
    public class RisenScoreCalculatorTests
    {
        [Theory]
        [InlineData(0, 0, 0, 0.00)]
        [InlineData(10, 1500, 3, 10 + 1500 / 150.0 + 3 * 0.5)]
        public void Calculate_ReturnsExpected(int weight, long xp, int streak, double expected)
        {
            var res = RisenScoreCalculator.Calculate(weight, xp, streak);
            var expectedDec = Math.Round((decimal)expected, 2);
            Assert.Equal(expectedDec, res);
        }
    }
}
