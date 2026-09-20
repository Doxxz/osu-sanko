// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Sanko.Configuration
{
    public class SankoRulesetConfigManager : RulesetConfigManager<SankoRulesetSetting>
    {
        public SankoRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(SankoRulesetSetting.TouchControlScheme, SankoTouchControlScheme.KDDK);
            SetDefault(SankoRulesetSetting.RateAdjustedHitAnimation, true);
            SetDefault(SankoRulesetSetting.HitAnimations, true);
        }
    }

    public enum SankoRulesetSetting
    {
        TouchControlScheme,
        RateAdjustedHitAnimation,
        HitAnimations,
    }
}
