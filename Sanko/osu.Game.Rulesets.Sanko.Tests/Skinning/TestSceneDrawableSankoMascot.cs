// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.Judgements;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Scoring;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sanko.Tests.Skinning
{
    [TestFixture]
    public partial class TestSceneDrawableSankoMascot : SankoSkinnableTestScene
    {
        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        private SankoScoreProcessor scoreProcessor = null!;

        private IEnumerable<DrawableSankoMascot> mascots => this.ChildrenOfType<DrawableSankoMascot>();

        private IEnumerable<DrawableSankoMascot> animatedMascots =>
            mascots.Where(mascot => mascot.ChildrenOfType<TextureAnimation>().All(animation => animation.FrameCount > 0));

        private IEnumerable<SankoPlayfield> playfields => this.ChildrenOfType<SankoPlayfield>();

        [SetUp]
        public void SetUp()
        {
            scoreProcessor = new SankoScoreProcessor();
        }

        [Test]
        public void TestStateAnimations()
        {
            AddStep("set beatmap", () => setBeatmap());

            AddStep("clear state", () => SetContents(_ => new SankoMascotAnimation(SankoMascotAnimationState.Clear)));
            AddStep("idle state", () => SetContents(_ => new SankoMascotAnimation(SankoMascotAnimationState.Idle)));
            AddStep("kiai state", () => SetContents(_ => new SankoMascotAnimation(SankoMascotAnimationState.Kiai)));
            AddStep("fail state", () => SetContents(_ => new SankoMascotAnimation(SankoMascotAnimationState.Fail)));
        }

        [Test]
        public void TestInitialState()
        {
            AddStep("set beatmap", () => setBeatmap());

            AddStep("create mascot", () => SetContents(_ => new DrawableSankoMascot { RelativeSizeAxes = Axes.Both }));

            AddAssert("mascot initially idle", () => allMascotsIn(SankoMascotAnimationState.Idle));
        }

        [Test]
        public void TestClearStateTransition()
        {
            AddStep("set beatmap", () => setBeatmap());

            AddStep("create mascot", () => SetContents(_ => new DrawableSankoMascot { RelativeSizeAxes = Axes.Both }));

            AddStep("set clear state", () => mascots.ForEach(mascot => mascot.State.Value = SankoMascotAnimationState.Clear));
            AddStep("miss", () => mascots.ForEach(mascot => mascot.LastResult.Value = new JudgementResult(new Hit(), new SankoJudgement()) { Type = HitResult.Miss }));
            AddAssert("skins with animations remain in clear state", () => animatedMascotsIn(SankoMascotAnimationState.Clear));
            AddUntilStep("state reverts to fail", () => allMascotsIn(SankoMascotAnimationState.Fail));

            AddStep("set clear state again", () => mascots.ForEach(mascot => mascot.State.Value = SankoMascotAnimationState.Clear));
            AddAssert("skins with animations change to clear", () => animatedMascotsIn(SankoMascotAnimationState.Clear));
        }

        [Test]
        public void TestIdleState()
        {
            prepareDrawableRulesetAndBeatmap(false);

            var hit = new Hit();
            assertStateAfterResult(new JudgementResult(hit, new SankoJudgement()) { Type = HitResult.Great }, SankoMascotAnimationState.Idle);
            assertStateAfterResult(new JudgementResult(new Hit.StrongNestedHit(hit), new SankoStrongJudgement()) { Type = HitResult.IgnoreMiss }, SankoMascotAnimationState.Idle);
        }

        [Test]
        public void TestKiaiState()
        {
            prepareDrawableRulesetAndBeatmap(true);

            assertStateAfterResult(new JudgementResult(new Hit(), new SankoJudgement()) { Type = HitResult.Ok }, SankoMascotAnimationState.Kiai);
            assertStateAfterResult(new JudgementResult(new Hit(), new SankoStrongJudgement()) { Type = HitResult.IgnoreMiss }, SankoMascotAnimationState.Kiai);
            assertStateAfterResult(new JudgementResult(new Hit(), new SankoJudgement()) { Type = HitResult.Miss }, SankoMascotAnimationState.Fail);
        }

        [Test]
        public void TestMissState()
        {
            prepareDrawableRulesetAndBeatmap(false);

            assertStateAfterResult(new JudgementResult(new Hit(), new SankoJudgement()) { Type = HitResult.Great }, SankoMascotAnimationState.Idle);
            assertStateAfterResult(new JudgementResult(new Hit(), new SankoJudgement()) { Type = HitResult.Miss }, SankoMascotAnimationState.Fail);
            assertStateAfterResult(new JudgementResult(new Hit(), new SankoJudgement()) { Type = HitResult.Ok }, SankoMascotAnimationState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestClearStateOnComboMilestone(bool kiai)
        {
            prepareDrawableRulesetAndBeatmap(kiai);

            AddRepeatStep("reach 49 combo", () => applyNewResult(new JudgementResult(new Hit(), new SankoJudgement()) { Type = HitResult.Great }), 49);

            assertStateAfterResult(new JudgementResult(new Hit(), new SankoJudgement()) { Type = HitResult.Ok }, SankoMascotAnimationState.Clear);
        }

        [TestCase(true, SankoMascotAnimationState.Kiai)]
        [TestCase(false, SankoMascotAnimationState.Idle)]
        public void TestClearStateOnClearedSwell(bool kiai, SankoMascotAnimationState expectedStateAfterClear)
        {
            prepareDrawableRulesetAndBeatmap(kiai);

            assertStateAfterResult(new JudgementResult(new Swell(), new SankoSwellJudgement()) { Type = HitResult.Great }, SankoMascotAnimationState.Clear);
            AddUntilStep($"state reverts to {expectedStateAfterClear.ToString().ToLowerInvariant()}", () => allMascotsIn(expectedStateAfterClear));
        }

        private void setBeatmap(bool kiai = false)
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint { BeatLength = 90 });

            if (kiai)
                controlPointInfo.Add(0, new EffectControlPoint { KiaiMode = true });

            Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject> { new Hit { Type = HitType.Centre } },
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = "Unknown",
                        Title = "Sample Beatmap",
                        Author = { Username = "Craftplacer" },
                    },
                    Ruleset = new SankoRuleset().RulesetInfo
                },
                ControlPointInfo = controlPointInfo
            });

            scoreProcessor.ApplyBeatmap(Beatmap.Value.Beatmap);
        }

        private void prepareDrawableRulesetAndBeatmap(bool kiai)
        {
            AddStep("set beatmap", () => setBeatmap(kiai));

            AddStep("create drawable ruleset", () =>
            {
                SetContents(_ =>
                {
                    var ruleset = new SankoRuleset();
                    return new DrawableSankoRuleset(ruleset, Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo));
                });
            });

            AddUntilStep("wait for track to be loaded", () => MusicController.TrackLoaded);
            AddStep("start track", () => MusicController.CurrentTrack.Restart());
            AddUntilStep("wait for track started", () => MusicController.IsPlaying);
        }

        private void assertStateAfterResult(JudgementResult judgementResult, SankoMascotAnimationState expectedState)
        {
            SankoMascotAnimationState[] mascotStates = null!;

            AddStep($"{judgementResult.Type.ToString().ToLowerInvariant()} result for {judgementResult.Judgement.GetType().Name.Humanize(LetterCasing.LowerCase)}",
                () =>
                {
                    applyNewResult(judgementResult);
                    // store the states as soon as possible, so that the delay between steps doesn't incorrectly fail the test
                    // due to not checking if the state changed quickly enough.
                    Schedule(() => mascotStates = animatedMascots.Select(mascot => mascot.State.Value).ToArray());
                });

            AddAssert($"state is {expectedState.ToString().ToLowerInvariant()}", () => mascotStates.Distinct(), () => Is.EquivalentTo(new[] { expectedState }));
        }

        private void applyNewResult(JudgementResult judgementResult)
        {
            scoreProcessor.ApplyResult(judgementResult);

            foreach (var playfield in playfields)
            {
                var hit = new DrawableTestHit(new Hit(), judgementResult.Type);
                playfield.Add(hit);

                playfield.OnNewResult(hit, judgementResult);
            }

            foreach (var mascot in mascots)
            {
                mascot.LastResult.Value = judgementResult;
            }
        }

        private bool allMascotsIn(SankoMascotAnimationState state) => mascots.All(d => d.State.Value == state);
        private bool animatedMascotsIn(SankoMascotAnimationState state) => animatedMascots.Any(d => d.State.Value == state);
    }
}
