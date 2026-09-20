// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Sanko.Configuration;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Objects.Drawables;
using osu.Game.Rulesets.Sanko.Replays;
using osu.Game.Rulesets.Sanko.Tests.Mods;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public partial class TestSceneHitAnimations : SankoModTestScene
    {
        [Test]
        public void TestGameplayCompletesWithHitAnimationsDisabled()
        {
            setHitAnimations(false);

            bool allSuccessfulHitsImmediatelyHidden = true;

            CreateModTest(new ModTestData
            {
                Autoplay = true,
                CreateBeatmap = createBeatmapWithStrongHit,
                PassCondition = () =>
                {
                    allSuccessfulHitsImmediatelyHidden &= getSuccessfulHits().All(h => h.Alpha == 0);
                    return allSuccessfulHitsImmediatelyHidden
                           && Player.ScoreProcessor.HasCompleted.Value
                           && Player.Results.All(result => result.Type == result.Judgement.MaxResult);
                },
            });
        }

        [Test]
        public void TestGameplayCompletesWhenStrongHitPressedWithSingleKey()
        {
            setHitAnimations(false);

            bool allSuccessfulHitsImmediatelyHidden = true;

            CreateModTest(new ModTestData
            {
                Autoplay = false,
                CreateBeatmap = createBeatmapWithStrongHit,
                // hit the strong note with a single key, such that its nested strong hit
                // has to wait out the full second-hit window before it can be judged.
                ReplayFrames = new List<ReplayFrame>
                {
                    new SankoReplayFrame(100, SankoAction.LeftCentre),
                    new SankoReplayFrame(120),
                    new SankoReplayFrame(300, SankoAction.LeftRim),
                    new SankoReplayFrame(320),
                },
                PassCondition = () =>
                {
                    allSuccessfulHitsImmediatelyHidden &= getSuccessfulHits().All(h => h.Alpha == 0);
                    return allSuccessfulHitsImmediatelyHidden && Player.ScoreProcessor.HasCompleted.Value;
                },
            });
        }

        [Test]
        public void TestGameplayCompletesWhenMissingWithHitAnimationsDisabled()
        {
            setHitAnimations(false);

            bool allSuccessfulHitsImmediatelyHidden = true;

            CreateModTest(new ModTestData
            {
                Autoplay = false,
                CreateBeatmap = createBeatmapWithStrongHit,
                PassCondition = () =>
                {
                    allSuccessfulHitsImmediatelyHidden &= getSuccessfulHits().All(h => h.Alpha == 0);
                    return allSuccessfulHitsImmediatelyHidden && Player.ScoreProcessor.HasCompleted.Value;
                },
            });
        }

        [Test]
        public void TestHitsStillVisibleAfterJudgementWithHitAnimationsEnabled()
        {
            setHitAnimations(true);

            bool allSuccessfulHitsImmediatelyHidden = true;

            CreateModTest(new ModTestData
            {
                Autoplay = true,
                CreateBeatmap = createBeatmapWithStrongHit,
                PassCondition = () =>
                {
                    allSuccessfulHitsImmediatelyHidden &= getSuccessfulHits().All(h => h.Alpha == 0);
                    return !allSuccessfulHitsImmediatelyHidden
                           && Player.ScoreProcessor.HasCompleted.Value
                           && Player.Results.All(result => result.Type == result.Judgement.MaxResult);
                },
            });
        }

        private IEnumerable<DrawableHit> getSuccessfulHits()
            => Player.ChildrenOfType<DrawableHit>().Where(hit => hit.Judged && hit.Result.IsHit && hit.Time.Current > hit.HitStateUpdateTime);

        private void setHitAnimations(bool enabled)
            => AddStep($"set hit animations to {enabled}", () =>
            {
                var config = (SankoRulesetConfigManager)RulesetConfigs.GetConfigFor(Ruleset.Value.CreateInstance()).AsNonNull();
                config.SetValue(SankoRulesetSetting.HitAnimations, enabled);
            });

        private static IBeatmap createBeatmapWithStrongHit()
        {
            var beatmap = new Beatmap<SankoHitObject>
            {
                HitObjects = new List<SankoHitObject>
                {
                    new Hit { Type = HitType.Centre, StartTime = 100 },
                    new Hit { Type = HitType.Rim, StartTime = 300, IsStrong = true },
                    new DrumRoll { StartTime = 500, Duration = 600 },
                    new Swell { StartTime = 1300, Duration = 500 },
                },
                BeatmapInfo =
                {
                    Ruleset = new SankoRuleset().RulesetInfo,
                },
            };

            // shorten the beat length such that the drum roll generates some ticks.
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 200 });

            return beatmap;
        }
    }
}
