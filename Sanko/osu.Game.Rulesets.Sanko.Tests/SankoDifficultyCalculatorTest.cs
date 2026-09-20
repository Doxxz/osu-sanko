// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Sanko.Difficulty;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public class SankoDifficultyCalculatorTest : DifficultyCalculatorTest
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Sanko";

        // These beatmaps contain no tsu notes, so their rating must match osu!taiko's exactly.
        [TestCase(3.319084940658167d, 200, "diffcalc-test")]
        [TestCase(3.319084940658167d, 200, "diffcalc-test-strong")]
        public void Test(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(4.455142137225538d, 200, "diffcalc-test")]
        [TestCase(4.455142137225538d, 200, "diffcalc-test-strong")]
        public void TestClockRateAdjusted(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new SankoModDoubleTime());

        protected override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new SankoDifficultyCalculator(new SankoRuleset().RulesetInfo, beatmap);

        protected override Ruleset CreateRuleset() => new SankoRuleset();
    }
}
