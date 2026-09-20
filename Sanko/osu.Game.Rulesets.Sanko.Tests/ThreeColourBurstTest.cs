// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Difficulty;
using osu.Game.Rulesets.Sanko.Difficulty.Evaluators;
using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Rhythm;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public class ThreeColourBurstTest
    {
        [Test]
        public void TestThreeColourBurstIsRewarded()
        {
            var objects = createDifficultyObjects(100, HitType.Centre, HitType.Rim, HitType.Tsu, HitType.Centre, HitType.Rim, HitType.Tsu);

            Assert.That(ColourEvaluator.EvaluateThreeColourBurst(objects[^1]), Is.GreaterThan(0));
        }

        [Test]
        public void TestTwoColourBurstIsNotRewarded()
        {
            var objects = createDifficultyObjects(100, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim);

            Assert.That(objects.Select(o => ColourEvaluator.EvaluateThreeColourBurst(o)), Is.All.Zero);
        }

        [Test]
        public void TestSparseThreeColourBurstIsNotRewarded()
        {
            // The same three colours, but spread over a full 600ms, so they are not read as one burst.
            var objects = createDifficultyObjects(200, HitType.Centre, HitType.Rim, HitType.Tsu, HitType.Centre, HitType.Rim, HitType.Tsu);

            Assert.That(ColourEvaluator.EvaluateThreeColourBurst(objects[^1]), Is.Zero);
        }

        [Test]
        public void TestTighterBurstIsRewardedMore()
        {
            var fast = createDifficultyObjects(100, HitType.Centre, HitType.Rim, HitType.Tsu, HitType.Centre, HitType.Rim, HitType.Tsu);
            var slow = createDifficultyObjects(150, HitType.Centre, HitType.Rim, HitType.Tsu, HitType.Centre, HitType.Rim, HitType.Tsu);

            Assert.That(ColourEvaluator.EvaluateThreeColourBurst(fast[^1]), Is.GreaterThan(ColourEvaluator.EvaluateThreeColourBurst(slow[^1])));
        }

        [Test]
        public void TestThreeColourStreamRatesHigherThanTwoColour()
        {
            // "dkt" repeated vs "dkd" repeated: identical timing, but only the first mixes all three colours.
            double threeColour = calculateStarRating(repeat(new[] { HitType.Centre, HitType.Rim, HitType.Tsu }, 60));
            double twoColour = calculateStarRating(repeat(new[] { HitType.Centre, HitType.Rim, HitType.Centre }, 60));

            Assert.That(threeColour, Is.GreaterThan(twoColour));
        }

        private static HitType[] repeat(HitType[] pattern, int times)
        {
            var result = new List<HitType>();

            for (int i = 0; i < times; i++)
                result.AddRange(pattern);

            return result.ToArray();
        }

        private static double calculateStarRating(HitType[] types)
        {
            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 400 });

            var beatmap = new SankoBeatmap
            {
                ControlPointInfo = cpi,
                Difficulty = new BeatmapDifficulty { OverallDifficulty = 7, SliderMultiplier = 1.6 },
                BeatmapInfo = { Ruleset = new SankoRuleset().RulesetInfo },
            };

            for (int i = 0; i < types.Length; i++)
                beatmap.HitObjects.Add(new Hit { StartTime = i * 100, Type = types[i] });

            return new SankoDifficultyCalculator(new SankoRuleset().RulesetInfo, new TestWorkingBeatmap(beatmap)).Calculate().StarRating;
        }

        private static List<SankoDifficultyHitObject> createDifficultyObjects(double interval, params HitType[] types)
        {
            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 400 });

            var beatmap = new SankoBeatmap
            {
                ControlPointInfo = cpi,
                Difficulty = new BeatmapDifficulty { OverallDifficulty = 7, SliderMultiplier = 1.6 },
            };

            for (int i = 0; i < types.Length; i++)
            {
                var hit = new Hit { StartTime = i * interval, Type = types[i] };
                hit.ApplyDefaults(cpi, beatmap.Difficulty);
                beatmap.HitObjects.Add(hit);
            }

            var difficultyHitObjects = new List<DifficultyHitObject>();
            var centreObjects = new List<SankoDifficultyHitObject>();
            var rimObjects = new List<SankoDifficultyHitObject>();
            var noteObjects = new List<SankoDifficultyHitObject>();

            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                difficultyHitObjects.Add(new SankoDifficultyHitObject(
                    beatmap.HitObjects[i], beatmap.HitObjects[i - 1], 1,
                    difficultyHitObjects, centreObjects, rimObjects, noteObjects, difficultyHitObjects.Count,
                    beatmap.ControlPointInfo, beatmap.Difficulty.SliderMultiplier));
            }

            SankoColourDifficultyPreprocessor.ProcessAndAssign(difficultyHitObjects);
            SankoRhythmDifficultyPreprocessor.ProcessAndAssign(noteObjects);

            return difficultyHitObjects.Cast<SankoDifficultyHitObject>().ToList();
        }
    }
}
