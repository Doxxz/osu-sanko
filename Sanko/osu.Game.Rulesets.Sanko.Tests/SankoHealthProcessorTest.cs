// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Judgements;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Scoring;

namespace osu.Game.Rulesets.Sanko.Tests
{
    [TestFixture]
    public class SankoHealthProcessorTest
    {
        [Test]
        public void TestHitsOnlyGreat()
        {
            var beatmap = new SankoBeatmap
            {
                HitObjects =
                {
                    new Hit(),
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 },
                    new Hit { StartTime = 3000 },
                    new Hit { StartTime = 4000 },
                }
            };

            var healthProcessor = new SankoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new SankoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], new SankoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[2], new SankoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[3], new SankoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[4], new SankoJudgement()) { Type = HitResult.Great });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.EqualTo(1));
                Assert.That(healthProcessor.HasFailed, Is.False);
            });
        }

        [Test]
        public void TestHitsAboveThreshold()
        {
            var beatmap = new SankoBeatmap
            {
                HitObjects =
                {
                    new Hit(),
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 },
                    new Hit { StartTime = 3000 },
                    new Hit { StartTime = 4000 },
                }
            };

            var healthProcessor = new SankoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new SankoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], new SankoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[2], new SankoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[3], new SankoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[4], new SankoJudgement()) { Type = HitResult.Miss });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.GreaterThan(0.5));
                Assert.That(healthProcessor.HasFailed, Is.False);
            });
        }

        [Test]
        public void TestHitsBelowThreshold()
        {
            var beatmap = new SankoBeatmap
            {
                HitObjects =
                {
                    new Hit(),
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 },
                    new Hit { StartTime = 3000 },
                    new Hit { StartTime = 4000 },
                }
            };

            var healthProcessor = new SankoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new SankoJudgement()) { Type = HitResult.Miss });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], new SankoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[2], new SankoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[3], new SankoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[4], new SankoJudgement()) { Type = HitResult.Miss });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.LessThan(0.5));
                Assert.That(healthProcessor.HasFailed, Is.True);
            });
        }

        [Test]
        public void TestDrumRollOnly()
        {
            var beatmap = new SankoBeatmap
            {
                HitObjects =
                {
                    new DrumRoll { Duration = 2000 }
                }
            };

            foreach (var ho in beatmap.HitObjects)
                ho.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            var healthProcessor = new SankoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            foreach (var nested in beatmap.HitObjects[0].NestedHitObjects)
            {
                var nestedJudgement = nested.Judgement;
                healthProcessor.ApplyResult(new JudgementResult(nested, nestedJudgement) { Type = nestedJudgement.MaxResult });
            }

            var judgement = beatmap.HitObjects[0].Judgement;
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], judgement) { Type = judgement.MaxResult });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.EqualTo(1));
                Assert.That(healthProcessor.HasFailed, Is.False);
            });
        }

        [Test]
        public void TestSwellOnly()
        {
            var beatmap = new SankoBeatmap
            {
                HitObjects =
                {
                    new Swell { Duration = 2000 }
                }
            };

            foreach (var ho in beatmap.HitObjects)
                ho.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            var healthProcessor = new SankoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            foreach (var nested in beatmap.HitObjects[0].NestedHitObjects)
            {
                var nestedJudgement = nested.Judgement;
                healthProcessor.ApplyResult(new JudgementResult(nested, nestedJudgement) { Type = nestedJudgement.MaxResult });
            }

            var judgement = beatmap.HitObjects[0].Judgement;
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], judgement) { Type = judgement.MaxResult });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.EqualTo(1));
                Assert.That(healthProcessor.HasFailed, Is.False);
            });
        }

        [Test]
        public void TestMissHitAndHitSwell()
        {
            var beatmap = new SankoBeatmap
            {
                HitObjects =
                {
                    new Hit(),
                    new Swell { Duration = 2000 }
                }
            };

            foreach (var ho in beatmap.HitObjects)
                ho.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            var healthProcessor = new SankoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new SankoJudgement()) { Type = HitResult.Miss });

            foreach (var nested in beatmap.HitObjects[1].NestedHitObjects)
            {
                var nestedJudgement = nested.CreateJudgement();
                healthProcessor.ApplyResult(new JudgementResult(nested, nestedJudgement) { Type = nestedJudgement.MaxResult });
            }

            var judgement = beatmap.HitObjects[1].CreateJudgement();
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], judgement) { Type = judgement.MaxResult });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.EqualTo(0));
                Assert.That(healthProcessor.HasFailed, Is.True);
            });
        }

        private static readonly object[][] test_cases =
        [
            // hitobject, fail expected after miss
            [new Hit(), true],
            [new Hit.StrongNestedHit(new Hit()), false],
            [new DrumRollTick(new DrumRoll()), false],
            [new DrumRollTick.StrongNestedHit(new DrumRollTick(new DrumRoll())), false],
            [new DrumRoll(), false],
            [new SwellTick(), false],
            [new Swell(), false]
        ];

        [TestCaseSource(nameof(test_cases))]
        public void TestFailAfterMinResult(SankoHitObject hitObject, bool failExpected)
        {
            var healthProcessor = new SankoHealthProcessor();
            healthProcessor.ApplyBeatmap(new SankoBeatmap
            {
                HitObjects = { hitObject }
            });

            var result = new JudgementResult(hitObject, hitObject.CreateJudgement());
            result.Type = result.Judgement.MinResult;
            healthProcessor.ApplyResult(result);

            Assert.That(healthProcessor.HasFailed, Is.EqualTo(failExpected));
        }

        [TestCaseSource(nameof(test_cases))]
        public void TestNoFailAfterMaxResult(SankoHitObject hitObject, bool _)
        {
            var healthProcessor = new SankoHealthProcessor();
            healthProcessor.ApplyBeatmap(new SankoBeatmap
            {
                HitObjects = { hitObject }
            });

            var result = new JudgementResult(hitObject, hitObject.CreateJudgement());
            result.Type = result.Judgement.MaxResult;
            healthProcessor.ApplyResult(result);

            Assert.That(healthProcessor.HasFailed, Is.False);
        }
    }
}
