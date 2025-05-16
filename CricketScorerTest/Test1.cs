

namespace CricketScorerTest
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var match = new Match
            {
                TeamA = "Team A",
                TeamB = "Team B",
                FirstInningsOvers = new List<Over>(),
                SecondInningsOvers = new List<Over>()
            };
            // Act
            var result = MatchConverter.BuildMatchResult(match);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Team A", result.TeamA);
            Assert.AreEqual("Team B", result.TeamB);
        }
    }
}
