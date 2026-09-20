// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public partial class TestSceneSankoSuddenDeath : TestSceneSankoPlayer
    {
        protected override bool AllowFail => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Concat(new[] { new SankoModSuddenDeath() }).ToArray();
            return base.CreatePlayer(ruleset);
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) =>
            new SankoBeatmap
            {
                HitObjects =
                {
                    new Swell { StartTime = 1500 },
                    new Hit { StartTime = 100000 },
                },
                BeatmapInfo =
                {
                    Ruleset = new SankoRuleset().RulesetInfo
                }
            };

        [Test]
        public void TestSwellDoesNotFail()
        {
            bool judged = false;
            AddStep("Setup judgements", () =>
            {
                judged = false;
                Player.ScoreProcessor.NewJudgement += _ => judged = true;
            });
            AddUntilStep("swell judged", () => judged);
            AddAssert("not failed", () => !Player.GameplayState.HasFailed);
        }
    }
}
