// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Replays;

namespace osu.Game.Rulesets.Sanko.Tests.Judgements
{
    public partial class TestSceneDrumRollJudgements : JudgementTest
    {
        [Test]
        public void TestHitAllDrumRoll()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
                new SankoReplayFrame(1000, SankoAction.LeftCentre),
                new SankoReplayFrame(1001),
                new SankoReplayFrame(1250, SankoAction.LeftCentre),
                new SankoReplayFrame(1251),
                new SankoReplayFrame(1500, SankoAction.LeftCentre),
                new SankoReplayFrame(1501),
                new SankoReplayFrame(1750, SankoAction.LeftCentre),
                new SankoReplayFrame(1751),
                new SankoReplayFrame(2000, SankoAction.LeftCentre),
                new SankoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(false)));

            AssertJudgementCount(6);
            AssertResult<DrumRollTick>(0, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(1, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(2, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(3, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(4, HitResult.SmallBonus);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeDrumRoll()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
                new SankoReplayFrame(2000, SankoAction.LeftCentre),
                new SankoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(false)));

            AssertJudgementCount(6);
            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(1, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(2, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(3, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(4, HitResult.SmallBonus);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitNoneDrumRoll()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
            }, CreateBeatmap(createDrumRoll(false)));

            AssertJudgementCount(6);
            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(1, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(2, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(3, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(4, HitResult.IgnoreMiss);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitNoneStrongDrumRoll()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            for (int i = 0; i < 5; ++i)
            {
                AssertResult<DrumRollTick>(i, HitResult.IgnoreMiss);
                AssertResult<DrumRollTick.StrongNestedHit>(i, HitResult.IgnoreMiss);
            }

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitNoneStrongDrumRollWithTicksPastEnd()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
            }, CreateBeatmap(new DrumRoll
            {
                StartTime = 1000,
                // duration intentionally chosen.
                // default tick spacing is 250ms, so take half of that and add 1
                // to ensure a tick is spawned past the end time of the drum roll
                Duration = 1126,
                IsStrong = true
            }));

            AssertJudgementCount(14);

            for (int i = 0; i < 6; ++i)
            {
                AssertResult<DrumRollTick>(i, HitResult.IgnoreMiss);
                AssertResult<DrumRollTick.StrongNestedHit>(i, HitResult.IgnoreMiss);
            }

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitAllStrongDrumRollWithOneKey()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
                new SankoReplayFrame(1000, SankoAction.LeftCentre),
                new SankoReplayFrame(1001),
                new SankoReplayFrame(1250, SankoAction.LeftCentre),
                new SankoReplayFrame(1251),
                new SankoReplayFrame(1500, SankoAction.LeftCentre),
                new SankoReplayFrame(1501),
                new SankoReplayFrame(1750, SankoAction.LeftCentre),
                new SankoReplayFrame(1751),
                new SankoReplayFrame(2000, SankoAction.LeftCentre),
                new SankoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            for (int i = 0; i < 5; i++)
            {
                AssertResult<DrumRollTick>(i, HitResult.SmallBonus);
                AssertResult<StrongNestedHitObject>(i, HitResult.LargeBonus);
            }

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(5, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeStrongDrumRollWithOneKey()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
                new SankoReplayFrame(2000, SankoAction.LeftCentre),
                new SankoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<StrongNestedHitObject>(0, HitResult.IgnoreMiss);

            AssertResult<DrumRollTick>(4, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(4, HitResult.LargeBonus);

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(5, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitAllStrongDrumRollWithBothKeys()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
                new SankoReplayFrame(1000, SankoAction.LeftCentre, SankoAction.RightCentre),
                new SankoReplayFrame(1001),
                new SankoReplayFrame(1250, SankoAction.LeftCentre, SankoAction.RightCentre),
                new SankoReplayFrame(1251),
                new SankoReplayFrame(1500, SankoAction.LeftCentre, SankoAction.RightCentre),
                new SankoReplayFrame(1501),
                new SankoReplayFrame(1750, SankoAction.LeftCentre, SankoAction.RightCentre),
                new SankoReplayFrame(1751),
                new SankoReplayFrame(2000, SankoAction.LeftCentre, SankoAction.RightCentre),
                new SankoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            for (int i = 0; i < 5; i++)
            {
                AssertResult<DrumRollTick>(i, HitResult.SmallBonus);
                AssertResult<StrongNestedHitObject>(i, HitResult.LargeBonus);
            }

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(5, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeStrongDrumRollWithBothKeys()
        {
            PerformTest(new List<ReplayFrame>
            {
                new SankoReplayFrame(0),
                new SankoReplayFrame(2000, SankoAction.LeftCentre, SankoAction.RightCentre),
                new SankoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<StrongNestedHitObject>(0, HitResult.IgnoreMiss);

            AssertResult<DrumRollTick>(4, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(4, HitResult.LargeBonus);

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(5, HitResult.IgnoreHit);
        }

        private DrumRoll createDrumRoll(bool strong) => new DrumRoll
        {
            StartTime = 1000,
            Duration = 1000,
            IsStrong = strong
        };
    }
}
