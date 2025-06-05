using CricketScorer.Core.Models;
using CricketScorer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Tests
{
    [TestClass]
    public sealed class TargetLabelFormatterTests
    {
        private readonly Formatter _summaryService = new();

        [TestMethod]
        public void TargetLabel_Shows_NeedsMoreToWin()
        {
            var match = new Match
            {
                TeamAScore = 80,
                Runs = 75,
                TeamB = "Opposition"
            };

            var result = _summaryService.FormatTargetLabel(match);
            Assert.AreEqual("Opposition requires 6 more", result);
        }

        [TestMethod]
        public void TargetLabel_Shows_ScoresLevel()
        {
            var match = new Match
            {
                TeamAScore = 80,
                Runs = 80,
                TeamB = "Opposition"
            };

            var result = _summaryService.FormatTargetLabel(match);
            Assert.AreEqual("Scores level!", result);
        }

        [TestMethod]
        public void TargetLabel_Shows_CurrentlyAhead_Singular()
        {
            var match = new Match
            {
                TeamAScore = 80,
                Runs = 81,
                TeamB = "Opposition"
            };

            var result = _summaryService.FormatTargetLabel(match);
            Assert.AreEqual("Opposition is currently ahead by 1 run.", result);
        }

        [TestMethod]
        public void TargetLabel_Shows_CurrentlyAhead_Plural()
        {
            var match = new Match
            {
                TeamAScore = 80,
                Runs = 85,
                TeamB = "Opposition"
            };

            var result = _summaryService.FormatTargetLabel(match);
            Assert.AreEqual("Opposition is currently ahead by 5 runs.", result);
        }
    }

}
