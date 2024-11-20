using ZwiftDataCollectionAgent.Console;

namespace ZwiftDataCollection.Tests
{
    [TestClass]
    public class ProcessTests
    {
        private class MockConfig : IBespokeConfig
        {
            public string zwiftPassword { get; set; }
            public string zwiftUsername { get; set; }
            public int zwiftId { get; set; }
        }

        [DataTestMethod]
        [DataRow(0f, 100f, 5f, 0)]
        [DataRow(2.5f, 100f, 5f, 50)]
        [DataRow(5f, 100f, 5f, 100)]
        [DataRow(10f, 100f, 5f, 100)]
        public void GetGradeAddend_ReturnsExpectedResult(float currentGrade, float maxAdditionalPower, float gradeDenominator, int expected)
        {
            // Arrange
            var cfg = new MockConfig();
            var process = new Process(cfg);

            // Act
            int result = process.GetAddend(currentGrade, maxAdditionalPower, gradeDenominator);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CalculateAdditionalWatts_ShouldReturnCorrectValue_WhenDraftAndGradeArePositive()
        {
            // Arrange
            var process = new Process(new MockConfig());
            int draft = 10;
            float grade = 2.5f;
            int maxAdditionalPower = 100;
            float draftDenominator = 50f;
            float gradeDenominator = 5f;

            // Act
            int result = process.CalculateAdditionalWatts(draft, grade, maxAdditionalPower, draftDenominator, gradeDenominator);

            // Assert
            Assert.AreEqual(70, result);
        }

        [TestMethod]
        public void CalculateAdditionalWatts_ShouldReturnMaxAdditionalPower_WhenSumExceedsMax()
        {
            // Arrange
            var process = new Process(new MockConfig());
            int draft = 50;
            float grade = 5f;
            int maxAdditionalPower = 100;
            float draftDenominator = 50f;
            float gradeDenominator = 5f;

            // Act
            int result = process.CalculateAdditionalWatts(draft, grade, maxAdditionalPower, draftDenominator, gradeDenominator);

            // Assert
            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void CalculateAdditionalWatts_ShouldReturnZero_WhenDraftAndGradeAreZero()
        {
            // Arrange
            var process = new Process(new MockConfig());
            int draft = 0;
            float grade = 0f;
            int maxAdditionalPower = 100;
            float draftDenominator = 50f;
            float gradeDenominator = 5f;

            // Act
            int result = process.CalculateAdditionalWatts(draft, grade, maxAdditionalPower, draftDenominator, gradeDenominator);

            // Assert
            Assert.AreEqual(0, result);
        }
    }
}