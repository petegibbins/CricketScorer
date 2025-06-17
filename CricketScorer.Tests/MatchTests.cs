using CricketScorer.Core.Services;
using CricketScorer.Core.Models;

namespace CricketScorer.Tests
{
    [TestClass]
    public sealed class MatchTests
    {

        [TestMethod]
        public void WicketsSummary_CorrectlyDistinguishesBowlerAndTotalWickets()
        {
            // Arrange
            var match = new Match
            {
                TeamA = "Tuddenham",
                TeamB = "Opposition",
                FirstInningsOvers = new List<Over>
        {
            new Over
            {
                Bowler = "Zoe",
                Batter1 = "Alice",
                Batter2 = "Beth",
                Deliveries = new List<Ball>
                {
                    new Ball { Runs = 0, IsWicket = true, DismissalType = "Bowled" },   // credited
                    new Ball { Runs = 0, IsWicket = true, DismissalType = "Stumped" },  // credited
                    new Ball { Runs = 0, IsWicket = true, DismissalType = "Caught" },   // credited
                    new Ball { Runs = 0, IsWicket = true, DismissalType = "Run Out" },   // NOT credited
                    new Ball { Runs = 0, IsWicket = true, DismissalType = "Hit Wicket" }   // NOT credited
                }
            }
        }
            };

            var result = MatchConverter.BuildMatchResult(match);

            // Act
            var bowlingStat = result.TeamBBowlingStats.FirstOrDefault(b => b.Bowler == "Zoe");

            // Assert
            Assert.AreEqual(5, result.TeamAWickets, "All dismissals count towards total wickets lost.");
            Assert.IsNotNull(bowlingStat, "Zoe should appear in bowling stats.");
            Assert.AreEqual(3, bowlingStat.Wickets, "Only Bowled, stumped and Caught count for the bowler.");
        }


        [TestMethod]
        public void TeamBCanPassTargetButStillLoseDueToWickets()
        {
            var match = new Match
            {
                TeamAScore = 50,
                TeamBScore = 0,
                TotalOvers = 2,
                Format = Match.MatchFormat.Standard,
                OversDetails = new List<Over>
        {
            new Over
            {
                Deliveries = new List<Ball>
                {
                    new Ball { Runs = 51 }, // massive hit
                    new Ball { Runs = 0, IsWicket = true }, // -5 penalty
                    new Ball { Runs = 0, IsWicket = true },
                    new Ball { Runs = 0 },
                    new Ball { Runs = 0 },
                    new Ball { Runs = 0 }
                }
            },
            new Over
            {
                Deliveries = new List<Ball>
                {
                    new Ball { Runs = 0 },
                    new Ball { Runs = 0 },
                    new Ball { Runs = 0, IsWicket = true }, // more penalties
                    new Ball { Runs = 0 },
                    new Ball { Runs = 0 },
                    new Ball { Runs = 0 }
                }
            }
        },
                IsFirstInnings = false
            };

            match.Runs = match.OversDetails
                .SelectMany(o => o.Deliveries)
                .Sum(b => b.Runs) - match.OversDetails
                .SelectMany(o => o.Deliveries)
                .Count(b => b.IsWicket) * 5;

            Assert.IsTrue(match.Runs < match.TeamAScore, "Despite passing the score initially, Team B lost due to penalties.");
        }

