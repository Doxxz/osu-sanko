// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sanko.Configuration;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sanko.Tests
{
    [TestFixture]
    public partial class TestSceneDrumTouchInputArea : OsuTestScene
    {
        private DrumTouchInputArea drumTouchInputArea = null!;

        private readonly Bindable<SankoTouchControlScheme> controlScheme = new Bindable<SankoTouchControlScheme>();

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SankoRulesetConfigManager)RulesetConfigs.GetConfigFor(Ruleset.Value.CreateInstance()).AsNonNull();
            config.BindWith(SankoRulesetSetting.TouchControlScheme, controlScheme);
        }

        private void createDrum()
        {
            Child = new SankoInputManager(new SankoRuleset().RulesetInfo)
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new InputDrum
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = 0.2f,
                    },
                    drumTouchInputArea = new DrumTouchInputArea
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                    }
                }
            };
        }

        [Test]
        public void TestDrum()
        {
            AddStep("create drum", createDrum);
            AddStep("show drum", () => drumTouchInputArea.Show());

            AddStep("change scheme (kddk)", () => controlScheme.Value = SankoTouchControlScheme.KDDK);
            AddStep("change scheme (kkdd)", () => controlScheme.Value = SankoTouchControlScheme.KKDD);
            AddStep("change scheme (ddkk)", () => controlScheme.Value = SankoTouchControlScheme.DDKK);
        }

        protected override Ruleset CreateRuleset() => new SankoRuleset();
    }
}
