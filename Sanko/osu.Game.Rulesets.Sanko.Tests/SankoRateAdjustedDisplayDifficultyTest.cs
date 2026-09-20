// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sanko.Mods;

namespace osu.Game.Rulesets.Sanko.Tests
{
    [TestFixture]
    public class SankoRateAdjustedDisplayDifficultyTest
    {
        private static IEnumerable<float> difficultyValuesToTest()
        {
            for (float i = 0; i <= 10; i += 0.5f)
                yield return i;
        }

        [TestCaseSource(nameof(difficultyValuesToTest))]
        public void TestOverallDifficultyIsUnchangedWithRateEqualToOne(float originalOverallDifficulty)
        {
            var ruleset = new SankoRuleset();
            var difficulty = new BeatmapDifficulty { OverallDifficulty = originalOverallDifficulty };
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, []);

            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(originalOverallDifficulty));
        }

        [Test]
        public void TestRateBelowOne()
        {
            var ruleset = new SankoRuleset();
            var difficulty = new BeatmapDifficulty();
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, [new SankoModHalfTime()]);

            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(1.11).Within(0.01));
        }

        [Test]
        public void TestRateAboveOne()
        {
            var ruleset = new SankoRuleset();
            var difficulty = new BeatmapDifficulty();
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, [new SankoModDoubleTime()]);

            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(8.89).Within(0.01));
        }
    }
}