        [TestMethod]
        public void SummariseBowling_AcrossMultiplePairs_ComputesCorrectly()
        {
            var overs = new List<Over>
            {
                new Over
                {
                    Bowler = "Zoe",
                    Batter1 = "Alice",
                    Batter2 = "Beth",
                    Deliveries = new List<Ball>
                    {
                        new Ball { Runs = 1 },
                        new Ball { Runs = 0, IsWicket = true, DismissalType = "Caught" }, // counts
                        new Ball { Runs = 2 },
                        new Ball { Runs = 0 }
                    }
                },
                new Over
                {
                    Bowler = "Zoe",
                    Batter1 = "Carol",
                    Batter2 = "Diana",
                    Deliveries = new List<Ball>
                    {
                        new Ball { Runs = 0, IsWicket = true, DismissalType = "Run Out" }, // NOT counted
                        new Ball { Runs = 1, IsWide = true },
                        new Ball { Runs = 4 },
                        new Ball { Runs = 0, IsWicket = true, DismissalType = "Bowled" }  // counts
                    }
                }
            };

            var match = new Match
            {
                TeamA = "Tuddenham",
                TeamB = "Opposition",
                FirstInningsOvers = new List<Over>(), // irrelevant here
                SecondInningsOvers = overs // Team A bowled, so this is their data
            };

            var result = MatchConverter.BuildMatchResult(match);

            var zoeStats = result.TeamABowlingStats.FirstOrDefault(b => b.Bowler == "Zoe");
            Assert.IsNotNull(zoeStats);

            // Runs: 1 + 0 + 2 + 0 + 0 (run out) + 1 (wide) + 4 + 0 = 8
            Assert.AreEqual(8, zoeStats.RunsConceded);

            // Wickets: Caught and Bowled only (Run Out doesn't count)
            Assert.AreEqual(2, zoeStats.Wickets);

            // Extras: Only 1 wide
            Assert.AreEqual(1, zoeStats.ExtrasConceded);
        }


        [TestMethod]
        public void GroupBattingPairs_MultiplePairsAcrossOvers()
        {
            var overs = new List<Over>
    {
        new Over
        {
            Batter1 = "Alice",
            Batter2 = "Beth",
            Deliveries = new List<Ball>
            {
                new Ball { Runs = 1 },
                new Ball { Runs = 2 },
                new Ball { Runs = 3 },
                new Ball { Runs = 0, IsWicket = true },
                new Ball { Runs = 0 },
                new Ball { Runs = 1 }
            }
        },
        new Over
        {
            Batter1 = "Carol",
            Batter2 = "Diana",
            Deliveries = new List<Ball>
            {
                new Ball { Runs = 4 },
                new Ball { Runs = 1 },
                new Ball { Runs = 0, IsWicket = true },
                new Ball { Runs = 2 },
                new Ball { Runs = 1, IsWide = true },  // not counted in batting pair runs
                new Ball { Runs = 3 }
            }
        }
    };

            var match = new Match
            {
                TeamA = "Tuddenham",
                TeamB = "Opposition",
                TeamAPlayers = new() { "Alice", "Beth", "Carol", "Diana" },
                TeamBPlayers = new() { "Zoe", "Nina" },
                FirstInningsOvers = overs
            };

            var result = MatchConverter.BuildMatchResult(match);

            Assert.AreEqual(2, result.TeamABattingPairs.Count);

            var pair1 = result.TeamABattingPairs.First(p => p.Batter1 == "Alice");
            Assert.AreEqual(7, pair1.RunsScored);     // 1 + 2 + 3 + 0 + 0 + 1
            Assert.AreEqual(1, pair1.WicketsLost);

            var pair2 = result.TeamABattingPairs.First(p => p.Batter1 == "Carol");
            Assert.AreEqual(10, pair2.RunsScored);    // 4 + 1 + 2 + 3 (ignores wide)
            Assert.AreEqual(1, pair2.WicketsLost);
        }
        [TestMethod]
        public void GroupBattingPairs_ComputesRunsAndWicketsCorrectly()
        {
            var overs = new List<Over>
    {
        new Over
        {
            Batter1 = "Alice",
            Batter2 = "Beth",
            Deliveries = new List<Ball>
            {
                new Ball { Runs = 1 },                         // 1 run
                new Ball { Runs = 4 },                         // 4 runs
                new Ball { Runs = 0, IsWicket = true },        // 1 wicket
                new Ball { Runs = 2 },
                new Ball { Runs = 1, IsWide = true },          // extra – ignored for pair
                new Ball { Runs = 0, IsWicket = true },        // another wicket
                new Ball { Runs = 3 }
            }
        }
    };

            var match = new Match
            {
                TeamA = "Tuddenham",
                TeamB = "Opposition",
                FirstInningsOvers = overs,
                TeamAPlayers = new List<string> { "Alice", "Beth", "Carol" },
                TeamBPlayers = new List<string> { "Zoe", "Nina" }
            };

            var result = MatchConverter.BuildMatchResult(match);

            Assert.AreEqual(1, result.TeamABattingPairs.Count);

            var pair = result.TeamABattingPairs[0];
            Assert.AreEqual("Alice", pair.Batter1);
            Assert.AreEqual("Beth", pair.Batter2);
            Assert.AreEqual(10, pair.RunsScored); // 1 + 4 + 2 + 3 (ignores wide)
            Assert.AreEqual(2, pair.WicketsLost);
        }


