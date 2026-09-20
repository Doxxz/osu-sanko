// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Sanko.Configuration;

namespace osu.Game.Rulesets.Sanko
{
    public partial class SankoSettingsSubsection : RulesetSettingsSubsection
    {
        public SankoSettingsSubsection(SankoRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SankoRulesetConfigManager)Config;

            FormCheckBox rateAdjustedAnimations;
            FormCheckBox hitAnimations;

            Children = new Drawable[]
            {
                new SettingsItemV2(new FormEnumDropdown<SankoTouchControlScheme>
                {
                    Caption = RulesetSettingsStrings.TouchControlScheme,
                    Current = config.GetBindable<SankoTouchControlScheme>(SankoRulesetSetting.TouchControlScheme)
                }),
                new SettingsItemV2(hitAnimations = new FormCheckBox
                {
                    Caption = RulesetSettingsStrings.HitAnimations,
                    HintText = RulesetSettingsStrings.HitAnimationsTaikoTooltip,
                    Current = config.GetBindable<bool>(SankoRulesetSetting.HitAnimations)
                }),
                new SettingsItemV2(rateAdjustedAnimations = new FormCheckBox
                {
                    Caption = RulesetSettingsStrings.RateAdjustedHitAnimation,
                    HintText = RulesetSettingsStrings.RateAdjustedHitAnimationTooltip,
                    Current = config.GetBindable<bool>(SankoRulesetSetting.RateAdjustedHitAnimation)
                })
                {
                    ApplyClassicDefault = c => ((IHasCurrentValue<bool>)c).Current.Value = false,
                }
            };

            hitAnimations.Current.BindValueChanged(val =>
            {
                rateAdjustedAnimations.Current.Disabled = !val.NewValue;
            }, true);
        }
    }
}
