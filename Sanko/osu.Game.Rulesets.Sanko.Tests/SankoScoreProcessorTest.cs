// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.Judgements;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Scoring;

namespace osu.Game.Rulesets.Sanko.Tests
{
    [TestFixture]
    public class SankoScoreProcessorTest
    {
        [Test]
        public void TestInaccurateHitScore()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new Hit(),
                    new Hit { StartTime = 1000 }
                }
            };

            var scoreProcessor = new SankoScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a miss judgement
            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new SankoJudgement()) { Type = HitResult.Great });
            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], new SankoJudgement()) { Type = HitResult.Ok });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(453745));
            Assert.That(scoreProcessor.Accuracy.Value, Is.EqualTo(0.75).Within(0.0001));
        }
    }
}