        [TestMethod]
        public void SummariseBowling_CorrectlyCountsWicketsCreditedToBowler()
        {
            var overs = new List<Over>
            {
                new Over
                {
                    Bowler = "Zoe",
                    Deliveries = new List<Ball>
                    {
                        new Ball { Runs = 0, IsWicket = true, DismissalType = "Hit Wicket" },     // counts for bowler
                        new Ball { Runs = 0, IsWicket = true, DismissalType = "Caught" },     // counts for bowler
                        new Ball { Runs = 0, IsWicket = true, DismissalType = "Stumped" },    // counts for bowler
                        new Ball { Runs = 0, IsWicket = true, DismissalType = "Bowled" },     // counts for bowler
                        new Ball { Runs = 0, IsWicket = true, DismissalType = "Run Out" },    // NOT counted
                        new Ball { Runs = 2 }
                    }
                }
            };

            var result = MatchConverter.BuildMatchResult(new Match
            {
                TeamA = "Tuddenham",
                TeamB = "Opposition",
                FirstInningsOvers = overs,
                SecondInningsOvers = new List<Over>() // irrelevant for this test
            });

            var bowlerStat = result.TeamBBowlingStats.FirstOrDefault(b => b.Bowler == "Zoe");

            Assert.IsNotNull(bowlerStat);
            Assert.AreEqual(3, bowlerStat.Wickets, "Only bowled, stumped and caught should be credited");
        }


        [TestMethod]
        public void BuildMatchResult_ComputesBattingAndExtrasCorrectly()
        {
            var match = new Match
            {
                TeamA = "Tuddenham",
                TeamB = "Opposition",
                FirstInningsOvers = new List<Over>
            {
                new Over
                {
                    Bowler = "Zoe",
                    Batter1 = "Alice",
                    Batter2 = "Beth",
                    Deliveries = new List<Ball>
                    {
                        new Ball { Runs = 1 },             // Legal
                        new Ball { Runs = 4 },             // Legal
                        new Ball { Runs = 0 },             // Dot
                        new Ball { Runs = 2 },             // Legal
                        new Ball { Runs = 1, IsWide = true },  // Wide (should count as extra)
                        new Ball { Runs = 1, IsNoBall = true } // No-ball (should count as extra)
                    }
                }
            },
                SecondInningsOvers = new List<Over>() // Not used here
            };

            var result = MatchConverter.BuildMatchResult(match);

            Assert.AreEqual(7, result.TeamABattingRuns); // 1+4+0+2 = 7 (legal scoring)
            Assert.AreEqual(2, result.TeamAExtras);      // 1 wide + 1 no-ball
            Assert.AreEqual(9, result.TeamAScore);       // Total = 7 + 2
        }

        [TestMethod]
        public void BuildMatchResult_ProducesCorrectTotal()
        {
            var match = new Match
            {
                TeamA = "Team A",
                TeamB = "Team B",
                FirstInningsOvers = new List<Over>
        {
            new Over
            {
                Batter1 = "Alice",
                Batter2 = "Beth",
                Deliveries = new List<Ball>
                {
                    new Ball { Runs = 4 },
                    new Ball { Runs = 2 },
                    new Ball { Runs = 1, IsWide = true }, // Should count as extra
                    new Ball { Runs = 3 }
                }
            }
        }
            };

            var result = MatchConverter.BuildMatchResult(match);

            Assert.AreEqual(9, result.TeamABattingRuns); // 4 + 2 + 3
            Assert.AreEqual(1, result.TeamAExtras);      // 1 wide
            Assert.AreEqual(10, result.TeamAScore);      // 9 + 1
        }

        [TestMethod]
        public void NewMatch_HasZeroRunsAndWickets()
        {
            var match = new Match();
            Assert.AreEqual(0, match.Runs);
            Assert.AreEqual(0, match.Wickets);
        }
    }
}
